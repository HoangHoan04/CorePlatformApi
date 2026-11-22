-- Sinh tự động từ docs/base_erp_core.sql — các phần EF Core không biểu diễn được.

CREATE TABLE storage.file_access_logs (
    id           uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    file_id      uuid NOT NULL,                          -- File
    tenant_id    uuid,                                   -- Tenant
    user_id      uuid,                                   -- Người truy cập (NULL nếu qua share link)
    share_link_id uuid,                                  -- Link chia sẻ được dùng
    action       varchar(20) NOT NULL CHECK (action IN ('VIEW','DOWNLOAD','PRESIGN','DELETE','SHARE')), -- Hành động
    ip_address   inet,                                   -- IP
    user_agent   text,                                   -- User-Agent
    occurred_at  timestamptz NOT NULL DEFAULT now(),     -- Thời điểm
    PRIMARY KEY (id, occurred_at)
) PARTITION BY RANGE (occurred_at);
COMMENT ON TABLE storage.file_access_logs IS 'Nhật ký truy cập file nhạy cảm (xem, tải, cấp presigned URL) phục vụ kiểm toán';
COMMENT ON COLUMN storage.file_access_logs.id IS 'Khóa chính';
COMMENT ON COLUMN storage.file_access_logs.file_id IS 'File';
COMMENT ON COLUMN storage.file_access_logs.tenant_id IS 'Tenant';
COMMENT ON COLUMN storage.file_access_logs.user_id IS 'Người truy cập (NULL nếu qua share link)';
COMMENT ON COLUMN storage.file_access_logs.share_link_id IS 'Link chia sẻ được dùng';
COMMENT ON COLUMN storage.file_access_logs.action IS 'Hành động';
COMMENT ON COLUMN storage.file_access_logs.ip_address IS 'IP';
COMMENT ON COLUMN storage.file_access_logs.user_agent IS 'User-Agent';
COMMENT ON COLUMN storage.file_access_logs.occurred_at IS 'Thời điểm';
CREATE INDEX ix_file_access_logs_file ON storage.file_access_logs (file_id, occurred_at DESC);

