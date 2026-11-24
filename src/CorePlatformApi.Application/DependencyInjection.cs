using System.Reflection;
using CorePlatformApi.Application.Features.Auth;
using CorePlatformApi.Application.Features.Dashboard;
using Microsoft.Extensions.DependencyInjection;

namespace CorePlatformApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        _ = services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        _ = services.AddMemoryCache();
        _ = services.AddScoped<Common.Localization.IAppLocalizer, Common.Localization.AppLocalizer>();

        // Danh mục dùng chung (MDM)
        _ = services.AddScoped<Features.MasterData.Countries.ICountryService, Features.MasterData.Countries.CountryService>();
        _ = services.AddScoped<Features.MasterData.Currencies.ICurrencyService, Features.MasterData.Currencies.CurrencyService>();
        _ = services.AddScoped<Features.MasterData.ExchangeRates.IExchangeRateService, Features.MasterData.ExchangeRates.ExchangeRateService>();
        _ = services.AddScoped<Features.MasterData.Banks.IBankService, Features.MasterData.Banks.BankService>();
        _ = services.AddScoped<Features.MasterData.AdministrativeUnits.IAdministrativeUnitService, Features.MasterData.AdministrativeUnits.AdministrativeUnitService>();
        _ = services.AddScoped<Features.MasterData.AdministrativeUnitMappings.IAdministrativeUnitMappingService, Features.MasterData.AdministrativeUnitMappings.AdministrativeUnitMappingService>();

        _ = services.AddScoped<IUserPermissionService, UserPermissionService>();
        _ = services.AddScoped<IAuthService, AuthService>();
        _ = services.AddScoped<IDashboardService, DashboardService>();
        _ = services.AddScoped<Features.Localization.ITranslationService, Features.Localization.TranslationService>();
        _ = services.AddScoped<Features.Localization.ILanguageService, Features.Localization.LanguageService>();
        _ = services.AddScoped<Features.Localization.ITranslationAdminService, Features.Localization.TranslationAdminService>();
        return services;
    }
}
