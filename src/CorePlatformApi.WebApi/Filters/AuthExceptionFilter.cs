using CorePlatformApi.Application.Common.Exceptions;
using CorePlatformApi.Application.Common.Localization;
using CorePlatformApi.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CorePlatformApi.WebApi.Filters;

/// <summary>
/// Lỗi nghiệp vụ (sai mật khẩu, tài khoản khóa, dữ liệu không hợp lệ...) là tình huống dự kiến:
/// trả về { code, message } với HTTP status tương ứng, không ghi log lỗi kèm stack trace.
/// Message là khóa dịch -> dịch theo ngôn ngữ của request (Accept-Language) từ bảng translations.
/// Áp dụng cho <see cref="AppException"/> và lớp con (VD: AuthException).
/// </summary>
public sealed class AuthExceptionFilter(IAppLocalizer localizer) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not AppException error)
        {
            return;
        }

        var message = await localizer.GetAsync(error.Message, error.Args, context.HttpContext.RequestAborted);
        context.Result = new ObjectResult(new ApiError(error.Code, message)) { StatusCode = error.StatusCode };
        context.ExceptionHandled = true;
    }
}
