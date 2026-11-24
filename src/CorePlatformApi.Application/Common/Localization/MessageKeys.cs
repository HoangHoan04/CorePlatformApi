using System.Reflection;

namespace CorePlatformApi.Application.Common.Localization;

/// <summary>
/// Khóa dịch mà API dùng (thông báo lỗi, tiêu đề cột Excel...). Chỉ khai báo KHÓA — nội dung từng ngôn ngữ
/// nhập ở màn hình Bản dịch (bảng translations). Nút "Đồng bộ khóa" tự đưa mọi khóa ở đây lên database
/// (nền tảng API) nhờ <see cref="All"/>, nên thêm khóa mới chỉ cần khai báo hằng số trong class này.
/// </summary>
public static class MessageKeys
{
    public static class Common
    {
        public const string Yes = "common.yes";
        public const string No = "common.no";
        public const string StatusActive = "common.statusActive";
        public const string StatusInactive = "common.statusInactive";
    }

    /// <summary>
    /// Lỗi kiểm tra dữ liệu dùng chung. Tham số: field (tên trường đã dịch), max, value.
    /// </summary>
    public static class Validation
    {
        public const string Required = "common.validation.required";
        public const string MaxLength = "common.validation.maxLength";
        public const string InvalidFormat = "common.validation.invalidFormat";
        public const string InvalidBoolean = "common.validation.invalidBoolean";
        public const string InvalidNumber = "common.validation.invalidNumber";
        public const string NotFoundValue = "common.validation.notFoundValue";
        public const string Duplicate = "common.validation.duplicate";
        public const string InvalidDate = "common.validation.invalidDate";
        public const string InvalidValue = "common.validation.invalidValue";
        public const string OutOfRange = "common.validation.outOfRange";
        public const string DateRange = "common.validation.dateRange";
        public const string MustBePositive = "common.validation.mustBePositive";
    }

    /// <summary>
    /// Nhập Excel dùng chung. Tham số: row (số dòng Excel), message, columns.
    /// </summary>
    public static class Import
    {
        public const string RowError = "common.import.rowError";
        public const string DuplicateInFile = "common.import.duplicateInFile";
        public const string FileRequired = "common.import.fileRequired";
        public const string InvalidFile = "common.import.invalidFile";
        public const string EmptyFile = "common.import.emptyFile";
        public const string MissingColumns = "common.import.missingColumns";
        public const string TooManyRows = "common.import.tooManyRows";
    }

    public static class Country
    {
        public const string EntityName = "mdm.country.entityName";
        public const string Code = "mdm.country.code";
        public const string Alpha3Code = "mdm.country.alpha3Code";
        public const string NumericCode = "mdm.country.numericCode";
        public const string Name = "mdm.country.name";
        public const string NameEn = "mdm.country.nameEn";
        public const string PhoneCode = "mdm.country.phoneCode";
        public const string CurrencyCode = "mdm.country.currencyCode";
        public const string FlagEmoji = "mdm.country.flagEmoji";
        public const string SortOrder = "mdm.country.sortOrder";
        public const string IsActive = "mdm.country.isActive";

        public const string NotFound = "mdm.country.notFound";
        public const string CodeExists = "mdm.country.codeExists";
        public const string Alpha3Exists = "mdm.country.alpha3Exists";
    }

    public static class Currency
    {
        public const string EntityName = "mdm.currency.entityName";
        public const string Code = "mdm.currency.code";
        public const string NumericCode = "mdm.currency.numericCode";
        public const string Name = "mdm.currency.name";
        public const string Symbol = "mdm.currency.symbol";
        public const string DecimalPlaces = "mdm.currency.decimalPlaces";
        public const string RoundingUnit = "mdm.currency.roundingUnit";
        public const string SymbolPosition = "mdm.currency.symbolPosition";
        public const string SortOrder = "mdm.currency.sortOrder";
        public const string IsActive = "mdm.currency.isActive";

        public const string NotFound = "mdm.currency.notFound";
        public const string CodeExists = "mdm.currency.codeExists";
    }

    public static class ExchangeRate
    {
        public const string EntityName = "mdm.exchangeRate.entityName";
        public const string FromCurrency = "mdm.exchangeRate.fromCurrency";
        public const string ToCurrency = "mdm.exchangeRate.toCurrency";
        public const string RateType = "mdm.exchangeRate.rateType";
        public const string Rate = "mdm.exchangeRate.rate";
        public const string EffectiveAt = "mdm.exchangeRate.effectiveAt";
        public const string Source = "mdm.exchangeRate.source";
        public const string Note = "mdm.exchangeRate.note";

