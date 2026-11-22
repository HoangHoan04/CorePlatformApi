-- Sinh tự động từ docs/base_erp_core.sql — các phần EF Core không biểu diễn được.
CREATE UNIQUE INDEX uq_tenants_code ON org.tenants (lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_tenants_subdomain ON org.tenants (lower(subdomain)) WHERE subdomain IS NOT NULL AND NOT is_deleted;
CREATE UNIQUE INDEX uq_companies_code ON org.companies (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_branches_code ON org.branches (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_departments_code ON org.departments (company_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_roles_code ON iam.roles (tenant_id, lower(code)) NULLS NOT DISTINCT WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_security_policies_default ON iam.security_policies ((tenant_id IS NULL)) WHERE tenant_id IS NULL;
CREATE UNIQUE INDEX uq_uoms_code ON mdm.uoms (tenant_id, lower(code)) NULLS NOT DISTINCT WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_work_calendars_code ON mdm.work_calendars (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_device_tokens ON notify.device_tokens (app_code, md5(token));
CREATE UNIQUE INDEX uq_wf_definitions_code ON workflow.definitions (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_inbound_sources_code ON integration.inbound_sources (tenant_id, lower(code)) WHERE NOT is_deleted;
CREATE UNIQUE INDEX uq_ai_kb_code ON ai.knowledge_bases (tenant_id, lower(code)) WHERE NOT is_deleted;
