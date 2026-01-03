using System.Collections;
using System.Linq;
using Ideageek.Examiner.Api.Models;
using Microsoft.AspNetCore.Http;

namespace Ideageek.Examiner.Api.Helpers;

public static class ApiResponseFactory
{
    private const string DefaultSuccessMessage = "success";

    public static ApiResponse<T> Success<T>(T? value, string? message = null, int statusCode = StatusCodes.Status200OK, int? count = null)
        => Build(value, statusCode, false, message ?? DefaultSuccessMessage, count);

    public static ApiResponse<T> Failure<T>(int statusCode, string message, T? value = default, int? count = null)
        => Build(value, statusCode, true, message, count);

    public static int ComputeCount<T>(T? value, int? countOverride = null)
    {
        if (countOverride.HasValue)
        {
            return countOverride.Value;
        }

        if (value is null)
        {
            return 0;
        }

        if (value is string)
        {
            return 1;
        }

        if (value is ICollection collection)
        {
            return collection.Count;
        }

        if (value is IEnumerable enumerable)
        {
            return enumerable.Cast<object?>().Count();
        }

        return 1;
    }

    private static ApiResponse<T> Build<T>(T? value, int statusCode, bool isError, string message, int? countOverride)
    {
        return new ApiResponse<T>
        {
            Code = statusCode,
            Date = DateTime.Now.ToString("dddd, d MMMM yyyy"),
            Error = isError,
            Message = message,
            Value = value,
            Count = ComputeCount(value, countOverride)
        };
    }
}
