using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace TexasMediaDart.Infrastructure.Http;

public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BearerTokenHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        var authorizationHeader =
            httpContext?
                .Request
                .Headers
                .Authorization
                .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authorizationHeader) &&
            AuthenticationHeaderValue.TryParse(
                authorizationHeader,
                out var authenticationHeader))
        {
            request.Headers.Authorization =
                authenticationHeader;
        }

        return base.SendAsync(
            request,
            cancellationToken);
    }
}