CREATE TABLE ai.usage_logs (
    id                 uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    tenant_id          uuid,                             -- Tenant
    module_code        varchar(30),                      -- Phân hệ gọi
    gateway_client_id  uuid,                             -- Client nội bộ
    user_id            uuid,                             -- User cuối
    prompt_template_id uuid,                             -- Prompt
    prompt_version_id  uuid,                             -- Phiên bản prompt
    alias_code         varchar(100),                     -- Alias được yêu cầu
    model_id           uuid,                             -- Model thực tế phục vụ
    credential_id      uuid,                             -- Key provider đã dùng
    request_type       varchar(15) NOT NULL DEFAULT 'CHAT' CHECK (request_type IN ('CHAT','EMBEDDING','RERANK','IMAGE','AUDIO','MODERATION')), -- Loại request
    input_tokens       int NOT NULL DEFAULT 0,           -- Token đầu vào
    output_tokens      int NOT NULL DEFAULT 0,           -- Token đầu ra
    cached_tokens      int NOT NULL DEFAULT 0,           -- Token đọc từ cache
    total_tokens       int GENERATED ALWAYS AS (input_tokens + output_tokens) STORED, -- Tổng token
    estimated_cost_usd numeric(14,8) NOT NULL DEFAULT 0, -- Chi phí ước tính (USD)
    latency_ms         int,                              -- Độ trễ tổng
    time_to_first_token_ms int,                          -- Thời gian tới token đầu (streaming)
    is_fallback        boolean NOT NULL DEFAULT false,   -- Phục vụ bởi model fallback
    is_cache_hit       boolean NOT NULL DEFAULT false,   -- Trả về từ semantic cache
    status             varchar(15) NOT NULL CHECK (status IN ('SUCCESS','ERROR','BLOCKED','QUOTA_EXCEEDED','TIMEOUT')), -- Kết quả
    error_code         varchar(100),                     -- Mã lỗi
    guardrail_codes    text[],                           -- Guardrail đã kích hoạt
    trace_id           varchar(64),                      -- Trace id
    created_at         timestamptz NOT NULL DEFAULT now(), -- Thời điểm gọi
    PRIMARY KEY (id, created_at)
) PARTITION BY RANGE (created_at);
COMMENT ON TABLE ai.usage_logs IS 'Log từng lần gọi AI: token, chi phí, độ trễ (không lưu nội dung prompt theo mặc định vì dữ liệu nhạy cảm)';
COMMENT ON COLUMN ai.usage_logs.id IS 'Khóa chính';
COMMENT ON COLUMN ai.usage_logs.tenant_id IS 'Tenant';
COMMENT ON COLUMN ai.usage_logs.module_code IS 'Phân hệ gọi';
COMMENT ON COLUMN ai.usage_logs.gateway_client_id IS 'Client nội bộ';
COMMENT ON COLUMN ai.usage_logs.user_id IS 'User cuối';
COMMENT ON COLUMN ai.usage_logs.prompt_template_id IS 'Prompt';
COMMENT ON COLUMN ai.usage_logs.prompt_version_id IS 'Phiên bản prompt';
COMMENT ON COLUMN ai.usage_logs.alias_code IS 'Alias được yêu cầu';
COMMENT ON COLUMN ai.usage_logs.model_id IS 'Model thực tế phục vụ';
COMMENT ON COLUMN ai.usage_logs.credential_id IS 'Key provider đã dùng';
COMMENT ON COLUMN ai.usage_logs.request_type IS 'Loại request';
COMMENT ON COLUMN ai.usage_logs.input_tokens IS 'Token đầu vào';
COMMENT ON COLUMN ai.usage_logs.output_tokens IS 'Token đầu ra';
COMMENT ON COLUMN ai.usage_logs.cached_tokens IS 'Token đọc từ cache';
COMMENT ON COLUMN ai.usage_logs.total_tokens IS 'Tổng token';
COMMENT ON COLUMN ai.usage_logs.estimated_cost_usd IS 'Chi phí ước tính (USD)';
COMMENT ON COLUMN ai.usage_logs.latency_ms IS 'Độ trễ tổng';
COMMENT ON COLUMN ai.usage_logs.time_to_first_token_ms IS 'Thời gian tới token đầu (streaming)';
COMMENT ON COLUMN ai.usage_logs.is_fallback IS 'Phục vụ bởi model fallback';
COMMENT ON COLUMN ai.usage_logs.is_cache_hit IS 'Trả về từ semantic cache';
COMMENT ON COLUMN ai.usage_logs.status IS 'Kết quả';
COMMENT ON COLUMN ai.usage_logs.error_code IS 'Mã lỗi';
COMMENT ON COLUMN ai.usage_logs.guardrail_codes IS 'Guardrail đã kích hoạt';
COMMENT ON COLUMN ai.usage_logs.trace_id IS 'Trace id';
COMMENT ON COLUMN ai.usage_logs.created_at IS 'Thời điểm gọi';
CREATE INDEX ix_ai_usage_tenant ON ai.usage_logs (tenant_id, created_at DESC);
CREATE INDEX ix_ai_usage_module ON ai.usage_logs (module_code, created_at DESC);

