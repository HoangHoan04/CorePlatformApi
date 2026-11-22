"""Parse base_erp_core.sql into a table model."""
import re
from dataclasses import dataclass, field

KEYWORDS_AFTER_DEFAULT = ('NOT NULL', 'NULL', 'REFERENCES', 'CHECK', 'UNIQUE', 'PRIMARY KEY', 'GENERATED', 'CONSTRAINT')


@dataclass
class Column:
    name: str
    sql_type: str
    not_null: bool = False
    pk: bool = False
    unique: bool = False
    default: str | None = None
    check: str | None = None
    generated: str | None = None
    ref_table: str | None = None   # schema.table
    ref_column: str | None = None
    on_delete: str | None = None
    comment: str | None = None


@dataclass
class Index:
    name: str
    table: str
    unique: bool
    method: str | None
    items: list            # raw item strings
    nulls_not_distinct: bool
    where: str | None
    raw: str


@dataclass
class ForeignKey:
    name: str | None
    columns: list
    ref_table: str
    ref_columns: list
    on_delete: str | None


@dataclass
class Table:
    schema: str
    name: str
    columns: list = field(default_factory=list)
    pk: list = field(default_factory=list)
    uniques: list = field(default_factory=list)     # (name, [cols])
    checks: list = field(default_factory=list)      # (name, expr)
    fks: list = field(default_factory=list)
    indexes: list = field(default_factory=list)
    partition_by: str | None = None
    comment: str | None = None
    raw: str = ''
    leading_comment: str | None = None

    @property
    def full(self):
        return f'{self.schema}.{self.name}'

    def col(self, name):
        for c in self.columns:
            if c.name == name:
                return c
        raise KeyError(f'{self.full}.{name}')


def strip_line_comment(line: str) -> str:
    """Remove trailing -- comment, respecting single-quoted strings."""
    out = []
    in_str = False
    i = 0
    while i < len(line):
        ch = line[i]
        if ch == "'":
            in_str = not in_str
        if not in_str and line.startswith('--', i):
            break
        out.append(ch)
        i += 1
    return ''.join(out).rstrip()


def split_top_level(s: str, sep=','):
    parts, depth, in_str, cur = [], 0, False, []
    for ch in s:
        if ch == "'":
            in_str = not in_str
        if not in_str:
            if ch == '(':
                depth += 1
            elif ch == ')':
                depth -= 1
            elif ch == sep and depth == 0:
                parts.append(''.join(cur).strip())
                cur = []
                continue
        cur.append(ch)
    if ''.join(cur).strip():
        parts.append(''.join(cur).strip())
    return parts


def read_paren(s: str, start: int):
    """s[start] == '(' ; return (inner, end_index_after_close)."""
    assert s[start] == '(', s[start:start + 30]
    depth, in_str = 0, False
    for i in range(start, len(s)):
        ch = s[i]
        if ch == "'":
            in_str = not in_str
        if in_str:
            continue
        if ch == '(':
            depth += 1
        elif ch == ')':
            depth -= 1
            if depth == 0:
                return s[start + 1:i], i + 1
    raise ValueError('unbalanced: ' + s[start:start + 80])


TYPE_RE = re.compile(r'(double precision|timestamp with time zone|[a-z_0-9]+)(\s*\(\s*\d+(?:\s*,\s*\d+)?\s*\))?(\[\])?', re.I)


def parse_column(defn: str) -> Column:
    m = re.match(r'(\w+)\s+', defn)
    name = m.group(1)
    rest = defn[m.end():]
    tm = TYPE_RE.match(rest)
    sql_type = (tm.group(1) + (tm.group(2) or '').replace(' ', '') + (tm.group(3) or '')).lower()
    rest = rest[tm.end():].strip()
    col = Column(name=name, sql_type=sql_type)
    i = 0
    up = rest.upper()
    while i < len(rest):
        if rest[i].isspace():
            i += 1
            continue
        if up.startswith('NOT NULL', i):
            col.not_null = True
            i += 8
        elif up.startswith('NULL', i):
            i += 4
        elif up.startswith('PRIMARY KEY', i):
            col.pk = True
            col.not_null = True
            i += 11
        elif up.startswith('UNIQUE', i):
            col.unique = True
            i += 6
        elif up.startswith('DEFAULT', i):
            i += 7
            j = i
            depth, in_str = 0, False
            while j < len(rest):
                ch = rest[j]
                if ch == "'":
                    in_str = not in_str
                if not in_str:
                    if ch == '(':
                        depth += 1
                    elif ch == ')':
                        depth -= 1
                    elif depth == 0 and ch.isspace():
                        tail = up[j:].lstrip()
                        if any(tail.startswith(k) for k in KEYWORDS_AFTER_DEFAULT):
                            break
                j += 1
            col.default = rest[i:j].strip()
            i = j
        elif up.startswith('CHECK', i):
            k = rest.index('(', i)
            inner, end = read_paren(rest, k)
            col.check = inner.strip()
            i = end
        elif up.startswith('GENERATED ALWAYS AS', i):
            k = rest.index('(', i)
            inner, end = read_paren(rest, k)
            col.generated = inner.strip()
            i = end
            tail = rest[i:].lstrip()
            assert tail.upper().startswith('STORED'), defn
            i = len(rest) - len(tail) + 6
        elif up.startswith('REFERENCES', i):
            rm = re.match(r'REFERENCES\s+(\w+\.\w+)\s*\((\w+)\)(\s+ON DELETE (CASCADE|SET NULL|RESTRICT|NO ACTION))?', rest[i:], re.I)
            col.ref_table, col.ref_column = rm.group(1), rm.group(2)
            col.on_delete = rm.group(4).upper() if rm.group(4) else None
            i += rm.end()
        else:
            raise ValueError(f'Cannot parse column tail: {defn!r} at {rest[i:]!r}')
    return col


