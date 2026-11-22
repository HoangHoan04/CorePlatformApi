"""Generate Domain entities, EF Core configurations, DbContext and raw SQL scripts from base_erp_core.sql."""
import os
import re
import sys
from collections import defaultdict

sys.stdout.reconfigure(encoding='utf-8')
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sqlparse import parse, strip_line_comment  # noqa: E402
from naming import SCHEMA_NS, class_name, plural, prop_name  # noqa: E402

SQL_PATH, ROOT = sys.argv[1], sys.argv[2]   # ROOT = CorePlatformApi/src
DOMAIN = os.path.join(ROOT, 'CorePlatformApi.Domain')
INFRA = os.path.join(ROOT, 'CorePlatformApi.Infrastructure')
APP = os.path.join(ROOT, 'CorePlatformApi.Application')

src, tables = parse(SQL_PATH)
by_full = {t.full: t for t in tables}
cls = {t.full: class_name(t) for t in tables}
assert len(set(cls.values())) == len(cls), 'duplicate class names'

HEADER = '// Nguồn schema: docs/base_erp_core.sql\n'

AUDIT_COLS = ['created_at', 'created_by', 'updated_at', 'updated_by']
SOFT_COLS = ['is_deleted', 'deleted_at', 'deleted_by']
VECTOR_TABLE, VECTOR_COL = 'ai.kb_chunks', 'embedding'
SHADOW_COLS = {('ai.kb_chunks', 'content_tsv'): ('ContentTsv', 'NpgsqlTsVector')}


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


def has_cols(t, names, pred=None):
    cols = {c.name: c for c in t.columns}
    return all(n in cols for n in names)


def base_class(t):
    cols = {c.name: c for c in t.columns}
    audit = (has_cols(t, AUDIT_COLS)
             and cols['created_at'].sql_type == 'timestamptz' and cols['created_at'].not_null
             and cols['created_by'].sql_type == 'uuid' and not cols['created_by'].not_null
             and cols['updated_at'].sql_type == 'timestamptz' and not cols['updated_at'].not_null
             and cols['updated_by'].sql_type == 'uuid' and not cols['updated_by'].not_null
             and not any(c.ref_table for c in t.columns if c.name in AUDIT_COLS))
    soft = (has_cols(t, SOFT_COLS) and cols['is_deleted'].sql_type == 'boolean' and cols['is_deleted'].not_null)
    if t.partition_by:
        return None, set()
    if audit and soft:
        return 'SoftDeletableEntity', set(AUDIT_COLS + SOFT_COLS)
    if audit:
        return 'AuditableEntity', set(AUDIT_COLS)
    return None, set()


def tenant_iface(t):
    for c in t.columns:
        if c.name == 'tenant_id' and c.sql_type == 'uuid':
            return 'IMustHaveTenant' if c.not_null else 'IMayHaveTenant'
    return None


def ns(schema):
    return f'CorePlatformApi.Domain.Entities.{SCHEMA_NS[schema]}'


# ---------------------------------------------------------------- navigations
class Nav:
    def __init__(self, table, fk, dep_name, principal, inverse):
        self.table, self.fk, self.dep_name, self.principal, self.inverse = table, fk, dep_name, principal, inverse


props_of = {}
for t in tables:
    props_of[t.full] = {prop_name(c.name) for c in t.columns} | {'RowVersion'}

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
    ti = tenant_iface(t)
    if ti:
        ifaces.append(ti)
    inherit = ([base] if base else []) + ifaces
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
        line = f'    public {clr} {prop_name(c.name)} {{ {setter} }}'
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
    out += f'/// <summary>\n/// {xml(summary)}\n/// </summary>\n/// <remarks>Bảng <c>{t.full}</c>'
    if t.partition_by:
        out += f'; partition RANGE theo <c>{t.partition_by}</c> (tạo bằng SQL thô, không qua EF migration)'
    out += '.</remarks>\n'
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
    return prop_name(col)