CREATE TABLE audit.login_logs (
    id                 uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    tenant_id          uuid,                             -- Tenant
    user_id            uuid,                             -- User (NULL nếu username không tồn tại)
    username_attempted varchar(255),                     -- Username/email đã nhập
    event_type         varchar(20) NOT NULL CHECK (event_type IN ('LOGIN_SUCCESS','LOGIN_FAILED','LOGOUT','TOKEN_REFRESH','TOKEN_REUSE_DETECTED','MFA_SUCCESS','MFA_FAILED','ACCOUNT_LOCKED','SSO_LOGIN','IMPERSONATE')), -- Sự kiện
    failure_reason     varchar(50),                      -- Lý do thất bại (INVALID_PASSWORD, USER_NOT_FOUND, ACCOUNT_LOCKED, IP_NOT_ALLOWED, DEVICE_LIMIT)
    auth_method        varchar(30),                      -- Phương thức (PASSWORD, GOOGLE, OTP)
    client_id          varchar(100),                     -- Ứng dụng
    session_id         uuid,                             -- Phiên tạo ra
    device_id          uuid,                             -- Thiết bị
    ip_address         inet,                             -- IP
    user_agent         text,                             -- User-Agent
    geo_country        varchar(2),                       -- Quốc gia (GeoIP)
    geo_city           varchar(100),                     -- Thành phố (GeoIP)
    is_suspicious      boolean NOT NULL DEFAULT false,   -- Đánh dấu bất thường (IP lạ, quốc gia lạ)
    occurred_at        timestamptz NOT NULL DEFAULT now(), -- Thời điểm
    PRIMARY KEY (id, occurred_at)
) PARTITION BY RANGE (occurred_at);
COMMENT ON TABLE audit.login_logs IS 'Lịch sử đăng nhập/đăng xuất/refresh token (thành công & thất bại) kèm IP, thiết bị';
COMMENT ON COLUMN audit.login_logs.id IS 'Khóa chính';
COMMENT ON COLUMN audit.login_logs.tenant_id IS 'Tenant';
COMMENT ON COLUMN audit.login_logs.user_id IS 'User (NULL nếu username không tồn tại)';
COMMENT ON COLUMN audit.login_logs.username_attempted IS 'Username/email đã nhập';
COMMENT ON COLUMN audit.login_logs.event_type IS 'Sự kiện';
COMMENT ON COLUMN audit.login_logs.failure_reason IS 'Lý do thất bại (INVALID_PASSWORD, USER_NOT_FOUND, ACCOUNT_LOCKED, IP_NOT_ALLOWED, DEVICE_LIMIT)';
COMMENT ON COLUMN audit.login_logs.auth_method IS 'Phương thức (PASSWORD, GOOGLE, OTP)';
COMMENT ON COLUMN audit.login_logs.client_id IS 'Ứng dụng';
COMMENT ON COLUMN audit.login_logs.session_id IS 'Phiên tạo ra';
COMMENT ON COLUMN audit.login_logs.device_id IS 'Thiết bị';
COMMENT ON COLUMN audit.login_logs.ip_address IS 'IP';
COMMENT ON COLUMN audit.login_logs.user_agent IS 'User-Agent';
COMMENT ON COLUMN audit.login_logs.geo_country IS 'Quốc gia (GeoIP)';
COMMENT ON COLUMN audit.login_logs.geo_city IS 'Thành phố (GeoIP)';
COMMENT ON COLUMN audit.login_logs.is_suspicious IS 'Đánh dấu bất thường (IP lạ, quốc gia lạ)';
COMMENT ON COLUMN audit.login_logs.occurred_at IS 'Thời điểm';
CREATE INDEX ix_login_logs_user ON audit.login_logs (user_id, occurred_at DESC);
CREATE INDEX ix_login_logs_tenant ON audit.login_logs (tenant_id, occurred_at DESC);
CREATE INDEX ix_login_logs_ip ON audit.login_logs (ip_address, occurred_at DESC);

CREATE TABLE audit.security_events (
    id             uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    tenant_id      uuid,                                 -- Tenant
    event_type     varchar(40) NOT NULL,                 -- Loại (PASSWORD_CHANGED, PASSWORD_RESET, USER_LOCKED, USER_UNLOCKED, ROLE_ASSIGNED, ROLE_REVOKED, ROLE_PERMISSION_CHANGED, MFA_ENABLED, API_KEY_CREATED, SESSION_REVOKED...)
    severity       varchar(10) NOT NULL DEFAULT 'INFO' CHECK (severity IN ('INFO','WARNING','CRITICAL')), -- Mức độ
    actor_user_id  uuid,                                 -- Người thực hiện (NULL = hệ thống)
    target_user_id uuid,                                 -- User bị tác động
    target_type    varchar(50),                          -- Loại đối tượng bị tác động (ROLE, API_KEY, OAUTH_CLIENT)
    target_id      uuid,                                 -- Id đối tượng
    description    text,                                 -- Mô tả dễ đọc
    details        jsonb,                                -- Chi tiết (quyền thêm/bớt, vai trò cũ/mới)
    ip_address     inet,                                 -- IP
    user_agent     text,                                 -- User-Agent
    trace_id       varchar(64),                          -- Trace id
    occurred_at    timestamptz NOT NULL DEFAULT now(),   -- Thời điểm
    PRIMARY KEY (id, occurred_at)
) PARTITION BY RANGE (occurred_at);
COMMENT ON TABLE audit.security_events IS 'Sự kiện bảo mật quan trọng: đổi/reset mật khẩu, khóa/mở khóa, thay đổi vai trò/quyền, cấp API key, bật/tắt 2FA';
COMMENT ON COLUMN audit.security_events.id IS 'Khóa chính';
COMMENT ON COLUMN audit.security_events.tenant_id IS 'Tenant';
COMMENT ON COLUMN audit.security_events.event_type IS 'Loại (PASSWORD_CHANGED, PASSWORD_RESET, USER_LOCKED, USER_UNLOCKED, ROLE_ASSIGNED, ROLE_REVOKED, ROLE_PERMISSION_CHANGED, MFA_ENABLED, API_KEY_CREATED, SESSION_REVOKED...)';
COMMENT ON COLUMN audit.security_events.severity IS 'Mức độ';
COMMENT ON COLUMN audit.security_events.actor_user_id IS 'Người thực hiện (NULL = hệ thống)';
COMMENT ON COLUMN audit.security_events.target_user_id IS 'User bị tác động';
COMMENT ON COLUMN audit.security_events.target_type IS 'Loại đối tượng bị tác động (ROLE, API_KEY, OAUTH_CLIENT)';
COMMENT ON COLUMN audit.security_events.target_id IS 'Id đối tượng';
COMMENT ON COLUMN audit.security_events.description IS 'Mô tả dễ đọc';
COMMENT ON COLUMN audit.security_events.details IS 'Chi tiết (quyền thêm/bớt, vai trò cũ/mới)';
COMMENT ON COLUMN audit.security_events.ip_address IS 'IP';
COMMENT ON COLUMN audit.security_events.user_agent IS 'User-Agent';
COMMENT ON COLUMN audit.security_events.trace_id IS 'Trace id';
COMMENT ON COLUMN audit.security_events.occurred_at IS 'Thời điểm';
CREATE INDEX ix_security_events_target ON audit.security_events (target_user_id, occurred_at DESC);
CREATE INDEX ix_security_events_tenant ON audit.security_events (tenant_id, event_type, occurred_at DESC);