def unquote(s: str) -> str:
    return s.replace("''", "'")


def parse(path: str):
    src = open(path, encoding='utf-8').read()
    tables: dict[str, Table] = {}
    order = []

    for m in re.finditer(r'^CREATE TABLE (\w+)\.(\w+) \(', src, re.M):
        schema, name = m.group(1), m.group(2)
        start = m.end() - 1
        # find matching paren on raw text but with comments removed per line
        body_lines = []
        pos = start + 1
        depth = 1
        lines = src[pos:].split('\n')
        consumed = 0
        raw_body = []
        for line in lines:
            consumed += len(line) + 1
            clean = strip_line_comment(line)
            # track depth on clean text
            d = depth
            in_str = False
            close_at = None
            for idx, ch in enumerate(clean):
                if ch == "'":
                    in_str = not in_str
                if in_str:
                    continue
                if ch == '(':
                    d += 1
                elif ch == ')':
                    d -= 1
                    if d == 0:
                        close_at = idx
                        break
            if close_at is not None:
                body_lines.append(clean[:close_at])
                after_line = clean[close_at + 1:]
                raw_body.append(line)
                break
            depth = d
            body_lines.append(clean)
            raw_body.append(line)
        end_pos = pos + consumed
        # statement tail up to ';'
        after = after_line
        if ';' not in after:
            semi = src.index(';', end_pos)
            after += src[end_pos:semi]
            end_pos = semi + 1
        after = after.strip().rstrip(';').strip()
        t = Table(schema=schema, name=name)
        t.raw = src[m.start():end_pos].rstrip()
        if not t.raw.endswith(';'):
            t.raw += ';'
        pm = re.match(r'PARTITION BY RANGE \((\w+)\)', after)
        if pm:
            t.partition_by = pm.group(1)
        elif after:
            raise ValueError(f'Unexpected table tail {t.full}: {after}')
        body = ' '.join(l.strip() for l in body_lines)
        for item in split_top_level(body):
            up = item.upper()
            if up.startswith('CONSTRAINT'):
                cm = re.match(r'CONSTRAINT\s+(\w+)\s+(UNIQUE|CHECK|PRIMARY KEY)\s*', item, re.I)
                cname, kind = cm.group(1), cm.group(2).upper()
                inner, _ = read_paren(item, cm.end())
                if kind == 'UNIQUE':
                    t.uniques.append((cname, [c.strip() for c in inner.split(',')]))
                elif kind == 'CHECK':
                    t.checks.append((cname, inner.strip()))
                else:
                    t.pk = [c.strip() for c in inner.split(',')]
            elif re.match(r'PRIMARY KEY\s*\(', up):
                inner, _ = read_paren(item, item.index('('))
                t.pk = [c.strip() for c in inner.split(',')]
            elif re.match(r'UNIQUE\s*\(', up):
                inner, _ = read_paren(item, item.index('('))
                t.uniques.append((None, [c.strip() for c in inner.split(',')]))
            elif re.match(r'CHECK\s*\(', up):
                inner, _ = read_paren(item, item.index('('))
                t.checks.append((None, inner.strip()))
            else:
                t.columns.append(parse_column(item))
        if not t.pk:
            t.pk = [c.name for c in t.columns if c.pk]
        for c in t.columns:
            if c.name in t.pk:
                c.not_null = True
            if c.ref_table:
                t.fks.append(ForeignKey(None, [c.name], c.ref_table, [c.ref_column], c.on_delete))
        tables[t.full] = t
        order.append(t.full)

    # comments
    for m in re.finditer(r"^COMMENT ON TABLE (\w+\.\w+) IS '((?:[^']|'')*)';", src, re.M):
        if m.group(1) in tables:
            tables[m.group(1)].comment = unquote(m.group(2))
    for m in re.finditer(r"^COMMENT ON COLUMN (\w+\.\w+)\.(\w+) IS '((?:[^']|'')*)';", src, re.M):
        if m.group(1) in tables:
            tables[m.group(1)].col(m.group(2)).comment = unquote(m.group(3))

    # indexes
    for m in re.finditer(r'^CREATE (UNIQUE )?INDEX (\w+) ON (\w+\.\w+)( USING (\w+))? \((.*)$', src, re.M):
        line = m.group(0)
        stmt = strip_line_comment(line).rstrip(';')
        k = stmt.index('(', stmt.index(' ON '))
        inner, end = read_paren(stmt, k)
        tail = stmt[end:].strip()
        nnd = False
        if tail.upper().startswith('NULLS NOT DISTINCT'):
            nnd = True
            tail = tail[len('NULLS NOT DISTINCT'):].strip()
        where = None
        if tail.upper().startswith('WHERE'):
            where = tail[5:].strip()
        elif tail:
            raise ValueError('index tail ' + line)
        idx = Index(name=m.group(2), table=m.group(3), unique=bool(m.group(1)), method=m.group(5),
                    items=split_top_level(inner), nulls_not_distinct=nnd, where=where,
                    raw=strip_line_comment(line))
        tables[idx.table].indexes.append(idx)

    # extra FKs
    for m in re.finditer(r'^ALTER TABLE (\w+\.\w+)\s+ADD CONSTRAINT (\w+)\s+FOREIGN KEY \((\w+)\)\s+REFERENCES (\w+\.\w+)\((\w+)\);', src, re.M):
        tables[m.group(1)].fks.append(ForeignKey(m.group(2), [m.group(3)], m.group(4), [m.group(5)], None))

    return src, [tables[n] for n in order]