def key_expr(t, cols):
    names = [member(t, c) for c in cols]
    if any(n is None for n in names):
        return 'new[] { ' + ', '.join(cs_str(SHADOW_COLS[(t.full, c)][0] if member(t, c) is None else prop_name(c)) for c in cols) + ' }', True
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
            tb.append(f'tb.HasCheckConstraint({cs_str(f"{t.name}_{c.name}_check")}, {cs_str(c.check)});')
    for i, (name, expr) in enumerate(t.checks):
        tb.append(f'tb.HasCheckConstraint({cs_str(name or f"{t.name}_check{i or ""}")}, {cs_str(expr)});')
    if t.partition_by:
        tb.append('tb.ExcludeFromMigrations(); // bảng partition: tạo bằng Sql/010_partitioned_tables.sql')
    a(f'        builder.ToTable({cs_str(t.name)}, {cs_str(t.schema)}, tb =>')
    a('        {')
    for x in tb:
        a('            ' + x)
    a('        });')
    a('')
    kexpr, shadow = key_expr(t, t.pk)
    a(f'        builder.HasKey({kexpr}).HasName({cs_str(t.name + "_pkey")});')
    a('')
    # columns
    for order, c in enumerate(t.columns):
        shadow_info = SHADOW_COLS.get((t.full, c.name))
        if shadow_info:
            usings.add('NpgsqlTypes')
            chain = [f'builder.Property<{shadow_info[1]}>({cs_str(shadow_info[0])})']
        else:
            chain = [f'builder.Property(x => x.{prop_name(c.name)})']
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
    # unique constraints -> unique indexes
    for c in t.columns:
        if c.unique and not c.pk:
            kexpr, _ = key_expr(t, [c.name])
            a(f'        builder.HasIndex({kexpr}, {cs_str(f"{t.name}_{c.name}_key")}).IsUnique();')
    for name, cols in t.uniques:
        kexpr, _ = key_expr(t, cols)
        a(f'        builder.HasIndex({kexpr}, {cs_str(name or (t.name + "_" + "_".join(cols) + "_key"))}).IsUnique();')
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
        fkname = n.fk.name or f'{t.name}_{col}_fkey'
        behavior = 'Cascade' if n.fk.on_delete == 'CASCADE' else 'NoAction'
        rel.append(f'        builder.HasOne(x => x.{n.dep_name})\n'
                   f'            {inv}\n'
                   f'            .HasForeignKey(x => x.{prop_name(col)})\n'
                   + (f'            .HasPrincipalKey(p => p.{prop_name(n.fk.ref_columns[0])})\n' if n.fk.ref_columns != n.principal.pk else '')
                   + f'            .HasConstraintName({cs_str(fkname)})\n'
                   f'            .OnDelete(DeleteBehavior.{behavior});')
    if rel:
        a('')
        L.extend(rel)
    out = HEADER
    for u in sorted(usings):
        out += f'using {u};\n'
    out += f'\nnamespace CorePlatformApi.Infrastructure.Persistence.Configurations.{SCHEMA_NS[t.schema]};\n\n'
    out += f'/// <summary>\n/// Cấu hình EF Core cho <c>{t.full}</c>.\n/// </summary>\n'
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
    hdr = '-- Sinh tự động từ docs/base_erp_core.sql — các phần EF Core không biểu diễn được.\n'
    # 000: schemas comments + core functions
    m_schema_comments = re.findall(r'^COMMENT ON SCHEMA .*;$', src, re.M)
    fstart = src.index('CREATE OR REPLACE FUNCTION core.set_updated_at')
    fend = src.index("COMMENT ON FUNCTION core.create_monthly_partitions")
    fend = src.index('\n', fend) + 1
    core_fn = src[fstart:fend]
    s000 = hdr + 'CREATE SCHEMA IF NOT EXISTS core;\n' + \
        ''.join(f'CREATE SCHEMA IF NOT EXISTS {s};\n' for s in SCHEMA_NS) + '\n' + \
        '\n'.join(m_schema_comments) + '\n\n' + core_fn
    write(os.path.join(out_dir, '000_core_functions.sql'), s000)
    # 010: partitioned tables
    parts = [hdr]
    for t in tables:
        if not t.partition_by:
            continue
        parts.append(t.raw)
        for line in src.split('\n'):
            if line.startswith((f'COMMENT ON TABLE {t.full} ', f'COMMENT ON COLUMN {t.full}.')):
                parts.append(line)
        for ix in t.indexes:
            parts.append(ix.raw)
        parts.append('')
    for line in src.split('\n'):
        if re.match(r'^CREATE TABLE \w+\.\w+_default\s+PARTITION OF', line) or line.startswith('SELECT core.create_monthly_partitions'):
            parts.append(line)
    write(os.path.join(out_dir, '010_partitioned_tables.sql'), '\n'.join(parts) + '\n')
    # 020: expression indexes
    write(os.path.join(out_dir, '020_expression_indexes.sql'), hdr + '\n'.join(raw_indexes) + '\n')
    # 030: updated_at triggers (idempotent)
    dstart = src.index('DO $$\nDECLARE r record;')
    dend = src.index('END $$;', dstart) + len('END $$;')
    block = src[dstart:dend].replace("EXECUTE format('CREATE TRIGGER", "EXECUTE format('CREATE OR REPLACE TRIGGER")
    write(os.path.join(out_dir, '030_updated_at_triggers.sql'), hdr + block + '\n')
    # 040: document numbering function
    nstart = src.index('CREATE OR REPLACE FUNCTION mdm.next_document_number(')
    nend = src.index('COMMENT ON FUNCTION mdm.next_document_number')
    nend = src.index('\n', nend) + 1
    write(os.path.join(out_dir, '040_document_number_function.sql'), hdr + src[nstart:nend])
    # 050: seed
    sstart = src.index('INSERT INTO mdm.currencies')
    write(os.path.join(out_dir, '050_seed_data.sql'), hdr + src[sstart:].rstrip() + '\n')
    # 060: pgvector (conditional)
    vt = by_full[VECTOR_TABLE]
    vc = vt.col(VECTOR_COL)
    vix = next(ix for ix in vt.indexes if ix.name == 'ix_ai_kb_chunks_embedding')
    s060 = hdr + f"""-- pgvector là extension ngoài (không có sẵn trong PostgreSQL). Script idempotent:
--   * Nếu server đã cài pgvector  -> tạo extension, cột {VECTOR_TABLE}.{VECTOR_COL} và index HNSW.
--   * Nếu chưa cài                -> bỏ qua kèm NOTICE; ứng dụng chạy lại script này mỗi lần khởi động.
DO $pgv$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_available_extensions WHERE name = 'vector') THEN
        RAISE NOTICE 'pgvector chưa được cài trên server — bỏ qua cột {VECTOR_TABLE}.{VECTOR_COL}';
        RETURN;
    END IF;

    CREATE EXTENSION IF NOT EXISTS vector;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = '{vt.schema}' AND table_name = '{vt.name}' AND column_name = '{VECTOR_COL}'
    ) THEN
        EXECUTE 'ALTER TABLE {VECTOR_TABLE} ADD COLUMN {VECTOR_COL} {vc.sql_type}';
        EXECUTE $c$COMMENT ON COLUMN {VECTOR_TABLE}.{VECTOR_COL} IS '{vc.comment.replace("'", "''")}'$c$;
    END IF;

    EXECUTE '{vix.raw.rstrip(';').replace('CREATE INDEX', 'CREATE INDEX IF NOT EXISTS')}';
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
    b, _ = base_class(t)
    print(f'{t.full:40} {cls[t.full]:32} base={b} tenant={tenant_iface(t)} navs={[n.dep_name for n in navs[t.full]]} inv={[n.inverse for n in inverses[t.full]]}')