CREATE TABLE audit.data_change_logs (
    id             uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    tenant_id      uuid,                                 -- Tenant
    company_id     uuid,                                 -- Công ty (lọc theo data scope)
    module_code    varchar(30) NOT NULL,                 -- Phân hệ
    entity_type    varchar(100) NOT NULL,                -- Loại đối tượng
    entity_id      varchar(100) NOT NULL,                -- Id đối tượng (text để hỗ trợ khóa không phải uuid)
    entity_display varchar(255),                         -- Tên/mã hiển thị (NV-2026-0001 - Nguyễn Văn A)
    action         varchar(10) NOT NULL CHECK (action IN ('INSERT','UPDATE','DELETE','RESTORE','APPROVE','EXPORT')), -- Hành động
    changed_fields text[],                               -- Danh sách trường thay đổi
    old_values     jsonb,                                -- Giá trị cũ {field: value}
    new_values     jsonb,                                -- Giá trị mới {field: value}
    reason         varchar(500),                         -- Lý do thay đổi (bắt buộc với một số nghiệp vụ)
    changed_by     uuid,                                 -- Người thay đổi
    changed_by_name varchar(150),                        -- Tên người thay đổi (snapshot)
    on_behalf_of   uuid,                                 -- Thay mặt (impersonation)
    source         varchar(15) NOT NULL DEFAULT 'WEB' CHECK (source IN ('WEB','MOBILE','API','IMPORT','JOB','WORKFLOW','SYSTEM')), -- Nguồn thay đổi
    ip_address     inet,                                 -- IP
    trace_id       varchar(64),                          -- Trace id liên kết request
    changed_at     timestamptz NOT NULL DEFAULT now(),   -- Thời điểm
    PRIMARY KEY (id, changed_at)
) PARTITION BY RANGE (changed_at);
COMMENT ON TABLE audit.data_change_logs IS 'Lịch sử thay đổi dữ liệu nhạy cảm (old/new value) phục vụ đối soát kế toán, kho, nhân sự. Các phân hệ gửi qua event hoặc ghi trực tiếp';
COMMENT ON COLUMN audit.data_change_logs.id IS 'Khóa chính';
COMMENT ON COLUMN audit.data_change_logs.tenant_id IS 'Tenant';
COMMENT ON COLUMN audit.data_change_logs.company_id IS 'Công ty (lọc theo data scope)';
COMMENT ON COLUMN audit.data_change_logs.module_code IS 'Phân hệ';
COMMENT ON COLUMN audit.data_change_logs.entity_type IS 'Loại đối tượng';
COMMENT ON COLUMN audit.data_change_logs.entity_id IS 'Id đối tượng (text để hỗ trợ khóa không phải uuid)';
COMMENT ON COLUMN audit.data_change_logs.entity_display IS 'Tên/mã hiển thị (NV-2026-0001 - Nguyễn Văn A)';
COMMENT ON COLUMN audit.data_change_logs.action IS 'Hành động';
COMMENT ON COLUMN audit.data_change_logs.changed_fields IS 'Danh sách trường thay đổi';
COMMENT ON COLUMN audit.data_change_logs.old_values IS 'Giá trị cũ {field: value}';
COMMENT ON COLUMN audit.data_change_logs.new_values IS 'Giá trị mới {field: value}';
COMMENT ON COLUMN audit.data_change_logs.reason IS 'Lý do thay đổi (bắt buộc với một số nghiệp vụ)';
COMMENT ON COLUMN audit.data_change_logs.changed_by IS 'Người thay đổi';
COMMENT ON COLUMN audit.data_change_logs.changed_by_name IS 'Tên người thay đổi (snapshot)';
COMMENT ON COLUMN audit.data_change_logs.on_behalf_of IS 'Thay mặt (impersonation)';
COMMENT ON COLUMN audit.data_change_logs.source IS 'Nguồn thay đổi';
COMMENT ON COLUMN audit.data_change_logs.ip_address IS 'IP';
COMMENT ON COLUMN audit.data_change_logs.trace_id IS 'Trace id liên kết request';
COMMENT ON COLUMN audit.data_change_logs.changed_at IS 'Thời điểm';
CREATE INDEX ix_data_change_entity ON audit.data_change_logs (entity_type, entity_id, changed_at DESC);
CREATE INDEX ix_data_change_user ON audit.data_change_logs (changed_by, changed_at DESC);
CREATE INDEX ix_data_change_tenant ON audit.data_change_logs (tenant_id, module_code, changed_at DESC);

