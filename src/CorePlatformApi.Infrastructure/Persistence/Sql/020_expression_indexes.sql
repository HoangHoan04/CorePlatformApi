-- Sinh tự động từ docs/base_erp_core.sql (đã làm phẳng về schema public) — các phần EF Core không biểu diễn được.
CREATE UNIQUE INDEX uq_tenants_code ON tenants (lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_tenants_subdomain ON tenants (lower(subdomain)) WHERE subdomain IS NOT NULL AND NOT is_deleted;
CREATE UNIQUE INDEX uq_companies_code ON companies (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_branches_code ON branches (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_departments_code ON departments (company_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_roles_code ON roles (tenant_id, lower(code)) NULLS NOT DISTINCT WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_security_policies_default ON security_policies ((tenant_id IS NULL)) WHERE tenant_id IS NULL;
CREATE UNIQUE INDEX uq_uoms_code ON uoms (tenant_id, lower(code)) NULLS NOT DISTINCT WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_work_calendars_code ON work_calendars (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_device_tokens ON device_tokens (app_code, md5(token));
CREATE UNIQUE INDEX uq_wf_definitions_code ON workflow_definitions (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_inbound_sources_code ON inbound_sources (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_ai_kb_code ON knowledge_bases (tenant_id, lower(code)) WHERE NOT is_deleted;
