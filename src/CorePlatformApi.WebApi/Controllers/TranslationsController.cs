using CorePlatformApi.Application.Common.Interfaces;
using CorePlatformApi.Application.Features.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorePlatformApi.WebApi.Controllers;

/// <summary>
/// Bản dịch giao diện (đa ngôn ngữ) lấy từ config.translation_keys / config.translations.
/// </summary>
[ApiController]
[Route("api/translations")]
public class TranslationsController(ITranslationService translations, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>
    /// Toàn bộ bản dịch của một ngôn ngữ, dạng object lồng nhau theo namespace (dùng cho ngx-translate).
    /// </summary>
    /// <param name="lang">Mã ngôn ngữ: vi, en hoặc mã đầy đủ vi-VN, en-US.</param>
    [HttpGet("{lang}")]
    [AllowAnonymous]
    [ProducesResponseType<Dictionary<string, object>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, object>>> Get(string lang, CancellationToken ct) =>
        Ok(await translations.GetAsync(lang, currentUser.TenantId, ct));
}