        public const string NotFound = "mdm.exchangeRate.notFound";
        public const string Exists = "mdm.exchangeRate.exists";
        public const string SameCurrency = "mdm.exchangeRate.sameCurrency";
        public const string SharedReadOnly = "mdm.exchangeRate.sharedReadOnly";
        public const string NoRate = "mdm.exchangeRate.noRate";
    }

    public static class Bank
    {
        public const string EntityName = "mdm.bank.entityName";
        public const string CountryCode = "mdm.bank.countryCode";
        public const string Code = "mdm.bank.code";
        public const string Bin = "mdm.bank.bin";
        public const string ShortName = "mdm.bank.shortName";
        public const string Name = "mdm.bank.name";
        public const string SwiftCode = "mdm.bank.swiftCode";
        public const string LogoUrl = "mdm.bank.logoUrl";
        public const string SupportsVietqr = "mdm.bank.supportsVietqr";
        public const string IsActive = "mdm.bank.isActive";

        public const string NotFound = "mdm.bank.notFound";
        public const string CodeExists = "mdm.bank.codeExists";
    }

    public static class AdminUnit
    {
        public const string EntityName = "mdm.adminUnit.entityName";
        public const string CountryCode = "mdm.adminUnit.countryCode";
        public const string ParentCode = "mdm.adminUnit.parentCode";
        public const string Code = "mdm.adminUnit.code";
        public const string Name = "mdm.adminUnit.name";
        public const string FullName = "mdm.adminUnit.fullName";
        public const string NameEn = "mdm.adminUnit.nameEn";
        public const string UnitType = "mdm.adminUnit.unitType";
        public const string Level = "mdm.adminUnit.level";
        public const string PhoneAreaCode = "mdm.adminUnit.phoneAreaCode";
        public const string PostalCode = "mdm.adminUnit.postalCode";
        public const string Latitude = "mdm.adminUnit.latitude";
        public const string Longitude = "mdm.adminUnit.longitude";
        public const string EffectiveFrom = "mdm.adminUnit.effectiveFrom";
        public const string EffectiveTo = "mdm.adminUnit.effectiveTo";
        public const string SortOrder = "mdm.adminUnit.sortOrder";
        public const string IsActive = "mdm.adminUnit.isActive";

        public const string NotFound = "mdm.adminUnit.notFound";
        public const string CodeExists = "mdm.adminUnit.codeExists";
        public const string ParentNotFound = "mdm.adminUnit.parentNotFound";
        public const string ParentOtherCountry = "mdm.adminUnit.parentOtherCountry";
        public const string MaxLevelExceeded = "mdm.adminUnit.maxLevelExceeded";
        public const string ParentImmutable = "mdm.adminUnit.parentImmutable";
    }

    public static class AdminUnitMapping
    {
        public const string EntityName = "mdm.adminUnitMapping.entityName";
        public const string CountryCode = "mdm.adminUnitMapping.countryCode";
        public const string OldUnitCode = "mdm.adminUnitMapping.oldUnitCode";
        public const string NewUnitCode = "mdm.adminUnitMapping.newUnitCode";
        public const string OldUnit = "mdm.adminUnitMapping.oldUnit";
        public const string NewUnit = "mdm.adminUnitMapping.newUnit";
        public const string MappingType = "mdm.adminUnitMapping.mappingType";
        public const string EffectiveDate = "mdm.adminUnitMapping.effectiveDate";
        public const string LegalReference = "mdm.adminUnitMapping.legalReference";
        public const string Note = "mdm.adminUnitMapping.note";

        public const string NotFound = "mdm.adminUnitMapping.notFound";
        public const string Exists = "mdm.adminUnitMapping.exists";
        public const string SameUnit = "mdm.adminUnitMapping.sameUnit";
        public const string OtherCountry = "mdm.adminUnitMapping.otherCountry";
    }

    /// <summary>
    /// Mọi khóa khai báo trong class này (đọc bằng reflection) — dùng cho đồng bộ khóa.
    /// </summary>
    public static IReadOnlyList<string> All() =>
        typeof(MessageKeys).GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f is { IsLiteral: true, FieldType.Name: nameof(String) })
            .Select(f => (string)f.GetRawConstantValue()!)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
}
