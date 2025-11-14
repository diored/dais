using Dais.Core;

using Microsoft.AspNetCore.Http;

namespace Dais.Core;

public static class Extensions
{
    extension(QueryString queryString)
    {
        public QueryString AddIfNotEmpty(string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return queryString.Add(name, value);
            }

            return queryString;
        }
    }

    extension(ReadOnlySpan<byte> byteArray)
    {
        public string ToBase64Url()
        {
            return Convert.ToBase64String(byteArray)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }

    extension(IResult inner)
    {
        public IResult WithHeader(string key, string value)
        {
            return new HeaderResult(inner, key, value);
        }
    }

    private sealed class HeaderResult(IResult inner, string key, string value) : IResult
    {
        public async Task ExecuteAsync(HttpContext context)
        {
            context.Response.Headers.Append(key, value);
            await inner.ExecuteAsync(context);
        }
    }
}