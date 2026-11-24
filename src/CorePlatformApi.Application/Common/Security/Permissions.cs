namespace CorePlatformApi.Application.Common.Security;

/// <summary>
/// Mã quyền (iam.permissions.code) dạng MODULE.RESOURCE.ACTION — khớp với PERMISSION_CODES ở CorePlatformAdmin.
/// Platform Admin luôn có toàn quyền; người dùng khác nhận quyền qua vai trò (iam.role_permissions)
/// hoặc quyền ngoại lệ (iam.user_permission_overrides).
/// </summary>
public static class Permissions
{
    public const string SystemView = "CORE.SYSTEM.VIEW";

    public const string LanguageView = "CORE.LANGUAGE.VIEW";
    public const string LanguageCreate = "CORE.LANGUAGE.CREATE";
    public const string LanguageUpdate = "CORE.LANGUAGE.UPDATE";
    public const string LanguageDelete = "CORE.LANGUAGE.DELETE";

    public const string TranslationView = "CORE.TRANSLATION.VIEW";
    public const string TranslationUpdate = "CORE.TRANSLATION.UPDATE";
    public const string TranslationImport = "CORE.TRANSLATION.IMPORT";
    public const string TranslationExport = "CORE.TRANSLATION.EXPORT";

    // Danh mục dùng chung — quốc gia (mẫu CRUD)
    public const string CountryView = "CORE.COUNTRY.VIEW";
    public const string CountryCreate = "CORE.COUNTRY.CREATE";
    public const string CountryUpdate = "CORE.COUNTRY.UPDATE";
    public const string CountryImport = "CORE.COUNTRY.IMPORT";
    public const string CountryExport = "CORE.COUNTRY.EXPORT";

    public const string CurrencyView = "CORE.CURRENCY.VIEW";
    public const string CurrencyCreate = "CORE.CURRENCY.CREATE";
    public const string CurrencyUpdate = "CORE.CURRENCY.UPDATE";
    public const string CurrencyImport = "CORE.CURRENCY.IMPORT";
    public const string CurrencyExport = "CORE.CURRENCY.EXPORT";

    public const string ExchangeRateView = "CORE.EXCHANGE_RATE.VIEW";
    public const string ExchangeRateCreate = "CORE.EXCHANGE_RATE.CREATE";
    public const string ExchangeRateUpdate = "CORE.EXCHANGE_RATE.UPDATE";
    public const string ExchangeRateDelete = "CORE.EXCHANGE_RATE.DELETE";
    public const string ExchangeRateImport = "CORE.EXCHANGE_RATE.IMPORT";
    public const string ExchangeRateExport = "CORE.EXCHANGE_RATE.EXPORT";

    public const string BankView = "CORE.BANK.VIEW";
    public const string BankCreate = "CORE.BANK.CREATE";
    public const string BankUpdate = "CORE.BANK.UPDATE";
    public const string BankImport = "CORE.BANK.IMPORT";
    public const string BankExport = "CORE.BANK.EXPORT";

    public const string AdminUnitView = "CORE.ADMIN_UNIT.VIEW";
    public const string AdminUnitCreate = "CORE.ADMIN_UNIT.CREATE";
    public const string AdminUnitUpdate = "CORE.ADMIN_UNIT.UPDATE";
    public const string AdminUnitImport = "CORE.ADMIN_UNIT.IMPORT";
    public const string AdminUnitExport = "CORE.ADMIN_UNIT.EXPORT";

    public const string AdminUnitMappingView = "CORE.ADMIN_UNIT_MAPPING.VIEW";
    public const string AdminUnitMappingCreate = "CORE.ADMIN_UNIT_MAPPING.CREATE";
    public const string AdminUnitMappingUpdate = "CORE.ADMIN_UNIT_MAPPING.UPDATE";
    public const string AdminUnitMappingDelete = "CORE.ADMIN_UNIT_MAPPING.DELETE";
    public const string AdminUnitMappingImport = "CORE.ADMIN_UNIT_MAPPING.IMPORT";
    public const string AdminUnitMappingExport = "CORE.ADMIN_UNIT_MAPPING.EXPORT";

    /// <summary>
    /// Chức năng (iam.resources) của module CORE và các hành động được cấp quyền — dùng để seed iam.permissions.
    /// </summary>
    public static readonly IReadOnlyList<ResourceDefinition> CoreResources =
    [
        new("SYSTEM", "Tình trạng hệ thống", "/core/system", ["VIEW"]),
        new("LANGUAGE", "Cài đặt ngôn ngữ", "/config/i18n/languages", ["VIEW", "CREATE", "UPDATE", "DELETE"]),
        new("TRANSLATION", "Bản dịch", "/config/i18n/translations", ["VIEW", "UPDATE", "IMPORT", "EXPORT"]),
        new("COUNTRY", "Quốc gia", "/mdm/geography/countries", ["VIEW", "CREATE", "UPDATE", "IMPORT", "EXPORT"]),
        new("CURRENCY", "Tiền tệ", "/mdm/currency/currencies", ["VIEW", "CREATE", "UPDATE", "IMPORT", "EXPORT"]),
        new("EXCHANGE_RATE", "Tỷ giá", "/mdm/currency/exchange_rates", ["VIEW", "CREATE", "UPDATE", "DELETE", "IMPORT", "EXPORT"]),
        new("BANK", "Ngân hàng", "/mdm/currency/banks", ["VIEW", "CREATE", "UPDATE", "IMPORT", "EXPORT"]),
        new("ADMIN_UNIT", "Đơn vị hành chính", "/mdm/geography/administrative_units", ["VIEW", "CREATE", "UPDATE", "IMPORT", "EXPORT"]),
        new("ADMIN_UNIT_MAPPING", "Ánh xạ sáp nhập đơn vị hành chính", "/mdm/geography/administrative_unit_mappings",
            ["VIEW", "CREATE", "UPDATE", "DELETE", "IMPORT", "EXPORT"]),
    ];

    public const string CoreModule = "CORE";

    public static string Code(string resource, string action) => $"{CoreModule}.{resource}.{action}";
}

public sealed record ResourceDefinition(string Code, string Name, string Route, IReadOnlyList<string> Actions);
