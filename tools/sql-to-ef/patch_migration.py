"""Splice raw-SQL steps into the generated InitialCoreSchema migration."""
import re
import sys

path = sys.argv[1]
src = open(path, encoding='utf-8').read()

# 1) pgvector column is added conditionally by Sql/060_pgvector.sql
src, n = re.subn(r'\n\s*embedding = table\.Column<Vector>\([^\n]*\n', '\n', src)
assert n == 1, n
src = src.replace('using Pgvector;\n', '')

# 2) using
src = src.replace('using Microsoft.EntityFrameworkCore.Migrations;\n',
                  'using CorePlatformApi.Infrastructure.Persistence.Sql;\nusing Microsoft.EntityFrameworkCore.Migrations;\n', 1)

# 3) core functions right after extensions are created
anchor = '.Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");\n'
assert src.count(anchor) == 1
src = src.replace(anchor, anchor + '''
            // Schema core + hàm tiện ích (set_updated_at, create_monthly_partitions)
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.CoreFunctions));
''', 1)

# 4) tail of Up
down_sig = '        /// <inheritdoc />\n        protected override void Down(MigrationBuilder migrationBuilder)\n        {\n'
assert src.count(down_sig) == 1
up_tail = '''
            // Phần EF Core không biểu diễn được — giữ nguyên từ docs/base_erp_core.sql
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.PartitionedTables));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.ExpressionIndexes));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.UpdatedAtTriggers));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.DocumentNumberFunction));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.SeedData));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.Pgvector));
        }

'''
idx = src.index(down_sig)
before = src[:idx].rstrip()
assert before.endswith('}'), before[-50:]
before = before[:-1].rstrip() + '\n' + up_tail
down_head = '''
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS mdm.next_document_number(uuid, varchar, uuid, uuid, date);
                DROP TABLE IF EXISTS audit.login_logs, audit.security_events, audit.data_change_logs,
                    audit.api_request_logs, monitor.health_checks, ai.usage_logs, storage.file_access_logs CASCADE;
                """);

'''
src = before + down_sig + down_head + src[idx + len(down_sig):]

# 5) tail of Down
end_marker = '        }\n    }\n}'
assert src.rstrip().endswith(end_marker.rstrip()), src[-80:]
body = src.rstrip()[:-len(end_marker)].rstrip()
src = body + '''

            migrationBuilder.Sql("DROP SCHEMA IF EXISTS core CASCADE;");
''' + end_marker + '\n'

open(path, 'w', encoding='utf-8', newline='\n').write(src)
print('patched', path)
