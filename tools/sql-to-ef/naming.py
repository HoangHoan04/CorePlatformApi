"""Naming helpers: table -> class name, column -> property name."""

SCHEMA_NS = {
    'org': 'Org', 'iam': 'Iam', 'gateway': 'Gateway', 'mdm': 'Mdm', 'storage': 'Storage',
    'notify': 'Notify', 'audit': 'Audit', 'monitor': 'Monitor', 'workflow': 'Workflow',
    'integration': 'Integration', 'ai': 'Ai', 'config': 'Config', 'job': 'Job',
}

# Explicit class names where the default singular form is ambiguous / too generic
CLASS_OVERRIDES = {
    'storage.providers': 'StorageProvider',
    'storage.buckets': 'StorageBucket',
    'storage.files': 'StorageFile',
    'notify.event_types': 'NotificationEventType',
    'notify.templates': 'NotificationTemplate',
    'notify.deliveries': 'NotificationDelivery',
    'notify.delivery_attachments': 'NotificationDeliveryAttachment',
    'notify.user_preferences': 'UserNotificationPreference',
    'notify.push_configs': 'PushConfig',
    'notify.smtp_configs': 'SmtpConfig',
    'notify.device_tokens': 'DeviceToken',
    'integration.event_types': 'IntegrationEventType',
    'workflow.definitions': 'WorkflowDefinition',
    'workflow.definition_versions': 'WorkflowDefinitionVersion',
    'workflow.steps': 'WorkflowStep',
    'workflow.step_approvers': 'WorkflowStepApprover',
    'workflow.transitions': 'WorkflowTransition',
    'workflow.instances': 'WorkflowInstance',
    'workflow.instance_steps': 'WorkflowInstanceStep',
    'workflow.tasks': 'WorkflowTask',
    'workflow.action_logs': 'WorkflowActionLog',
    'workflow.delegations': 'WorkflowDelegation',
    'job.definitions': 'JobDefinition',
    'job.tenant_overrides': 'JobTenantOverride',
    'job.executions': 'JobExecution',
    'ai.providers': 'AiProvider',
    'ai.provider_credentials': 'AiProviderCredential',
    'ai.models': 'AiModel',
    'ai.model_aliases': 'AiModelAlias',
    'ai.model_alias_targets': 'AiModelAliasTarget',
    'ai.prompt_templates': 'AiPromptTemplate',
    'ai.prompt_versions': 'AiPromptVersion',
    'ai.guardrail_policies': 'AiGuardrailPolicy',
    'ai.prompt_guardrails': 'AiPromptGuardrail',
    'ai.gateway_clients': 'AiGatewayClient',
    'ai.quotas': 'AiQuota',
    'ai.usage_logs': 'AiUsageLog',
    'ai.usage_daily': 'AiUsageDaily',
    'ai.knowledge_bases': 'KnowledgeBase',
    'ai.knowledge_base_access': 'KnowledgeBaseAccess',
    'ai.kb_documents': 'KbDocument',
    'ai.kb_chunks': 'KbChunk',
    'ai.conversations': 'AiConversation',
    'ai.messages': 'AiMessage',
    'gateway.clusters': 'GatewayCluster',
    'gateway.destinations': 'GatewayDestination',
    'gateway.routes': 'GatewayRoute',
    'gateway.error_codes': 'ErrorCode',
    'monitor.services': 'MonitoredService',
    'monitor.health_checks': 'HealthCheck',
    'monitor.incidents': 'Incident',
    'config.languages': 'Language',
    'config.translations': 'Translation',
    'mdm.uoms': 'Uom',
}

IRREGULAR_SINGULAR = {
    'aliases': 'alias', 'statuses': 'status', 'addresses': 'address', 'access': 'access',
    'daily': 'daily', 'data': 'data', 'policies': 'policy', 'histories': 'history',
    'currencies': 'currency', 'deliveries': 'delivery', 'buses': 'bus', 'indexes': 'index',
}


def pascal(s: str) -> str:
    return ''.join(p[:1].upper() + p[1:] for p in s.split('_') if p)


def singular_word(w: str) -> str:
    if w in IRREGULAR_SINGULAR:
        return IRREGULAR_SINGULAR[w]
    if w.endswith('ies'):
        return w[:-3] + 'y'
    if w.endswith(('sses', 'xes', 'ches', 'shes')):
        return w[:-2]
    if w.endswith('ss') or w.endswith('us') or w.endswith('is'):
        return w
    if w.endswith('s'):
        return w[:-1]
    return w


def class_name(t) -> str:
    if t.full in CLASS_OVERRIDES:
        return CLASS_OVERRIDES[t.full]
    parts = t.name.split('_')
    parts[-1] = singular_word(parts[-1])
    return pascal('_'.join(parts))


def plural(name: str) -> str:
    if name.endswith('y') and len(name) > 1 and name[-2] not in 'aeiou':
        return name[:-1] + 'ies'
    if name.endswith(('s', 'x', 'ch', 'sh')):
        return name + 'es'
    return name + 's'


def prop_name(col: str) -> str:
    return pascal(col)
