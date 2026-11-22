"""Chuẩn hóa format XML doc comment (///) trong các file C#.

Sửa các lỗi:
  * Dòng /// bị lệch thụt lề (ví dụ `<summary>` thụt 4 nhưng nội dung và `</summary>` ở cột 0)
    -> cả khối /// được thụt theo dòng code ngay bên dưới nó (attribute / khai báo).
  * Dòng /// trống thừa ngay sau `<summary>` hoặc ngay trước `</summary>`.
  * Khoảng trắng thừa cuối dòng, thiếu dấu cách sau `///` (`///abc` -> `/// abc`).

Giữ nguyên BOM, kiểu xuống dòng (CRLF/LF) và mọi dòng không phải doc comment.

Dùng:
  python fix_doc_comments.py                 # sửa trong CorePlatformApi/src
  python fix_doc_comments.py path1 path2     # file hoặc thư mục tùy chọn
  python fix_doc_comments.py --check         # chỉ liệt kê file cần sửa, exit 1 nếu có
"""
from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

SKIP_DIRS = {'bin', 'obj', '.git', '.vs', 'node_modules', '_legacy_hrm'}
DOC_RE = re.compile(r'^[ \t]*///')
INDENT_RE = re.compile(r'^[ \t]*')


def is_doc(line: str) -> bool:
    return bool(DOC_RE.match(line))


def doc_body(line: str) -> str:
    """Phần sau `///`, đã bỏ khoảng trắng cuối và chuẩn hóa đúng 1 dấu cách đầu."""
    body = line.lstrip(' \t')[3:].rstrip()
    if not body:
        return ''
    if body[0] not in ' \t':
        return ' ' + body
    # Giữ thụt lề tương đối bên trong (vd <code>), chỉ bỏ dấu cách thừa nếu toàn khối thụt đều
    return body


def normalize_block(block: list[str], indent: str) -> list[str]:
    bodies = [doc_body(l) for l in block]

    # Nếu mọi dòng có nội dung đều bắt đầu bằng >1 dấu cách -> rút về 1 dấu cách
    non_empty = [b for b in bodies if b]
    if non_empty:
        common = min(len(b) - len(b.lstrip(' ')) for b in non_empty)
        if common > 1:
            bodies = [b[common - 1:] if b else b for b in bodies]

    # Bỏ dòng /// trống ngay sau <summary> / trước </summary> (và các thẻ khối tương tự)
    result: list[str] = []
    for b in bodies:
        if not b and result and re.fullmatch(r' <(summary|remarks|returns|example)>', result[-1]):
            continue
        result.append(b)
    cleaned: list[str] = []
    for b in result:
        if re.fullmatch(r' </(summary|remarks|returns|example)>', b):
            while cleaned and not cleaned[-1]:
                cleaned.pop()
        cleaned.append(b)
    # Bỏ dòng /// trống ở đầu/cuối cả khối
    while cleaned and not cleaned[0]:
        cleaned.pop(0)
    while cleaned and not cleaned[-1]:
        cleaned.pop()

    return [f'{indent}///{b}' for b in cleaned]


def fix_text(text: str) -> str:
    lines = text.split('\n')
    # Tách \r để xử lý, gắn lại sau
    has_cr = [l.endswith('\r') for l in lines]
    lines = [l[:-1] if cr else l for l, cr in zip(lines, has_cr)]
    crlf = sum(has_cr) > len(lines) // 2

    out: list[str] = []
    i = 0
    n = len(lines)
    while i < n:
        if not is_doc(lines[i]):
            out.append(lines[i])
            i += 1
            continue
        j = i
        while j < n and is_doc(lines[j]):
            j += 1
        block = lines[i:j]
        # Thụt lề theo dòng code kế tiếp (bỏ qua dòng trống); không có thì theo dòng /// đầu
        k = j
        while k < n and not lines[k].strip():
            k += 1
        anchor = lines[k] if k < n and k == j else block[0]
        indent = INDENT_RE.match(anchor).group(0)
        out.extend(normalize_block(block, indent))
        i = j

    nl = '\r\n' if crlf else '\n'
    return nl.join(out)


def iter_files(paths: list[Path]):
    for p in paths:
        if p.is_file() and p.suffix == '.cs':
            yield p
        elif p.is_dir():
            for f in sorted(p.rglob('*.cs')):
                if not SKIP_DIRS.intersection(f.relative_to(p).parts):
                    yield f


def main() -> int:
    default_root = Path(__file__).resolve().parent.parent / 'src'
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('paths', nargs='*', type=Path, default=[default_root])
    ap.add_argument('--check', action='store_true', help='không ghi file, chỉ báo file cần sửa')
    args = ap.parse_args()

    changed = 0
    total = 0
    for f in iter_files(args.paths):
        total += 1
        raw = f.read_bytes()
        bom = raw.startswith(b'\xef\xbb\xbf')
        text = raw[3:].decode('utf-8') if bom else raw.decode('utf-8')
        fixed = fix_text(text)
        if fixed == text:
            continue
        changed += 1
        print(('cần sửa: ' if args.check else 'đã sửa: ') + str(f))
        if not args.check:
            f.write_bytes((b'\xef\xbb\xbf' if bom else b'') + fixed.encode('utf-8'))

    print(f'{changed}/{total} file {"cần sửa" if args.check else "đã sửa"}.')
    return 1 if args.check and changed else 0


if __name__ == '__main__':
    sys.exit(main())