CREATE TABLE audit.api_request_logs (
    id              uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    trace_id        varchar(64) NOT NULL,                -- Trace id (trả về client qua header X-Trace-Id)
    tenant_id       uuid,                                -- Tenant
    user_id         uuid,                                -- User
    client_id       varchar(100),                        -- OAuth client
    api_key_prefix  varchar(16),                         -- API key (tiền tố)
    route_code      varchar(100),                        -- Route khớp
    module_code     varchar(30),                         -- Phân hệ
    http_method     varchar(10) NOT NULL,                -- Method
    path            varchar(2000) NOT NULL,              -- Path
    query_string    text,                                -- Query (đã che tham số nhạy cảm)
    status_code     smallint NOT NULL,                   -- HTTP status
    error_code      varchar(50),                         -- Mã lỗi chuẩn (gateway.error_codes)
    duration_ms     int NOT NULL,                        -- Thời gian xử lý
    upstream_ms     int,                                 -- Thời gian upstream
    request_bytes   int,                                 -- Kích thước request
    response_bytes  int,                                 -- Kích thước response
    ip_address      inet,                                -- IP client
    user_agent      text,                                -- User-Agent
    is_rate_limited boolean NOT NULL DEFAULT false,      -- Bị chặn rate limit
    occurred_at     timestamptz NOT NULL DEFAULT now(),  -- Thời điểm
    PRIMARY KEY (id, occurred_at)
) PARTITION BY RANGE (occurred_at);
COMMENT ON TABLE audit.api_request_logs IS 'Log request đi qua API Gateway (thường đẩy song song sang Loki/ELK; bảng này giữ ngắn hạn để tra cứu nhanh theo traceId)';
COMMENT ON COLUMN audit.api_request_logs.id IS 'Khóa chính';
COMMENT ON COLUMN audit.api_request_logs.trace_id IS 'Trace id (trả về client qua header X-Trace-Id)';
COMMENT ON COLUMN audit.api_request_logs.tenant_id IS 'Tenant';
COMMENT ON COLUMN audit.api_request_logs.user_id IS 'User';
COMMENT ON COLUMN audit.api_request_logs.client_id IS 'OAuth client';
COMMENT ON COLUMN audit.api_request_logs.api_key_prefix IS 'API key (tiền tố)';
COMMENT ON COLUMN audit.api_request_logs.route_code IS 'Route khớp';
COMMENT ON COLUMN audit.api_request_logs.module_code IS 'Phân hệ';
COMMENT ON COLUMN audit.api_request_logs.http_method IS 'Method';
COMMENT ON COLUMN audit.api_request_logs.path IS 'Path';
COMMENT ON COLUMN audit.api_request_logs.query_string IS 'Query (đã che tham số nhạy cảm)';
COMMENT ON COLUMN audit.api_request_logs.status_code IS 'HTTP status';
COMMENT ON COLUMN audit.api_request_logs.error_code IS 'Mã lỗi chuẩn (gateway.error_codes)';
COMMENT ON COLUMN audit.api_request_logs.duration_ms IS 'Thời gian xử lý';
COMMENT ON COLUMN audit.api_request_logs.upstream_ms IS 'Thời gian upstream';
COMMENT ON COLUMN audit.api_request_logs.request_bytes IS 'Kích thước request';
COMMENT ON COLUMN audit.api_request_logs.response_bytes IS 'Kích thước response';
COMMENT ON COLUMN audit.api_request_logs.ip_address IS 'IP client';
COMMENT ON COLUMN audit.api_request_logs.user_agent IS 'User-Agent';
COMMENT ON COLUMN audit.api_request_logs.is_rate_limited IS 'Bị chặn rate limit';
COMMENT ON COLUMN audit.api_request_logs.occurred_at IS 'Thời điểm';
CREATE INDEX ix_api_logs_trace ON audit.api_request_logs (trace_id);
CREATE INDEX ix_api_logs_tenant ON audit.api_request_logs (tenant_id, occurred_at DESC);
CREATE INDEX ix_api_logs_errors ON audit.api_request_logs (occurred_at DESC) WHERE status_code >= 500;

