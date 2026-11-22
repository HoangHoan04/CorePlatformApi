using CorePlatformApi.Domain.Entities.Ai;
using CorePlatformApi.Domain.Entities.Audit;
using CorePlatformApi.Domain.Entities.Config;
using CorePlatformApi.Domain.Entities.Gateway;
using CorePlatformApi.Domain.Entities.Iam;
using CorePlatformApi.Domain.Entities.Integration;
using CorePlatformApi.Domain.Entities.Job;
using CorePlatformApi.Domain.Entities.Mdm;
using CorePlatformApi.Domain.Entities.Monitor;
using CorePlatformApi.Domain.Entities.Notify;
using CorePlatformApi.Domain.Entities.Org;
using CorePlatformApi.Domain.Entities.Storage;
using CorePlatformApi.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;

namespace CorePlatformApi.Application.Common.Interfaces;

/// <summary>
/// Truy cập dữ liệu nền tảng lõi (schema theo docs/base_erp_core.sql).
/// </summary>
public interface IApplicationDbContext
{
    #region org
    DbSet<Tenant> Tenants { get; }
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Department> Departments { get; }
    #endregion

    #region iam
    DbSet<Module> Modules { get; }
    DbSet<TenantModule> TenantModules { get; }
    DbSet<Resource> Resources { get; }
    DbSet<PermissionAction> PermissionActions { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<Role> Roles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RolePermissionScope> RolePermissionScopes { get; }
    DbSet<User> Users { get; }
    DbSet<UserOrgAssignment> UserOrgAssignments { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<UserPermissionOverride> UserPermissionOverrides { get; }
    DbSet<UserExternalLogin> UserExternalLogins { get; }
    DbSet<UserRecoveryCode> UserRecoveryCodes { get; }
    DbSet<PasswordHistory> PasswordHistories { get; }
    DbSet<VerificationToken> VerificationTokens { get; }
    DbSet<SecurityPolicy> SecurityPolicies { get; }
    DbSet<OauthClient> OauthClients { get; }
    DbSet<ApiScope> ApiScopes { get; }
    DbSet<UserDevice> UserDevices { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<RevokedAccessToken> RevokedAccessTokens { get; }
    DbSet<SigningKey> SigningKeys { get; }
    #endregion

    #region gateway
    DbSet<GatewayCluster> GatewayClusters { get; }
    DbSet<GatewayDestination> GatewayDestinations { get; }
    DbSet<RateLimitPolicy> RateLimitPolicies { get; }
    DbSet<TenantRateLimit> TenantRateLimits { get; }
    DbSet<CorsPolicy> CorsPolicies { get; }
    DbSet<SecurityHeaderPolicy> SecurityHeaderPolicies { get; }
    DbSet<GatewayRoute> GatewayRoutes { get; }
    DbSet<ErrorCode> ErrorCodes { get; }
    #endregion

    #region mdm
    DbSet<Country> Countries { get; }
    DbSet<AdministrativeUnit> AdministrativeUnits { get; }
    DbSet<AdministrativeUnitMapping> AdministrativeUnitMappings { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<Bank> Banks { get; }
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<TaxType> TaxTypes { get; }
    DbSet<TaxRate> TaxRates { get; }
    DbSet<UomCategory> UomCategories { get; }
    DbSet<Uom> Uoms { get; }
    DbSet<UomConversion> UomConversions { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<WorkCalendar> WorkCalendars { get; }
    DbSet<WorkCalendarWeekday> WorkCalendarWeekdays { get; }
    DbSet<WorkCalendarException> WorkCalendarExceptions { get; }
    DbSet<DocumentType> DocumentTypes { get; }
    DbSet<NumberingRule> NumberingRules { get; }
    DbSet<NumberingCounter> NumberingCounters { get; }
    #endregion

    #region storage
    DbSet<StorageProvider> StorageProviders { get; }
    DbSet<StorageBucket> StorageBuckets { get; }
    DbSet<UploadPolicy> UploadPolicies { get; }
    DbSet<StorageFile> StorageFiles { get; }
    DbSet<FileVariant> FileVariants { get; }
    DbSet<FileShareLink> FileShareLinks { get; }
    DbSet<FileAccessLog> FileAccessLogs { get; }
    #endregion

    #region notify
    DbSet<NotificationEventType> NotificationEventTypes { get; }
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<SmtpConfig> SmtpConfigs { get; }
    DbSet<PushConfig> PushConfigs { get; }
    DbSet<DeviceToken> DeviceTokens { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<UserNotification> UserNotifications { get; }
    DbSet<NotificationDelivery> NotificationDeliveries { get; }
    DbSet<NotificationDeliveryAttachment> NotificationDeliveryAttachments { get; }
    DbSet<UserNotificationPreference> UserNotificationPreferences { get; }
    #endregion

    #region audit
    DbSet<LoginLog> LoginLogs { get; }
    DbSet<SecurityEvent> SecurityEvents { get; }
    DbSet<AuditPolicy> AuditPolicies { get; }
    DbSet<DataChangeLog> DataChangeLogs { get; }
    DbSet<ApiRequestLog> ApiRequestLogs { get; }
    #endregion

    #region monitor
    DbSet<MonitoredService> MonitoredServices { get; }
    DbSet<HealthCheck> HealthChecks { get; }
    DbSet<Incident> Incidents { get; }
    #endregion

    #region workflow
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowDefinitionVersion> WorkflowDefinitionVersions { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<WorkflowStepApprover> WorkflowStepApprovers { get; }
    DbSet<WorkflowTransition> WorkflowTransitions { get; }
    DbSet<WorkflowInstance> WorkflowInstances { get; }
    DbSet<WorkflowInstanceStep> WorkflowInstanceSteps { get; }
    DbSet<WorkflowTask> WorkflowTasks { get; }
    DbSet<WorkflowActionLog> WorkflowActionLogs { get; }
    DbSet<WorkflowDelegation> WorkflowDelegations { get; }
    #endregion

    #region integration
    DbSet<IntegrationEventType> IntegrationEventTypes { get; }
    DbSet<EventSubscription> EventSubscriptions { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<InboxMessage> InboxMessages { get; }
    DbSet<WebhookEndpoint> WebhookEndpoints { get; }
    DbSet<WebhookDelivery> WebhookDeliveries { get; }
    DbSet<InboundSource> InboundSources { get; }
    DbSet<InboundRequest> InboundRequests { get; }
    DbSet<ApiKey> ApiKeys { get; }
    DbSet<Connection> Connections { get; }
    DbSet<ExternalMapping> ExternalMappings { get; }
    #endregion

    #region ai
    DbSet<AiProvider> AiProviders { get; }
    DbSet<AiProviderCredential> AiProviderCredentials { get; }
    DbSet<AiModel> AiModels { get; }
    DbSet<AiModelAlias> AiModelAliases { get; }
    DbSet<AiModelAliasTarget> AiModelAliasTargets { get; }
    DbSet<AiPromptTemplate> AiPromptTemplates { get; }
    DbSet<AiPromptVersion> AiPromptVersions { get; }
    DbSet<AiGuardrailPolicy> AiGuardrailPolicies { get; }
    DbSet<AiPromptGuardrail> AiPromptGuardrails { get; }
    DbSet<AiGatewayClient> AiGatewayClients { get; }
    DbSet<AiQuota> AiQuotas { get; }
    DbSet<AiUsageLog> AiUsageLogs { get; }
    DbSet<AiUsageDaily> AiUsageDailies { get; }
    DbSet<KnowledgeBase> KnowledgeBases { get; }
    DbSet<KnowledgeBaseAccess> KnowledgeBaseAccesses { get; }
    DbSet<KbDocument> KbDocuments { get; }
    DbSet<KbChunk> KbChunks { get; }
    DbSet<AiConversation> AiConversations { get; }
    DbSet<AiMessage> AiMessages { get; }
    #endregion

    #region config
    DbSet<SettingDefinition> SettingDefinitions { get; }
    DbSet<SettingValue> SettingValues { get; }
    DbSet<FeatureFlag> FeatureFlags { get; }
    DbSet<FeatureFlagRule> FeatureFlagRules { get; }
    DbSet<Language> Languages { get; }
    DbSet<TranslationKey> TranslationKeys { get; }
    DbSet<Translation> Translations { get; }
    #endregion

    #region job
    DbSet<JobDefinition> JobDefinitions { get; }
    DbSet<JobTenantOverride> JobTenantOverrides { get; }
    DbSet<JobExecution> JobExecutions { get; }
    #endregion

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
