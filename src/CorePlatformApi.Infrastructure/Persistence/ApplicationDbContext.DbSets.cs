// Nguồn schema: docs/base_erp_core.sql
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

namespace CorePlatformApi.Infrastructure.Persistence;

public partial class ApplicationDbContext
{
    #region org
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    #endregion

    #region iam
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<TenantModule> TenantModules => Set<TenantModule>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<PermissionAction> PermissionActions => Set<PermissionAction>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RolePermissionScope> RolePermissionScopes => Set<RolePermissionScope>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserOrgAssignment> UserOrgAssignments => Set<UserOrgAssignment>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<UserRecoveryCode> UserRecoveryCodes => Set<UserRecoveryCode>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();
    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();
    public DbSet<SecurityPolicy> SecurityPolicies => Set<SecurityPolicy>();
    public DbSet<OauthClient> OauthClients => Set<OauthClient>();
    public DbSet<ApiScope> ApiScopes => Set<ApiScope>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RevokedAccessToken> RevokedAccessTokens => Set<RevokedAccessToken>();
    public DbSet<SigningKey> SigningKeys => Set<SigningKey>();
    #endregion

    #region gateway
    public DbSet<GatewayCluster> GatewayClusters => Set<GatewayCluster>();
    public DbSet<GatewayDestination> GatewayDestinations => Set<GatewayDestination>();
    public DbSet<RateLimitPolicy> RateLimitPolicies => Set<RateLimitPolicy>();
    public DbSet<TenantRateLimit> TenantRateLimits => Set<TenantRateLimit>();
    public DbSet<CorsPolicy> CorsPolicies => Set<CorsPolicy>();
    public DbSet<SecurityHeaderPolicy> SecurityHeaderPolicies => Set<SecurityHeaderPolicy>();
    public DbSet<GatewayRoute> GatewayRoutes => Set<GatewayRoute>();
    public DbSet<ErrorCode> ErrorCodes => Set<ErrorCode>();
    #endregion

    #region mdm
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<AdministrativeUnit> AdministrativeUnits => Set<AdministrativeUnit>();
    public DbSet<AdministrativeUnitMapping> AdministrativeUnitMappings => Set<AdministrativeUnitMapping>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<TaxType> TaxTypes => Set<TaxType>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<UomCategory> UomCategories => Set<UomCategory>();
    public DbSet<Uom> Uoms => Set<Uom>();
    public DbSet<UomConversion> UomConversions => Set<UomConversion>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<WorkCalendar> WorkCalendars => Set<WorkCalendar>();
    public DbSet<WorkCalendarWeekday> WorkCalendarWeekdays => Set<WorkCalendarWeekday>();
    public DbSet<WorkCalendarException> WorkCalendarExceptions => Set<WorkCalendarException>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<NumberingRule> NumberingRules => Set<NumberingRule>();
    public DbSet<NumberingCounter> NumberingCounters => Set<NumberingCounter>();
    #endregion

    #region storage
    public DbSet<StorageProvider> StorageProviders => Set<StorageProvider>();
    public DbSet<StorageBucket> StorageBuckets => Set<StorageBucket>();
    public DbSet<UploadPolicy> UploadPolicies => Set<UploadPolicy>();
    public DbSet<StorageFile> StorageFiles => Set<StorageFile>();
    public DbSet<FileVariant> FileVariants => Set<FileVariant>();
    public DbSet<FileShareLink> FileShareLinks => Set<FileShareLink>();
    public DbSet<FileAccessLog> FileAccessLogs => Set<FileAccessLog>();
    #endregion

    #region notify
    public DbSet<NotificationEventType> NotificationEventTypes => Set<NotificationEventType>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<SmtpConfig> SmtpConfigs => Set<SmtpConfig>();
    public DbSet<PushConfig> PushConfigs => Set<PushConfig>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<NotificationDeliveryAttachment> NotificationDeliveryAttachments => Set<NotificationDeliveryAttachment>();
    public DbSet<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();
    #endregion

    #region audit
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<AuditPolicy> AuditPolicies => Set<AuditPolicy>();
    public DbSet<DataChangeLog> DataChangeLogs => Set<DataChangeLog>();
    public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();
    #endregion

    #region monitor
    public DbSet<MonitoredService> MonitoredServices => Set<MonitoredService>();
    public DbSet<HealthCheck> HealthChecks => Set<HealthCheck>();
    public DbSet<Incident> Incidents => Set<Incident>();
    #endregion

    #region workflow
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowDefinitionVersion> WorkflowDefinitionVersions => Set<WorkflowDefinitionVersion>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowStepApprover> WorkflowStepApprovers => Set<WorkflowStepApprover>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowInstanceStep> WorkflowInstanceSteps => Set<WorkflowInstanceStep>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<WorkflowActionLog> WorkflowActionLogs => Set<WorkflowActionLog>();
    public DbSet<WorkflowDelegation> WorkflowDelegations => Set<WorkflowDelegation>();
    #endregion

    #region integration
    public DbSet<IntegrationEventType> IntegrationEventTypes => Set<IntegrationEventType>();
    public DbSet<EventSubscription> EventSubscriptions => Set<EventSubscription>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<InboundSource> InboundSources => Set<InboundSource>();
    public DbSet<InboundRequest> InboundRequests => Set<InboundRequest>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<ExternalMapping> ExternalMappings => Set<ExternalMapping>();
    #endregion

    #region ai
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();
    public DbSet<AiProviderCredential> AiProviderCredentials => Set<AiProviderCredential>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<AiModelAlias> AiModelAliases => Set<AiModelAlias>();
    public DbSet<AiModelAliasTarget> AiModelAliasTargets => Set<AiModelAliasTarget>();
    public DbSet<AiPromptTemplate> AiPromptTemplates => Set<AiPromptTemplate>();
    public DbSet<AiPromptVersion> AiPromptVersions => Set<AiPromptVersion>();
    public DbSet<AiGuardrailPolicy> AiGuardrailPolicies => Set<AiGuardrailPolicy>();
    public DbSet<AiPromptGuardrail> AiPromptGuardrails => Set<AiPromptGuardrail>();
    public DbSet<AiGatewayClient> AiGatewayClients => Set<AiGatewayClient>();
    public DbSet<AiQuota> AiQuotas => Set<AiQuota>();
    public DbSet<AiUsageLog> AiUsageLogs => Set<AiUsageLog>();
    public DbSet<AiUsageDaily> AiUsageDailies => Set<AiUsageDaily>();
    public DbSet<KnowledgeBase> KnowledgeBases => Set<KnowledgeBase>();
    public DbSet<KnowledgeBaseAccess> KnowledgeBaseAccesses => Set<KnowledgeBaseAccess>();
    public DbSet<KbDocument> KbDocuments => Set<KbDocument>();
    public DbSet<KbChunk> KbChunks => Set<KbChunk>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    #endregion

    #region config
    public DbSet<SettingDefinition> SettingDefinitions => Set<SettingDefinition>();
    public DbSet<SettingValue> SettingValues => Set<SettingValue>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<FeatureFlagRule> FeatureFlagRules => Set<FeatureFlagRule>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<TranslationKey> TranslationKeys => Set<TranslationKey>();
    public DbSet<Translation> Translations => Set<Translation>();
    #endregion

    #region job
    public DbSet<JobDefinition> JobDefinitions => Set<JobDefinition>();
    public DbSet<JobTenantOverride> JobTenantOverrides => Set<JobTenantOverride>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();
    #endregion
}
