using System;
using System.Collections.Generic;
using System.Net;
using CorePlatformApi.Infrastructure.Persistence.Sql;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CorePlatformApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            // Hàm trigger set_updated_at()
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.CoreFunctions));

            migrationBuilder.CreateTable(
                name: "ai_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mã (OPENAI, GEMINI, ANTHROPIC, AZURE_OPENAI, OLLAMA, VLLM)"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên"),
                    api_style = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Chuẩn API để chọn adapter"),
                    base_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Base URL mặc định"),
                    is_self_hosted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Model nội bộ (dữ liệu không rời hạ tầng)"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ai_providers_pkey", x => x.id);
                    table.CheckConstraint("ai_providers_api_style_check", "api_style IN ('OPENAI','ANTHROPIC','GEMINI','CUSTOM')");
                },
                comment: "Nhà cung cấp AI (OpenAI, Gemini, Anthropic, Azure OpenAI, model nội bộ qua Ollama/vLLM)");

            migrationBuilder.CreateTable(
                name: "api_request_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    trace_id = table.Column<string>(type: "varchar(64)", nullable: false, comment: "Trace id (trả về client qua header X-Trace-Id)"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "User"),
                    client_id = table.Column<string>(type: "varchar(100)", nullable: true, comment: "OAuth client"),
                    api_key_prefix = table.Column<string>(type: "varchar(16)", nullable: true, comment: "API key (tiền tố)"),
                    route_code = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Route khớp"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ"),
                    http_method = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Method"),
                    path = table.Column<string>(type: "varchar(2000)", nullable: false, comment: "Path"),
                    query_string = table.Column<string>(type: "text", nullable: true, comment: "Query (đã che tham số nhạy cảm)"),
                    status_code = table.Column<short>(type: "smallint", nullable: false, comment: "HTTP status"),
                    error_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Mã lỗi chuẩn (gateway.error_codes)"),
                    duration_ms = table.Column<int>(type: "int", nullable: false, comment: "Thời gian xử lý"),
                    upstream_ms = table.Column<int>(type: "int", nullable: true, comment: "Thời gian upstream"),
                    request_bytes = table.Column<int>(type: "int", nullable: true, comment: "Kích thước request"),
                    response_bytes = table.Column<int>(type: "int", nullable: true, comment: "Kích thước response"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP client"),
                    user_agent = table.Column<string>(type: "text", nullable: true, comment: "User-Agent"),
                    is_rate_limited = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Bị chặn rate limit"),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("api_request_logs_pkey", x => x.id);
                },
                comment: "Log request đi qua API Gateway (thường đẩy song song sang Loki/ELK; bảng này giữ ngắn hạn để tra cứu nhanh theo traceId)");

            migrationBuilder.CreateTable(
                name: "audit_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Phân hệ"),
                    entity_type = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Loại đối tượng (EMPLOYEE, SALARY, STOCK_ADJUSTMENT)"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bật lưu vết"),
                    track_actions = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{INSERT,UPDATE,DELETE}'", comment: "Hành động cần ghi"),
                    include_fields = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Chỉ ghi các trường này (NULL = tất cả)"),
                    exclude_fields = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{updated_at,updated_by}'", comment: "Bỏ qua trường này"),
                    masked_fields = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Trường được ghi nhưng che giá trị (***), VD: base_salary, id_number"),
                    retention_days = table.Column<int>(type: "int", nullable: false, defaultValueSql: "3650", comment: "Thời gian lưu (ngày) — chứng từ kế toán tối thiểu 10 năm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("audit_policies_pkey", x => x.id);
                },
                comment: "Cấu hình đối tượng cần lưu vết thay đổi và trường cần che (lương, CCCD...)");

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "char(2)", nullable: false, comment: "Mã ISO 3166-1 alpha-2 (VN, US, JP...)"),
                    alpha3_code = table.Column<string>(type: "char(3)", nullable: false, comment: "Mã ISO alpha-3 (VNM, USA...)"),
                    numeric_code = table.Column<string>(type: "char(3)", nullable: true, comment: "Mã số ISO (704 = Việt Nam)"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên tiếng Việt"),
                    name_en = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên tiếng Anh"),
                    phone_code = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Mã vùng điện thoại quốc tế (+84)"),
                    currency_code = table.Column<string>(type: "char(3)", nullable: true, comment: "Tiền tệ mặc định của quốc gia (VND)"),
                    flag_emoji = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Cờ dạng emoji để hiển thị nhanh"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn sử dụng hay không"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("countries_pkey", x => x.id);
                    table.UniqueConstraint("countries_code_key", x => x.code);
                },
                comment: "Danh mục quốc gia theo chuẩn ISO 3166");

            migrationBuilder.CreateTable(
                name: "currencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "char(3)", nullable: false, comment: "Mã ISO 4217 (VND, USD, EUR...)"),
                    numeric_code = table.Column<string>(type: "char(3)", nullable: true, comment: "Mã số ISO (704 = VND)"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên tiền tệ (Đồng Việt Nam)"),
                    symbol = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Ký hiệu hiển thị (₫, $)"),
                    decimal_places = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Số chữ số thập phân khi hiển thị/làm tròn (VND = 0, USD = 2)"),
                    rounding_unit = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValueSql: "1", comment: "Đơn vị làm tròn tối thiểu (VD: 1, 100, 1000 VND)"),
                    symbol_position = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'SUFFIX'", comment: "Vị trí ký hiệu khi hiển thị"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn sử dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("currencies_pkey", x => x.id);
                    table.UniqueConstraint("currencies_code_key", x => x.code);
                    table.CheckConstraint("currencies_symbol_position_check", "symbol_position IN ('PREFIX','SUFFIX')");
                },
                comment: "Danh mục tiền tệ theo ISO 4217");

            migrationBuilder.CreateTable(
                name: "data_change_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty (lọc theo data scope)"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Phân hệ"),
                    entity_type = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Loại đối tượng"),
                    entity_id = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Id đối tượng (text để hỗ trợ khóa không phải uuid)"),
                    entity_display = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Tên/mã hiển thị (NV-2026-0001 - Nguyễn Văn A)"),
                    action = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Hành động"),
                    changed_fields = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Danh sách trường thay đổi"),
                    old_values = table.Column<string>(type: "jsonb", nullable: true, comment: "Giá trị cũ {field: value}"),
                    new_values = table.Column<string>(type: "jsonb", nullable: true, comment: "Giá trị mới {field: value}"),
                    reason = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Lý do thay đổi (bắt buộc với một số nghiệp vụ)"),
                    changed_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người thay đổi"),
                    changed_by_name = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Tên người thay đổi (snapshot)"),
                    on_behalf_of = table.Column<Guid>(type: "uuid", nullable: true, comment: "Thay mặt (impersonation)"),
                    source = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'WEB'", comment: "Nguồn thay đổi"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP"),
                    trace_id = table.Column<string>(type: "varchar(64)", nullable: true, comment: "Trace id liên kết request"),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("data_change_logs_pkey", x => x.id);
                    table.CheckConstraint("data_change_logs_action_check", "action IN ('INSERT','UPDATE','DELETE','RESTORE','APPROVE','EXPORT')");
                    table.CheckConstraint("data_change_logs_source_check", "source IN ('WEB','MOBILE','API','IMPORT','JOB','WORKFLOW','SYSTEM')");
                },
                comment: "Lịch sử thay đổi dữ liệu nhạy cảm (old/new value) phục vụ đối soát kế toán, kho, nhân sự. Các phân hệ gửi qua event hoặc ghi trực tiếp");

            migrationBuilder.CreateTable(
                name: "file_access_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "File"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người truy cập (NULL nếu qua share link)"),
                    share_link_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Link chia sẻ được dùng"),
                    action = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Hành động"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP"),
                    user_agent = table.Column<string>(type: "text", nullable: true, comment: "User-Agent"),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("file_access_logs_pkey", x => x.id);
                    table.CheckConstraint("file_access_logs_action_check", "action IN ('VIEW','DOWNLOAD','PRESIGN','DELETE','SHARE')");
                },
                comment: "Nhật ký truy cập file nhạy cảm (xem, tải, cấp presigned URL) phục vụ kiểm toán");

            migrationBuilder.CreateTable(
                name: "health_checks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Service"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, comment: "Kết quả"),
                    http_status = table.Column<short>(type: "smallint", nullable: true, comment: "HTTP status"),
                    response_ms = table.Column<int>(type: "int", nullable: true, comment: "Thời gian phản hồi"),
                    details = table.Column<string>(type: "jsonb", nullable: true, comment: "Chi tiết từng dependency (db, redis, rabbitmq) trả về từ /health"),
                    error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi"),
                    checked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("health_checks_pkey", x => x.id);
                    table.CheckConstraint("health_checks_status_check", "status IN ('HEALTHY','DEGRADED','UNHEALTHY')");
                },
                comment: "Kết quả từng lần health check");

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    consumer_service = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Service tiêu thụ"),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Message id"),
                    event_type_code = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Mã event"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PROCESSED'", comment: "Trạng thái"),
                    error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi"),
                    received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Nhận lúc"),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Xử lý xong lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inbox_messages_pkey", x => x.id);
                    table.CheckConstraint("inbox_messages_status_check", "status IN ('PROCESSING','PROCESSED','FAILED')");
                },
                comment: "Inbox/Idempotency: consumer ghi nhận message đã xử lý để không xử lý trùng");

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Mã BCP-47 (vi-VN, en-US)"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên tiếng Việt"),
                    native_name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên bản ngữ (Tiếng Việt, English)"),
                    direction = table.Column<string>(type: "varchar(3)", nullable: false, defaultValueSql: "'LTR'", comment: "Hướng chữ"),
                    date_format = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'dd/MM/yyyy'", comment: "Định dạng ngày"),
                    number_format = table.Column<string>(type: "jsonb", nullable: true, comment: "Dấu thập phân, nhóm nghìn"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Ngôn ngữ mặc định"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang hỗ trợ"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("languages_pkey", x => x.id);
                    table.UniqueConstraint("languages_code_key", x => x.code);
                    table.CheckConstraint("languages_direction_check", "direction IN ('LTR','RTL')");
                },
                comment: "Ngôn ngữ hệ thống hỗ trợ");

            migrationBuilder.CreateTable(
                name: "login_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "User (NULL nếu username không tồn tại)"),
                    username_attempted = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Username/email đã nhập"),
                    event_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Sự kiện"),
                    failure_reason = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Lý do thất bại (INVALID_PASSWORD, USER_NOT_FOUND, ACCOUNT_LOCKED, IP_NOT_ALLOWED, DEVICE_LIMIT)"),
                    auth_method = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phương thức (PASSWORD, GOOGLE, OTP)"),
                    client_id = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Ứng dụng"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phiên tạo ra"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Thiết bị"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP"),
                    user_agent = table.Column<string>(type: "text", nullable: true, comment: "User-Agent"),
                    geo_country = table.Column<string>(type: "varchar(2)", nullable: true, comment: "Quốc gia (GeoIP)"),
                    geo_city = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Thành phố (GeoIP)"),
                    is_suspicious = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Đánh dấu bất thường (IP lạ, quốc gia lạ)"),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("login_logs_pkey", x => x.id);
                    table.CheckConstraint("login_logs_event_type_check", "event_type IN ('LOGIN_SUCCESS','LOGIN_FAILED','LOGOUT','TOKEN_REFRESH','TOKEN_REUSE_DETECTED','MFA_SUCCESS','MFA_FAILED','ACCOUNT_LOCKED','SSO_LOGIN','IMPERSONATE')");
                },
                comment: "Lịch sử đăng nhập/đăng xuất/refresh token (thành công & thất bại) kèm IP, thiết bị");

            migrationBuilder.CreateTable(
                name: "modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mã phân hệ (CORE, HRM, CRM, WMS, AI)"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên phân hệ"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    base_route = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Tiền tố route trên gateway (/hrm)"),
                    icon = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Icon menu"),
                    color = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Màu nhận diện"),
                    web_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL ứng dụng web của phân hệ"),
                    is_core = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Phân hệ lõi luôn bật cho mọi tenant"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn sử dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("modules_pkey", x => x.id);
                },
                comment: "Danh mục phân hệ/module của hệ sinh thái (CORE, HRM, CRM, WMS, AI...). Dùng cho phân quyền theo phân hệ và routing");

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Message id (dùng làm idempotency key phía consumer)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    event_type_code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã event"),
                    aggregate_type = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Loại aggregate (Order, Employee)"),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id aggregate"),
                    payload = table.Column<string>(type: "jsonb", nullable: false, comment: "Nội dung event"),
                    headers = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Header (user, correlation...)"),
                    trace_id = table.Column<string>(type: "varchar(64)", nullable: true, comment: "Trace id"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PENDING'", comment: "Trạng thái publish"),
                    attempts = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Số lần thử"),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Lần thử tiếp"),
                    last_error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi gần nhất"),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm sự kiện xảy ra"),
                    published_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Publish thành công lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("outbox_messages_pkey", x => x.id);
                    table.CheckConstraint("outbox_messages_status_check", "status IN ('PENDING','PUBLISHED','FAILED','DEAD')");
                },
                comment: "Transactional Outbox: event được ghi cùng transaction nghiệp vụ, worker đọc và publish lên message broker");

            migrationBuilder.CreateTable(
                name: "permission_actions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mã hành động"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên hiển thị (Xem, Thêm, Sửa...)"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị trên ma trận phân quyền"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("permission_actions_pkey", x => x.id);
                    table.UniqueConstraint("permission_actions_code_key", x => x.code);
                },
                comment: "Danh mục hành động chuẩn (VIEW, CREATE, UPDATE, DELETE, APPROVE, EXPORT, IMPORT, PRINT...)");

            migrationBuilder.CreateTable(
                name: "rate_limit_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã chính sách (default-user, login-strict, partner-basic)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên"),
                    partition_by = table.Column<string>(type: "varchar(15)", nullable: false, comment: "Đếm theo khóa nào"),
                    algorithm = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'SLIDING_WINDOW'", comment: "Thuật toán"),
                    permit_limit = table.Column<int>(type: "int", nullable: false, comment: "Số request cho phép trong cửa sổ"),
                    window_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "60", comment: "Độ dài cửa sổ (giây)"),
                    segments = table.Column<short>(type: "smallint", nullable: true, comment: "Số phân đoạn (SLIDING_WINDOW)"),
                    tokens_per_period = table.Column<int>(type: "int", nullable: true, comment: "Token nạp lại mỗi chu kỳ (TOKEN_BUCKET)"),
                    queue_limit = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số request được xếp hàng khi vượt"),
                    reject_status = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "429", comment: "HTTP status khi từ chối"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("rate_limit_policies_pkey", x => x.id);
                    table.CheckConstraint("rate_limit_policies_algorithm_check", "algorithm IN ('FIXED_WINDOW','SLIDING_WINDOW','TOKEN_BUCKET','CONCURRENCY')");
                    table.CheckConstraint("rate_limit_policies_partition_by_check", "partition_by IN ('IP','USER','TENANT','CLIENT','API_KEY','GLOBAL')");
                    table.CheckConstraint("rate_limit_policies_permit_limit_check", "permit_limit > 0");
                },
                comment: "Chính sách rate limit/throttling theo IP, user, tenant, client, API key");

            migrationBuilder.CreateTable(
                name: "revoked_access_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    jti = table.Column<string>(type: "varchar(100)", nullable: false, comment: "JWT ID"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "User"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phiên"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Hết hạn gốc của token (sau thời điểm này xóa bản ghi)"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thu hồi lúc"),
                    reason = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Lý do"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("revoked_access_tokens_pkey", x => x.id);
                },
                comment: "Danh sách đen access token (jti) bị thu hồi trước hạn. Bản chính nên ở Redis; bảng này để khôi phục khi Redis mất dữ liệu");

            migrationBuilder.CreateTable(
                name: "security_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    event_type = table.Column<string>(type: "varchar(40)", nullable: false, comment: "Loại (PASSWORD_CHANGED, PASSWORD_RESET, USER_LOCKED, USER_UNLOCKED, ROLE_ASSIGNED, ROLE_REVOKED, ROLE_PERMISSION_CHANGED, MFA_ENABLED, API_KEY_CREATED, SESSION_REVOKED...)"),
                    severity = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'INFO'", comment: "Mức độ"),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người thực hiện (NULL = hệ thống)"),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "User bị tác động"),
                    target_type = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Loại đối tượng bị tác động (ROLE, API_KEY, OAUTH_CLIENT)"),
                    target_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id đối tượng"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả dễ đọc"),
                    details = table.Column<string>(type: "jsonb", nullable: true, comment: "Chi tiết (quyền thêm/bớt, vai trò cũ/mới)"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP"),
                    user_agent = table.Column<string>(type: "text", nullable: true, comment: "User-Agent"),
                    trace_id = table.Column<string>(type: "varchar(64)", nullable: true, comment: "Trace id"),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("security_events_pkey", x => x.id);
                    table.CheckConstraint("security_events_severity_check", "severity IN ('INFO','WARNING','CRITICAL')");
                },
                comment: "Sự kiện bảo mật quan trọng: đổi/reset mật khẩu, khóa/mở khóa, thay đổi vai trò/quyền, cấp API key, bật/tắt 2FA");

            migrationBuilder.CreateTable(
                name: "security_header_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã chính sách (default, strict-admin)"),
                    headers = table.Column<string>(type: "jsonb", nullable: false, comment: "Map header → giá trị, VD: {\"Strict-Transport-Security\":\"max-age=31536000; includeSubDomains\"}"),
                    remove_headers = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{Server,X-Powered-By}'", comment: "Header cần gỡ khỏi response"),
                    force_https = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Chuyển hướng HTTP → HTTPS"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("security_header_policies_pkey", x => x.id);
                },
                comment: "Bộ security header gắn vào response (HSTS, CSP, X-Frame-Options...)");

            migrationBuilder.CreateTable(
                name: "signing_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    kid = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Key ID (header kid trong JWT)"),
                    algorithm = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'RS256'", comment: "Thuật toán ký"),
                    usage = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'SIGNING'", comment: "Mục đích"),
                    public_key_pem = table.Column<string>(type: "text", nullable: false, comment: "Public key (công bố qua /.well-known/jwks.json)"),
                    private_key_encrypted = table.Column<string>(type: "text", nullable: false, comment: "Private key đã mã hóa"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "PENDING: công bố trước, ACTIVE: đang ký, RETIRED: chỉ còn verify"),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Bắt đầu ký"),
                    retired_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Ngừng ký"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Gỡ khỏi JWKS sau thời điểm này"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("signing_keys_pkey", x => x.id);
                    table.CheckConstraint("signing_keys_algorithm_check", "algorithm IN ('RS256','RS384','RS512','ES256','ES384','PS256')");
                    table.CheckConstraint("signing_keys_status_check", "status IN ('PENDING','ACTIVE','RETIRED','REVOKED')");
                    table.CheckConstraint("signing_keys_usage_check", "usage IN ('SIGNING','ENCRYPTION')");
                },
                comment: "Khóa ký JWT (RS256/ES256) phục vụ JWKS và xoay vòng khóa định kỳ");

            migrationBuilder.CreateTable(
                name: "tax_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Mã loại thuế (VAT, CIT, PIT, IMPORT, SCT)"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên loại thuế"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tax_types_pkey", x => x.id);
                    table.UniqueConstraint("tax_types_code_key", x => x.code);
                },
                comment: "Loại thuế (GTGT, TNDN, TNCN, Nhập khẩu, Tiêu thụ đặc biệt...)");

            migrationBuilder.CreateTable(
                name: "uom_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mã nhóm (COUNT, WEIGHT, VOLUME, LENGTH, AREA, TIME)"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên nhóm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("uom_categories_pkey", x => x.id);
                },
                comment: "Nhóm đơn vị tính (Số lượng, Khối lượng, Thể tích, Chiều dài, Diện tích, Thời gian)");

            migrationBuilder.CreateTable(
                name: "usage_daily",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    usage_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Ngày"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: false, defaultValueSql: "'-'", comment: "Phân hệ ('-' nếu không xác định)"),
                    model_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Model"),
                    request_count = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số request"),
                    error_count = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số request lỗi"),
                    input_tokens = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0", comment: "Tổng token vào"),
                    output_tokens = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0", comment: "Tổng token ra"),
                    cost_usd = table.Column<decimal>(type: "numeric(14,6)", nullable: false, defaultValueSql: "0", comment: "Tổng chi phí"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Cập nhật lúc"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usage_daily_pkey", x => x.id);
                },
                comment: "Tổng hợp usage theo ngày (job cộng dồn từ usage_logs) — dùng kiểm tra quota nhanh và báo cáo chi phí");

            migrationBuilder.CreateTable(
                name: "usage_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ gọi"),
                    gateway_client_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Client nội bộ"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "User cuối"),
                    prompt_template_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Prompt"),
                    prompt_version_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phiên bản prompt"),
                    alias_code = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Alias được yêu cầu"),
                    model_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Model thực tế phục vụ"),
                    credential_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Key provider đã dùng"),
                    request_type = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'CHAT'", comment: "Loại request"),
                    input_tokens = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Token đầu vào"),
                    output_tokens = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Token đầu ra"),
                    cached_tokens = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Token đọc từ cache"),
                    total_tokens = table.Column<int>(type: "int", nullable: true, computedColumnSql: "input_tokens + output_tokens", stored: true, comment: "Tổng token"),
                    estimated_cost_usd = table.Column<decimal>(type: "numeric(14,8)", nullable: false, defaultValueSql: "0", comment: "Chi phí ước tính (USD)"),
                    latency_ms = table.Column<int>(type: "int", nullable: true, comment: "Độ trễ tổng"),
                    time_to_first_token_ms = table.Column<int>(type: "int", nullable: true, comment: "Thời gian tới token đầu (streaming)"),
                    is_fallback = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Phục vụ bởi model fallback"),
                    is_cache_hit = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Trả về từ semantic cache"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, comment: "Kết quả"),
                    error_code = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Mã lỗi"),
                    guardrail_codes = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Guardrail đã kích hoạt"),
                    trace_id = table.Column<string>(type: "varchar(64)", nullable: true, comment: "Trace id"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm gọi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usage_logs_pkey", x => x.id);
                    table.CheckConstraint("usage_logs_request_type_check", "request_type IN ('CHAT','EMBEDDING','RERANK','IMAGE','AUDIO','MODERATION')");
                    table.CheckConstraint("usage_logs_status_check", "status IN ('SUCCESS','ERROR','BLOCKED','QUOTA_EXCEEDED','TIMEOUT')");
                },
                comment: "Log từng lần gọi AI: token, chi phí, độ trễ (không lưu nội dung prompt theo mặc định vì dữ liệu nhạy cảm)");

            migrationBuilder.CreateTable(
                name: "models",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nhà cung cấp"),
                    model_code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã model gửi lên API"),
                    display_name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên hiển thị"),
                    capabilities = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{CHAT}'", comment: "Năng lực: CHAT, VISION, TOOLS, JSON_MODE, EMBEDDING, RERANK, AUDIO, IMAGE_GEN"),
                    context_window = table.Column<int>(type: "int", nullable: true, comment: "Số token ngữ cảnh tối đa"),
                    max_output_tokens = table.Column<int>(type: "int", nullable: true, comment: "Số token đầu ra tối đa"),
                    embedding_dimension = table.Column<int>(type: "int", nullable: true, comment: "Số chiều vector (model embedding)"),
                    input_price_per_1m = table.Column<decimal>(type: "numeric(12,6)", nullable: true, comment: "Giá input / 1 triệu token (USD)"),
                    output_price_per_1m = table.Column<decimal>(type: "numeric(12,6)", nullable: true, comment: "Giá output / 1 triệu token (USD)"),
                    cached_input_price_per_1m = table.Column<decimal>(type: "numeric(12,6)", nullable: true, comment: "Giá input cache / 1 triệu token"),
                    price_currency = table.Column<string>(type: "char(3)", nullable: false, defaultValueSql: "'USD'", comment: "Tiền tệ bảng giá"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    deprecated_at = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày provider ngừng hỗ trợ"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("models_pkey", x => x.id);
                    table.ForeignKey(
                        name: "models_provider_id_fkey",
                        column: x => x.provider_id,
                        principalTable: "ai_providers",
                        principalColumn: "id");
                },
                comment: "Model AI và bảng giá để ước tính chi phí");

            migrationBuilder.CreateTable(
                name: "administrative_units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, comment: "Quốc gia"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Đơn vị cấp trên (NULL nếu là cấp tỉnh)"),
                    code = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Mã đơn vị hành chính (mã GSO/Tổng cục Thống kê)"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên ngắn (VD: Hoàn Kiếm)"),
                    full_name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên đầy đủ (VD: Phường Hoàn Kiếm, Thành phố Hà Nội)"),
                    name_en = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Tên tiếng Anh / không dấu"),
                    unit_type = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Loại đơn vị: TP trực thuộc TW, tỉnh, quận, huyện, thị xã, phường, xã, thị trấn, đặc khu"),
                    level = table.Column<short>(type: "smallint", nullable: false, comment: "Cấp: 1 = tỉnh, 2 = huyện (mô hình cũ) hoặc xã (mô hình 2 cấp), 3 = xã (mô hình cũ)"),
                    path = table.Column<string>(type: "text", nullable: true, comment: "Đường dẫn mã từ gốc, VD: '01/00004' để truy vấn cây nhanh"),
                    phone_area_code = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Mã vùng điện thoại cố định (024, 028...)"),
                    postal_code = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Mã bưu chính"),
                    latitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true, comment: "Vĩ độ trung tâm"),
                    longitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true, comment: "Kinh độ trung tâm"),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày bắt đầu hiệu lực (VD: 2025-07-01 cho đơn vị sau sáp nhập)"),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày hết hiệu lực (đơn vị đã giải thể/sáp nhập); NULL = còn hiệu lực"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng để chọn trên form mới"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("administrative_units_pkey", x => x.id);
                    table.CheckConstraint("administrative_units_level_check", "level BETWEEN 1 AND 3");
                    table.CheckConstraint("administrative_units_unit_type_check", "unit_type IN ('CENTRAL_CITY','PROVINCE','DISTRICT','URBAN_DISTRICT','TOWN','PROVINCIAL_CITY','WARD','COMMUNE','TOWNSHIP','SPECIAL_ZONE')");
                    table.ForeignKey(
                        name: "administrative_units_country_code_fkey",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "administrative_units_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                },
                comment: "Đơn vị hành chính phân cấp tổng quát (Tỉnh/TP → [Quận/Huyện] → Phường/Xã). Hỗ trợ cả mô hình 3 cấp cũ và 2 cấp sau sáp nhập 01/07/2025 nhờ effective_from/effective_to");

            migrationBuilder.CreateTable(
                name: "banks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, defaultValueSql: "'VN'", comment: "Quốc gia"),
                    code = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Mã viết tắt (VCB, TCB, MB...)"),
                    bin = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Mã BIN NAPAS dùng tạo VietQR (970436...)"),
                    short_name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên ngắn (Vietcombank)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên đầy đủ"),
                    swift_code = table.Column<string>(type: "varchar(11)", nullable: true, comment: "Mã SWIFT/BIC"),
                    logo_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Logo ngân hàng"),
                    supports_vietqr = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Hỗ trợ VietQR hay không"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn sử dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("banks_pkey", x => x.id);
                    table.ForeignKey(
                        name: "banks_country_code_fkey",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "code");
                },
                comment: "Danh mục ngân hàng (dùng cho VietQR, chi lương, thanh toán NCC)");

            migrationBuilder.CreateTable(
                name: "api_scopes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã scope (hrm.employee.read)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên hiển thị"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả quyền truy cập"),
                    is_sensitive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Scope nhạy cảm (lương, hợp đồng) cần duyệt khi cấp"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("api_scopes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "api_scopes_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Danh mục scope API cấp cho client/API key (hrm.read, wms.write, openid...)");

            migrationBuilder.CreateTable(
                name: "clusters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã cluster (hrm-api, wms-api, auth-api)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    load_balancing = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ROUND_ROBIN'", comment: "Thuật toán cân bằng tải"),
                    health_check_path = table.Column<string>(type: "varchar(255)", nullable: false, defaultValueSql: "'/health'", comment: "Đường dẫn health check chủ động"),
                    health_check_interval_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "15", comment: "Chu kỳ health check"),
                    request_timeout_ms = table.Column<int>(type: "int", nullable: false, defaultValueSql: "30000", comment: "Timeout chuyển tiếp"),
                    http_version = table.Column<string>(type: "varchar(5)", nullable: false, defaultValueSql: "'1.1'", comment: "Phiên bản HTTP tới upstream"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("clusters_pkey", x => x.id);
                    table.CheckConstraint("clusters_load_balancing_check", "load_balancing IN ('ROUND_ROBIN','LEAST_REQUESTS','RANDOM','POWER_OF_TWO','FIRST')");
                    table.ForeignKey(
                        name: "clusters_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Cụm upstream (service nội bộ) mà gateway định tuyến tới");

            migrationBuilder.CreateTable(
                name: "document_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã loại chứng từ (HRM_EMPLOYEE, HRM_LEAVE_REQUEST, WMS_GOODS_RECEIPT, CRM_QUOTATION)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phân hệ sở hữu"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên loại chứng từ"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    entity_table = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Bảng dữ liệu gốc bên phân hệ (tham khảo)"),
                    supports_workflow = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Có thể gắn quy trình duyệt"),
                    supports_numbering = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Có sinh mã tự động"),
                    supports_attachment = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Cho phép đính kèm file"),
                    workflow_variables = table.Column<string>(type: "jsonb", nullable: true, comment: "Khai báo biến dùng trong điều kiện rẽ nhánh, VD: [{\"name\":\"amount\",\"type\":\"number\",\"label\":\"Tổng tiền\"}]"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("document_types_pkey", x => x.id);
                    table.UniqueConstraint("document_types_code_key", x => x.code);
                    table.ForeignKey(
                        name: "document_types_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Danh mục loại chứng từ/đối tượng nghiệp vụ toàn hệ sinh thái — khóa chung để gắn sinh mã, workflow, file, thông báo");

            migrationBuilder.CreateTable(
                name: "error_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã lỗi (AUTH_TOKEN_EXPIRED, HRM_LEAVE_BALANCE_INSUFFICIENT)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    http_status = table.Column<short>(type: "smallint", nullable: false, comment: "HTTP status tương ứng"),
                    category = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Nhóm lỗi"),
                    message_key = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Khóa i18n của thông báo (config.translation_keys)"),
                    default_message = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Thông báo mặc định tiếng Việt"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Giải thích cho dev / hướng xử lý"),
                    is_retryable = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Client có nên thử lại"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("error_codes_pkey", x => x.id);
                    table.CheckConstraint("error_codes_category_check", "category IN ('VALIDATION','AUTHENTICATION','AUTHORIZATION','NOT_FOUND','CONFLICT','BUSINESS','RATE_LIMIT','INTEGRATION','SYSTEM')");
                    table.CheckConstraint("error_codes_http_status_check", "http_status BETWEEN 400 AND 599");
                    table.ForeignKey(
                        name: "error_codes_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Mã lỗi chuẩn hóa toàn hệ sinh thái. Response lỗi: {success:false, code, message, traceId, errors[]}");

            migrationBuilder.CreateTable(
                name: "feature_flags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    key = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Khóa flag (wms.pos.offline_mode)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    flag_type = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'BOOLEAN'", comment: "Bật/tắt, theo % rollout, theo biến thể"),
                    default_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Mặc định khi không có rule khớp"),
                    variants = table.Column<string>(type: "jsonb", nullable: true, comment: "Danh sách biến thể (VARIANT)"),
                    is_kill_switch = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Công tắc khẩn cấp tắt tính năng toàn hệ thống"),
                    owner = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Người/team phụ trách"),
                    remove_after = table.Column<DateOnly>(type: "date", nullable: true, comment: "Hạn dọn flag khỏi code"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Flag còn được đánh giá"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("feature_flags_pkey", x => x.id);
                    table.UniqueConstraint("feature_flags_key_key", x => x.key);
                    table.CheckConstraint("feature_flags_flag_type_check", "flag_type IN ('BOOLEAN','PERCENTAGE','VARIANT')");
                    table.ForeignKey(
                        name: "feature_flags_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Feature flag bật/tắt tính năng không cần deploy");

            migrationBuilder.CreateTable(
                name: "integration_event_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã event (domain.entity.action)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phân hệ phát (producer)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên event"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    schema_version = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1", comment: "Phiên bản schema payload"),
                    payload_schema = table.Column<string>(type: "jsonb", nullable: true, comment: "JSON Schema của payload (validate + sinh tài liệu)"),
                    exchange_name = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Exchange/stream trên RabbitMQ/Redis Streams"),
                    routing_key = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Routing key"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cho phép đối tác đăng ký qua webhook"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("integration_event_types_pkey", x => x.id);
                    table.UniqueConstraint("integration_event_types_code_key", x => x.code);
                    table.ForeignKey(
                        name: "integration_event_types_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Danh mục event nghiệp vụ trao đổi giữa các phân hệ (crm.order.confirmed, hrm.payroll.closed...) kèm schema payload");

            migrationBuilder.CreateTable(
                name: "job_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã job (core.storage.cleanup_temp_files)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ sở hữu"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    handler = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Tên class/handler thực thi (Assembly.Namespace.JobClass)"),
                    target_service = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Service chạy job (hrm-worker, core-worker)"),
                    cron_expression = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Lịch cron (NULL = chỉ chạy tay)"),
                    timezone = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "'Asia/Ho_Chi_Minh'", comment: "Múi giờ tính cron"),
                    run_per_tenant = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Chạy lặp qua từng tenant"),
                    parameters = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Tham số mặc định"),
                    queue_name = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "'default'", comment: "Hàng đợi"),
                    timeout_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "3600", comment: "Thời gian chạy tối đa"),
                    max_retries = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "3", comment: "Số lần retry khi lỗi"),
                    allow_concurrent = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cho phép chạy chồng"),
                    notify_on_failure = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Gửi cảnh báo khi lỗi"),
                    notify_user_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: true, comment: "Người nhận cảnh báo"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang bật"),
                    last_run_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần chạy gần nhất"),
                    last_status = table.Column<string>(type: "varchar(15)", nullable: true, comment: "Kết quả lần gần nhất"),
                    next_run_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần chạy kế tiếp (tham khảo)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("job_definitions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "job_definitions_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Danh mục job định kỳ toàn hệ thống (chốt công cuối ngày, cảnh báo tồn kho, dọn file rác, đồng bộ tỷ giá...)");

            migrationBuilder.CreateTable(
                name: "notify_event_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phân hệ phát sinh"),
                    code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã sự kiện"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên hiển thị trong cài đặt thông báo"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    default_channels = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{IN_APP}'", comment: "Kênh mặc định: IN_APP, PUSH, EMAIL, SMS, ZALO"),
                    priority = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'NORMAL'", comment: "Mức ưu tiên"),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Bắt buộc nhận, user không được tắt (VD: reset mật khẩu)"),
                    variables = table.Column<string>(type: "jsonb", nullable: true, comment: "Khai báo biến template, VD: [{\"name\":\"employee_name\",\"sample\":\"Nguyễn Văn A\"}]"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notify_event_types_pkey", x => x.id);
                    table.CheckConstraint("notify_event_types_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
                    table.ForeignKey(
                        name: "notify_event_types_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Loại sự kiện thông báo do các phân hệ khai báo (hrm.leave_request.submitted, wms.stock.low...)");

            migrationBuilder.CreateTable(
                name: "resources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phân hệ"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tài nguyên cha (nhóm menu)"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã tài nguyên (EMPLOYEE, LEAVE_REQUEST, GOODS_RECEIPT)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên hiển thị"),
                    resource_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'FUNCTION'", comment: "Loại: nhóm menu, chức năng, báo cáo, API thuần"),
                    route = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Route frontend (/hrm/employees)"),
                    icon = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Icon menu"),
                    supports_data_scope = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Tài nguyên có áp dụng phạm vi dữ liệu không"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự menu"),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Hiển thị trên menu"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn sử dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("resources_pkey", x => x.id);
                    table.CheckConstraint("resources_resource_type_check", "resource_type IN ('MENU_GROUP','FUNCTION','REPORT','API')");
                    table.ForeignKey(
                        name: "resources_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "resources_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "resources",
                        principalColumn: "id");
                },
                comment: "Tài nguyên/chức năng (menu, màn hình, API) của từng phân hệ dạng cây. Permission = resource + action");

            migrationBuilder.CreateTable(
                name: "services",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã service (auth-api, hrm-api, core-worker, postgres, redis, rabbitmq, minio)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    service_type = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'API'", comment: "Loại"),
                    environment = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PRODUCTION'", comment: "Môi trường"),
                    base_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL nội bộ"),
                    health_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL health (/health/ready)"),
                    liveness_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL liveness (/health/live)"),
                    check_interval_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "30", comment: "Chu kỳ kiểm tra"),
                    timeout_ms = table.Column<int>(type: "int", nullable: false, defaultValueSql: "5000", comment: "Timeout"),
                    alert_after_failures = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "3", comment: "Cảnh báo sau N lần lỗi liên tiếp"),
                    alert_channels = table.Column<string>(type: "jsonb", nullable: true, comment: "Kênh cảnh báo (email, Telegram, Slack webhook)"),
                    current_version = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Phiên bản đang chạy (đọc từ /health)"),
                    current_status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'UNKNOWN'", comment: "Trạng thái hiện tại"),
                    consecutive_failures = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số lần lỗi liên tiếp"),
                    last_checked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Kiểm tra gần nhất"),
                    last_status_change_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đổi trạng thái gần nhất"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang giám sát"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("services_pkey", x => x.id);
                    table.CheckConstraint("services_current_status_check", "current_status IN ('HEALTHY','DEGRADED','UNHEALTHY','UNKNOWN','MAINTENANCE')");
                    table.CheckConstraint("services_environment_check", "environment IN ('DEVELOPMENT','STAGING','PRODUCTION')");
                    table.CheckConstraint("services_service_type_check", "service_type IN ('API','WORKER','WEB','DATABASE','CACHE','QUEUE','STORAGE','EXTERNAL')");
                    table.ForeignKey(
                        name: "services_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Danh mục service trong hệ sinh thái và endpoint /health để Docker, Reverse Proxy, Uptime Monitor kiểm tra");

            migrationBuilder.CreateTable(
                name: "setting_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    key = table.Column<string>(type: "varchar(200)", nullable: false, comment: "Khóa cấu hình (hrm.attendance.late_grace_minutes)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    group_name = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Nhóm hiển thị trên màn hình cài đặt"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên hiển thị"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Giải thích"),
                    data_type = table.Column<string>(type: "varchar(15)", nullable: false, comment: "Kiểu dữ liệu"),
                    default_value = table.Column<string>(type: "jsonb", nullable: true, comment: "Giá trị mặc định"),
                    options = table.Column<string>(type: "jsonb", nullable: true, comment: "Danh sách lựa chọn (ENUM)"),
                    validation = table.Column<string>(type: "jsonb", nullable: true, comment: "Ràng buộc (min, max, regex)"),
                    allowed_scopes = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{SYSTEM,TENANT}'", comment: "Phạm vi được ghi đè: SYSTEM, TENANT, COMPANY, BRANCH, USER"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Trả về cho frontend (không bí mật)"),
                    requires_restart = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cần restart service để có hiệu lực"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("setting_definitions_pkey", x => x.id);
                    table.UniqueConstraint("setting_definitions_key_key", x => x.key);
                    table.CheckConstraint("setting_definitions_data_type_check", "data_type IN ('STRING','INT','DECIMAL','BOOL','DATE','TIME','JSON','ENUM','SECRET')");
                    table.ForeignKey(
                        name: "setting_definitions_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Định nghĩa khóa cấu hình (kiểu dữ liệu, mặc định, phạm vi cho phép ghi đè) — thay cho hardcode");

            migrationBuilder.CreateTable(
                name: "translation_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    @namespace = table.Column<string>(name: "namespace", type: "varchar(50)", nullable: false, comment: "Nhóm: common, errors, validation, hrm, wms, crm"),
                    key = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Khóa (button.save, AUTH_INVALID_TOKEN)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Ngữ cảnh cho người dịch"),
                    platforms = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{WEB,MOBILE,API}'", comment: "Nền tảng sử dụng (lọc khi export file JSON)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("translation_keys_pkey", x => x.id);
                    table.ForeignKey(
                        name: "translation_keys_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                },
                comment: "Khóa dịch dùng chung (nhãn UI, thông báo lỗi API) theo namespace");

            migrationBuilder.CreateTable(
                name: "administrative_unit_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    old_unit_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Đơn vị cũ"),
                    new_unit_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Đơn vị mới thay thế"),
                    mapping_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'MERGE'", comment: "Hình thức: sáp nhập, chia tách, đổi tên, một phần"),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Ngày có hiệu lực theo nghị quyết"),
                    legal_reference = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Số nghị quyết/văn bản pháp lý"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("administrative_unit_mappings_pkey", x => x.id);
                    table.CheckConstraint("administrative_unit_mappings_mapping_type_check", "mapping_type IN ('MERGE','SPLIT','RENAME','PARTIAL')");
                    table.ForeignKey(
                        name: "administrative_unit_mappings_new_unit_id_fkey",
                        column: x => x.new_unit_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "administrative_unit_mappings_old_unit_id_fkey",
                        column: x => x.old_unit_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                },
                comment: "Bảng ánh xạ đơn vị hành chính cũ → mới (sau sáp nhập), dùng để chuyển đổi địa chỉ lịch sử");

            migrationBuilder.CreateTable(
                name: "destinations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    cluster_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Cluster"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên instance (hrm-api-1)"),
                    address = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Địa chỉ nội bộ (http://hrm-api:8080)"),
                    weight = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "100", comment: "Trọng số"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang nhận traffic"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("destinations_pkey", x => x.id);
                    table.ForeignKey(
                        name: "destinations_cluster_id_fkey",
                        column: x => x.cluster_id,
                        principalTable: "clusters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Địa chỉ instance cụ thể của cluster (container trong Docker network)");

            migrationBuilder.CreateTable(
                name: "feature_flag_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    flag_key = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Flag"),
                    scope_type = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Đối tượng áp dụng"),
                    scope_value = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Id hoặc mã đối tượng (uuid, mã gói, production/staging)"),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bật/tắt cho đối tượng"),
                    percentage = table.Column<short>(type: "smallint", nullable: true, comment: "% rollout (PERCENTAGE)"),
                    variant = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Biến thể trả về (VARIANT)"),
                    priority = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Ưu tiên (số lớn xét trước)"),
                    start_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Bắt đầu hiệu lực"),
                    end_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Kết thúc hiệu lực"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("feature_flag_rules_pkey", x => x.id);
                    table.CheckConstraint("feature_flag_rules_percentage_check", "percentage BETWEEN 0 AND 100");
                    table.CheckConstraint("feature_flag_rules_scope_type_check", "scope_type IN ('TENANT','COMPANY','BRANCH','ROLE','USER','PLAN','ENV')");
                    table.ForeignKey(
                        name: "feature_flag_rules_flag_key_fkey",
                        column: x => x.flag_key,
                        principalTable: "feature_flags",
                        principalColumn: "key",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Rule target của flag theo tenant/công ty/chi nhánh/vai trò/user, có thời gian hiệu lực");

            migrationBuilder.CreateTable(
                name: "event_subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    event_type_code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Event"),
                    consumer_service = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên service tiêu thụ (wms-api, accounting-api)"),
                    queue_name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Queue nhận"),
                    handler_name = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Handler xử lý (tham khảo)"),
                    max_retries = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "5", comment: "Số lần retry trước khi vào DLQ"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang hoạt động"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("event_subscriptions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "event_subscriptions_event_type_code_fkey",
                        column: x => x.event_type_code,
                        principalTable: "integration_event_types",
                        principalColumn: "code");
                },
                comment: "Service nào tiêu thụ event nào (bản đồ luồng dữ liệu giữa các phân hệ)");

            migrationBuilder.CreateTable(
                name: "executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (job chạy theo tenant)"),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Job"),
                    trigger_type = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'SCHEDULED'", comment: "Nguồn kích hoạt"),
                    triggered_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "User chạy tay"),
                    engine_job_id = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Id job bên Hangfire/Quartz"),
                    worker_node = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Máy/container thực thi"),
                    parameters = table.Column<string>(type: "jsonb", nullable: true, comment: "Tham số thực tế"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'RUNNING'", comment: "Trạng thái"),
                    progress_percent = table.Column<short>(type: "smallint", nullable: true, comment: "Tiến độ %"),
                    processed_count = table.Column<int>(type: "int", nullable: true, comment: "Số bản ghi đã xử lý"),
                    failed_count = table.Column<int>(type: "int", nullable: true, comment: "Số bản ghi lỗi"),
                    result_summary = table.Column<string>(type: "text", nullable: true, comment: "Tóm tắt kết quả"),
                    error_message = table.Column<string>(type: "text", nullable: true, comment: "Lỗi"),
                    error_stack = table.Column<string>(type: "text", nullable: true, comment: "Stack trace"),
                    attempt_no = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Lần thử"),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Bắt đầu"),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Kết thúc"),
                    duration_ms = table.Column<int>(type: "int", nullable: true, comment: "Thời gian chạy"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("executions_pkey", x => x.id);
                    table.CheckConstraint("executions_status_check", "status IN ('QUEUED','RUNNING','SUCCEEDED','FAILED','CANCELLED','TIMEOUT','SKIPPED')");
                    table.CheckConstraint("executions_trigger_type_check", "trigger_type IN ('SCHEDULED','MANUAL','RETRY','EVENT','API')");
                    table.ForeignKey(
                        name: "executions_job_id_fkey",
                        column: x => x.job_id,
                        principalTable: "job_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Lịch sử từng lần chạy job");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phân hệ (denormalize để lọc nhanh)"),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tài nguyên"),
                    action_code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Hành động"),
                    code = table.Column<string>(type: "varchar(200)", nullable: false, comment: "Mã quyền đầy đủ đưa vào token/policy (HRM:EMPLOYEE:VIEW)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên quyền"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Quyền do hệ thống khai báo (seed từ code), không cho xóa"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("permissions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "permissions_action_code_fkey",
                        column: x => x.action_code,
                        principalTable: "permission_actions",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "permissions_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "permissions_resource_id_fkey",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id");
                },
                comment: "Quyền cụ thể = tài nguyên × hành động. Mã quyền dạng MODULE:RESOURCE:ACTION (HRM:EMPLOYEE:CREATE)");

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    service_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Service"),
                    severity = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'MAJOR'", comment: "Mức độ"),
                    title = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Tiêu đề"),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Bắt đầu sự cố"),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Khắc phục lúc"),
                    root_cause = table.Column<string>(type: "text", nullable: true, comment: "Nguyên nhân"),
                    resolution = table.Column<string>(type: "text", nullable: true, comment: "Cách xử lý"),
                    acknowledged_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tiếp nhận"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("incidents_pkey", x => x.id);
                    table.CheckConstraint("incidents_severity_check", "severity IN ('MINOR','MAJOR','CRITICAL')");
                    table.ForeignKey(
                        name: "incidents_service_id_fkey",
                        column: x => x.service_id,
                        principalTable: "services",
                        principalColumn: "id");
                },
                comment: "Sự cố (downtime) mở/đóng tự động từ health check, phục vụ báo cáo uptime");

            migrationBuilder.CreateTable(
                name: "setting_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (denormalize để cache theo tenant)"),
                    setting_key = table.Column<string>(type: "varchar(200)", nullable: false, comment: "Khóa cấu hình"),
                    scope_type = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Phạm vi"),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id tenant/công ty/chi nhánh/user (NULL khi SYSTEM)"),
                    value = table.Column<string>(type: "jsonb", nullable: true, comment: "Giá trị"),
                    value_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Giá trị đã mã hóa (kiểu SECRET)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("setting_values_pkey", x => x.id);
                    table.CheckConstraint("ck_setting_scope", "(scope_type = 'SYSTEM') = (scope_id IS NULL)");
                    table.CheckConstraint("setting_values_scope_type_check", "scope_type IN ('SYSTEM','TENANT','COMPANY','BRANCH','USER')");
                    table.ForeignKey(
                        name: "setting_values_setting_key_fkey",
                        column: x => x.setting_key,
                        principalTable: "setting_definitions",
                        principalColumn: "key",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Giá trị cấu hình theo phạm vi. Thứ tự ưu tiên khi đọc: USER > BRANCH > COMPANY > TENANT > SYSTEM > default_value");

            migrationBuilder.CreateTable(
                name: "action_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Instance"),
                    task_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Nhiệm vụ liên quan"),
                    instance_step_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lượt bước"),
                    action = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Hành động"),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người thao tác (NULL = hệ thống)"),
                    on_behalf_of_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Thao tác thay cho ai (ủy quyền)"),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người nhận (khi chuyển/ủy quyền/xin ý kiến)"),
                    from_step_name = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Bước trước"),
                    to_step_name = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Bước sau"),
                    comment = table.Column<string>(type: "text", nullable: true, comment: "Ý kiến"),
                    attachments = table.Column<List<Guid>>(type: "uuid[]", nullable: true, comment: "File đính kèm (storage.files)"),
                    data_changes = table.Column<string>(type: "jsonb", nullable: true, comment: "Dữ liệu chứng từ được người duyệt sửa"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("action_logs_pkey", x => x.id);
                    table.CheckConstraint("action_logs_action_check", "action IN ('SUBMIT','APPROVE','REJECT','RETURN','RECALL','CANCEL','DELEGATE','REASSIGN','CONSULT','COMMENT','REMIND','ESCALATE','AUTO_APPROVE','AUTO_REJECT','SKIP','RESUBMIT')");
                },
                comment: "Lịch sử thao tác trên instance (timeline duyệt)");

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    oauth_client_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Gắn với client OAuth (nếu có)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên key (Kết nối website bán hàng)"),
                    key_prefix = table.Column<string>(type: "varchar(16)", nullable: false, comment: "Tiền tố hiển thị để nhận diện (berp_live_ab12)"),
                    key_hash = table.Column<string>(type: "varchar(128)", nullable: false, comment: "SHA-256 của API key (bản rõ chỉ hiển thị 1 lần khi tạo)"),
                    scopes = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'", comment: "Scope được phép (iam.api_scopes.code)"),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người chịu trách nhiệm"),
                    act_as_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Thực thi dưới danh nghĩa user (để áp RBAC/data scope)"),
                    environment = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'LIVE'", comment: "Môi trường"),
                    ip_whitelist = table.Column<List<IPNetwork>>(type: "cidr[]", nullable: true, comment: "IP được phép"),
                    allowed_origins = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Origin được phép (gọi từ trình duyệt)"),
                    rate_limit_policy_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Chính sách rate limit"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hết hạn"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần dùng gần nhất"),
                    last_used_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP dùng gần nhất"),
                    usage_count = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0", comment: "Tổng số lần gọi"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thu hồi lúc"),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người thu hồi"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("api_keys_pkey", x => x.id);
                    table.CheckConstraint("api_keys_environment_check", "environment IN ('LIVE','TEST')");
                    table.CheckConstraint("api_keys_status_check", "status IN ('ACTIVE','REVOKED','EXPIRED')");
                },
                comment: "API Key cấp cho đối tác/ứng dụng ngoài gọi vào ERP, có scope, IP whitelist, hạn dùng");

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Công ty sở hữu chi nhánh"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chi nhánh cấp trên (VD: vùng → cửa hàng)"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã chi nhánh (dùng trong sinh mã {BRANCH}, VD: HN01)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên chi nhánh"),
                    branch_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'OFFICE'", comment: "Loại: trụ sở, văn phòng, cửa hàng, kho, nhà máy, vùng"),
                    tax_code = table.Column<string>(type: "varchar(20)", nullable: true, comment: "MST chi nhánh (nếu hạch toán phụ thuộc có MST 13 số)"),
                    address_line = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Địa chỉ chi tiết"),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/Xã"),
                    province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh/Thành phố"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, defaultValueSql: "'VN'", comment: "Quốc gia"),
                    latitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true, comment: "Vĩ độ (dùng cho chấm công GPS, giao hàng)"),
                    longitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true, comment: "Kinh độ"),
                    geofence_radius_m = table.Column<int>(type: "int", nullable: true, comment: "Bán kính geofence (mét) cho chấm công mobile"),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Điện thoại"),
                    email = table.Column<string>(type: "citext", nullable: true, comment: "Email"),
                    manager_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người quản lý chi nhánh (iam.users)"),
                    timezone = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Múi giờ riêng (NULL = theo tenant)"),
                    work_calendar_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lịch làm việc riêng (NULL = theo công ty)"),
                    opening_date = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày khai trương/thành lập"),
                    closing_date = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày đóng cửa"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("branches_pkey", x => x.id);
                    table.CheckConstraint("branches_branch_type_check", "branch_type IN ('HEAD_OFFICE','OFFICE','STORE','WAREHOUSE','FACTORY','REGION','OTHER')");
                    table.CheckConstraint("branches_status_check", "status IN ('ACTIVE','INACTIVE','CLOSED')");
                    table.ForeignKey(
                        name: "branches_country_code_fkey",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "branches_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "branches_province_id_fkey",
                        column: x => x.province_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "branches_ward_id_fkey",
                        column: x => x.ward_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                },
                comment: "Chi nhánh / cửa hàng / kho / văn phòng thuộc công ty");

            migrationBuilder.CreateTable(
                name: "buckets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Provider"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên bucket thật trên storage"),
                    purpose = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Mục đích sử dụng"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cho phép đọc công khai"),
                    versioning = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Bật versioning trên storage"),
                    encryption = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'SSE'", comment: "Mã hóa phía server"),
                    retention_days = table.Column<int>(type: "int", nullable: true, comment: "Tự xóa file sau N ngày (lifecycle)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("buckets_pkey", x => x.id);
                    table.CheckConstraint("buckets_encryption_check", "encryption IN ('NONE','SSE','SSE_KMS')");
                },
                comment: "Bucket lưu trữ theo mục đích (public-assets, private-docs, hr-sensitive...)");

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant sở hữu"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty mẹ (NULL nếu là công ty cấp cao nhất)"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã công ty (dùng trong quy tắc sinh mã {COMPANY})"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên giao dịch"),
                    legal_name = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Tên pháp lý trên giấy ĐKKD"),
                    short_name = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Tên viết tắt"),
                    tax_code = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Mã số thuế"),
                    business_reg_no = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Số giấy chứng nhận ĐKKD"),
                    business_reg_date = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày cấp ĐKKD"),
                    legal_representative = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Người đại diện pháp luật"),
                    representative_title = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Chức danh người đại diện"),
                    address_line = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Địa chỉ chi tiết (số nhà, đường)"),
                    ward_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phường/Xã"),
                    province_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tỉnh/Thành phố"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, defaultValueSql: "'VN'", comment: "Quốc gia"),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Điện thoại"),
                    email = table.Column<string>(type: "citext", nullable: true, comment: "Email"),
                    website = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Website"),
                    logo_file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Logo (storage.files)"),
                    base_currency_code = table.Column<string>(type: "char(3)", nullable: false, defaultValueSql: "'VND'", comment: "Tiền tệ hạch toán"),
                    fiscal_year_start_month = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Tháng bắt đầu năm tài chính"),
                    work_calendar_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Lịch làm việc mặc định (mdm.work_calendars)"),
                    path = table.Column<string>(type: "text", nullable: true, comment: "Đường dẫn id từ gốc '/uuid1/uuid2/' để truy vấn cây"),
                    level = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Cấp trong cây công ty"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái hoạt động"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("companies_pkey", x => x.id);
                    table.CheckConstraint("companies_fiscal_year_start_month_check", "fiscal_year_start_month BETWEEN 1 AND 12");
                    table.CheckConstraint("companies_status_check", "status IN ('ACTIVE','INACTIVE')");
                    table.ForeignKey(
                        name: "companies_base_currency_code_fkey",
                        column: x => x.base_currency_code,
                        principalTable: "currencies",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "companies_country_code_fkey",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "companies_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "companies_province_id_fkey",
                        column: x => x.province_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "companies_ward_id_fkey",
                        column: x => x.ward_id,
                        principalTable: "administrative_units",
                        principalColumn: "id");
                },
                comment: "Công ty con / pháp nhân thuộc tenant. Hỗ trợ công ty mẹ - con nhiều cấp qua parent_id");

            migrationBuilder.CreateTable(
                name: "connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty sở hữu kết nối"),
                    provider = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Nhà cung cấp (VNPT_EINVOICE, VIETTEL_SINVOICE, MISA_MEINVOICE, SHOPEE, ZALO_OA, GHN, GHTK...)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên kết nối"),
                    auth_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Kiểu xác thực"),
                    base_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL API"),
                    config = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Cấu hình không bí mật (mẫu số HĐ, ký hiệu, shop_id...)"),
                    credentials_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Credential đã mã hóa (username/password/api key/chứng thư)"),
                    access_token_encrypted = table.Column<string>(type: "text", nullable: true, comment: "OAuth access token đã mã hóa"),
                    refresh_token_encrypted = table.Column<string>(type: "text", nullable: true, comment: "OAuth refresh token đã mã hóa"),
                    token_expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hạn access token (job tự refresh)"),
                    external_account_id = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Id tài khoản phía đối tác (shop_id, oa_id)"),
                    last_sync_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đồng bộ gần nhất"),
                    last_error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi gần nhất"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("connections_pkey", x => x.id);
                    table.CheckConstraint("connections_auth_type_check", "auth_type IN ('API_KEY','BASIC','OAUTH2','CERTIFICATE','CUSTOM')");
                    table.CheckConstraint("connections_status_check", "status IN ('PENDING','ACTIVE','EXPIRED','ERROR','DISABLED')");
                    table.ForeignKey(
                        name: "connections_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                },
                comment: "Kết nối tới hệ thống bên thứ 3 (lưu credential/OAuth token): sàn TMĐT, nhà cung cấp HĐĐT, ngân hàng, Zalo OA...");

            migrationBuilder.CreateTable(
                name: "external_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    connection_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Kết nối"),
                    entity_type = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Loại đối tượng nội bộ (PRODUCT, EMPLOYEE, CUSTOMER)"),
                    internal_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id nội bộ"),
                    external_system = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Hệ thống ngoài"),
                    external_id = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Id phía ngoài"),
                    external_data = table.Column<string>(type: "jsonb", nullable: true, comment: "Dữ liệu bổ sung phía ngoài"),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đồng bộ gần nhất"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Tạo lúc"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("external_mappings_pkey", x => x.id);
                    table.ForeignKey(
                        name: "external_mappings_connection_id_fkey",
                        column: x => x.connection_id,
                        principalTable: "connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Ánh xạ id nội bộ ↔ id hệ thống ngoài (sản phẩm ERP ↔ item_id Shopee, NV ↔ mã trên máy chấm công)");

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người hỏi"),
                    knowledge_base_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Kho tri thức (NULL = chat tự do)"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ mở chatbot"),
                    title = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Tiêu đề (tự sinh từ câu đầu)"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Lưu trữ"),
                    last_message_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Tin nhắn cuối"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Tạo lúc"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("conversations_pkey", x => x.id);
                },
                comment: "Hội thoại chatbot nội bộ");

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Hội thoại"),
                    role = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Vai trò"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "Nội dung"),
                    citations = table.Column<string>(type: "jsonb", nullable: true, comment: "Nguồn trích dẫn [{\"chunk_id\":\"...\",\"document_id\":\"...\",\"score\":0.82}]"),
                    usage_log_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Liên kết ai.usage_logs"),
                    input_tokens = table.Column<int>(type: "int", nullable: true, comment: "Token vào"),
                    output_tokens = table.Column<int>(type: "int", nullable: true, comment: "Token ra"),
                    feedback = table.Column<short>(type: "smallint", nullable: true, comment: "Đánh giá: 1 hữu ích, -1 không hữu ích"),
                    feedback_note = table.Column<string>(type: "text", nullable: true, comment: "Góp ý"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("messages_pkey", x => x.id);
                    table.CheckConstraint("messages_feedback_check", "feedback IN (-1, 1)");
                    table.CheckConstraint("messages_role_check", "role IN ('USER','ASSISTANT','SYSTEM','TOOL')");
                    table.ForeignKey(
                        name: "messages_conversation_id_fkey",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Tin nhắn trong hội thoại, kèm trích dẫn chunk nguồn và đánh giá của người dùng");

            migrationBuilder.CreateTable(
                name: "cors_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = mặc định)"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã chính sách"),
                    allowed_origins = table.Column<List<string>>(type: "text[]", nullable: false, comment: "Origin cho phép (https://admin.baseerp.vn); hỗ trợ wildcard subdomain"),
                    allowed_methods = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{GET,POST,PUT,PATCH,DELETE,OPTIONS}'", comment: "Method cho phép"),
                    allowed_headers = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{Authorization,Content-Type,X-Tenant-Id,X-Request-Id,Accept-Language}'", comment: "Header cho phép"),
                    exposed_headers = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{X-Trace-Id,X-RateLimit-Remaining,Content-Disposition}'", comment: "Header trình duyệt được đọc"),
                    allow_credentials = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Cho phép cookie/credentials"),
                    max_age_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "600", comment: "Cache preflight"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cors_policies_pkey", x => x.id);
                },
                comment: "Chính sách CORS (toàn hệ thống hoặc theo tenant khi tenant có domain riêng)");

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã route"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    cluster_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Cluster đích"),
                    path_pattern = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Mẫu path (/hrm/{**catch-all})"),
                    methods = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Method khớp (NULL = tất cả)"),
                    hosts = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Host khớp (NULL = tất cả)"),
                    path_transform = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Biến đổi path (bỏ tiền tố /hrm → /api)"),
                    order_no = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự khớp (nhỏ ưu tiên)"),
                    auth_mode = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'JWT'", comment: "Cách xác thực tại gateway: verify JWT cục bộ hoặc introspect với IdP"),
                    required_scopes = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Scope bắt buộc"),
                    required_permission = table.Column<string>(type: "varchar(200)", nullable: true, comment: "Quyền bắt buộc kiểm tra sớm tại gateway (tùy chọn)"),
                    rate_limit_policy_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Rate limit"),
                    cors_policy_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "CORS"),
                    security_header_policy_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Security header"),
                    timeout_ms = table.Column<int>(type: "int", nullable: true, comment: "Timeout riêng (ghi đè cluster)"),
                    max_request_body_mb = table.Column<int>(type: "int", nullable: true, comment: "Giới hạn kích thước body (upload)"),
                    retry_count = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Retry khi upstream lỗi (chỉ method idempotent)"),
                    cache_seconds = table.Column<int>(type: "int", nullable: true, comment: "Cache response tại gateway (danh mục MDM)"),
                    forward_headers = table.Column<string>(type: "jsonb", nullable: true, comment: "Header thêm khi chuyển tiếp (X-User-Id, X-Tenant-Id, X-Permissions)"),
                    wrap_response = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bọc response/lỗi theo định dạng chuẩn"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang bật"),
                    maintenance_message = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Thông báo bảo trì (khi có giá trị gateway trả 503)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("routes_pkey", x => x.id);
                    table.CheckConstraint("routes_auth_mode_check", "auth_mode IN ('ANONYMOUS','JWT','INTROSPECT','API_KEY','JWT_OR_API_KEY','MTLS')");
                    table.ForeignKey(
                        name: "routes_cluster_id_fkey",
                        column: x => x.cluster_id,
                        principalTable: "clusters",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "routes_cors_policy_id_fkey",
                        column: x => x.cors_policy_id,
                        principalTable: "cors_policies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "routes_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "routes_rate_limit_policy_id_fkey",
                        column: x => x.rate_limit_policy_id,
                        principalTable: "rate_limit_policies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "routes_security_header_policy_id_fkey",
                        column: x => x.security_header_policy_id,
                        principalTable: "security_header_policies",
                        principalColumn: "id");
                },
                comment: "Route định tuyến: /auth/*, /hrm/*, /wms/*, /crm/*, /ai/* → cluster nội bộ");

            migrationBuilder.CreateTable(
                name: "definition_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    definition_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Quy trình"),
                    version_no = table.Column<int>(type: "int", nullable: false, comment: "Số phiên bản"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'DRAFT'", comment: "Nháp, đã phát hành, lưu trữ"),
                    designer_json = table.Column<string>(type: "jsonb", nullable: true, comment: "Dữ liệu sơ đồ trên designer (vị trí node, cạnh) để vẽ lại UI"),
                    change_note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú thay đổi"),
                    published_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Phát hành lúc"),
                    published_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người phát hành"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("definition_versions_pkey", x => x.id);
                    table.CheckConstraint("definition_versions_status_check", "status IN ('DRAFT','PUBLISHED','ARCHIVED')");
                },
                comment: "Phiên bản quy trình. Instance đang chạy giữ nguyên phiên bản cũ khi admin sửa quy trình");

            migrationBuilder.CreateTable(
                name: "steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phiên bản"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã bước (trong phiên bản)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên bước (Trưởng bộ phận duyệt)"),
                    step_type = table.Column<string>(type: "varchar(15)", nullable: false, comment: "Loại node"),
                    approval_mode = table.Column<string>(type: "varchar(15)", nullable: true, comment: "Cách duyệt khi nhiều người: 1 người bất kỳ, tất cả, lần lượt, đủ tỷ lệ"),
                    quorum_percent = table.Column<short>(type: "smallint", nullable: true, comment: "Tỷ lệ % cần đồng ý (QUORUM)"),
                    sla_hours = table.Column<decimal>(type: "numeric(8,2)", nullable: true, comment: "Hạn xử lý (giờ)"),
                    sla_use_working_calendar = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Tính hạn theo giờ làm việc (mdm.work_calendars), bỏ qua lễ/CN"),
                    remind_before_hours = table.Column<decimal>(type: "numeric(8,2)", nullable: true, comment: "Nhắc trước hạn N giờ"),
                    remind_interval_hours = table.Column<decimal>(type: "numeric(8,2)", nullable: true, comment: "Nhắc lặp lại mỗi N giờ khi quá hạn"),
                    max_reminders = table.Column<short>(type: "smallint", nullable: true, defaultValueSql: "3", comment: "Số lần nhắc tối đa"),
                    timeout_action = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'REMIND'", comment: "Hành động khi quá hạn"),
                    escalate_to_type = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Loại người nhận khi leo thang (giống approver_type)"),
                    escalate_to_ref = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id người/vai trò nhận leo thang"),
                    allow_reject = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Cho phép từ chối"),
                    allow_return = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Cho phép trả lại bước trước/người tạo"),
                    allow_delegate = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Cho phép chuyển/ủy quyền"),
                    allow_add_approver = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cho phép xin ý kiến thêm người"),
                    require_comment_on_reject = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bắt buộc ghi lý do khi từ chối"),
                    require_signature = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Yêu cầu chữ ký (ảnh/số)"),
                    editable_fields = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Trường chứng từ người duyệt bước này được sửa"),
                    on_enter_actions = table.Column<string>(type: "jsonb", nullable: true, comment: "Hành động hệ thống khi vào bước (gọi webhook, phát event)"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("steps_pkey", x => x.id);
                    table.CheckConstraint("steps_approval_mode_check", "approval_mode IN ('ANY','ALL','SEQUENTIAL','QUORUM')");
                    table.CheckConstraint("steps_quorum_percent_check", "quorum_percent BETWEEN 1 AND 100");
                    table.CheckConstraint("steps_step_type_check", "step_type IN ('START','APPROVAL','CONDITION','PARALLEL_SPLIT','PARALLEL_JOIN','NOTIFY','SYSTEM','END')");
                    table.CheckConstraint("steps_timeout_action_check", "timeout_action IN ('NONE','REMIND','ESCALATE','AUTO_APPROVE','AUTO_REJECT')");
                    table.ForeignKey(
                        name: "steps_version_id_fkey",
                        column: x => x.version_id,
                        principalTable: "definition_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bước (node) trong quy trình: bắt đầu, duyệt, điều kiện, song song, thông báo, kết thúc");

            migrationBuilder.CreateTable(
                name: "step_approvers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    step_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Bước"),
                    approver_type = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Cách xác định người duyệt"),
                    approver_ref_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id user / role / phòng ban tương ứng"),
                    manager_level = table.Column<short>(type: "smallint", nullable: true, comment: "Cấp quản lý thứ N (MANAGER_LEVEL)"),
                    field_name = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Lấy người duyệt từ trường của chứng từ (FIELD_VALUE)"),
                    expression = table.Column<string>(type: "text", nullable: true, comment: "Biểu thức tùy biến (EXPRESSION)"),
                    scope_by = table.Column<string>(type: "varchar(20)", nullable: true, comment: "ROLE: lọc người có vai trò trong đơn vị của người tạo hay của chứng từ"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự khi duyệt SEQUENTIAL"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("step_approvers_pkey", x => x.id);
                    table.CheckConstraint("step_approvers_approver_type_check", "approver_type IN ('USER','ROLE','DIRECT_MANAGER','MANAGER_LEVEL','DEPARTMENT_MANAGER','BRANCH_MANAGER','COMPANY_DIRECTOR','REQUESTER','FIELD_VALUE','EXPRESSION')");
                    table.CheckConstraint("step_approvers_scope_by_check", "scope_by IN ('REQUESTER','DOCUMENT')");
                    table.ForeignKey(
                        name: "step_approvers_step_id_fkey",
                        column: x => x.step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Người duyệt của một bước: chỉ định cụ thể, theo vai trò, quản lý trực tiếp, trưởng phòng, cấp quản lý thứ N...");

            migrationBuilder.CreateTable(
                name: "transitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phiên bản"),
                    from_step_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Bước nguồn"),
                    to_step_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Bước đích"),
                    trigger_outcome = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'APPROVED'", comment: "Kết quả bước nguồn kích hoạt cạnh này"),
                    name = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Nhãn cạnh (Dưới 10 triệu)"),
                    condition_json = table.Column<string>(type: "jsonb", nullable: true, comment: "Điều kiện dạng cây JSON, VD: {\"all\":[{\"field\":\"amount\",\"op\":\"lt\",\"value\":10000000}]}"),
                    condition_expr = table.Column<string>(type: "text", nullable: true, comment: "Điều kiện dạng biểu thức (thay thế condition_json), VD: amount >= 10000000 && amount < 50000000"),
                    priority = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự xét điều kiện (nhỏ xét trước)"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Nhánh mặc định khi không điều kiện nào khớp"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("transitions_pkey", x => x.id);
                    table.CheckConstraint("transitions_trigger_outcome_check", "trigger_outcome IN ('APPROVED','REJECTED','RETURNED','ANY','TIMEOUT')");
                    table.ForeignKey(
                        name: "transitions_from_step_id_fkey",
                        column: x => x.from_step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "transitions_to_step_id_fkey",
                        column: x => x.to_step_id,
                        principalTable: "steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "transitions_version_id_fkey",
                        column: x => x.version_id,
                        principalTable: "definition_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Cạnh nối giữa các bước kèm điều kiện rẽ nhánh (VD: amount < 10000000 → Trưởng BP; >= 50000000 → Giám đốc)");

            migrationBuilder.CreateTable(
                name: "delegations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    delegator_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người ủy quyền"),
                    delegate_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người được ủy quyền"),
                    scope = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ALL'", comment: "Phạm vi ủy quyền"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ (scope = MODULE)"),
                    document_type_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Loại chứng từ (scope = DOCUMENT_TYPE)"),
                    definition_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Quy trình (scope = DEFINITION)"),
                    max_amount = table.Column<decimal>(type: "numeric(20,2)", nullable: true, comment: "Chỉ ủy quyền chứng từ có giá trị ≤ mức này"),
                    start_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Bắt đầu"),
                    end_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Kết thúc"),
                    reason = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Lý do"),
                    include_pending = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Chuyển cả các task đang chờ cho người được ủy quyền"),
                    notify_delegator = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Vẫn gửi thông báo cho người ủy quyền"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thu hồi lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("delegations_pkey", x => x.id);
                    table.CheckConstraint("ck_delegation_period", "end_at > start_at");
                    table.CheckConstraint("ck_delegation_self", "delegator_id <> delegate_id");
                    table.CheckConstraint("delegations_scope_check", "scope IN ('ALL','MODULE','DOCUMENT_TYPE','DEFINITION')");
                    table.CheckConstraint("delegations_status_check", "status IN ('ACTIVE','REVOKED','EXPIRED')");
                    table.ForeignKey(
                        name: "delegations_document_type_code_fkey",
                        column: x => x.document_type_code,
                        principalTable: "document_types",
                        principalColumn: "code");
                },
                comment: "Ủy quyền duyệt khi vắng mặt (đi công tác, nghỉ phép)");

            migrationBuilder.CreateTable(
                name: "deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Thông báo gốc"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người nhận (NULL nếu gửi tới email ngoài hệ thống)"),
                    channel = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Kênh"),
                    destination = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Địa chỉ đích: email, SĐT, device token id"),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Template đã dùng"),
                    config_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Cấu hình gửi (smtp_configs / push_configs)"),
                    subject = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Tiêu đề đã render"),
                    body = table.Column<string>(type: "text", nullable: true, comment: "Nội dung đã render"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PENDING'", comment: "Trạng thái gửi"),
                    attempts = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Số lần đã thử"),
                    max_attempts = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "5", comment: "Số lần thử tối đa"),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Lần thử kế tiếp (exponential backoff)"),
                    provider_message_id = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Message id phía provider (đối soát)"),
                    last_error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi gần nhất"),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Gửi thành công lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("deliveries_pkey", x => x.id);
                    table.CheckConstraint("deliveries_channel_check", "channel IN ('PUSH','EMAIL','SMS','ZALO','WEBHOOK')");
                    table.CheckConstraint("deliveries_status_check", "status IN ('PENDING','SENDING','SENT','DELIVERED','OPENED','FAILED','CANCELLED','SKIPPED')");
                },
                comment: "Hàng đợi/nhật ký gửi theo từng kênh (outbox): email, push, SMS... có retry");

            migrationBuilder.CreateTable(
                name: "delivery_attachments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    delivery_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Lượt gửi"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "File đính kèm"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("delivery_attachments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "delivery_attachments_delivery_id_fkey",
                        column: x => x.delivery_id,
                        principalTable: "deliveries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "File đính kèm email");

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Công ty"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chi nhánh (NULL nếu phòng ban cấp công ty)"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phòng ban cha"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã phòng ban"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên phòng ban"),
                    dept_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'DEPARTMENT'", comment: "Loại: khối, phòng, tổ, nhóm"),
                    manager_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Trưởng bộ phận (iam.users) — dùng cho workflow approver DEPARTMENT_MANAGER"),
                    deputy_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phó bộ phận"),
                    cost_center_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Mã trung tâm chi phí (đối soát kế toán)"),
                    path = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'/'", comment: "Đường dẫn id '/root/child/' — truy vấn cây con bằng LIKE path || '%'"),
                    level = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Cấp trong cây"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("departments_pkey", x => x.id);
                    table.CheckConstraint("departments_dept_type_check", "dept_type IN ('DIVISION','DEPARTMENT','TEAM','GROUP')");
                    table.CheckConstraint("departments_status_check", "status IN ('ACTIVE','INACTIVE')");
                    table.ForeignKey(
                        name: "departments_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "departments_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "departments_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "departments",
                        principalColumn: "id");
                },
                comment: "Phòng ban / bộ phận / tổ nhóm dạng cây. Dùng cho Data Scope DEPARTMENT / DEPARTMENT_TREE");

            migrationBuilder.CreateTable(
                name: "device_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Thiết bị"),
                    app_code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "App (HRM_MOBILE, WMS_MOBILE)"),
                    provider = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Nhà cung cấp"),
                    token = table.Column<string>(type: "text", nullable: false, comment: "Token push"),
                    locale = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Ngôn ngữ app trên thiết bị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn hiệu lực (FCM báo UNREGISTERED → false)"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần gửi thành công gần nhất"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Đăng ký lúc"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Cập nhật token lúc"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("device_tokens_pkey", x => x.id);
                    table.CheckConstraint("device_tokens_provider_check", "provider IN ('FCM','APNS','HMS','WEB_PUSH')");
                },
                comment: "Push token của thiết bị (FCM registration token / APNs device token)");

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = tỷ giá hệ thống đồng bộ tự động)"),
                    from_currency = table.Column<string>(type: "char(3)", nullable: false, comment: "Tiền tệ nguồn (USD)"),
                    to_currency = table.Column<string>(type: "char(3)", nullable: false, comment: "Tiền tệ đích (VND)"),
                    rate_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'TRANSFER'", comment: "Loại tỷ giá: mua TM, mua CK, bán, chuyển khoản, trung tâm NHNN, hạch toán"),
                    rate = table.Column<decimal>(type: "numeric(20,8)", nullable: false, comment: "Tỷ giá: 1 from_currency = rate to_currency"),
                    effective_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Thời điểm áp dụng"),
                    source = table.Column<string>(type: "varchar(30)", nullable: false, defaultValueSql: "'MANUAL'", comment: "Nguồn: VCB, SBV, MANUAL..."),
                    note = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Ghi chú"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("exchange_rates_pkey", x => x.id);
                    table.CheckConstraint("ck_exchange_rate_pair", "from_currency <> to_currency");
                    table.CheckConstraint("exchange_rates_rate_check", "rate > 0");
                    table.CheckConstraint("exchange_rates_rate_type_check", "rate_type IN ('BUY_CASH','BUY_TRANSFER','SELL','TRANSFER','CENTRAL','ACCOUNTING')");
                    table.ForeignKey(
                        name: "exchange_rates_from_currency_fkey",
                        column: x => x.from_currency,
                        principalTable: "currencies",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "exchange_rates_to_currency_fkey",
                        column: x => x.to_currency,
                        principalTable: "currencies",
                        principalColumn: "code");
                },
                comment: "Tỷ giá quy đổi theo thời điểm");

            migrationBuilder.CreateTable(
                name: "file_share_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "File"),
                    token_hash = table.Column<string>(type: "varchar(128)", nullable: false, comment: "Hash token trong URL chia sẻ"),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Mật khẩu mở link (tùy chọn)"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Hết hạn"),
                    max_downloads = table.Column<int>(type: "int", nullable: true, comment: "Số lượt tải tối đa"),
                    download_count = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Đã tải"),
                    allowed_emails = table.Column<List<string>>(type: "citext[]", nullable: true, comment: "Chỉ cho phép email này (tùy chọn)"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thu hồi lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo link"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("file_share_links_pkey", x => x.id);
                },
                comment: "Link chia sẻ file có kiểm soát (hết hạn, giới hạn lượt tải, mật khẩu) — khác presigned URL ngắn hạn của S3");

            migrationBuilder.CreateTable(
                name: "file_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "File gốc"),
                    variant_name = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Tên biến thể (THUMB, SMALL, MEDIUM, LARGE)"),
                    object_key = table.Column<string>(type: "varchar(1000)", nullable: false, comment: "Object key biến thể"),
                    mime_type = table.Column<string>(type: "varchar(150)", nullable: false, comment: "MIME type"),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false, comment: "Dung lượng"),
                    width = table.Column<int>(type: "int", nullable: true, comment: "Rộng (px)"),
                    height = table.Column<int>(type: "int", nullable: true, comment: "Cao (px)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("file_variants_pkey", x => x.id);
                },
                comment: "Biến thể của file ảnh sau xử lý (thumbnail, small, medium, webp)");

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (file_id dùng ở các phân hệ)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    bucket_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Bucket"),
                    object_key = table.Column<string>(type: "varchar(1000)", nullable: false, comment: "Đường dẫn object trên storage"),
                    original_name = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Tên file gốc người dùng upload"),
                    extension = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Đuôi file (.pdf)"),
                    mime_type = table.Column<string>(type: "varchar(150)", nullable: false, comment: "MIME type đã kiểm tra bằng magic bytes"),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false, comment: "Dung lượng (byte)"),
                    checksum_sha256 = table.Column<string>(type: "char(64)", nullable: true, comment: "SHA-256 phát hiện trùng/toàn vẹn"),
                    category = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "'ATTACHMENT'", comment: "Loại file (theo storage.upload_policies.category)"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ sở hữu (HRM, WMS...)"),
                    entity_type = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Loại đối tượng gắn (EMPLOYEE, LEAVE_REQUEST, GOODS_RECEIPT)"),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id đối tượng gắn"),
                    field_name = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Trường của đối tượng (avatar, contract_scan, attachments)"),
                    sensitivity = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'INTERNAL'", comment: "Mức nhạy cảm"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'TEMP'", comment: "TEMP: đã upload chưa gắn entity; ACTIVE: đã gắn; QUARANTINED: nghi nhiễm virus"),
                    scan_status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'PENDING'", comment: "Kết quả quét virus"),
                    width = table.Column<int>(type: "int", nullable: true, comment: "Chiều rộng ảnh (px)"),
                    height = table.Column<int>(type: "int", nullable: true, comment: "Chiều cao ảnh (px)"),
                    duration_seconds = table.Column<decimal>(type: "numeric(10,2)", nullable: true, comment: "Thời lượng (video/audio)"),
                    page_count = table.Column<int>(type: "int", nullable: true, comment: "Số trang (PDF)"),
                    version_no = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1", comment: "Phiên bản của tài liệu"),
                    previous_version_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phiên bản trước"),
                    upload_source = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'WEB'", comment: "Nguồn upload"),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "User upload"),
                    uploaded_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP upload"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "File TEMP hết hạn sẽ bị job dọn"),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Metadata mở rộng (EXIF đã lọc, tag...)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("files_pkey", x => x.id);
                    table.CheckConstraint("files_scan_status_check", "scan_status IN ('PENDING','CLEAN','INFECTED','SKIPPED','ERROR')");
                    table.CheckConstraint("files_sensitivity_check", "sensitivity IN ('PUBLIC','INTERNAL','CONFIDENTIAL','RESTRICTED')");
                    table.CheckConstraint("files_size_bytes_check", "size_bytes >= 0");
                    table.CheckConstraint("files_status_check", "status IN ('UPLOADING','TEMP','ACTIVE','QUARANTINED','DELETED')");
                    table.CheckConstraint("files_upload_source_check", "upload_source IN ('WEB','MOBILE','API','SYSTEM','IMPORT')");
                    table.ForeignKey(
                        name: "files_bucket_id_fkey",
                        column: x => x.bucket_id,
                        principalTable: "buckets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "files_previous_version_id_fkey",
                        column: x => x.previous_version_id,
                        principalTable: "files",
                        principalColumn: "id");
                },
                comment: "Metadata file đã upload. File vật lý nằm trên Object Storage; mọi phân hệ chỉ lưu file_id");

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã tenant duy nhất, dùng trong subdomain/đăng nhập (VD: remak)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên hiển thị"),
                    legal_name = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Tên pháp lý đầy đủ"),
                    tax_code = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Mã số thuế của tập đoàn/công ty mẹ"),
                    logo_file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Logo (storage.files)"),
                    primary_domain = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Domain chính (erp.congty.vn)"),
                    subdomain = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Subdomain trên nền tảng (remak.baseerp.vn)"),
                    contact_name = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Người liên hệ chính"),
                    contact_email = table.Column<string>(type: "citext", nullable: true, comment: "Email liên hệ"),
                    contact_phone = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Số điện thoại liên hệ"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, defaultValueSql: "'VN'", comment: "Quốc gia hoạt động chính"),
                    timezone = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "'Asia/Ho_Chi_Minh'", comment: "Múi giờ mặc định (IANA)"),
                    default_locale = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'vi-VN'", comment: "Ngôn ngữ mặc định"),
                    default_currency_code = table.Column<string>(type: "char(3)", nullable: false, defaultValueSql: "'VND'", comment: "Tiền tệ hạch toán mặc định"),
                    plan_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Gói dịch vụ (BASIC, PRO, ENTERPRISE)"),
                    max_users = table.Column<int>(type: "int", nullable: true, comment: "Số user tối đa theo gói; NULL = không giới hạn"),
                    max_storage_mb = table.Column<long>(type: "bigint", nullable: true, comment: "Dung lượng lưu trữ tối đa (MB)"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái tenant"),
                    trial_ends_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hết hạn dùng thử"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hết hạn hợp đồng/thuê bao"),
                    isolation_mode = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'SHARED'", comment: "Mô hình cô lập dữ liệu ở các phân hệ: dùng chung DB, schema riêng, DB riêng"),
                    connection_info = table.Column<string>(type: "jsonb", nullable: true, comment: "Thông tin kết nối DB riêng (đã mã hóa) khi isolation_mode = DATABASE"),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Thiết lập mở rộng (theme, màu thương hiệu...)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tenants_pkey", x => x.id);
                    table.CheckConstraint("tenants_isolation_mode_check", "isolation_mode IN ('SHARED','SCHEMA','DATABASE')");
                    table.CheckConstraint("tenants_status_check", "status IN ('TRIAL','ACTIVE','SUSPENDED','EXPIRED','CLOSED')");
                    table.ForeignKey(
                        name: "fk_tenants_logo",
                        column: x => x.logo_file_id,
                        principalTable: "files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tenants_country_code_fkey",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "tenants_default_currency_code_fkey",
                        column: x => x.default_currency_code,
                        principalTable: "currencies",
                        principalColumn: "code");
                },
                comment: "Tenant = Tập đoàn/khách hàng thuê hệ thống. Mọi dữ liệu nghiệp vụ đều gắn tenant_id để cô lập dữ liệu");

            migrationBuilder.CreateTable(
                name: "gateway_clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = dùng chung)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên client (hrm-api, crm-api)"),
                    key_prefix = table.Column<string>(type: "varchar(16)", nullable: false, comment: "Tiền tố key hiển thị"),
                    key_hash = table.Column<string>(type: "varchar(128)", nullable: false, comment: "Hash key nội bộ"),
                    allowed_aliases = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Alias model được phép dùng (NULL = tất cả)"),
                    requests_per_minute = table.Column<int>(type: "int", nullable: true, comment: "Rate limit RPM"),
                    tokens_per_minute = table.Column<int>(type: "int", nullable: true, comment: "Rate limit TPM"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Dùng gần nhất"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("gateway_clients_pkey", x => x.id);
                    table.CheckConstraint("gateway_clients_status_check", "status IN ('ACTIVE','DISABLED','REVOKED')");
                    table.ForeignKey(
                        name: "gateway_clients_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "gateway_clients_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Client nội bộ gọi AI Gateway (mỗi phân hệ/ứng dụng một client key) để định danh, rate limit, tính quota");

            migrationBuilder.CreateTable(
                name: "guardrail_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = hệ thống)"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã chính sách"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên"),
                    guard_type = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Loại guardrail"),
                    apply_on = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'INPUT'", comment: "Áp dụng cho đầu vào/đầu ra"),
                    config = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Cấu hình (regex CCCD/SĐT/STK, danh sách từ khóa...)"),
                    action = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'BLOCK'", comment: "Hành động khi vi phạm"),
                    is_global = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Áp dụng cho mọi request của tenant"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang bật"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("guardrail_policies_pkey", x => x.id);
                    table.CheckConstraint("guardrail_policies_action_check", "action IN ('BLOCK','MASK','WARN','LOG')");
                    table.CheckConstraint("guardrail_policies_apply_on_check", "apply_on IN ('INPUT','OUTPUT','BOTH')");
                    table.CheckConstraint("guardrail_policies_guard_type_check", "guard_type IN ('PII_MASK','BLOCKED_KEYWORDS','BLOCKED_TOPICS','PROMPT_INJECTION','OUTPUT_SCHEMA','MAX_INPUT_LENGTH','TOXICITY','CUSTOM')");
                    table.ForeignKey(
                        name: "guardrail_policies_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Chính sách guardrail: che PII, chặn chủ đề, phát hiện prompt injection, kiểm tra output");

            migrationBuilder.CreateTable(
                name: "holidays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = lễ quốc gia)"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false, defaultValueSql: "'VN'", comment: "Quốc gia"),
                    code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Mã ngày lễ (TET, NATIONAL_DAY, LABOR_DAY)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên ngày lễ"),
                    holiday_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Ngày dương lịch cụ thể (đã quy đổi nếu là âm lịch)"),
                    year = table.Column<short>(type: "smallint", nullable: true, computedColumnSql: "EXTRACT(YEAR FROM holiday_date)::smallint", stored: true, comment: "Năm (tính tự động để lọc nhanh)"),
                    is_lunar = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Gốc là ngày âm lịch (Tết, Giỗ Tổ)"),
                    lunar_day = table.Column<short>(type: "smallint", nullable: true, comment: "Ngày âm"),
                    lunar_month = table.Column<short>(type: "smallint", nullable: true, comment: "Tháng âm"),
                    holiday_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'PUBLIC'", comment: "Loại: lễ chính thức, nghỉ bù, nghỉ công ty, ngày kỷ niệm (không nghỉ)"),
                    is_day_off = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Có được nghỉ không"),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Nghỉ hưởng lương (HRM tính công)"),
                    pay_multiplier = table.Column<decimal>(type: "numeric(5,2)", nullable: true, defaultValueSql: "3.00", comment: "Hệ số lương nếu đi làm ngày này (BLLĐ: tối thiểu 300%)"),
                    legal_reference = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Văn bản quy định (thông báo của Bộ LĐ)"),
                    note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("holidays_pkey", x => x.id);
                    table.CheckConstraint("holidays_holiday_type_check", "holiday_type IN ('PUBLIC','COMPENSATORY','COMPANY','OBSERVANCE')");
                    table.ForeignKey(
                        name: "holidays_country_code_fkey",
                        column: x => x.country_code,
                        principalTable: "countries",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "holidays_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Ngày lễ quốc gia (tenant_id NULL) và ngày lễ/nghỉ riêng của tenant");

            migrationBuilder.CreateTable(
                name: "inbound_sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã nguồn (SEPAY_MAIN, SHOPEE_STORE1)"),
                    provider = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Nhà cung cấp"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên hiển thị"),
                    endpoint_token = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Token ngẫu nhiên trong URL nhận: /integration/inbound/{endpoint_token}"),
                    auth_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'HMAC'", comment: "Cách xác thực request đến"),
                    secret_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Secret/checksum key đã mã hóa"),
                    signature_header = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Header chứa chữ ký"),
                    signature_algorithm = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Thuật toán (HMAC_SHA256, HMAC_SHA512, MD5)"),
                    ip_whitelist = table.Column<List<IPNetwork>>(type: "cidr[]", nullable: true, comment: "IP nguồn được phép"),
                    target_event_type = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Event nội bộ phát ra sau khi xác thực (integration.event_types)"),
                    mapping_script = table.Column<string>(type: "text", nullable: true, comment: "Mapping payload ngoài → payload nội bộ (JSONata/Liquid)"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty nhận dữ liệu"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chi nhánh nhận dữ liệu (máy chấm công tại chi nhánh)"),
                    config = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Cấu hình riêng (số tài khoản nhận tiền, merchant id...)"),
                    last_received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần nhận gần nhất"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inbound_sources_pkey", x => x.id);
                    table.CheckConstraint("inbound_sources_auth_type_check", "auth_type IN ('NONE','HMAC','API_KEY','BASIC','CHECKSUM','JWT','IP_ONLY')");
                    table.CheckConstraint("inbound_sources_provider_check", "provider IN ('VNPAY','SEPAY','VIETQR','MOMO','PAYOS','SHOPEE','TIKTOK_SHOP','LAZADA','TIKI','ZALO_OA','ATTENDANCE_DEVICE','EINVOICE','BANK','CUSTOM')");
                    table.CheckConstraint("inbound_sources_status_check", "status IN ('ACTIVE','PAUSED','DISABLED')");
                    table.ForeignKey(
                        name: "inbound_sources_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "inbound_sources_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "inbound_sources_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Nguồn webhook INBOUND: cổng thanh toán (VNPay, SePay, VietQR), sàn TMĐT, máy chấm công, hóa đơn điện tử");

            migrationBuilder.CreateTable(
                name: "model_aliases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = alias hệ thống)"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã alias"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả mục đích"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("model_aliases_pkey", x => x.id);
                    table.ForeignKey(
                        name: "model_aliases_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Alias model logic (default-chat, cheap-summary, embedding-default) để đổi model không cần sửa code; hỗ trợ fallback");

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Loại sự kiện (NULL = thông báo thủ công/broadcast)"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ phát sinh"),
                    entity_type = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Đối tượng liên quan"),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Id đối tượng liên quan"),
                    priority = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'NORMAL'", comment: "Ưu tiên"),
                    payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Dữ liệu biến để render template"),
                    channels = table.Column<List<string>>(type: "text[]", nullable: false, comment: "Kênh sẽ gửi"),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hẹn giờ gửi (NULL = gửi ngay)"),
                    idempotency_key = table.Column<string>(type: "varchar(200)", nullable: true, comment: "Chống gửi trùng khi event bị phát lại"),
                    sender_user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người gửi (thông báo thủ công)"),
                    trace_id = table.Column<string>(type: "varchar(64)", nullable: true, comment: "Trace id liên kết request gốc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("notifications_pkey", x => x.id);
                    table.CheckConstraint("notifications_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
                    table.ForeignKey(
                        name: "notifications_event_type_id_fkey",
                        column: x => x.event_type_id,
                        principalTable: "notify_event_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "notifications_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Thông báo gốc phát sinh từ một sự kiện nghiệp vụ (một thông báo → nhiều người nhận, nhiều kênh)");

            migrationBuilder.CreateTable(
                name: "numbering_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    document_type_code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Loại chứng từ"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Áp dụng riêng công ty (NULL = toàn tenant)"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Áp dụng riêng chi nhánh (ưu tiên cao nhất)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên quy tắc"),
                    pattern = table.Column<string>(type: "varchar(200)", nullable: true, comment: "Mẫu đầy đủ, VD: 'PNK-{BRANCH}-{00001}', 'NV-{YYYY}-{0001}'. Nếu NULL thì ghép prefix + số + suffix"),
                    prefix = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Tiền tố (có thể chứa token)"),
                    suffix = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Hậu tố (có thể chứa token)"),
                    padding_length = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "4", comment: "Độ dài phần số tự tăng (khi không dùng pattern)"),
                    start_value = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "1", comment: "Giá trị bắt đầu"),
                    step = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1", comment: "Bước tăng"),
                    reset_policy = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'NEVER'", comment: "Chu kỳ reset số đếm"),
                    counter_scope = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'RULE'", comment: "Số đếm dùng chung theo quy tắc hay tách riêng từng công ty/chi nhánh"),
                    allow_manual = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cho phép người dùng tự nhập mã"),
                    preview = table.Column<string>(type: "varchar(200)", nullable: true, comment: "Ví dụ mã sinh ra (hiển thị UI)"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang áp dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("numbering_rules_pkey", x => x.id);
                    table.CheckConstraint("numbering_rules_counter_scope_check", "counter_scope IN ('RULE','COMPANY','BRANCH')");
                    table.CheckConstraint("numbering_rules_padding_length_check", "padding_length BETWEEN 1 AND 12");
                    table.CheckConstraint("numbering_rules_reset_policy_check", "reset_policy IN ('NEVER','YEARLY','MONTHLY','DAILY')");
                    table.CheckConstraint("numbering_rules_step_check", "step > 0");
                    table.ForeignKey(
                        name: "numbering_rules_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "numbering_rules_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "numbering_rules_document_type_code_fkey",
                        column: x => x.document_type_code,
                        principalTable: "document_types",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "numbering_rules_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Quy tắc sinh mã tự động theo loại chứng từ, có thể riêng theo công ty/chi nhánh. Pattern hỗ trợ token {YYYY} {YY} {MM} {DD} {YYMM} {BRANCH} {COMPANY} {0001}");

            migrationBuilder.CreateTable(
                name: "oauth_clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (NULL = client hệ thống dùng chung)"),
                    client_id = table.Column<string>(type: "varchar(100)", nullable: false, comment: "client_id công khai"),
                    client_secret_hash = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Hash client_secret (NULL với public client như SPA/Mobile dùng PKCE)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên ứng dụng"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ gắn với client (HRM Mobile → HRM)"),
                    client_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Public (SPA, mobile) hay Confidential (backend)"),
                    app_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Loại ứng dụng"),
                    grant_types = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{authorization_code,refresh_token}'", comment: "Grant cho phép: authorization_code, refresh_token, client_credentials, password"),
                    redirect_uris = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'", comment: "Danh sách redirect URI hợp lệ"),
                    post_logout_redirect_uris = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'", comment: "URI chuyển hướng sau đăng xuất"),
                    allowed_origins = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'", comment: "Origin CORS cho client"),
                    allowed_scopes = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{openid,profile,offline_access}'", comment: "Scope cho phép (iam.api_scopes.code)"),
                    require_pkce = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bắt buộc PKCE"),
                    require_consent = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Hiển thị màn hình đồng ý (ứng dụng bên thứ 3)"),
                    access_token_minutes = table.Column<int>(type: "int", nullable: true, comment: "Ghi đè thời hạn access token"),
                    refresh_token_days = table.Column<int>(type: "int", nullable: true, comment: "Ghi đè thời hạn refresh token"),
                    rotate_refresh_token = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bật Refresh Token Rotation"),
                    ip_whitelist = table.Column<List<IPNetwork>>(type: "cidr[]", nullable: true, comment: "IP được phép dùng client_credentials"),
                    rate_limit_policy_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Chính sách rate limit áp dụng (gateway.rate_limit_policies.code)"),
                    logo_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Logo hiển thị trên trang đồng ý"),
                    secret_expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hạn của client_secret"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần dùng gần nhất"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("oauth_clients_pkey", x => x.id);
                    table.CheckConstraint("oauth_clients_app_type_check", "app_type IN ('WEB','SPA','MOBILE','SERVICE','PARTNER')");
                    table.CheckConstraint("oauth_clients_client_type_check", "client_type IN ('PUBLIC','CONFIDENTIAL')");
                    table.CheckConstraint("oauth_clients_status_check", "status IN ('ACTIVE','DISABLED','REVOKED')");
                    table.ForeignKey(
                        name: "oauth_clients_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "oauth_clients_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Ứng dụng client (OAuth2/OIDC): Core Admin, HRM Web, HRM Mobile, WMS Mobile, service nội bộ, ứng dụng đối tác");

            migrationBuilder.CreateTable(
                name: "provider_credentials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = key hệ thống)"),
                    provider_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nhà cung cấp"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên key"),
                    api_key_encrypted = table.Column<string>(type: "text", nullable: false, comment: "API key đã mã hóa"),
                    base_url_override = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL riêng (Azure endpoint, proxy)"),
                    organization_id = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Org/Project id phía provider"),
                    priority = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự dùng khi có nhiều key (cân bằng tải/failover)"),
                    monthly_budget_usd = table.Column<decimal>(type: "numeric(12,2)", nullable: true, comment: "Ngân sách tháng cho key"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Dùng gần nhất"),
                    last_error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi gần nhất (401, 429...)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("provider_credentials_pkey", x => x.id);
                    table.CheckConstraint("provider_credentials_status_check", "status IN ('ACTIVE','DISABLED','EXHAUSTED','INVALID')");
                    table.ForeignKey(
                        name: "provider_credentials_provider_id_fkey",
                        column: x => x.provider_id,
                        principalTable: "ai_providers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "provider_credentials_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "API key của nhà cung cấp AI — lưu tập trung, các service con không giữ key. Tenant có thể dùng key riêng (BYOK)");

            migrationBuilder.CreateTable(
                name: "push_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = app hệ thống; khác NULL khi build app white-label)"),
                    app_code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã app (HRM_MOBILE, WMS_MOBILE)"),
                    provider = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Nhà cung cấp"),
                    project_id = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Firebase project id"),
                    bundle_id = table.Column<string>(type: "varchar(255)", nullable: true, comment: "iOS bundle id / Android package name"),
                    credentials_encrypted = table.Column<string>(type: "text", nullable: false, comment: "Service account JSON (FCM) hoặc .p8 key (APNs) đã mã hóa"),
                    apns_key_id = table.Column<string>(type: "varchar(50)", nullable: true, comment: "APNs Key ID"),
                    apns_team_id = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Apple Team ID"),
                    is_production = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Môi trường production/sandbox"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("push_configs_pkey", x => x.id);
                    table.CheckConstraint("push_configs_provider_check", "provider IN ('FCM','APNS','HMS')");
                    table.ForeignKey(
                        name: "push_configs_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Cấu hình nhà cung cấp push theo ứng dụng mobile (FCM, APNs)");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = vai trò mẫu hệ thống)"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Vai trò riêng của một công ty (NULL = dùng toàn tenant)"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã vai trò (HR_MANAGER, WAREHOUSE_KEEPER)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên vai trò"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Vai trò hệ thống (Super Admin, Tenant Admin) — không cho sửa/xóa"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Tự gán cho user mới"),
                    priority = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Độ ưu tiên khi hợp nhất data scope (số lớn ưu tiên)"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                    table.CheckConstraint("roles_status_check", "status IN ('ACTIVE','INACTIVE')");
                    table.ForeignKey(
                        name: "roles_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "roles_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Vai trò. tenant_id NULL = vai trò mẫu toàn hệ thống (template) để tenant sao chép");

            migrationBuilder.CreateTable(
                name: "security_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = chính sách mặc định)"),
                    password_min_length = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "8", comment: "Độ dài tối thiểu"),
                    password_require_upper = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bắt buộc chữ hoa"),
                    password_require_lower = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bắt buộc chữ thường"),
                    password_require_digit = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bắt buộc chữ số"),
                    password_require_special = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Bắt buộc ký tự đặc biệt"),
                    password_expire_days = table.Column<int>(type: "int", nullable: true, comment: "Số ngày hết hạn mật khẩu (NULL = không hết hạn)"),
                    password_history_count = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "3", comment: "Không được dùng lại N mật khẩu gần nhất"),
                    max_failed_attempts = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "5", comment: "Số lần sai tối đa trước khi khóa"),
                    lockout_minutes = table.Column<int>(type: "int", nullable: false, defaultValueSql: "15", comment: "Thời gian khóa tạm (phút)"),
                    force_change_on_first_login = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Ép đổi mật khẩu lần đầu"),
                    require_mfa = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Bắt buộc 2FA cho mọi user"),
                    require_mfa_for_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bắt buộc 2FA cho admin"),
                    max_devices_per_user = table.Column<int>(type: "int", nullable: false, defaultValueSql: "3", comment: "Số thiết bị đăng nhập đồng thời tối đa"),
                    device_limit_action = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'REVOKE_OLDEST'", comment: "Khi vượt giới hạn: đá thiết bị cũ nhất hoặc chặn đăng nhập mới"),
                    access_token_minutes = table.Column<int>(type: "int", nullable: false, defaultValueSql: "15", comment: "Thời hạn access token (phút)"),
                    refresh_token_days = table.Column<int>(type: "int", nullable: false, defaultValueSql: "30", comment: "Thời hạn refresh token (ngày)"),
                    session_idle_minutes = table.Column<int>(type: "int", nullable: true, comment: "Tự đăng xuất khi không hoạt động (phút)"),
                    allowed_ip_ranges = table.Column<List<IPNetwork>>(type: "cidr[]", nullable: true, comment: "Chỉ cho phép đăng nhập từ dải IP này (NULL = không giới hạn)"),
                    allowed_login_hours = table.Column<string>(type: "jsonb", nullable: true, comment: "Khung giờ cho phép đăng nhập theo thứ, VD: {\"mon\":[\"07:00\",\"19:00\"]}"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("security_policies_pkey", x => x.id);
                    table.CheckConstraint("security_policies_device_limit_action_check", "device_limit_action IN ('REVOKE_OLDEST','BLOCK_NEW')");
                    table.ForeignKey(
                        name: "security_policies_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Chính sách bảo mật tài khoản theo tenant (NULL = mặc định toàn hệ thống)");

            migrationBuilder.CreateTable(
                name: "smtp_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = SMTP hệ thống)"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty dùng SMTP riêng"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên cấu hình"),
                    provider = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'SMTP'", comment: "Cách gửi"),
                    host = table.Column<string>(type: "varchar(255)", nullable: true, comment: "SMTP host"),
                    port = table.Column<int>(type: "int", nullable: true, comment: "SMTP port (587, 465)"),
                    security = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'STARTTLS'", comment: "Mã hóa kết nối"),
                    username = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Tài khoản"),
                    password_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Mật khẩu/API key đã mã hóa"),
                    from_email = table.Column<string>(type: "citext", nullable: false, comment: "Email người gửi"),
                    from_name = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Tên người gửi"),
                    reply_to = table.Column<string>(type: "citext", nullable: true, comment: "Reply-To"),
                    rate_limit_per_minute = table.Column<int>(type: "int", nullable: true, comment: "Giới hạn số email/phút"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Mặc định trong phạm vi"),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần test gửi thành công"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("smtp_configs_pkey", x => x.id);
                    table.CheckConstraint("smtp_configs_provider_check", "provider IN ('SMTP','SES','SENDGRID','MAILGUN','GMAIL_API','M365')");
                    table.CheckConstraint("smtp_configs_security_check", "security IN ('NONE','SSL','STARTTLS')");
                    table.ForeignKey(
                        name: "smtp_configs_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "smtp_configs_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Cấu hình SMTP theo hệ thống, tenant hoặc từng công ty");

            migrationBuilder.CreateTable(
                name: "storage_providers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = storage hệ thống)"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã cấu hình"),
                    provider_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Loại provider"),
                    endpoint = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Endpoint (https://minio.internal:9000)"),
                    public_base_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "URL công khai/CDN cho file public"),
                    region = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Region"),
                    access_key_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Access key đã mã hóa"),
                    secret_key_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Secret key đã mã hóa"),
                    use_path_style = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Path-style URL (MinIO cần true)"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Provider mặc định"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("storage_providers_pkey", x => x.id);
                    table.CheckConstraint("storage_providers_provider_type_check", "provider_type IN ('MINIO','S3','R2','GCS','AZURE_BLOB','LOCAL')");
                    table.ForeignKey(
                        name: "storage_providers_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Cấu hình nhà cung cấp Object Storage (MinIO, S3, R2...). Hỗ trợ tenant dùng storage riêng");

            migrationBuilder.CreateTable(
                name: "tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = biểu thuế chuẩn)"),
                    tax_type_code = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Loại thuế"),
                    code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mã thuế suất (VAT10, VAT8, VAT5, VAT0, KCT, KKKNT)"),
                    name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Tên hiển thị (Thuế GTGT 10%)"),
                    rate = table.Column<decimal>(type: "numeric(7,4)", nullable: true, comment: "Thuế suất % (NULL với KCT/KKKNT không tính thuế)"),
                    calc_method = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'PERCENT'", comment: "Cách tính: theo %, không chịu thuế, không kê khai, miễn thuế"),
                    einvoice_code = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Mã thuế suất trên hóa đơn điện tử (10%, 8%, KCT, KKKNT)"),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false, comment: "Hiệu lực từ"),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true, comment: "Hiệu lực đến (NULL = còn hiệu lực)"),
                    legal_reference = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Văn bản pháp lý"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Mặc định khi tạo chứng từ"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tax_rates_pkey", x => x.id);
                    table.CheckConstraint("ck_tax_rate_period", "effective_to IS NULL OR effective_to >= effective_from");
                    table.CheckConstraint("tax_rates_calc_method_check", "calc_method IN ('PERCENT','NOT_SUBJECT','NOT_DECLARED','EXEMPT')");
                    table.CheckConstraint("tax_rates_rate_check", "rate >= 0");
                    table.ForeignKey(
                        name: "tax_rates_tax_type_code_fkey",
                        column: x => x.tax_type_code,
                        principalTable: "tax_types",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "tax_rates_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Biểu thuế suất có hiệu lực theo thời gian (VD: VAT giảm 8% theo từng nghị định)");

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = mẫu hệ thống)"),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Sự kiện"),
                    channel = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Kênh"),
                    locale = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'vi-VN'", comment: "Ngôn ngữ"),
                    subject = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Tiêu đề (email) / title (push, in-app)"),
                    body = table.Column<string>(type: "text", nullable: false, comment: "Nội dung có biến {{...}}"),
                    body_format = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'TEXT'", comment: "Định dạng nội dung"),
                    action_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Link mở khi bấm (có biến, VD: /hrm/leave-requests/{{id}})"),
                    deep_link = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Deep link mobile app"),
                    external_template_id = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Mã template phía nhà cung cấp (Zalo ZNS, SMS brandname)"),
                    version = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1", comment: "Phiên bản"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("templates_pkey", x => x.id);
                    table.CheckConstraint("templates_body_format_check", "body_format IN ('TEXT','HTML','MARKDOWN')");
                    table.CheckConstraint("templates_channel_check", "channel IN ('IN_APP','PUSH','EMAIL','SMS','ZALO')");
                    table.ForeignKey(
                        name: "templates_event_type_id_fkey",
                        column: x => x.event_type_id,
                        principalTable: "notify_event_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "templates_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Mẫu nội dung theo sự kiện × kênh × ngôn ngữ, dùng biến {{employee_name}}. Tenant có thể ghi đè mẫu hệ thống");

            migrationBuilder.CreateTable(
                name: "tenant_modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phân hệ"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang bật"),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Ngày bắt đầu sử dụng"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Ngày hết hạn (NULL = vô thời hạn)"),
                    max_users = table.Column<int>(type: "int", nullable: true, comment: "Giới hạn số user dùng phân hệ này"),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Cấu hình riêng phân hệ cho tenant"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tenant_modules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "tenant_modules_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tenant_modules_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Phân hệ được kích hoạt cho từng tenant (gói thuê bao). User chỉ vào được phân hệ tenant đã mua");

            migrationBuilder.CreateTable(
                name: "tenant_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Job"),
                    cron_expression = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Cron riêng"),
                    parameters = table.Column<string>(type: "jsonb", nullable: true, comment: "Tham số riêng (merge với mặc định)"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bật/tắt cho tenant"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tenant_overrides_pkey", x => x.id);
                    table.ForeignKey(
                        name: "tenant_overrides_job_id_fkey",
                        column: x => x.job_id,
                        principalTable: "job_definitions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tenant_overrides_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Ghi đè lịch/tham số job theo tenant (VD: tenant A chốt công lúc 23:00, tenant B lúc 01:00)");

            migrationBuilder.CreateTable(
                name: "tenant_rate_limits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Chính sách gốc"),
                    permit_limit = table.Column<int>(type: "int", nullable: false, comment: "Hạn mức riêng"),
                    window_seconds = table.Column<int>(type: "int", nullable: true, comment: "Cửa sổ riêng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tenant_rate_limits_pkey", x => x.id);
                    table.ForeignKey(
                        name: "tenant_rate_limits_policy_id_fkey",
                        column: x => x.policy_id,
                        principalTable: "rate_limit_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tenant_rate_limits_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Ghi đè hạn mức rate limit cho tenant cụ thể (gói cao cấp được nhiều request hơn)");

            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant ghi đè (NULL = bản dịch chuẩn)"),
                    key_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Khóa dịch"),
                    language_code = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Ngôn ngữ"),
                    value = table.Column<string>(type: "text", nullable: false, comment: "Nội dung dịch (hỗ trợ ICU {count, plural, ...})"),
                    is_reviewed = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Đã duyệt"),
                    is_machine = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Dịch máy (AI)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("translations_pkey", x => x.id);
                    table.ForeignKey(
                        name: "translations_key_id_fkey",
                        column: x => x.key_id,
                        principalTable: "translation_keys",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "translations_language_code_fkey",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "translations_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Bản dịch theo ngôn ngữ; tenant có thể ghi đè thuật ngữ riêng (VD: \"Nhân viên\" → \"Cộng sự\")");

            migrationBuilder.CreateTable(
                name: "uoms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = ĐVT chuẩn hệ thống)"),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nhóm ĐVT"),
                    code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mã ĐVT (PCS, BOX, CTN, KG, L)"),
                    name = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Tên hiển thị (Cái, Hộp, Thùng)"),
                    symbol = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Ký hiệu (kg, l, m²)"),
                    is_base = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Là đơn vị cơ sở của nhóm"),
                    ratio_to_base = table.Column<decimal>(type: "numeric(20,8)", nullable: false, defaultValueSql: "1", comment: "1 đơn vị này = ratio_to_base đơn vị cơ sở (1 tấn = 1000 kg). Với nhóm COUNT, quy đổi theo sản phẩm do WMS quản lý"),
                    decimal_places = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Số lẻ cho phép khi nhập số lượng"),
                    einvoice_name = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Tên ĐVT in trên hóa đơn điện tử"),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Thứ tự hiển thị"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("uoms_pkey", x => x.id);
                    table.CheckConstraint("uoms_ratio_to_base_check", "ratio_to_base > 0");
                    table.ForeignKey(
                        name: "uoms_category_id_fkey",
                        column: x => x.category_id,
                        principalTable: "uom_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "uoms_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Đơn vị tính (Cái, Hộp, Thùng, Kg, Lít...). Quy đổi trong cùng nhóm qua ratio_to_base");

            migrationBuilder.CreateTable(
                name: "upload_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = mặc định)"),
                    category = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Loại file (AVATAR, LABOR_CONTRACT, PAYSLIP, INVOICE, PRODUCT_IMAGE, ATTACHMENT, KB_DOCUMENT)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ sử dụng"),
                    bucket_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Bucket lưu"),
                    path_template = table.Column<string>(type: "varchar(255)", nullable: false, defaultValueSql: "'{tenant}/{module}/{yyyy}/{mm}/{uuid}{ext}'", comment: "Mẫu object key"),
                    max_size_mb = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValueSql: "10", comment: "Dung lượng tối đa/file (MB)"),
                    allowed_mime_types = table.Column<List<string>>(type: "text[]", nullable: true, comment: "MIME cho phép (NULL = tất cả trừ blacklist)"),
                    allowed_extensions = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Đuôi file cho phép"),
                    sensitivity = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'INTERNAL'", comment: "Mức nhạy cảm: RESTRICTED (bảng lương, HĐLĐ) bắt buộc presigned URL ngắn hạn + ghi log truy cập"),
                    presigned_ttl_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "300", comment: "Thời hạn link tạm (giây)"),
                    auto_resize = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Tự resize ảnh"),
                    max_width = table.Column<int>(type: "int", nullable: true, comment: "Chiều rộng tối đa sau resize (px)"),
                    max_height = table.Column<int>(type: "int", nullable: true, comment: "Chiều cao tối đa (px)"),
                    image_quality = table.Column<short>(type: "smallint", nullable: true, defaultValueSql: "80", comment: "Chất lượng nén ảnh"),
                    convert_to_format = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Chuyển định dạng ảnh"),
                    variants = table.Column<string>(type: "jsonb", nullable: true, comment: "Biến thể cần sinh, VD: [{\"name\":\"THUMB\",\"w\":150,\"h\":150,\"fit\":\"cover\"}]"),
                    strip_exif = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Xóa EXIF (GPS) khi upload từ mobile"),
                    virus_scan = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Quét virus"),
                    temp_ttl_hours = table.Column<int>(type: "int", nullable: false, defaultValueSql: "24", comment: "File tạm (chưa gắn entity) bị dọn sau N giờ"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("upload_policies_pkey", x => x.id);
                    table.CheckConstraint("upload_policies_convert_to_format_check", "convert_to_format IN ('WEBP','JPEG','PNG','AVIF')");
                    table.CheckConstraint("upload_policies_image_quality_check", "image_quality BETWEEN 1 AND 100");
                    table.CheckConstraint("upload_policies_sensitivity_check", "sensitivity IN ('PUBLIC','INTERNAL','CONFIDENTIAL','RESTRICTED')");
                    table.ForeignKey(
                        name: "upload_policies_bucket_id_fkey",
                        column: x => x.bucket_id,
                        principalTable: "buckets",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "upload_policies_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "upload_policies_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Chính sách upload theo loại file: dung lượng, định dạng, nén/resize ảnh, bucket đích, mức nhạy cảm");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (sub trong JWT)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = Platform Admin quản trị toàn nền tảng)"),
                    username = table.Column<string>(type: "citext", nullable: false, comment: "Tên đăng nhập (duy nhất trong tenant)"),
                    email = table.Column<string>(type: "citext", nullable: true, comment: "Email (đăng nhập, khôi phục mật khẩu)"),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xác thực email"),
                    phone = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Số điện thoại (định dạng E.164 khuyến nghị)"),
                    phone_verified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xác thực SĐT"),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Hash mật khẩu (Argon2id/BCrypt/PBKDF2). NULL nếu chỉ đăng nhập SSO ngoài"),
                    password_changed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần đổi mật khẩu gần nhất (tính hết hạn mật khẩu)"),
                    must_change_password = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Ép đổi mật khẩu ở lần đăng nhập kế tiếp (lần đầu / sau reset)"),
                    security_stamp = table.Column<string>(type: "varchar(64)", nullable: false, defaultValueSql: "encode(gen_random_bytes(16), 'hex')", comment: "Đổi giá trị khi đổi mật khẩu/quyền/khóa → vô hiệu toàn bộ token cũ"),
                    full_name = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Họ tên đầy đủ"),
                    display_name = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Tên hiển thị"),
                    avatar_file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ảnh đại diện (storage.files)"),
                    gender = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Giới tính (phục vụ xưng hô trong thông báo)"),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true, comment: "Ngày sinh"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Liên kết hồ sơ nhân viên bên HRM (không FK vì khác service)"),
                    employee_code = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Mã nhân viên (denormalize để hiển thị)"),
                    default_company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty mặc định khi đăng nhập"),
                    default_branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chi nhánh mặc định"),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phòng ban chính (OWN/DEPARTMENT scope)"),
                    direct_manager_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Quản lý trực tiếp (workflow approver DIRECT_MANAGER)"),
                    job_title = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Chức danh hiển thị"),
                    locale = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Ngôn ngữ ưu tiên (NULL = theo tenant)"),
                    timezone = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Múi giờ ưu tiên"),
                    user_type = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'INTERNAL'", comment: "Loại: nhân viên, đối tác, khách hàng portal, tài khoản dịch vụ"),
                    is_tenant_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Quản trị viên tenant (bỏ qua RBAC trong tenant)"),
                    is_platform_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Quản trị viên nền tảng (đa tenant)"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái: chờ kích hoạt, hoạt động, bị khóa, vô hiệu"),
                    locked_reason = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Lý do khóa"),
                    locked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm khóa"),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Khóa tạm đến thời điểm (do nhập sai mật khẩu); NULL = khóa vô thời hạn"),
                    locked_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người khóa (NULL = hệ thống tự khóa)"),
                    failed_login_count = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số lần đăng nhập sai liên tiếp"),
                    last_failed_login_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần đăng nhập sai gần nhất"),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần đăng nhập thành công gần nhất"),
                    last_login_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP đăng nhập gần nhất"),
                    mfa_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Bật xác thực 2 lớp"),
                    mfa_type = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Phương thức 2FA"),
                    mfa_secret_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Secret TOTP đã mã hóa"),
                    max_devices = table.Column<int>(type: "int", nullable: true, comment: "Số thiết bị đăng nhập đồng thời tối đa (NULL = theo chính sách tenant)"),
                    activated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm kích hoạt tài khoản"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.CheckConstraint("users_gender_check", "gender IN ('MALE','FEMALE','OTHER')");
                    table.CheckConstraint("users_mfa_type_check", "mfa_type IN ('TOTP','EMAIL_OTP','SMS_OTP')");
                    table.CheckConstraint("users_status_check", "status IN ('PENDING','ACTIVE','LOCKED','DISABLED')");
                    table.CheckConstraint("users_user_type_check", "user_type IN ('INTERNAL','PARTNER','CUSTOMER','SERVICE')");
                    table.ForeignKey(
                        name: "fk_users_avatar",
                        column: x => x.avatar_file_id,
                        principalTable: "files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "users_default_branch_id_fkey",
                        column: x => x.default_branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "users_default_company_id_fkey",
                        column: x => x.default_company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "users_department_id_fkey",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "users_direct_manager_id_fkey",
                        column: x => x.direct_manager_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "users_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Tài khoản người dùng dùng chung SSO cho mọi phân hệ (Web & Mobile)");

            migrationBuilder.CreateTable(
                name: "webhook_endpoints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên endpoint"),
                    url = table.Column<string>(type: "varchar(1000)", nullable: false, comment: "URL nhận"),
                    http_method = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'POST'", comment: "Phương thức"),
                    event_types = table.Column<List<string>>(type: "text[]", nullable: false, comment: "Event đăng ký (integration.event_types.code), hỗ trợ wildcard 'crm.order.*'"),
                    secret_encrypted = table.Column<string>(type: "text", nullable: true, comment: "Secret ký HMAC-SHA256 (header X-Signature)"),
                    custom_headers = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Header bổ sung (Authorization...) — giá trị nhạy cảm nên mã hóa"),
                    content_type = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "'application/json'", comment: "Content-Type"),
                    timeout_seconds = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "10", comment: "Timeout mỗi lần gọi"),
                    max_retries = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "8", comment: "Số lần retry tối đa"),
                    retry_strategy = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'EXPONENTIAL'", comment: "Chiến lược retry"),
                    retry_base_seconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "30", comment: "Khoảng chờ cơ sở giữa các lần retry"),
                    filter_condition = table.Column<string>(type: "jsonb", nullable: true, comment: "Chỉ gửi khi payload thỏa điều kiện"),
                    consecutive_failures = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số lần lỗi liên tiếp"),
                    auto_disable_after = table.Column<int>(type: "int", nullable: false, defaultValueSql: "50", comment: "Tự tắt sau N lần lỗi liên tiếp"),
                    last_success_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần gửi thành công gần nhất"),
                    last_failure_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần lỗi gần nhất"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("webhook_endpoints_pkey", x => x.id);
                    table.CheckConstraint("webhook_endpoints_http_method_check", "http_method IN ('POST','PUT','PATCH')");
                    table.CheckConstraint("webhook_endpoints_retry_strategy_check", "retry_strategy IN ('FIXED','EXPONENTIAL')");
                    table.CheckConstraint("webhook_endpoints_status_check", "status IN ('ACTIVE','PAUSED','DISABLED')");
                    table.ForeignKey(
                        name: "webhook_endpoints_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Endpoint webhook OUTBOUND: ERP bắn sự kiện ra hệ thống bên thứ 3");

            migrationBuilder.CreateTable(
                name: "work_calendars",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty áp dụng (NULL = toàn tenant)"),
                    code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Mã lịch (HC_T2_T6, HC_T2_T7_SANG)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên lịch"),
                    timezone = table.Column<string>(type: "varchar(50)", nullable: false, defaultValueSql: "'Asia/Ho_Chi_Minh'", comment: "Múi giờ"),
                    standard_hours_per_day = table.Column<decimal>(type: "numeric(4,2)", nullable: false, defaultValueSql: "8", comment: "Số giờ công chuẩn/ngày"),
                    apply_public_holidays = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Tự áp dụng lễ quốc gia từ mdm.holidays"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Lịch mặc định của tenant"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("work_calendars_pkey", x => x.id);
                    table.CheckConstraint("work_calendars_status_check", "status IN ('ACTIVE','INACTIVE')");
                    table.ForeignKey(
                        name: "work_calendars_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "work_calendars_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Lịch làm việc (ca hành chính chuẩn) dùng chung: HRM tính công, WMS tính hạn giao, CRM hẹn lịch chăm sóc");

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    document_type_code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Loại chứng từ áp dụng"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã quy trình"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên quy trình"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Áp dụng riêng công ty (NULL = toàn tenant)"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Áp dụng riêng chi nhánh"),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Áp dụng riêng phòng ban"),
                    applicable_condition = table.Column<string>(type: "jsonb", nullable: true, comment: "Điều kiện chọn quy trình khi có nhiều quy trình cùng loại chứng từ"),
                    priority = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Ưu tiên khi nhiều quy trình cùng khớp (số lớn ưu tiên)"),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phiên bản đang áp dụng (workflow.definition_versions)"),
                    allow_recall = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Người tạo được thu hồi khi chưa có ai duyệt"),
                    allow_edit_while_pending = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cho sửa chứng từ khi đang chờ duyệt"),
                    auto_approve_if_requester_is_approver = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Tự bỏ qua bước nếu người duyệt chính là người tạo"),
                    skip_duplicate_approver = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Một người xuất hiện ở nhiều bước chỉ cần duyệt một lần"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang áp dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("workflow_definitions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_wf_def_current_version",
                        column: x => x.current_version_id,
                        principalTable: "definition_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "workflow_definitions_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "workflow_definitions_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "workflow_definitions_department_id_fkey",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "workflow_definitions_document_type_code_fkey",
                        column: x => x.document_type_code,
                        principalTable: "document_types",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "workflow_definitions_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Định nghĩa quy trình duyệt cho một loại chứng từ (đơn nghỉ phép, đề xuất mua hàng, chiết khấu vượt khung...)");

            migrationBuilder.CreateTable(
                name: "inbound_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Nguồn"),
                    external_id = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Id giao dịch phía ngoài (mã GD ngân hàng, order_sn) để chống trùng"),
                    http_method = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Phương thức"),
                    headers = table.Column<string>(type: "jsonb", nullable: true, comment: "Header nhận được"),
                    query_string = table.Column<string>(type: "text", nullable: true, comment: "Query string (VNPay IPN dùng query)"),
                    raw_body = table.Column<string>(type: "text", nullable: true, comment: "Body gốc (giữ nguyên để verify chữ ký)"),
                    parsed_payload = table.Column<string>(type: "jsonb", nullable: true, comment: "Payload đã parse/mapping"),
                    source_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP gửi"),
                    signature_valid = table.Column<bool>(type: "boolean", nullable: true, comment: "Kết quả kiểm tra chữ ký"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'RECEIVED'", comment: "Trạng thái xử lý"),
                    response_status = table.Column<short>(type: "smallint", nullable: true, comment: "HTTP status đã trả cho bên gửi"),
                    response_body = table.Column<string>(type: "text", nullable: true, comment: "Body đã trả (VNPay yêu cầu RspCode)"),
                    emitted_message_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Event nội bộ đã phát (outbox_messages.id)"),
                    error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi xử lý"),
                    retry_count = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Số lần xử lý lại"),
                    received_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Nhận lúc"),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Xử lý xong lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("inbound_requests_pkey", x => x.id);
                    table.CheckConstraint("inbound_requests_status_check", "status IN ('RECEIVED','PROCESSING','PROCESSED','FAILED','DUPLICATE','REJECTED')");
                    table.ForeignKey(
                        name: "inbound_requests_source_id_fkey",
                        column: x => x.source_id,
                        principalTable: "inbound_sources",
                        principalColumn: "id");
                },
                comment: "Nhật ký request inbound nhận được (lưu raw để đối soát, xử lý lại, chống trùng)");

            migrationBuilder.CreateTable(
                name: "numbering_counters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Quy tắc"),
                    period_key = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Khóa kỳ: 'ALL', '2026', '202611', '20261121'"),
                    scope_key = table.Column<string>(type: "varchar(100)", nullable: false, defaultValueSql: "'-'", comment: "Khóa phạm vi: '-' hoặc id công ty/chi nhánh"),
                    current_value = table.Column<long>(type: "bigint", nullable: false, comment: "Giá trị đã cấp gần nhất"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Lần cấp gần nhất"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("numbering_counters_pkey", x => x.id);
                    table.ForeignKey(
                        name: "numbering_counters_rule_id_fkey",
                        column: x => x.rule_id,
                        principalTable: "numbering_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Bộ đếm hiện tại của từng quy tắc theo kỳ (năm/tháng/ngày) và phạm vi (công ty/chi nhánh)");

            migrationBuilder.CreateTable(
                name: "model_alias_targets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    alias_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Alias"),
                    model_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Model"),
                    credential_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ép dùng key này (NULL = tự chọn)"),
                    priority = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "1 = chính, 2,3 = fallback khi lỗi/429"),
                    weight = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "100", comment: "Trọng số chia tải khi cùng priority"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("model_alias_targets_pkey", x => x.id);
                    table.ForeignKey(
                        name: "model_alias_targets_alias_id_fkey",
                        column: x => x.alias_id,
                        principalTable: "model_aliases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "model_alias_targets_credential_id_fkey",
                        column: x => x.credential_id,
                        principalTable: "provider_credentials",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "model_alias_targets_model_id_fkey",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "id");
                },
                comment: "Danh sách model đích của alias theo thứ tự ưu tiên (model chính → fallback)");

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Vai trò"),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Quyền"),
                    data_scope = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'OWN'", comment: "Phạm vi: cá nhân, phòng ban, phòng ban + cấp dưới, chi nhánh, công ty, toàn tenant, tùy chọn"),
                    conditions = table.Column<string>(type: "jsonb", nullable: true, comment: "Điều kiện ABAC bổ sung (VD: {\"amount_lte\": 50000000})"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permissions_pkey", x => x.id);
                    table.CheckConstraint("role_permissions_data_scope_check", "data_scope IN ('OWN','DEPARTMENT','DEPARTMENT_TREE','BRANCH','COMPANY','ALL','CUSTOM')");
                    table.ForeignKey(
                        name: "role_permissions_permission_id_fkey",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Gán quyền cho vai trò kèm phạm vi dữ liệu (Data Scope) trên từng quyền");

            migrationBuilder.CreateTable(
                name: "uom_conversions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = chuẩn hệ thống)"),
                    from_uom_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Đơn vị nguồn"),
                    to_uom_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Đơn vị đích"),
                    factor = table.Column<decimal>(type: "numeric(20,8)", nullable: false, comment: "1 from = factor to"),
                    rounding_mode = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'HALF_UP'", comment: "Kiểu làm tròn"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("uom_conversions_pkey", x => x.id);
                    table.CheckConstraint("ck_uom_conversion_diff", "from_uom_id <> to_uom_id");
                    table.CheckConstraint("uom_conversions_factor_check", "factor > 0");
                    table.CheckConstraint("uom_conversions_rounding_mode_check", "rounding_mode IN ('HALF_UP','UP','DOWN','NONE')");
                    table.ForeignKey(
                        name: "uom_conversions_from_uom_id_fkey",
                        column: x => x.from_uom_id,
                        principalTable: "uoms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "uom_conversions_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "uom_conversions_to_uom_id_fkey",
                        column: x => x.to_uom_id,
                        principalTable: "uoms",
                        principalColumn: "id");
                },
                comment: "Bảng quy đổi ĐVT tổng quát giữa hai đơn vị (kể cả khác nhóm/không tỷ lệ cố định). Quy đổi riêng theo sản phẩm (1 thùng = 24 lon) đặt ở WMS");

            migrationBuilder.CreateTable(
                name: "password_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    password_hash = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Hash mật khẩu cũ"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm đặt mật khẩu này"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("password_histories_pkey", x => x.id);
                    table.ForeignKey(
                        name: "password_histories_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Lịch sử mật khẩu để chặn dùng lại N mật khẩu gần nhất");

            migrationBuilder.CreateTable(
                name: "quotas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ (NULL = toàn tenant)"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "User (NULL = không giới hạn theo user)"),
                    period = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Chu kỳ"),
                    max_requests = table.Column<int>(type: "int", nullable: true, comment: "Số request tối đa"),
                    max_tokens = table.Column<long>(type: "bigint", nullable: true, comment: "Tổng token tối đa"),
                    max_cost_usd = table.Column<decimal>(type: "numeric(12,4)", nullable: true, comment: "Chi phí tối đa (USD)"),
                    alert_threshold_pct = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "80", comment: "Ngưỡng % gửi cảnh báo"),
                    action_on_exceed = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'BLOCK'", comment: "Khi vượt: chặn, chỉ cảnh báo, chuyển model rẻ hơn"),
                    downgrade_alias_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Alias rẻ hơn khi DOWNGRADE"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Đang áp dụng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quotas_pkey", x => x.id);
                    table.CheckConstraint("quotas_action_on_exceed_check", "action_on_exceed IN ('BLOCK','ALERT_ONLY','DOWNGRADE')");
                    table.CheckConstraint("quotas_alert_threshold_pct_check", "alert_threshold_pct BETWEEN 1 AND 100");
                    table.CheckConstraint("quotas_period_check", "period IN ('DAILY','MONTHLY')");
                    table.ForeignKey(
                        name: "quotas_downgrade_alias_id_fkey",
                        column: x => x.downgrade_alias_id,
                        principalTable: "model_aliases",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotas_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotas_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "quotas_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                },
                comment: "Hạn mức sử dụng AI theo tenant / phân hệ / user theo ngày hoặc tháng");

            migrationBuilder.CreateTable(
                name: "user_devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    device_key = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Định danh thiết bị do client sinh và lưu (fingerprint/installation id)"),
                    device_name = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Tên thiết bị (iPhone 15 của Hoàng, Chrome - Windows)"),
                    platform = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Nền tảng"),
                    os_version = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Phiên bản hệ điều hành"),
                    app_version = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Phiên bản app"),
                    user_agent = table.Column<string>(type: "text", nullable: true, comment: "User-Agent gần nhất"),
                    last_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP gần nhất"),
                    is_trusted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Thiết bị tin cậy (bỏ qua 2FA)"),
                    first_seen_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Lần đầu thấy"),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Lần cuối hoạt động"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm thu hồi"),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người thu hồi"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_devices_pkey", x => x.id);
                    table.CheckConstraint("user_devices_platform_check", "platform IN ('WEB','ANDROID','IOS','DESKTOP','OTHER')");
                    table.CheckConstraint("user_devices_status_check", "status IN ('ACTIVE','REVOKED','BLOCKED')");
                    table.ForeignKey(
                        name: "user_devices_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Thiết bị đã đăng nhập của user (giới hạn thiết bị, thu hồi theo thiết bị)");

            migrationBuilder.CreateTable(
                name: "user_external_logins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    provider = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Nhà cung cấp định danh"),
                    provider_key = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Id user phía provider (sub)"),
                    provider_email = table.Column<string>(type: "citext", nullable: true, comment: "Email phía provider"),
                    linked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm liên kết"),
                    last_used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần dùng gần nhất"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_external_logins_pkey", x => x.id);
                    table.CheckConstraint("user_external_logins_provider_check", "provider IN ('GOOGLE','MICROSOFT','APPLE','ZALO','LDAP','SAML','OIDC')");
                    table.ForeignKey(
                        name: "user_external_logins_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Liên kết đăng nhập ngoài (Google, Microsoft, Zalo, Apple) với tài khoản nội bộ");

            migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (denormalize)"),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Thông báo gốc"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người nhận"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: true, comment: "Phân hệ (lọc theo tab)"),
                    title = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Tiêu đề đã render"),
                    body = table.Column<string>(type: "text", nullable: true, comment: "Nội dung đã render"),
                    icon = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Icon/ảnh đại diện người gửi"),
                    action_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Link khi bấm"),
                    category = table.Column<string>(type: "varchar(30)", nullable: false, defaultValueSql: "'INFO'", comment: "Nhóm hiển thị"),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Đã đọc"),
                    read_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đọc lúc"),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Ghim"),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Đã lưu trữ/ẩn"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm nhận"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_notifications_pkey", x => x.id);
                    table.CheckConstraint("user_notifications_category_check", "category IN ('INFO','TASK','APPROVAL','WARNING','SYSTEM')");
                    table.ForeignKey(
                        name: "user_notifications_notification_id_fkey",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_notifications_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Hộp thư thông báo in-app của từng user (thanh chuông). Đã render sẵn nội dung theo ngôn ngữ user");

            migrationBuilder.CreateTable(
                name: "user_org_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Công ty"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chi nhánh"),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phòng ban"),
                    job_title = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Chức danh tại đơn vị này"),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Đơn vị chính"),
                    is_manager = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Là quản lý của đơn vị này"),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "current_date", comment: "Hiệu lực từ ngày"),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true, comment: "Hiệu lực đến ngày (NULL = vô thời hạn)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_org_assignments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_org_assignments_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_org_assignments_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_org_assignments_department_id_fkey",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_org_assignments_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Gán user vào nhiều công ty/chi nhánh/phòng ban (kiêm nhiệm). Dùng để tính Data Scope và chọn ngữ cảnh làm việc");

            migrationBuilder.CreateTable(
                name: "user_permission_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    permission_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Quyền"),
                    effect = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'GRANT'", comment: "GRANT = cấp thêm, DENY = chặn (ưu tiên cao nhất)"),
                    data_scope = table.Column<string>(type: "varchar(20)", nullable: true, comment: "Phạm vi dữ liệu khi GRANT"),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hết hạn"),
                    reason = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Lý do"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_permission_overrides_pkey", x => x.id);
                    table.CheckConstraint("user_permission_overrides_data_scope_check", "data_scope IN ('OWN','DEPARTMENT','DEPARTMENT_TREE','BRANCH','COMPANY','ALL')");
                    table.CheckConstraint("user_permission_overrides_effect_check", "effect IN ('GRANT','DENY')");
                    table.ForeignKey(
                        name: "user_permission_overrides_permission_id_fkey",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_permission_overrides_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Quyền gán trực tiếp cho user (cộng thêm hoặc loại trừ so với vai trò)");

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    event_type_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Sự kiện (NULL = áp dụng mọi sự kiện)"),
                    channel = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Kênh"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bật/tắt"),
                    quiet_start = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Bắt đầu giờ yên lặng (không push)"),
                    quiet_end = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Kết thúc giờ yên lặng"),
                    digest_mode = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'INSTANT'", comment: "Gửi ngay hay gom tóm tắt"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Cập nhật lúc"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_preferences_pkey", x => x.id);
                    table.CheckConstraint("user_preferences_channel_check", "channel IN ('IN_APP','PUSH','EMAIL','SMS','ZALO')");
                    table.CheckConstraint("user_preferences_digest_mode_check", "digest_mode IN ('INSTANT','HOURLY','DAILY')");
                    table.ForeignKey(
                        name: "user_preferences_event_type_id_fkey",
                        column: x => x.event_type_id,
                        principalTable: "notify_event_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_preferences_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Tùy chọn nhận thông báo của user theo sự kiện × kênh, giờ yên lặng");

            migrationBuilder.CreateTable(
                name: "user_recovery_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    code_hash = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Hash mã khôi phục"),
                    used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm đã dùng (NULL = chưa dùng)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_recovery_codes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_recovery_codes_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Mã khôi phục 2FA dùng một lần");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Vai trò"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Vai trò chỉ có hiệu lực trong công ty này (NULL = mọi công ty)"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Vai trò chỉ có hiệu lực trong chi nhánh này"),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Hiệu lực từ"),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hiệu lực đến (gán quyền tạm thời)"),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cấp quyền"),
                    note = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Ghi chú lý do cấp"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_roles_pkey", x => x.id);
                    table.ForeignKey(
                        name: "user_roles_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_roles_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_roles_role_id_fkey",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_roles_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Gán vai trò cho user, có thể giới hạn vai trò trong một công ty/chi nhánh và có thời hạn");

            migrationBuilder.CreateTable(
                name: "verification_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    purpose = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Mục đích"),
                    token_hash = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Hash của token/OTP (không lưu bản rõ)"),
                    target = table.Column<string>(type: "varchar(255)", nullable: true, comment: "Email/SĐT nhận token (khi đổi email)"),
                    attempts = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số lần nhập sai OTP"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Hết hạn"),
                    used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đã dùng lúc"),
                    created_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP yêu cầu"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("verification_tokens_pkey", x => x.id);
                    table.CheckConstraint("verification_tokens_purpose_check", "purpose IN ('RESET_PASSWORD','VERIFY_EMAIL','VERIFY_PHONE','INVITATION','LOGIN_OTP','CHANGE_EMAIL')");
                    table.ForeignKey(
                        name: "verification_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Token một lần: quên mật khẩu, xác thực email/SĐT, lời mời kích hoạt, OTP đăng nhập");

            migrationBuilder.CreateTable(
                name: "webhook_deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    endpoint_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Endpoint"),
                    message_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Event gốc (outbox_messages.id)"),
                    event_type_code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã event"),
                    request_url = table.Column<string>(type: "varchar(1000)", nullable: false, comment: "URL thực tế"),
                    request_headers = table.Column<string>(type: "jsonb", nullable: true, comment: "Header đã gửi (đã che secret)"),
                    request_body = table.Column<string>(type: "jsonb", nullable: false, comment: "Payload đã gửi"),
                    attempt_no = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Lần thử thứ"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PENDING'", comment: "Trạng thái"),
                    response_status = table.Column<short>(type: "smallint", nullable: true, comment: "HTTP status trả về"),
                    response_body = table.Column<string>(type: "text", nullable: true, comment: "Body trả về (cắt tối đa vài KB)"),
                    duration_ms = table.Column<int>(type: "int", nullable: true, comment: "Thời gian phản hồi"),
                    error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi mạng/timeout"),
                    next_retry_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Lần retry tiếp"),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hoàn tất lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Tạo lúc"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("webhook_deliveries_pkey", x => x.id);
                    table.CheckConstraint("webhook_deliveries_status_check", "status IN ('PENDING','SUCCESS','FAILED','RETRYING','DEAD')");
                    table.ForeignKey(
                        name: "webhook_deliveries_endpoint_id_fkey",
                        column: x => x.endpoint_id,
                        principalTable: "webhook_endpoints",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Nhật ký từng lần gửi webhook outbound (payload, response, retry)");

            migrationBuilder.CreateTable(
                name: "work_calendar_exceptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    calendar_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Lịch"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chỉ áp dụng cho chi nhánh này (NULL = toàn lịch)"),
                    exception_date = table.Column<DateOnly>(type: "date", nullable: false, comment: "Ngày ngoại lệ"),
                    exception_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Loại: nghỉ, làm bù, nửa ngày, giờ đặc biệt"),
                    holiday_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Liên kết ngày lễ (nếu do lễ)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên/lý do"),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Nghỉ có lương"),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Giờ bắt đầu (CUSTOM_HOURS)"),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Giờ kết thúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("work_calendar_exceptions_pkey", x => x.id);
                    table.CheckConstraint("work_calendar_exceptions_exception_type_check", "exception_type IN ('DAY_OFF','MAKEUP_WORKDAY','HALF_DAY','CUSTOM_HOURS')");
                    table.ForeignKey(
                        name: "work_calendar_exceptions_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "work_calendar_exceptions_calendar_id_fkey",
                        column: x => x.calendar_id,
                        principalTable: "work_calendars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "work_calendar_exceptions_holiday_id_fkey",
                        column: x => x.holiday_id,
                        principalTable: "holidays",
                        principalColumn: "id");
                },
                comment: "Ngoại lệ của lịch: nghỉ riêng công ty, ngày làm bù, ngày đổi ca... áp dụng cho cả lịch hoặc riêng chi nhánh");

            migrationBuilder.CreateTable(
                name: "work_calendar_weekdays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    calendar_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Lịch"),
                    day_of_week = table.Column<short>(type: "smallint", nullable: false, comment: "Thứ theo ISO: 1 = Thứ Hai ... 7 = Chủ Nhật"),
                    is_working_day = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Có làm việc không"),
                    work_ratio = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValueSql: "1", comment: "Tỷ lệ công (1 = cả ngày, 0.5 = nửa ngày T7)"),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Giờ bắt đầu"),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Giờ kết thúc"),
                    break_start = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Bắt đầu nghỉ trưa"),
                    break_end = table.Column<TimeOnly>(type: "time", nullable: true, comment: "Kết thúc nghỉ trưa"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("work_calendar_weekdays_pkey", x => x.id);
                    table.CheckConstraint("work_calendar_weekdays_day_of_week_check", "day_of_week BETWEEN 1 AND 7");
                    table.CheckConstraint("work_calendar_weekdays_work_ratio_check", "work_ratio BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "work_calendar_weekdays_calendar_id_fkey",
                        column: x => x.calendar_id,
                        principalTable: "work_calendars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Mẫu ngày làm việc trong tuần của lịch (T2..CN, giờ vào/ra, nghỉ trưa)");

            migrationBuilder.CreateTable(
                name: "instances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    definition_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Quy trình"),
                    version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phiên bản đã dùng"),
                    document_type_code = table.Column<string>(type: "varchar(50)", nullable: false, comment: "Loại chứng từ"),
                    module_code = table.Column<string>(type: "varchar(30)", nullable: false, comment: "Phân hệ"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id chứng từ bên phân hệ"),
                    document_code = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Mã chứng từ hiển thị (DXMH-2611-0012)"),
                    title = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Tiêu đề hiển thị trong danh sách chờ duyệt"),
                    summary = table.Column<string>(type: "text", nullable: true, comment: "Mô tả ngắn"),
                    variables = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Ảnh chụp biến dùng rẽ nhánh lúc gửi duyệt (amount, leave_days...)"),
                    document_url = table.Column<string>(type: "varchar(500)", nullable: true, comment: "Link mở chứng từ"),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người gửi duyệt"),
                    on_behalf_of_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Gửi thay cho ai (nếu có)"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Công ty của chứng từ"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Chi nhánh"),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phòng ban"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PENDING'", comment: "Trạng thái tổng"),
                    current_step_names = table.Column<string>(type: "text", nullable: true, comment: "Tên bước hiện tại (denormalize hiển thị)"),
                    priority = table.Column<string>(type: "varchar(10)", nullable: false, defaultValueSql: "'NORMAL'", comment: "Độ khẩn"),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Gửi duyệt lúc"),
                    due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hạn hoàn thành toàn quy trình"),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Kết thúc lúc"),
                    final_comment = table.Column<string>(type: "text", nullable: true, comment: "Ý kiến cuối cùng"),
                    callback_event = table.Column<string>(type: "varchar(150)", nullable: true, comment: "Event phát ra khi kết thúc để phân hệ cập nhật chứng từ"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("instances_pkey", x => x.id);
                    table.CheckConstraint("instances_priority_check", "priority IN ('LOW','NORMAL','HIGH','URGENT')");
                    table.CheckConstraint("instances_status_check", "status IN ('DRAFT','PENDING','APPROVED','REJECTED','RETURNED','CANCELLED','RECALLED','ERROR')");
                    table.ForeignKey(
                        name: "instances_branch_id_fkey",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_company_id_fkey",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_definition_id_fkey",
                        column: x => x.definition_id,
                        principalTable: "workflow_definitions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_department_id_fkey",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_document_type_code_fkey",
                        column: x => x.document_type_code,
                        principalTable: "document_types",
                        principalColumn: "code");
                    table.ForeignKey(
                        name: "instances_on_behalf_of_id_fkey",
                        column: x => x.on_behalf_of_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_requester_id_fkey",
                        column: x => x.requester_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "instances_version_id_fkey",
                        column: x => x.version_id,
                        principalTable: "definition_versions",
                        principalColumn: "id");
                },
                comment: "Một lượt chạy quy trình cho một chứng từ cụ thể");

            migrationBuilder.CreateTable(
                name: "role_permission_scopes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    role_permission_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Quyền của vai trò"),
                    scope_type = table.Column<string>(type: "varchar(20)", nullable: false, comment: "Loại đơn vị"),
                    scope_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id công ty / chi nhánh / phòng ban"),
                    include_children = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Bao gồm đơn vị con"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("role_permission_scopes_pkey", x => x.id);
                    table.CheckConstraint("role_permission_scopes_scope_type_check", "scope_type IN ('COMPANY','BRANCH','DEPARTMENT')");
                    table.ForeignKey(
                        name: "role_permission_scopes_role_permission_id_fkey",
                        column: x => x.role_permission_id,
                        principalTable: "role_permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Danh sách đơn vị cụ thể khi data_scope = CUSTOM (VD: chỉ xem chi nhánh HN01 và HN02)");

            migrationBuilder.CreateTable(
                name: "user_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Session id (claim sid trong token)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Thiết bị"),
                    client_id = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Ứng dụng khởi tạo phiên (iam.oauth_clients.client_id)"),
                    auth_method = table.Column<string>(type: "varchar(30)", nullable: false, defaultValueSql: "'PASSWORD'", comment: "Phương thức xác thực"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ngữ cảnh công ty đang chọn"),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ngữ cảnh chi nhánh đang chọn"),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP khởi tạo"),
                    user_agent = table.Column<string>(type: "text", nullable: true, comment: "User-Agent"),
                    impersonator_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Admin đang đăng nhập thay (hỗ trợ khách hàng)"),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Bắt đầu"),
                    last_activity_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Hoạt động cuối (tính idle timeout)"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Hết hạn tuyệt đối"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thu hồi lúc"),
                    revoked_reason = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Lý do thu hồi"),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người thu hồi"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_sessions_pkey", x => x.id);
                    table.CheckConstraint("user_sessions_auth_method_check", "auth_method IN ('PASSWORD','PASSWORD_MFA','EXTERNAL','OTP','IMPERSONATION','API')");
                    table.CheckConstraint("user_sessions_revoked_reason_check", "revoked_reason IN ('LOGOUT','ADMIN_REVOKE','PASSWORD_CHANGED','ACCOUNT_LOCKED','DEVICE_LIMIT','TOKEN_REUSE','EXPIRED','ROLE_CHANGED')");
                    table.ForeignKey(
                        name: "user_sessions_device_id_fkey",
                        column: x => x.device_id,
                        principalTable: "user_devices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_sessions_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Phiên đăng nhập SSO (sid). Một phiên trên một thiết bị/client; thu hồi phiên → thu hồi toàn bộ refresh token");

            migrationBuilder.CreateTable(
                name: "instance_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Instance"),
                    step_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Bước định nghĩa"),
                    iteration = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Lần chạy thứ mấy của bước (tăng khi bị trả lại)"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'IN_PROGRESS'", comment: "Trạng thái bước"),
                    outcome = table.Column<string>(type: "varchar(15)", nullable: true, comment: "Kết quả để chọn transition"),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Bắt đầu"),
                    due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hạn xử lý (đã tính theo lịch làm việc)"),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hoàn thành"),
                    escalated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đã leo thang lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("instance_steps_pkey", x => x.id);
                    table.CheckConstraint("instance_steps_status_check", "status IN ('WAITING','IN_PROGRESS','APPROVED','REJECTED','RETURNED','SKIPPED','TIMEOUT','CANCELLED')");
                    table.ForeignKey(
                        name: "instance_steps_instance_id_fkey",
                        column: x => x.instance_id,
                        principalTable: "instances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "instance_steps_step_id_fkey",
                        column: x => x.step_id,
                        principalTable: "steps",
                        principalColumn: "id");
                },
                comment: "Lượt thực thi của từng bước trong instance (một bước có thể lặp lại khi bị trả về)");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phiên"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "User"),
                    client_id = table.Column<string>(type: "varchar(100)", nullable: true, comment: "Client được cấp"),
                    token_hash = table.Column<string>(type: "varchar(128)", nullable: false, comment: "SHA-256 của refresh token (không lưu bản rõ)"),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Họ token: chung cho chuỗi rotation từ một lần đăng nhập"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Token trước đó trong chuỗi rotation"),
                    replaced_by_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Token mới thay thế (khi đã rotate)"),
                    scopes = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Scope được cấp"),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm cấp"),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, comment: "Hết hạn"),
                    used_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đã dùng để rotate lúc"),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thu hồi lúc"),
                    revoked_reason = table.Column<string>(type: "varchar(50)", nullable: true, comment: "Lý do thu hồi"),
                    created_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP yêu cầu cấp"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_tokens_pkey", x => x.id);
                    table.ForeignKey(
                        name: "refresh_tokens_parent_id_fkey",
                        column: x => x.parent_id,
                        principalTable: "refresh_tokens",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "refresh_tokens_session_id_fkey",
                        column: x => x.session_id,
                        principalTable: "user_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "refresh_tokens_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Refresh token có rotation: mỗi lần refresh sinh token mới cùng family; dùng lại token cũ → thu hồi cả family (phát hiện đánh cắp)");

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Instance"),
                    instance_step_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Lượt bước"),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Người đang phải xử lý"),
                    original_assignee_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người được giao gốc (khác assignee khi ủy quyền/chuyển)"),
                    delegation_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ủy quyền đã áp dụng (workflow.delegations)"),
                    task_type = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'APPROVE'", comment: "Duyệt, xin ý kiến, chỉ xem/biết"),
                    sequence_no = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "1", comment: "Thứ tự khi duyệt tuần tự"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PENDING'", comment: "Trạng thái"),
                    comment = table.Column<string>(type: "text", nullable: true, comment: "Ý kiến khi xử lý"),
                    signature_file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Ảnh chữ ký"),
                    due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Hạn xử lý"),
                    reminder_count = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "0", comment: "Số lần đã nhắc"),
                    last_reminded_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Nhắc gần nhất"),
                    read_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Đã xem lúc"),
                    acted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Xử lý lúc"),
                    acted_ip = table.Column<IPAddress>(type: "inet", nullable: true, comment: "IP khi xử lý"),
                    acted_via = table.Column<string>(type: "varchar(10)", nullable: true, comment: "Kênh xử lý"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Giao lúc"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tasks_pkey", x => x.id);
                    table.CheckConstraint("tasks_acted_via_check", "acted_via IN ('WEB','MOBILE','EMAIL','API','SYSTEM')");
                    table.CheckConstraint("tasks_status_check", "status IN ('WAITING','PENDING','APPROVED','REJECTED','RETURNED','DELEGATED','REASSIGNED','CANCELLED','EXPIRED','SKIPPED')");
                    table.CheckConstraint("tasks_task_type_check", "task_type IN ('APPROVE','CONSULT','ACKNOWLEDGE')");
                    table.ForeignKey(
                        name: "fk_wf_tasks_delegation",
                        column: x => x.delegation_id,
                        principalTable: "delegations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tasks_assignee_id_fkey",
                        column: x => x.assignee_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tasks_instance_id_fkey",
                        column: x => x.instance_id,
                        principalTable: "instances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tasks_instance_step_id_fkey",
                        column: x => x.instance_step_id,
                        principalTable: "instance_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tasks_original_assignee_id_fkey",
                        column: x => x.original_assignee_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tasks_signature_file_id_fkey",
                        column: x => x.signature_file_id,
                        principalTable: "files",
                        principalColumn: "id");
                },
                comment: "Nhiệm vụ duyệt giao cho từng người (danh sách \"Việc chờ tôi duyệt\")");

            migrationBuilder.CreateTable(
                name: "kb_chunks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tài liệu"),
                    knowledge_base_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Kho (denormalize để lọc nhanh)"),
                    chunk_index = table.Column<int>(type: "int", nullable: false, comment: "Thứ tự chunk trong tài liệu"),
                    content = table.Column<string>(type: "text", nullable: false, comment: "Nội dung chunk"),
                    heading_path = table.Column<string>(type: "text", nullable: true, comment: "Đường dẫn tiêu đề (Chương 2 > Điều 5) để trích dẫn"),
                    page_number = table.Column<int>(type: "int", nullable: true, comment: "Số trang (PDF)"),
                    token_count = table.Column<int>(type: "int", nullable: true, comment: "Số token"),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'", comment: "Metadata lọc (department, tags...)"),
                    content_tsv = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true, computedColumnSql: "to_tsvector('simple', content)", stored: true, comment: "Chỉ mục full-text (cấu hình 'simple' phù hợp tiếng Việt)"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Tạo lúc"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("kb_chunks_pkey", x => x.id);
                },
                comment: "Chunk văn bản + vector embedding (pgvector) + full-text để hybrid search");

            migrationBuilder.CreateTable(
                name: "kb_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant (denormalize)"),
                    knowledge_base_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Kho tri thức"),
                    title = table.Column<string>(type: "varchar(500)", nullable: false, comment: "Tiêu đề"),
                    source_type = table.Column<string>(type: "varchar(10)", nullable: false, comment: "Nguồn: file, URL, văn bản nhập tay, bản ghi nghiệp vụ"),
                    file_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "File gốc"),
                    source_url = table.Column<string>(type: "varchar(1000)", nullable: true, comment: "URL nguồn"),
                    content_text = table.Column<string>(type: "text", nullable: true, comment: "Nội dung text (TEXT) hoặc text đã trích xuất"),
                    content_hash = table.Column<string>(type: "char(64)", nullable: true, comment: "Hash nội dung để bỏ qua re-index khi không đổi"),
                    language = table.Column<string>(type: "varchar(10)", nullable: true, defaultValueSql: "'vi'", comment: "Ngôn ngữ"),
                    tags = table.Column<List<string>>(type: "text[]", nullable: true, comment: "Thẻ phân loại"),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: true, comment: "Hiệu lực từ (VD: nội quy bản 2026)"),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: true, comment: "Hết hiệu lực → loại khỏi kết quả tìm kiếm"),
                    chunk_count = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0", comment: "Số chunk đã tạo"),
                    token_count = table.Column<int>(type: "int", nullable: true, comment: "Tổng token"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'PENDING'", comment: "Trạng thái index"),
                    error = table.Column<string>(type: "text", nullable: true, comment: "Lỗi index"),
                    indexed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Index xong lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("kb_documents_pkey", x => x.id);
                    table.CheckConstraint("kb_documents_source_type_check", "source_type IN ('FILE','URL','TEXT','ENTITY')");
                    table.CheckConstraint("kb_documents_status_check", "status IN ('PENDING','PROCESSING','INDEXED','FAILED','DISABLED')");
                    table.ForeignKey(
                        name: "kb_documents_file_id_fkey",
                        column: x => x.file_id,
                        principalTable: "files",
                        principalColumn: "id");
                },
                comment: "Tài liệu nguồn trong kho tri thức");

            migrationBuilder.CreateTable(
                name: "knowledge_base_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    knowledge_base_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Kho tri thức"),
                    principal_type = table.Column<string>(type: "varchar(15)", nullable: false, comment: "Loại đối tượng"),
                    principal_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Id đối tượng"),
                    can_manage = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Được quản lý tài liệu"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("knowledge_base_access_pkey", x => x.id);
                    table.CheckConstraint("knowledge_base_access_principal_type_check", "principal_type IN ('ROLE','DEPARTMENT','BRANCH','COMPANY','USER')");
                },
                comment: "Phân quyền truy cập kho tri thức theo vai trò/phòng ban/chi nhánh/user");

            migrationBuilder.CreateTable(
                name: "knowledge_bases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Tenant"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ liên quan (NULL = dùng chung)"),
                    code = table.Column<string>(type: "varchar(100)", nullable: false, comment: "Mã kho"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên kho tri thức"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    embedding_model_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Model embedding (đổi model phải re-index)"),
                    embedding_dimension = table.Column<int>(type: "int", nullable: false, defaultValueSql: "1536", comment: "Số chiều vector"),
                    chunk_size = table.Column<int>(type: "int", nullable: false, defaultValueSql: "800", comment: "Kích thước chunk (token)"),
                    chunk_overlap = table.Column<int>(type: "int", nullable: false, defaultValueSql: "100", comment: "Phần chồng lấn giữa các chunk"),
                    retrieval_top_k = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "5", comment: "Số chunk lấy mỗi truy vấn"),
                    min_score = table.Column<decimal>(type: "numeric(4,3)", nullable: true, defaultValueSql: "0.3", comment: "Ngưỡng tương đồng tối thiểu"),
                    answer_prompt_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Prompt trả lời dùng cho kho này"),
                    visibility = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'RESTRICTED'", comment: "Ai được hỏi đáp"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'ACTIVE'", comment: "Trạng thái"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("knowledge_bases_pkey", x => x.id);
                    table.CheckConstraint("knowledge_bases_status_check", "status IN ('ACTIVE','INDEXING','DISABLED')");
                    table.CheckConstraint("knowledge_bases_visibility_check", "visibility IN ('ALL_USERS','RESTRICTED')");
                    table.ForeignKey(
                        name: "knowledge_bases_embedding_model_id_fkey",
                        column: x => x.embedding_model_id,
                        principalTable: "models",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "knowledge_bases_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "knowledge_bases_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Kho tri thức RAG (nội quy, chính sách bán hàng, quy trình kho) cho chatbot nội bộ");

            migrationBuilder.CreateTable(
                name: "prompt_guardrails",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính (UUID v7)"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    prompt_version_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Phiên bản prompt"),
                    guardrail_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Guardrail"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("prompt_guardrails_pkey", x => x.id);
                    table.ForeignKey(
                        name: "prompt_guardrails_guardrail_id_fkey",
                        column: x => x.guardrail_id,
                        principalTable: "guardrail_policies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Gắn guardrail vào phiên bản prompt");

            migrationBuilder.CreateTable(
                name: "prompt_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant (NULL = prompt hệ thống, tenant có thể ghi đè)"),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phân hệ sử dụng"),
                    code = table.Column<string>(type: "varchar(150)", nullable: false, comment: "Mã prompt service gọi (hrm.cv_summary)"),
                    name = table.Column<string>(type: "varchar(255)", nullable: false, comment: "Tên"),
                    description = table.Column<string>(type: "text", nullable: true, comment: "Mô tả"),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Phiên bản đang dùng (ai.prompt_versions)"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true", comment: "Còn dùng"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (iam.users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("prompt_templates_pkey", x => x.id);
                    table.ForeignKey(
                        name: "prompt_templates_module_id_fkey",
                        column: x => x.module_id,
                        principalTable: "modules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "prompt_templates_tenant_id_fkey",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id");
                },
                comment: "Prompt template dùng chung theo phân hệ (tóm tắt CV - HRM, phân tích KH - CRM, gợi ý đặt hàng - WMS)");

            migrationBuilder.CreateTable(
                name: "prompt_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()", comment: "Khóa chính"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Tenant sở hữu (tenants.id); NULL = dữ liệu dùng chung toàn hệ thống"),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Prompt"),
                    version_no = table.Column<int>(type: "int", nullable: false, comment: "Số phiên bản"),
                    system_prompt = table.Column<string>(type: "text", nullable: true, comment: "System prompt"),
                    user_prompt = table.Column<string>(type: "text", nullable: false, comment: "User prompt có biến {{variable}}"),
                    variables = table.Column<string>(type: "jsonb", nullable: true, comment: "Khai báo biến đầu vào, VD: [{\"name\":\"cv_text\",\"required\":true}]"),
                    model_alias_id = table.Column<Guid>(type: "uuid", nullable: true, comment: "Alias model dùng"),
                    temperature = table.Column<decimal>(type: "numeric(3,2)", nullable: true, comment: "Nhiệt độ"),
                    top_p = table.Column<decimal>(type: "numeric(3,2)", nullable: true, comment: "Top-p"),
                    max_output_tokens = table.Column<int>(type: "int", nullable: true, comment: "Giới hạn token đầu ra"),
                    response_format = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'TEXT'", comment: "Định dạng trả về"),
                    json_schema = table.Column<string>(type: "jsonb", nullable: true, comment: "JSON Schema đầu ra (JSON_SCHEMA)"),
                    tools = table.Column<string>(type: "jsonb", nullable: true, comment: "Khai báo function/tool calling"),
                    traffic_percent = table.Column<short>(type: "smallint", nullable: false, defaultValueSql: "100", comment: "% lưu lượng (A/B test giữa các phiên bản PUBLISHED)"),
                    status = table.Column<string>(type: "varchar(15)", nullable: false, defaultValueSql: "'DRAFT'", comment: "Trạng thái"),
                    change_note = table.Column<string>(type: "text", nullable: true, comment: "Ghi chú thay đổi"),
                    published_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Phát hành lúc"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "now()", comment: "Thời điểm tạo bản ghi"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người tạo (iam.users.id); NULL nếu hệ thống tạo"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm cập nhật gần nhất (trigger tự gán)"),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người cập nhật gần nhất (iam.users.id)"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false", comment: "Cờ xóa mềm (true = đã xóa)"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true, comment: "Thời điểm xóa mềm"),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true, comment: "Người xóa mềm (users.id)"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("prompt_versions_pkey", x => x.id);
                    table.CheckConstraint("prompt_versions_response_format_check", "response_format IN ('TEXT','JSON','JSON_SCHEMA')");
                    table.CheckConstraint("prompt_versions_status_check", "status IN ('DRAFT','PUBLISHED','ARCHIVED')");
                    table.CheckConstraint("prompt_versions_traffic_percent_check", "traffic_percent BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "prompt_versions_model_alias_id_fkey",
                        column: x => x.model_alias_id,
                        principalTable: "model_aliases",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "prompt_versions_template_id_fkey",
                        column: x => x.template_id,
                        principalTable: "prompt_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Phiên bản prompt (quản lý thay đổi, rollback, A/B test)");

            migrationBuilder.CreateIndex(
                name: "ix_wf_action_logs_instance",
                table: "action_logs",
                columns: new[] { "instance_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "uq_admin_unit_mapping",
                table: "administrative_unit_mappings",
                columns: new[] { "old_unit_id", "new_unit_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_admin_units_level",
                table: "administrative_units",
                columns: new[] { "country_code", "level" },
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_admin_units_name_trgm",
                table: "administrative_units",
                column: "full_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_admin_units_parent",
                table: "administrative_units",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "uq_admin_unit_code",
                table: "administrative_units",
                columns: new[] { "country_code", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ai_providers_code_key",
                table: "ai_providers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "api_keys_key_hash_key",
                table: "api_keys",
                column: "key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "api_keys_key_prefix_key",
                table: "api_keys",
                column: "key_prefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "api_request_logs_id_occurred_at_key",
                table: "api_request_logs",
                columns: new[] { "id", "occurred_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_api_logs_errors",
                table: "api_request_logs",
                column: "occurred_at",
                descending: new bool[0],
                filter: "status_code >= 500");

            migrationBuilder.CreateIndex(
                name: "ix_api_logs_tenant",
                table: "api_request_logs",
                columns: new[] { "tenant_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_api_logs_trace",
                table: "api_request_logs",
                column: "trace_id");

            migrationBuilder.CreateIndex(
                name: "api_scopes_code_key",
                table: "api_scopes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_audit_policies",
                table: "audit_policies",
                columns: new[] { "module_code", "entity_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "banks_code_key",
                table: "banks",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_branches_company",
                table: "branches",
                column: "company_id",
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "uq_buckets",
                table: "buckets",
                columns: new[] { "provider_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "clusters_code_key",
                table: "clusters",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_companies_parent",
                table: "companies",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_path",
                table: "companies",
                column: "path")
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_ai_conversations_user",
                table: "conversations",
                columns: new[] { "user_id", "last_message_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "uq_cors_policies",
                table: "cors_policies",
                columns: new[] { "tenant_id", "code" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "countries_alpha3_code_key",
                table: "countries",
                column: "alpha3_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "data_change_logs_id_changed_at_key",
                table: "data_change_logs",
                columns: new[] { "id", "changed_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_change_entity",
                table: "data_change_logs",
                columns: new[] { "entity_type", "entity_id", "changed_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_data_change_tenant",
                table: "data_change_logs",
                columns: new[] { "tenant_id", "module_code", "changed_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "ix_data_change_user",
                table: "data_change_logs",
                columns: new[] { "changed_by", "changed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "uq_wf_versions",
                table: "definition_versions",
                columns: new[] { "definition_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wf_delegations_active",
                table: "delegations",
                columns: new[] { "delegator_id", "start_at", "end_at" },
                filter: "status = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "ix_deliveries_notification",
                table: "deliveries",
                column: "notification_id");

            migrationBuilder.CreateIndex(
                name: "ix_deliveries_queue",
                table: "deliveries",
                column: "next_attempt_at",
                filter: "status IN ('PENDING','FAILED')");

            migrationBuilder.CreateIndex(
                name: "delivery_attachments_delivery_id_file_id_key",
                table: "delivery_attachments",
                columns: new[] { "delivery_id", "file_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_branch",
                table: "departments",
                column: "branch_id",
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_departments_parent",
                table: "departments",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_departments_path",
                table: "departments",
                column: "path")
                .Annotation("Npgsql:IndexOperators", new[] { "text_pattern_ops" });

            migrationBuilder.CreateIndex(
                name: "uq_gateway_destinations",
                table: "destinations",
                columns: new[] { "cluster_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_device_tokens_user",
                table: "device_tokens",
                column: "user_id",
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "error_codes_code_key",
                table: "error_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_event_subscriptions",
                table: "event_subscriptions",
                columns: new[] { "event_type_code", "consumer_service" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_lookup",
                table: "exchange_rates",
                columns: new[] { "from_currency", "to_currency", "rate_type", "effective_at" },
                descending: new[] { false, false, false, true });

            migrationBuilder.CreateIndex(
                name: "uq_exchange_rates",
                table: "exchange_rates",
                columns: new[] { "tenant_id", "from_currency", "to_currency", "rate_type", "effective_at" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_job_executions_job",
                table: "executions",
                columns: new[] { "job_id", "started_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_job_executions_status",
                table: "executions",
                columns: new[] { "status", "started_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_external_mappings_internal",
                table: "external_mappings",
                columns: new[] { "entity_type", "internal_id" });

            migrationBuilder.CreateIndex(
                name: "uq_external_mapping_ext",
                table: "external_mappings",
                columns: new[] { "tenant_id", "external_system", "entity_type", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_feature_flag_rules",
                table: "feature_flag_rules",
                columns: new[] { "flag_key", "scope_type", "scope_value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "file_access_logs_id_occurred_at_key",
                table: "file_access_logs",
                columns: new[] { "id", "occurred_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_file_access_logs_file",
                table: "file_access_logs",
                columns: new[] { "file_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "file_share_links_token_hash_key",
                table: "file_share_links",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_file_variants",
                table: "file_variants",
                columns: new[] { "file_id", "variant_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_files_checksum",
                table: "files",
                columns: new[] { "tenant_id", "checksum_sha256" });

            migrationBuilder.CreateIndex(
                name: "ix_files_entity",
                table: "files",
                columns: new[] { "module_code", "entity_type", "entity_id" },
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_files_temp_expires",
                table: "files",
                column: "expires_at",
                filter: "status = 'TEMP'");

            migrationBuilder.CreateIndex(
                name: "ix_files_tenant_created",
                table: "files",
                columns: new[] { "tenant_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "uq_files_object",
                table: "files",
                columns: new[] { "bucket_id", "object_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "gateway_clients_key_hash_key",
                table: "gateway_clients",
                column: "key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "gateway_clients_key_prefix_key",
                table: "gateway_clients",
                column: "key_prefix",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_ai_guardrails",
                table: "guardrail_policies",
                columns: new[] { "tenant_id", "code" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "health_checks_id_checked_at_key",
                table: "health_checks",
                columns: new[] { "id", "checked_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_health_checks_service",
                table: "health_checks",
                columns: new[] { "service_id", "checked_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_holidays_date",
                table: "holidays",
                columns: new[] { "country_code", "holiday_date" },
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_holidays_tenant_year",
                table: "holidays",
                columns: new[] { "tenant_id", "year" },
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_inbound_requests_source",
                table: "inbound_requests",
                columns: new[] { "source_id", "received_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "uq_inbound_requests_external",
                table: "inbound_requests",
                columns: new[] { "source_id", "external_id" },
                unique: true,
                filter: "external_id IS NOT NULL AND status <> 'DUPLICATE'");

            migrationBuilder.CreateIndex(
                name: "inbound_sources_endpoint_token_key",
                table: "inbound_sources",
                column: "endpoint_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "inbox_messages_consumer_service_message_id_key",
                table: "inbox_messages",
                columns: new[] { "consumer_service", "message_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wf_instance_steps_due",
                table: "instance_steps",
                column: "due_at",
                filter: "status = 'IN_PROGRESS'");

            migrationBuilder.CreateIndex(
                name: "ix_wf_instance_steps_instance",
                table: "instance_steps",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "ix_wf_instances_document",
                table: "instances",
                columns: new[] { "document_type_code", "document_id" });

            migrationBuilder.CreateIndex(
                name: "ix_wf_instances_requester",
                table: "instances",
                columns: new[] { "requester_id", "submitted_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_wf_instances_status",
                table: "instances",
                columns: new[] { "tenant_id", "status", "submitted_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "uq_wf_instances_active_doc",
                table: "instances",
                columns: new[] { "document_type_code", "document_id" },
                unique: true,
                filter: "status = 'PENDING'");

            migrationBuilder.CreateIndex(
                name: "job_definitions_code_key",
                table: "job_definitions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_kb_chunks_kb",
                table: "kb_chunks",
                column: "knowledge_base_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_kb_chunks_tsv",
                table: "kb_chunks",
                column: "content_tsv")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "uq_ai_kb_chunks",
                table: "kb_chunks",
                columns: new[] { "document_id", "chunk_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_kb_documents_kb",
                table: "kb_documents",
                column: "knowledge_base_id",
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "uq_ai_kb_access",
                table: "knowledge_base_access",
                columns: new[] { "knowledge_base_id", "principal_type", "principal_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_ip",
                table: "login_logs",
                columns: new[] { "ip_address", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_tenant",
                table: "login_logs",
                columns: new[] { "tenant_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_login_logs_user",
                table: "login_logs",
                columns: new[] { "user_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "login_logs_id_occurred_at_key",
                table: "login_logs",
                columns: new[] { "id", "occurred_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_messages_conversation",
                table: "messages",
                columns: new[] { "conversation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "uq_ai_alias_targets",
                table: "model_alias_targets",
                columns: new[] { "alias_id", "model_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_ai_model_aliases",
                table: "model_aliases",
                columns: new[] { "tenant_id", "code" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_ai_models",
                table: "models",
                columns: new[] { "provider_id", "model_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "modules_code_key",
                table: "modules",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_entity",
                table: "notifications",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "uq_notifications_idem",
                table: "notifications",
                columns: new[] { "tenant_id", "idempotency_key" },
                unique: true,
                filter: "idempotency_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "notify_event_types_code_key",
                table: "notify_event_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_numbering_counters",
                table: "numbering_counters",
                columns: new[] { "rule_id", "period_key", "scope_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_numbering_rules_scope",
                table: "numbering_rules",
                columns: new[] { "tenant_id", "document_type_code", "company_id", "branch_id" },
                unique: true,
                filter: "is_active AND NOT is_deleted")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "oauth_clients_client_id_key",
                table: "oauth_clients",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_aggregate",
                table: "outbox_messages",
                columns: new[] { "aggregate_type", "aggregate_id" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_pending",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "status IN ('PENDING','FAILED')");

            migrationBuilder.CreateIndex(
                name: "ix_password_histories_user",
                table: "password_histories",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "permissions_code_key",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_permissions_resource_action",
                table: "permissions",
                columns: new[] { "resource_id", "action_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "prompt_guardrails_prompt_version_id_guardrail_id_key",
                table: "prompt_guardrails",
                columns: new[] { "prompt_version_id", "guardrail_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_ai_prompt_templates",
                table: "prompt_templates",
                columns: new[] { "tenant_id", "code" },
                unique: true,
                filter: "NOT is_deleted")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_ai_prompt_versions",
                table: "prompt_versions",
                columns: new[] { "template_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_push_configs",
                table: "push_configs",
                columns: new[] { "tenant_id", "app_code", "provider" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_ai_quotas",
                table: "quotas",
                columns: new[] { "tenant_id", "module_id", "user_id", "period" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "rate_limit_policies_code_key",
                table: "rate_limit_policies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_family",
                table: "refresh_tokens",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_session",
                table: "refresh_tokens",
                column: "session_id",
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "refresh_tokens_token_hash_key",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_resources_code",
                table: "resources",
                columns: new[] { "module_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revoked_tokens_expires",
                table: "revoked_access_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "revoked_access_tokens_jti_key",
                table: "revoked_access_tokens",
                column: "jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_role_permission_scopes",
                table: "role_permission_scopes",
                columns: new[] { "role_permission_id", "scope_type", "scope_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_perm",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "uq_role_permissions",
                table: "role_permissions",
                columns: new[] { "role_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "routes_code_key",
                table: "routes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_security_events_target",
                table: "security_events",
                columns: new[] { "target_user_id", "occurred_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_security_events_tenant",
                table: "security_events",
                columns: new[] { "tenant_id", "event_type", "occurred_at" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "security_events_id_occurred_at_key",
                table: "security_events",
                columns: new[] { "id", "occurred_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "security_header_policies_code_key",
                table: "security_header_policies",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "security_policies_tenant_id_key",
                table: "security_policies",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "services_code_key",
                table: "services",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_setting_values",
                table: "setting_values",
                columns: new[] { "setting_key", "scope_type", "scope_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "signing_keys_kid_key",
                table: "signing_keys",
                column: "kid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_wf_steps",
                table: "steps",
                columns: new[] { "version_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_storage_providers_code",
                table: "storage_providers",
                columns: new[] { "tenant_id", "code" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_wf_tasks_due",
                table: "tasks",
                column: "due_at",
                filter: "status = 'PENDING'");

            migrationBuilder.CreateIndex(
                name: "ix_wf_tasks_inbox",
                table: "tasks",
                columns: new[] { "assignee_id", "created_at" },
                descending: new[] { false, true },
                filter: "status = 'PENDING'");

            migrationBuilder.CreateIndex(
                name: "ix_wf_tasks_instance",
                table: "tasks",
                column: "instance_id");

            migrationBuilder.CreateIndex(
                name: "uq_tax_rates",
                table: "tax_rates",
                columns: new[] { "tenant_id", "code", "effective_from" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_notify_templates",
                table: "templates",
                columns: new[] { "tenant_id", "event_type_id", "channel", "locale" },
                unique: true,
                filter: "is_active AND NOT is_deleted")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_tenant_modules",
                table: "tenant_modules",
                columns: new[] { "tenant_id", "module_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_job_tenant_overrides",
                table: "tenant_overrides",
                columns: new[] { "job_id", "tenant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_tenant_rate_limits",
                table: "tenant_rate_limits",
                columns: new[] { "tenant_id", "policy_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wf_transitions_from",
                table: "transitions",
                columns: new[] { "from_step_id", "priority" });

            migrationBuilder.CreateIndex(
                name: "uq_translation_keys",
                table: "translation_keys",
                columns: new[] { "namespace", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_translations",
                table: "translations",
                columns: new[] { "key_id", "language_code", "tenant_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uom_categories_code_key",
                table: "uom_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_uom_conversions",
                table: "uom_conversions",
                columns: new[] { "tenant_id", "from_uom_id", "to_uom_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_upload_policies",
                table: "upload_policies",
                columns: new[] { "tenant_id", "category" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "usage_daily_usage_date_tenant_id_module_code_model_id_key",
                table: "usage_daily",
                columns: new[] { "usage_date", "tenant_id", "module_code", "model_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_module",
                table: "usage_logs",
                columns: new[] { "module_code", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_tenant",
                table: "usage_logs",
                columns: new[] { "tenant_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "usage_logs_id_created_at_key",
                table: "usage_logs",
                columns: new[] { "id", "created_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_user_devices",
                table: "user_devices",
                columns: new[] { "user_id", "device_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_external_login",
                table: "user_external_logins",
                columns: new[] { "provider", "provider_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_notifications_inbox",
                table: "user_notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true },
                filter: "NOT is_archived");

            migrationBuilder.CreateIndex(
                name: "ix_user_notifications_unread",
                table: "user_notifications",
                column: "user_id",
                filter: "NOT is_read");

            migrationBuilder.CreateIndex(
                name: "ix_user_org_dept",
                table: "user_org_assignments",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_org_user",
                table: "user_org_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_user_org_primary",
                table: "user_org_assignments",
                column: "user_id",
                unique: true,
                filter: "is_primary");

            migrationBuilder.CreateIndex(
                name: "uq_user_permission_overrides",
                table: "user_permission_overrides",
                columns: new[] { "user_id", "permission_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_user_notify_prefs",
                table: "user_preferences",
                columns: new[] { "user_id", "event_type_id", "channel" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_recovery_codes_user",
                table: "user_recovery_codes",
                column: "user_id",
                filter: "used_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "uq_user_roles",
                table: "user_roles",
                columns: new[] { "user_id", "role_id", "company_id", "branch_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_expires",
                table: "user_sessions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_user_sessions_user_active",
                table: "user_sessions",
                column: "user_id",
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_users_department",
                table: "users",
                column: "department_id",
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_users_employee",
                table: "users",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_name_trgm",
                table: "users",
                column: "full_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_users_phone",
                table: "users",
                columns: new[] { "tenant_id", "phone" },
                filter: "NOT is_deleted");

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                table: "users",
                columns: new[] { "tenant_id", "email" },
                unique: true,
                filter: "email IS NOT NULL AND NOT is_deleted")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_users_username",
                table: "users",
                columns: new[] { "tenant_id", "username" },
                unique: true,
                filter: "NOT is_deleted")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_verification_tokens_lookup",
                table: "verification_tokens",
                column: "token_hash",
                filter: "used_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_verification_tokens_user",
                table: "verification_tokens",
                columns: new[] { "user_id", "purpose" });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_endpoint",
                table: "webhook_deliveries",
                columns: new[] { "endpoint_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_webhook_deliveries_retry",
                table: "webhook_deliveries",
                column: "next_retry_at",
                filter: "status = 'RETRYING'");

            migrationBuilder.CreateIndex(
                name: "uq_work_calendar_exceptions",
                table: "work_calendar_exceptions",
                columns: new[] { "calendar_id", "exception_date", "branch_id" },
                unique: true,
                filter: "NOT is_deleted")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "uq_work_calendar_weekdays",
                table: "work_calendar_weekdays",
                columns: new[] { "calendar_id", "day_of_week" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wf_definitions_doc",
                table: "workflow_definitions",
                columns: new[] { "tenant_id", "document_type_code" },
                filter: "is_active AND NOT is_deleted");

            migrationBuilder.AddForeignKey(
                name: "action_logs_instance_id_fkey",
                table: "action_logs",
                column: "instance_id",
                principalTable: "instances",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "action_logs_instance_step_id_fkey",
                table: "action_logs",
                column: "instance_step_id",
                principalTable: "instance_steps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "action_logs_task_id_fkey",
                table: "action_logs",
                column: "task_id",
                principalTable: "tasks",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "api_keys_act_as_user_id_fkey",
                table: "api_keys",
                column: "act_as_user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "api_keys_owner_user_id_fkey",
                table: "api_keys",
                column: "owner_user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "api_keys_oauth_client_id_fkey",
                table: "api_keys",
                column: "oauth_client_id",
                principalTable: "oauth_clients",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "api_keys_tenant_id_fkey",
                table: "api_keys",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "branches_company_id_fkey",
                table: "branches",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "branches_tenant_id_fkey",
                table: "branches",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_branches_calendar",
                table: "branches",
                column: "work_calendar_id",
                principalTable: "work_calendars",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_branches_manager",
                table: "branches",
                column: "manager_user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "buckets_provider_id_fkey",
                table: "buckets",
                column: "provider_id",
                principalTable: "storage_providers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "companies_tenant_id_fkey",
                table: "companies",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_calendar",
                table: "companies",
                column: "work_calendar_id",
                principalTable: "work_calendars",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_companies_logo",
                table: "companies",
                column: "logo_file_id",
                principalTable: "files",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "connections_tenant_id_fkey",
                table: "connections",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "conversations_knowledge_base_id_fkey",
                table: "conversations",
                column: "knowledge_base_id",
                principalTable: "knowledge_bases",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "conversations_user_id_fkey",
                table: "conversations",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "cors_policies_tenant_id_fkey",
                table: "cors_policies",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "definition_versions_definition_id_fkey",
                table: "definition_versions",
                column: "definition_id",
                principalTable: "workflow_definitions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "delegations_definition_id_fkey",
                table: "delegations",
                column: "definition_id",
                principalTable: "workflow_definitions",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "delegations_delegate_id_fkey",
                table: "delegations",
                column: "delegate_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "delegations_delegator_id_fkey",
                table: "delegations",
                column: "delegator_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "delegations_tenant_id_fkey",
                table: "delegations",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "deliveries_notification_id_fkey",
                table: "deliveries",
                column: "notification_id",
                principalTable: "notifications",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "deliveries_template_id_fkey",
                table: "deliveries",
                column: "template_id",
                principalTable: "templates",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "delivery_attachments_file_id_fkey",
                table: "delivery_attachments",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "departments_tenant_id_fkey",
                table: "departments",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_departments_deputy",
                table: "departments",
                column: "deputy_user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_departments_manager",
                table: "departments",
                column: "manager_user_id",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "device_tokens_device_id_fkey",
                table: "device_tokens",
                column: "device_id",
                principalTable: "user_devices",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "device_tokens_user_id_fkey",
                table: "device_tokens",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "exchange_rates_tenant_id_fkey",
                table: "exchange_rates",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "file_share_links_file_id_fkey",
                table: "file_share_links",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "file_variants_file_id_fkey",
                table: "file_variants",
                column: "file_id",
                principalTable: "files",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "files_tenant_id_fkey",
                table: "files",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "kb_chunks_document_id_fkey",
                table: "kb_chunks",
                column: "document_id",
                principalTable: "kb_documents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "kb_chunks_knowledge_base_id_fkey",
                table: "kb_chunks",
                column: "knowledge_base_id",
                principalTable: "knowledge_bases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "kb_documents_knowledge_base_id_fkey",
                table: "kb_documents",
                column: "knowledge_base_id",
                principalTable: "knowledge_bases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "knowledge_base_access_knowledge_base_id_fkey",
                table: "knowledge_base_access",
                column: "knowledge_base_id",
                principalTable: "knowledge_bases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "knowledge_bases_answer_prompt_id_fkey",
                table: "knowledge_bases",
                column: "answer_prompt_id",
                principalTable: "prompt_templates",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "prompt_guardrails_prompt_version_id_fkey",
                table: "prompt_guardrails",
                column: "prompt_version_id",
                principalTable: "prompt_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_ai_prompt_current_version",
                table: "prompt_templates",
                column: "current_version_id",
                principalTable: "prompt_versions",
                principalColumn: "id");

            // Phần EF Core không biểu diễn được — sinh từ docs/base_erp_core.sql
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.ExpressionIndexes));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.UpdatedAtTriggers));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.DocumentNumberFunction));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.SeedData));
            migrationBuilder.Sql(SqlScripts.Load(SqlScripts.Pgvector));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS next_document_number(uuid, varchar, uuid, uuid, date);");

            migrationBuilder.DropForeignKey(
                name: "branches_province_id_fkey",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "branches_ward_id_fkey",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "companies_province_id_fkey",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "companies_ward_id_fkey",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "branches_country_code_fkey",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "companies_country_code_fkey",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "tenants_country_code_fkey",
                table: "tenants");

            migrationBuilder.DropForeignKey(
                name: "fk_branches_manager",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "fk_departments_deputy",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "fk_departments_manager",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "branches_tenant_id_fkey",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "companies_tenant_id_fkey",
                table: "companies");

            migrationBuilder.DropForeignKey(
                name: "departments_tenant_id_fkey",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "files_tenant_id_fkey",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "model_aliases_tenant_id_fkey",
                table: "model_aliases");

            migrationBuilder.DropForeignKey(
                name: "prompt_templates_tenant_id_fkey",
                table: "prompt_templates");

            migrationBuilder.DropForeignKey(
                name: "storage_providers_tenant_id_fkey",
                table: "storage_providers");

            migrationBuilder.DropForeignKey(
                name: "work_calendars_tenant_id_fkey",
                table: "work_calendars");

            migrationBuilder.DropForeignKey(
                name: "workflow_definitions_tenant_id_fkey",
                table: "workflow_definitions");

            migrationBuilder.DropForeignKey(
                name: "document_types_module_id_fkey",
                table: "document_types");

            migrationBuilder.DropForeignKey(
                name: "prompt_templates_module_id_fkey",
                table: "prompt_templates");

            migrationBuilder.DropForeignKey(
                name: "branches_company_id_fkey",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "departments_company_id_fkey",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "work_calendars_company_id_fkey",
                table: "work_calendars");

            migrationBuilder.DropForeignKey(
                name: "workflow_definitions_company_id_fkey",
                table: "workflow_definitions");

            migrationBuilder.DropForeignKey(
                name: "fk_branches_calendar",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "definition_versions_definition_id_fkey",
                table: "definition_versions");

            migrationBuilder.DropForeignKey(
                name: "prompt_versions_template_id_fkey",
                table: "prompt_versions");

            migrationBuilder.DropTable(
                name: "action_logs");

            migrationBuilder.DropTable(
                name: "administrative_unit_mappings");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "api_request_logs");

            migrationBuilder.DropTable(
                name: "api_scopes");

            migrationBuilder.DropTable(
                name: "audit_policies");

            migrationBuilder.DropTable(
                name: "banks");

            migrationBuilder.DropTable(
                name: "data_change_logs");

            migrationBuilder.DropTable(
                name: "delivery_attachments");

            migrationBuilder.DropTable(
                name: "destinations");

            migrationBuilder.DropTable(
                name: "device_tokens");

            migrationBuilder.DropTable(
                name: "error_codes");

            migrationBuilder.DropTable(
                name: "event_subscriptions");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "executions");

            migrationBuilder.DropTable(
                name: "external_mappings");

            migrationBuilder.DropTable(
                name: "feature_flag_rules");

            migrationBuilder.DropTable(
                name: "file_access_logs");

            migrationBuilder.DropTable(
                name: "file_share_links");

            migrationBuilder.DropTable(
                name: "file_variants");

            migrationBuilder.DropTable(
                name: "gateway_clients");

            migrationBuilder.DropTable(
                name: "health_checks");

            migrationBuilder.DropTable(
                name: "inbound_requests");

            migrationBuilder.DropTable(
                name: "inbox_messages");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "kb_chunks");

            migrationBuilder.DropTable(
                name: "knowledge_base_access");

            migrationBuilder.DropTable(
                name: "login_logs");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "model_alias_targets");

            migrationBuilder.DropTable(
                name: "numbering_counters");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "password_histories");

            migrationBuilder.DropTable(
                name: "prompt_guardrails");

            migrationBuilder.DropTable(
                name: "push_configs");

            migrationBuilder.DropTable(
                name: "quotas");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "revoked_access_tokens");

            migrationBuilder.DropTable(
                name: "role_permission_scopes");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "security_events");

            migrationBuilder.DropTable(
                name: "security_policies");

            migrationBuilder.DropTable(
                name: "setting_values");

            migrationBuilder.DropTable(
                name: "signing_keys");

            migrationBuilder.DropTable(
                name: "smtp_configs");

            migrationBuilder.DropTable(
                name: "step_approvers");

            migrationBuilder.DropTable(
                name: "tax_rates");

            migrationBuilder.DropTable(
                name: "tenant_modules");

            migrationBuilder.DropTable(
                name: "tenant_overrides");

            migrationBuilder.DropTable(
                name: "tenant_rate_limits");

            migrationBuilder.DropTable(
                name: "transitions");

            migrationBuilder.DropTable(
                name: "translations");

            migrationBuilder.DropTable(
                name: "uom_conversions");

            migrationBuilder.DropTable(
                name: "upload_policies");

            migrationBuilder.DropTable(
                name: "usage_daily");

            migrationBuilder.DropTable(
                name: "usage_logs");

            migrationBuilder.DropTable(
                name: "user_external_logins");

            migrationBuilder.DropTable(
                name: "user_notifications");

            migrationBuilder.DropTable(
                name: "user_org_assignments");

            migrationBuilder.DropTable(
                name: "user_permission_overrides");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "user_recovery_codes");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "verification_tokens");

            migrationBuilder.DropTable(
                name: "webhook_deliveries");

            migrationBuilder.DropTable(
                name: "work_calendar_exceptions");

            migrationBuilder.DropTable(
                name: "work_calendar_weekdays");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "oauth_clients");

            migrationBuilder.DropTable(
                name: "deliveries");

            migrationBuilder.DropTable(
                name: "integration_event_types");

            migrationBuilder.DropTable(
                name: "connections");

            migrationBuilder.DropTable(
                name: "feature_flags");

            migrationBuilder.DropTable(
                name: "inbound_sources");

            migrationBuilder.DropTable(
                name: "services");

            migrationBuilder.DropTable(
                name: "kb_documents");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "provider_credentials");

            migrationBuilder.DropTable(
                name: "numbering_rules");

            migrationBuilder.DropTable(
                name: "guardrail_policies");

            migrationBuilder.DropTable(
                name: "user_sessions");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "clusters");

            migrationBuilder.DropTable(
                name: "cors_policies");

            migrationBuilder.DropTable(
                name: "security_header_policies");

            migrationBuilder.DropTable(
                name: "setting_definitions");

            migrationBuilder.DropTable(
                name: "tax_types");

            migrationBuilder.DropTable(
                name: "job_definitions");

            migrationBuilder.DropTable(
                name: "rate_limit_policies");

            migrationBuilder.DropTable(
                name: "translation_keys");

            migrationBuilder.DropTable(
                name: "languages");

            migrationBuilder.DropTable(
                name: "uoms");

            migrationBuilder.DropTable(
                name: "webhook_endpoints");

            migrationBuilder.DropTable(
                name: "holidays");

            migrationBuilder.DropTable(
                name: "delegations");

            migrationBuilder.DropTable(
                name: "instance_steps");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "knowledge_bases");

            migrationBuilder.DropTable(
                name: "user_devices");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "uom_categories");

            migrationBuilder.DropTable(
                name: "instances");

            migrationBuilder.DropTable(
                name: "steps");

            migrationBuilder.DropTable(
                name: "notify_event_types");

            migrationBuilder.DropTable(
                name: "models");

            migrationBuilder.DropTable(
                name: "permission_actions");

            migrationBuilder.DropTable(
                name: "resources");

            migrationBuilder.DropTable(
                name: "ai_providers");

            migrationBuilder.DropTable(
                name: "administrative_units");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "modules");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "currencies");

            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "buckets");

            migrationBuilder.DropTable(
                name: "storage_providers");

            migrationBuilder.DropTable(
                name: "work_calendars");

            migrationBuilder.DropTable(
                name: "workflow_definitions");

            migrationBuilder.DropTable(
                name: "definition_versions");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "document_types");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "prompt_templates");

            migrationBuilder.DropTable(
                name: "prompt_versions");

            migrationBuilder.DropTable(
                name: "model_aliases");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_updated_at();");
        }
    }
}
