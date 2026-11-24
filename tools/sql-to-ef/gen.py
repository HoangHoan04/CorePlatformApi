"""Generate Domain entities, EF Core configurations, DbContext and raw SQL scripts from base_erp_core.sql."""
import os
import re
import sys
from collections import defaultdict

sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sqlparse import parse, strip_line_comment  # noqa: E402
from sqlparse import Column  # noqa: E402
from naming import PROP_OVERRIDES, SCHEMA_NS, TABLE_RENAMES, class_name, plural, prop_name  # noqa: E402

SQL_PATH, ROOT = sys.argv[1], sys.argv[2]   # ROOT = CorePlatformApi/src
DOMAIN = os.path.join(ROOT, 'CorePlatformApi.Domain')
INFRA = os.path.join(ROOT, 'CorePlatformApi.Infrastructure')
APP = os.path.join(ROOT, 'CorePlatformApi.Application')

src, tables = parse(SQL_PATH)
by_full = {t.full: t for t in tables}
cls = {t.full: class_name(t) for t in tables}
assert len(set(cls.values())) == len(cls), 'duplicate class names'

HEADER = '// Nguồn schema: docs/base_erp_core.sql (làm phẳng: một schema public, mọi bảng kế thừa BaseEntity)\n'

VECTOR_TABLE, VECTOR_COL = 'ai.kb_chunks', 'embedding'
SHADOW_COLS = {('ai.kb_chunks', 'content_tsv'): ('ContentTsv', 'NpgsqlTsVector')}

# ---------------------------------------------------------------- flatten model
# Mọi bảng: một schema (public), khóa chính id (uuid v7) và đủ cột của BaseEntity.
#  * Khóa tự nhiên cũ (code, key, composite...) -> UNIQUE; FK trỏ tới chúng vẫn giữ (alternate key).
#  * Bảng partition -> bảng thường.
#  * Bảng gốc đã có is_deleted -> [SoftDelete] (Remove() = xóa mềm); bảng khác Remove() = xóa thật,
#    tránh đụng UNIQUE khi thêm lại bản ghi đã xóa.
BASE_COLS = ['id', 'tenant_id', 'created_at', 'created_by', 'updated_at', 'updated_by', 'is_deleted', 'deleted_at', 'deleted_by']
BASE_PROPS = {prop_name(c) for c in BASE_COLS} | {'Version'}
BASE_DEFS = {
    'id': ('uuid', True, 'gen_random_uuid()', 'Khóa chính (UUID v7)'),
    'tenant_id': ('uuid', False, None, 'Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống'),
    'created_at': ('timestamptz', True, 'now()', 'Thời điểm tạo bản ghi'),
    'created_by': ('uuid', False, None, 'Người tạo (users.id); NULL nếu hệ thống tạo'),
    'updated_at': ('timestamptz', False, None, 'Thời điểm cập nhật gần nhất (trigger tự gán)'),
    'updated_by': ('uuid', False, None, 'Người cập nhật gần nhất (users.id)'),
    'is_deleted': ('boolean', True, 'false', 'Cờ xóa mềm (true = đã xóa)'),
    'deleted_at': ('timestamptz', False, None, 'Thời điểm xóa mềm'),
    'deleted_by': ('uuid', False, None, 'Người xóa mềm (users.id)'),
}


def flatten(t):
    t.table = TABLE_RENAMES.get(t.full, t.name)
    t.soft_delete = any(c.name == 'is_deleted' for c in t.columns)
    t.partition_by = None
    if t.pk != ['id']:
        old = list(t.pk)
        t.uniques.insert(0, (f'{t.table}_{"_".join(old)}_key', old))
        for c in t.columns:
            if c.name in old:
                c.pk = False
        t.pk = ['id']
    existing = {c.name: c for c in t.columns}
    base = {}
    for name in BASE_COLS:
        sql_type, not_null, default, comment = BASE_DEFS[name]
        c = existing.get(name)
        if c is None:
            c = Column(name=name, sql_type=sql_type, not_null=not_null, default=default, comment=comment)
        else:
            assert c.sql_type == sql_type, (t.full, name, c.sql_type)
            assert not c.ref_table or name == 'tenant_id', (t.full, name)
            if not_null:
                c.not_null = True
                c.default = c.default or default
            c.comment = c.comment or comment
        base[name] = c
    base['id'].pk, base['id'].unique = True, False
    business = [c for c in t.columns if c.name not in BASE_COLS]
    t.columns = [base['id'], base['tenant_id']] + business + [base[n] for n in BASE_COLS[2:]]


