using Ideageek.Examiner.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ideageek.Examiner.Api.Helpers;

public static class ApiResponseExtensions
{
    public static ActionResult<ApiResponse<T>> ApiOk<T>(this ControllerBase controller, T? value, string? message = null, int? count = null)
        => controller.ToApiResponse(ApiResponseFactory.Success(value, message, StatusCodes.Status200OK, count));

    public static ActionResult<ApiResponse<T>> ApiCreated<T>(this ControllerBase controller, T? value = default, string? message = null)
        => controller.ToApiResponse(ApiResponseFactory.Success(value, message, StatusCodes.Status201Created));

    public static ActionResult<ApiResponse<T>> ApiNotFound<T>(this ControllerBase controller, string message = "not found")
        => controller.ToApiResponse(ApiResponseFactory.Failure<T>(StatusCodes.Status404NotFound, message));

    public static ActionResult<ApiResponse<T>> ApiBadRequest<T>(this ControllerBase controller, string message)
        => controller.ToApiResponse(ApiResponseFactory.Failure<T>(StatusCodes.Status400BadRequest, message));

    public static ActionResult<ApiResponse<T>> ApiUnauthorized<T>(this ControllerBase controller, string message = "unauthorized")
        => controller.ToApiResponse(ApiResponseFactory.Failure<T>(StatusCodes.Status401Unauthorized, message));

    public static ActionResult<ApiResponse<T>> ApiServerError<T>(this ControllerBase controller, string message)
        => controller.ToApiResponse(ApiResponseFactory.Failure<T>(StatusCodes.Status500InternalServerError, message));

    public static ActionResult<ApiResponse<T>> ToApiResponse<T>(this ControllerBase controller, ApiResponse<T> response)
        => controller.StatusCode(response.Code, response);
}