CREATE TABLE monitor.health_checks (
    id            uuid NOT NULL DEFAULT gen_random_uuid(), -- Khóa chính
    service_id    uuid NOT NULL,                         -- Service
    status        varchar(15) NOT NULL CHECK (status IN ('HEALTHY','DEGRADED','UNHEALTHY')), -- Kết quả
    http_status   smallint,                              -- HTTP status
    response_ms   int,                                   -- Thời gian phản hồi
    details       jsonb,                                 -- Chi tiết từng dependency (db, redis, rabbitmq) trả về từ /health
    error         text,                                  -- Lỗi
    checked_at    timestamptz NOT NULL DEFAULT now(),    -- Thời điểm
    PRIMARY KEY (id, checked_at)
) PARTITION BY RANGE (checked_at);
COMMENT ON TABLE monitor.health_checks IS 'Kết quả từng lần health check';
COMMENT ON COLUMN monitor.health_checks.id IS 'Khóa chính';
COMMENT ON COLUMN monitor.health_checks.service_id IS 'Service';
COMMENT ON COLUMN monitor.health_checks.status IS 'Kết quả';
COMMENT ON COLUMN monitor.health_checks.http_status IS 'HTTP status';
COMMENT ON COLUMN monitor.health_checks.response_ms IS 'Thời gian phản hồi';
COMMENT ON COLUMN monitor.health_checks.details IS 'Chi tiết từng dependency (db, redis, rabbitmq) trả về từ /health';
COMMENT ON COLUMN monitor.health_checks.error IS 'Lỗi';
COMMENT ON COLUMN monitor.health_checks.checked_at IS 'Thời điểm';
CREATE INDEX ix_health_checks_service ON monitor.health_checks (service_id, checked_at DESC);

CREATE TABLE audit.login_logs_default       PARTITION OF audit.login_logs       DEFAULT;
CREATE TABLE audit.security_events_default  PARTITION OF audit.security_events  DEFAULT;
CREATE TABLE audit.data_change_logs_default PARTITION OF audit.data_change_logs DEFAULT;
CREATE TABLE audit.api_request_logs_default PARTITION OF audit.api_request_logs DEFAULT;
CREATE TABLE monitor.health_checks_default  PARTITION OF monitor.health_checks  DEFAULT;
CREATE TABLE ai.usage_logs_default          PARTITION OF ai.usage_logs          DEFAULT;
CREATE TABLE storage.file_access_logs_default PARTITION OF storage.file_access_logs DEFAULT;
SELECT core.create_monthly_partitions('audit.login_logs',        '2026-11-01', 3);
SELECT core.create_monthly_partitions('audit.security_events',   '2026-11-01', 3);
SELECT core.create_monthly_partitions('audit.data_change_logs',  '2026-11-01', 3);
SELECT core.create_monthly_partitions('audit.api_request_logs',  '2026-11-01', 3);
SELECT core.create_monthly_partitions('monitor.health_checks',   '2026-11-01', 3);
SELECT core.create_monthly_partitions('ai.usage_logs',           '2026-11-01', 3);
SELECT core.create_monthly_partitions('storage.file_access_logs','2026-11-01', 3);