for _t in tables:
    flatten(_t)
assert len({t.table for t in tables}) == len(tables), 'duplicate table names'


def cprop(t, col):
    """Tên thuộc tính C# của cột (có ngoại lệ khi trùng BaseEntity)."""
    return PROP_OVERRIDES.get((t.full, col), prop_name(col))


for _t in tables:
    for _c in _t.columns:
        if _c.name not in BASE_COLS:
            assert cprop(_t, _c.name) not in BASE_PROPS, (_t.full, _c.name)

# Cột khóa tự nhiên được FK trỏ tới -> alternate key
alt_keys = defaultdict(list)
for _t in tables:
    for _fk in _t.fks:
        if _fk.ref_columns != by_full[_fk.ref_table].pk and _fk.ref_columns not in alt_keys[_fk.ref_table]:
            alt_keys[_fk.ref_table].append(_fk.ref_columns)

SCHEMA_TABLE_RE = re.compile(r'\b(' + '|'.join(SCHEMA_NS) + r')\.(\w+)\b')
NEW_NAME = {t.full: t.table for t in tables}


def flat_sql(s: str) -> str:
    """Bỏ tiền tố schema trong SQL thô: iam.users -> users, core.set_updated_at -> set_updated_at."""
    s = s.replace('core.set_updated_at', 'set_updated_at')
    s = s.replace('mdm.next_document_number', 'next_document_number')
    return SCHEMA_TABLE_RE.sub(lambda m: NEW_NAME.get(m.group(0), m.group(0)), s)


# ---------------------------------------------------------------- helpers
def cs_str(s: str) -> str:
    return '"' + s.replace('\\', '\\\\').replace('"', '\\"').replace('\n', '\\n') + '"'


def xml(s: str) -> str:
    return s.replace('&', '&amp;').replace('<', '&lt;').replace('>', '&gt;')


def write(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(content)


def clr_type(c):
    t = c.sql_type
    if t == 'uuid':
        base = 'Guid'
    elif t == 'uuid[]':
        return 'List<Guid>' + ('' if c.not_null else '?')
    elif t in ('text[]', 'citext[]'):
        return 'List<string>' + ('' if c.not_null else '?')
    elif t == 'cidr[]':
        return 'List<IPNetwork>' + ('' if c.not_null else '?')
    elif t.startswith(('varchar', 'char')) or t in ('text', 'citext', 'jsonb'):
        return 'string' + ('' if c.not_null else '?')
    elif t == 'int':
        base = 'int'
    elif t == 'smallint':
        base = 'short'
    elif t == 'bigint':
        base = 'long'
    elif t == 'boolean':
        base = 'bool'
    elif t.startswith('numeric'):
        base = 'decimal'
    elif t == 'timestamptz':
        base = 'DateTimeOffset'
    elif t == 'date':
        base = 'DateOnly'
    elif t == 'time':
        base = 'TimeOnly'
    elif t == 'inet':
        return 'IPAddress' + ('' if c.not_null else '?')
    elif t.startswith('vector'):
        return 'float[]' + ('' if c.not_null else '?')
    else:
        raise ValueError(f'unmapped type {t} ({c.name})')
    nullable = (not c.not_null) or c.generated is not None
    return base + ('?' if nullable else '')


def array_literal(default: str):
    m = re.fullmatch(r"'\{(.*)\}'", default)
    if not m:
        return None
    inner = m.group(1).strip()
    if not inner:
        return '[]'
    items = [x.strip().strip('"') for x in inner.split(',')]
    return '[' + ', '.join(cs_str(i) for i in items) + ']'


def initializer(c, clr: str):
    """C# initializer mirroring the DB default (so EF always sends an explicit value)."""
    d = c.default
    t = c.sql_type
    if c.generated:
        return None
    if d is None:
        if c.not_null:
            if clr == 'string':
                return 'string.Empty'
            if clr.startswith('List<'):
                return '[]'
        return None
    if d.upper() == 'NULL':
        return None
    if d == 'gen_random_uuid()':
        return 'Guid.CreateVersion7()'
    if d == 'now()':
        return 'DateTimeOffset.UtcNow'
    if d == 'current_date':
        return 'DateOnly.FromDateTime(DateTime.UtcNow)'
    if d.startswith('encode(gen_random_bytes(16)'):
        return 'Convert.ToHexStringLower(System.Security.Cryptography.RandomNumberGenerator.GetBytes(16))'
    if t in ('boolean',):
        return d.lower()
    if t in ('int', 'smallint', 'bigint'):
        assert re.fullmatch(r'-?\d+', d), d
        return d
    if t.startswith('numeric'):
        assert re.fullmatch(r'-?\d+(\.\d+)?', d), d
        return d + 'm'
    if t.endswith('[]'):
        lit = array_literal(d)
        assert lit is not None, d
        return lit
    if t == 'jsonb':
        m = re.fullmatch(r"'(.*)'(::jsonb)?", d)
        assert m, d
        return cs_str(m.group(1).replace("''", "'"))
    m = re.fullmatch(r"'(.*)'", d)
    if m:
        return cs_str(m.group(1).replace("''", "'"))
    raise ValueError(f'cannot derive initializer for {c.name} default {d}')


def base_class(t):
    return 'BaseEntity', set(BASE_COLS)


def ns(schema):
    return f'CorePlatformApi.Domain.Entities.{SCHEMA_NS[schema]}'


# ---------------------------------------------------------------- navigations
class Nav:
    def __init__(self, table, fk, dep_name, principal, inverse):
        self.table, self.fk, self.dep_name, self.principal, self.inverse = table, fk, dep_name, principal, inverse


props_of = {}
for t in tables:
    props_of[t.full] = {cprop(t, c.name) for c in t.columns} | BASE_PROPS

navs = defaultdict(list)          # dependent table -> [Nav]
inverses = defaultdict(list)      # principal table -> [Nav]
for t in tables:
    used = set(props_of[t.full]) | {cls[t.full]}
    fk_count = defaultdict(int)
    for fk in t.fks:
        fk_count[fk.ref_table] += 1
    for fk in t.fks:
        col = fk.columns[0]
        base = re.sub(r'_(id|code|key)$', '', col)
        name = prop_name(base)
        if name in used:
            pcls_name = cls[fk.ref_table]
            name = pcls_name if fk_count[fk.ref_table] == 1 and pcls_name not in used else name + 'Navigation'
        used.add(name)
        principal = by_full[fk.ref_table]
        inverse = None
        self_ref = fk.ref_table == t.full
        if self_ref and col == 'parent_id':
            inverse = 'Children'
        elif not self_ref and fk.on_delete == 'CASCADE' and fk_count[fk.ref_table] == 1:
            inverse = plural(cls[t.full])
        n = Nav(t, fk, name, principal, inverse)
        navs[t.full].append(n)
        if inverse:
            inverses[principal.full].append(n)

# resolve inverse-name collisions
for pfull, lst in inverses.items():
    used = set(props_of[pfull]) | {n.dep_name for n in navs[pfull]}
    for n in lst:
        if n.inverse in used:
            n.inverse += 'Items'
        used.add(n.inverse)


def column_comment(c):
    return c.comment or c.name


# ---------------------------------------------------------------- entity emit
def emit_entity(t):
    cname = cls[t.full]
    base, skip = base_class(t)
    ifaces = []
    inherit = [base]
    usings = set()
    lines = []
    for c in t.columns:
        if c.name in skip or (t.full, c.name) in SHADOW_COLS:
            continue
        clr = clr_type(c)
        if 'IPAddress' in clr or 'IPNetwork' in clr:
            usings.add('System.Net')
        init = initializer(c, clr)
        doc = [f'    /// <summary>\n    /// {xml(column_comment(c))}\n    /// </summary>']
        extra = []
        if c.check:
            extra.append(f'Ràng buộc: <c>{xml(c.check)}</c>')
        if c.default and not c.generated:
            extra.append(f'Mặc định DB: <c>{xml(c.default)}</c>')
        if c.generated:
            extra.append(f'Cột tính toán (STORED): <c>{xml(c.generated)}</c>')
        if c.ref_table:
            extra.append(f'FK → <c>{c.ref_table}({c.ref_column})</c>')
        if (t.full, c.name) == (VECTOR_TABLE, VECTOR_COL):
            extra.append('Kiểu DB <c>vector(1536)</c> (pgvector)')
        if extra:
            doc.append('    /// <remarks>' + '; '.join(extra) + '</remarks>')
        setter = 'get; set;'
        line = f'    public {clr} {cprop(t, c.name)} {{ {setter} }}'
        if init:
            line += f' = {init};'
        lines.append('\n'.join(doc) + '\n' + line)
    # navigations
    nav_lines = []
    for n in navs[t.full]:
        col = t.col(n.fk.columns[0])
        pcls = cls[n.principal.full]
        if n.principal.schema != t.schema:
            usings.add(ns(n.principal.schema))
        nullable = '?' if not col.not_null else ''
        init = '' if nullable else ' = null!;'
        nav_lines.append(f'    /// <summary>\n    /// Điều hướng tới {xml(n.principal.comment.split(".")[0] if n.principal.comment else pcls)} (qua <c>{col.name}</c>).\n    /// </summary>\n'
                         f'    public {pcls}{nullable} {n.dep_name} {{ get; set; }}{init}')
    for n in inverses[t.full]:
        dcls = cls[n.table.full]
        if n.table.schema != t.schema:
            usings.add(ns(n.table.schema))
        nav_lines.append(f'    /// <summary>\n    /// Danh sách {xml(dcls)} con (qua <c>{n.table.full}.{n.fk.columns[0]}</c>).\n    /// </summary>\n'
                         f'    public ICollection<{dcls}> {n.inverse} {{ get; set; }} = [];')
    if base or ifaces:
        usings.add('CorePlatformApi.Domain.Common')
    usings.discard(ns(t.schema))
    out = HEADER
    for u in sorted(usings):
        out += f'using {u};\n'
    if usings:
        out += '\n'
    out += f'namespace {ns(t.schema)};\n\n'
    summary = t.comment or t.full
    out += f'/// <summary>\n/// {xml(summary)}\n/// </summary>\n/// <remarks>Bảng <c>{t.table}</c> (phân hệ {t.schema})'
    if t.soft_delete:
        out += '; Remove() = xóa mềm'
    out += '.</remarks>\n'
    if t.soft_delete:
        out += '[SoftDelete]\n'
    out += f'public class {cname}' + (' : ' + ', '.join(inherit) if inherit else '') + '\n{\n'
    out += '\n\n'.join(lines)
    if nav_lines:
        out += '\n\n    #region Navigations\n\n' + '\n\n'.join(nav_lines) + '\n\n    #endregion'
    out += '\n}\n'
    write(os.path.join(DOMAIN, 'Entities', SCHEMA_NS[t.schema], f'{cname}.cs'), out)


# ---------------------------------------------------------------- configuration emit
def is_simple_index(t, ix):
    cols = {c.name for c in t.columns}
    for it in ix.items:
        parts = it.split()
        if parts[0] not in cols:
            return False
        for p in parts[1:]:
            if p.upper() not in ('DESC', 'ASC') and not p.endswith('_ops'):
                return False
    return True


def member(t, col):
    if (t.full, col) in SHADOW_COLS:
        return None
    return cprop(t, col)


def key_expr(t, cols):
    names = [member(t, c) for c in cols]
    if any(n is None for n in names):
        return 'new[] { ' + ', '.join(cs_str(SHADOW_COLS[(t.full, c)][0] if member(t, c) is None else cprop(t, c)) for c in cols) + ' }', True
    if len(names) == 1:
        return f'x => x.{names[0]}', False
    return 'x => new { ' + ', '.join(f'x.{n}' for n in names) + ' }', False


raw_indexes = []   # expression indexes executed via raw SQL


def emit_config(t):
    cname = cls[t.full]
    base, _ = base_class(t)
    L = []
    a = L.append
    usings = {'Microsoft.EntityFrameworkCore', 'Microsoft.EntityFrameworkCore.Metadata.Builders', ns(t.schema)}
    # table
    tb = []
    if t.comment:
        tb.append(f'tb.HasComment({cs_str(t.comment)});')
    for c in t.columns:
        if c.check:
            tb.append(f'tb.HasCheckConstraint({cs_str(f"{t.table}_{c.name}_check")}, {cs_str(c.check)});')
    for i, (name, expr) in enumerate(t.checks):
        tb.append(f'tb.HasCheckConstraint({cs_str(name or f"{t.table}_check{i or ""}")}, {cs_str(expr)});')
    a(f'        builder.ToTable({cs_str(t.table)}, tb =>')
    a('        {')
    for x in tb:
        a('            ' + x)
    a('        });')
    a('')
    kexpr, shadow = key_expr(t, t.pk)
    a(f'        builder.HasKey({kexpr}).HasName({cs_str(t.table + "_pkey")});')
    a('')
    # columns
    for order, c in enumerate(t.columns):
        shadow_info = SHADOW_COLS.get((t.full, c.name))
        if shadow_info:
            usings.add('NpgsqlTypes')
            chain = [f'builder.Property<{shadow_info[1]}>({cs_str(shadow_info[0])})']
        else:
            chain = [f'builder.Property(x => x.{cprop(t, c.name)})']
        chain.append(f'.HasColumnName({cs_str(c.name)})')
        chain.append(f'.HasColumnType({cs_str(c.sql_type)})')
        chain.append(f'.HasColumnOrder({order})')
        if (t.full, c.name) == (VECTOR_TABLE, VECTOR_COL):
            usings.add('Pgvector')
            usings.add('Microsoft.EntityFrameworkCore.ChangeTracking')
            chain.append('.HasConversion(\n'
                         '                v => v == null ? null : new Vector(v),\n'
                         '                v => v == null ? null : v.ToArray(),\n'
                         '                new ValueComparer<float[]?>(\n'
                         '                    (a, b) => a == null ? b == null : b != null && a.SequenceEqual(b),\n'
                         '                    v => v == null ? 0 : v.Aggregate(0, (h, x) => HashCode.Combine(h, x)),\n'
                         '                    v => v == null ? null : v.ToArray()))')
        if c.not_null and not c.generated:
            chain.append('.IsRequired()')
        if c.generated:
            chain.append(f'.HasComputedColumnSql({cs_str(c.generated)}, stored: true)')
        elif c.default is not None and c.default.upper() != 'NULL':
            chain.append(f'.HasDefaultValueSql({cs_str(c.default)})')
            clr = clr_type(c)
            if initializer(c, clr) is not None:
                chain.append('.ValueGeneratedNever()')
        elif c.default is not None:
            chain.append('.HasDefaultValueSql("NULL")')
        if c.comment:
            chain.append(f'.HasComment({cs_str(c.comment)})')
        a('        ' + chain[0])
        for ch in chain[1:]:
            a('            ' + ch)
        L[-1] += ';'
    a('')
    # unique constraints -> unique indexes; khóa được FK trỏ tới -> alternate key (UNIQUE constraint)
    def emit_unique(cols, name):
        kexpr, _ = key_expr(t, cols)
        if cols in alt_keys[t.full]:
            a(f'        builder.HasAlternateKey({kexpr}).HasName({cs_str(name)});')
        else:
            a(f'        builder.HasIndex({kexpr}, {cs_str(name)}).IsUnique();')

    for c in t.columns:
        if c.unique and not c.pk:
            emit_unique([c.name], f"{t.table}_{c.name}_key")
    for name, cols in t.uniques:
        emit_unique(cols, name or (t.table + "_" + "_".join(cols) + "_key"))
    declared = [[c.name] for c in t.columns if c.unique and not c.pk] + [cols for _, cols in t.uniques]
    for cols in alt_keys[t.full]:
        assert cols in declared, (t.full, cols)
    # indexes
    if not t.partition_by:
        for ix in t.indexes:
            if (t.full, ix.name) == (VECTOR_TABLE, 'ix_ai_kb_chunks_embedding'):
                continue   # tạo trong Sql/060_pgvector.sql
            if not is_simple_index(t, ix):
                raw_indexes.append(ix.raw)
                continue
            cols = [it.split()[0] for it in ix.items]
            kexpr, _ = key_expr(t, cols)
            chain = [f'builder.HasIndex({kexpr}, {cs_str(ix.name)})']
            if ix.unique:
                chain.append('.IsUnique()')
            if ix.method:
                chain.append(f'.HasMethod({cs_str(ix.method)})')
            ops = [next((p for p in it.split()[1:] if p.endswith('_ops')), None) for it in ix.items]
            if any(ops):
                chain.append('.HasOperators(' + ', '.join(cs_str(o) if o else 'null!' for o in ops) + ')')
            desc = [any(p.upper() == 'DESC' for p in it.split()[1:]) for it in ix.items]
            if any(desc):
                chain.append('.IsDescending(' + ', '.join('true' if d else 'false' for d in desc) + ')')
            if ix.nulls_not_distinct:
                chain.append('.AreNullsDistinct(false)')
            if ix.where:
                chain.append(f'.HasFilter({cs_str(ix.where)})')
            a('        ' + chain[0])
            for ch in chain[1:]:
                a('            ' + ch)
            L[-1] += ';'
    # relationships
    rel = []
    for n in navs[t.full]:
        col = n.fk.columns[0]
        pcls = cls[n.principal.full]
        usings.add(ns(n.principal.schema))
        inv = f'.WithMany(p => p.{n.inverse})' if n.inverse else '.WithMany()'
        fkname = n.fk.name or f'{t.table}_{col}_fkey'
        behavior = 'Cascade' if n.fk.on_delete == 'CASCADE' else 'NoAction'
        rel.append(f'        builder.HasOne(x => x.{n.dep_name})\n'
                   f'            {inv}\n'
                   f'            .HasForeignKey(x => x.{cprop(t, col)})\n'
                   + (f'            .HasPrincipalKey(p => p.{cprop(n.principal, n.fk.ref_columns[0])})\n' if n.fk.ref_columns != n.principal.pk else '')
                   + f'            .HasConstraintName({cs_str(fkname)})\n'
                   f'            .OnDelete(DeleteBehavior.{behavior});')
    if rel:
        a('')
        L.extend(rel)
    out = HEADER
    for u in sorted(usings):
        out += f'using {u};\n'
    out += f'\nnamespace CorePlatformApi.Infrastructure.Persistence.Configurations.{SCHEMA_NS[t.schema]};\n\n'
    out += f'/// <summary>\n/// Cấu hình EF Core cho bảng <c>{t.table}</c>.\n/// </summary>\n'
    out += f'public sealed class {cname}Configuration : IEntityTypeConfiguration<{cname}>\n{{\n'
    out += f'    public void Configure(EntityTypeBuilder<{cname}> builder)\n    {{\n'
    out += '\n'.join(L) + '\n    }\n}\n'
    write(os.path.join(INFRA, 'Persistence', 'Configurations', SCHEMA_NS[t.schema], f'{cname}Configuration.cs'), out)


# ---------------------------------------------------------------- DbContext / interface
def dbset_name(t):
    return plural(cls[t.full])


def emit_context():
    groups = defaultdict(list)
    for t in tables:
        groups[t.schema].append(t)
    usings = sorted({ns(s) for s in groups})
    iface = HEADER + ''.join(f'using {u};\n' for u in usings) + 'using Microsoft.EntityFrameworkCore;\n\n'
    iface += 'namespace CorePlatformApi.Application.Common.Interfaces;\n\n'
    iface += '/// <summary>\n/// Truy cập dữ liệu nền tảng lõi (schema theo docs/base_erp_core.sql).\n/// </summary>\npublic interface IApplicationDbContext\n{\n'
    ctx = HEADER + ''.join(f'using {u};\n' for u in usings) + 'using Microsoft.EntityFrameworkCore;\n\n'
    ctx += 'namespace CorePlatformApi.Infrastructure.Persistence;\n\npublic partial class ApplicationDbContext\n{\n'
    for s in SCHEMA_NS:
        if s not in groups:
            continue
        iface += f'    #region {s}\n'
        ctx += f'    #region {s}\n'
        for t in groups[s]:
            iface += f'    DbSet<{cls[t.full]}> {dbset_name(t)} {{ get; }}\n'
            ctx += f'    public DbSet<{cls[t.full]}> {dbset_name(t)} => Set<{cls[t.full]}>();\n'
        iface += '    #endregion\n\n'
        ctx += '    #endregion\n\n'
    iface += '    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);\n}\n'
    ctx = ctx.rstrip() + '\n}\n'
    write(os.path.join(APP, 'Common', 'Interfaces', 'IApplicationDbContext.cs'), iface)
    write(os.path.join(INFRA, 'Persistence', 'ApplicationDbContext.DbSets.cs'), ctx)


# ---------------------------------------------------------------- raw SQL scripts
def emit_sql():
    out_dir = os.path.join(INFRA, 'Persistence', 'Sql')
    hdr = ('-- Sinh tự động từ docs/base_erp_core.sql (đã làm phẳng về schema public) — '
           'các phần EF Core không biểu diễn được.\n')
    # 000: hàm trigger set_updated_at (bỏ schema + hàm tạo partition vì không còn bảng partition)
    fstart = src.index('CREATE OR REPLACE FUNCTION core.set_updated_at')
    fend = src.index("COMMENT ON FUNCTION core.set_updated_at")
    fend = src.index('\n', fend) + 1
    write(os.path.join(out_dir, '000_core_functions.sql'), hdr + flat_sql(src[fstart:fend]))
    stale = os.path.join(out_dir, '010_partitioned_tables.sql')
    if os.path.exists(stale):
        os.remove(stale)
    # 020: expression indexes
    write(os.path.join(out_dir, '020_expression_indexes.sql'), hdr + flat_sql('\n'.join(raw_indexes)) + '\n')
    # 030: updated_at triggers (idempotent) cho mọi bảng trong public
    dstart = src.index('DO $$\nDECLARE r record;')
    dend = src.index('END $$;', dstart) + len('END $$;')
    block = src[dstart:dend].replace("EXECUTE format('CREATE TRIGGER", "EXECUTE format('CREATE OR REPLACE TRIGGER")
    block, n = re.subn(r"c\.table_schema IN \([^)]*\)", "c.table_schema = 'public'", block)
    assert n == 1, 'trigger block schema filter'
    write(os.path.join(out_dir, '030_updated_at_triggers.sql'), hdr + flat_sql(block) + '\n')
    # 040: document numbering function
    nstart = src.index('CREATE OR REPLACE FUNCTION mdm.next_document_number(')
    nend = src.index('COMMENT ON FUNCTION mdm.next_document_number')
    nend = src.index('\n', nend) + 1
    write(os.path.join(out_dir, '040_document_number_function.sql'), hdr + flat_sql(src[nstart:nend]))
    # 050: seed
    sstart = src.index('INSERT INTO mdm.currencies')
    write(os.path.join(out_dir, '050_seed_data.sql'), hdr + flat_sql(src[sstart:].rstrip()) + '\n')
    # 060: pgvector (conditional)
    vt = by_full[VECTOR_TABLE]
    vc = vt.col(VECTOR_COL)
    vix = next(ix for ix in vt.indexes if ix.name == 'ix_ai_kb_chunks_embedding')
    vtable = vt.table
    s060 = hdr + f"""-- pgvector là extension ngoài (không có sẵn trong PostgreSQL). Script idempotent:
--   * Nếu server đã cài pgvector  -> tạo extension, cột {vtable}.{VECTOR_COL} và index HNSW.
--   * Nếu chưa cài                -> bỏ qua kèm NOTICE; ứng dụng chạy lại script này mỗi lần khởi động.
DO $pgv$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_available_extensions WHERE name = 'vector') THEN
        RAISE NOTICE 'pgvector chưa được cài trên server — bỏ qua cột {vtable}.{VECTOR_COL}';
        RETURN;
    END IF;

    CREATE EXTENSION IF NOT EXISTS vector;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'public' AND table_name = '{vtable}' AND column_name = '{VECTOR_COL}'
    ) THEN
        EXECUTE 'ALTER TABLE {vtable} ADD COLUMN {VECTOR_COL} {vc.sql_type}';
        EXECUTE $c$COMMENT ON COLUMN {vtable}.{VECTOR_COL} IS '{vc.comment.replace("'", "''")}'$c$;
    END IF;

    EXECUTE '{flat_sql(vix.raw.rstrip(';')).replace('CREATE INDEX', 'CREATE INDEX IF NOT EXISTS')}';
END $pgv$;
"""
    write(os.path.join(out_dir, '060_pgvector.sql'), s060)


# ---------------------------------------------------------------- run
for t in tables:
    emit_entity(t)
    emit_config(t)
emit_context()
emit_sql()

print('entities:', len(tables))
print('raw indexes:', len(raw_indexes))
for t in tables:
    print(f'{t.full:40} -> {t.table:34} {cls[t.full]:32} soft={t.soft_delete} alt={alt_keys[t.full]}')
