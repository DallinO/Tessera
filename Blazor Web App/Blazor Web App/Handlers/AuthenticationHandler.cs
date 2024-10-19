using Tessera.Web.Services;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Tessera.Web.Handlers
{
    public class AuthenticationHandler : DelegatingHandler
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly IJSRuntime _jsRuntime;
        private bool _refreshing;

        public AuthenticationHandler(IApiService apiService, IConfiguration configuration, IJSRuntime jsRuntime)
        {
            _apiService = apiService;
            _configuration = configuration;
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var jwt = await _jsRuntime.InvokeAsync<string>("getCookie", "JWT");
            var isToServer = request.RequestUri?.AbsoluteUri.StartsWith(_configuration["ServerUrl"] ?? "") ?? false;

            if (isToServer && !string.IsNullOrEmpty(jwt))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await base.SendAsync(request, cancellationToken);

            if (!_refreshing && !string.IsNullOrEmpty(jwt) && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                try
                {
                    _refreshing = true;

                    if (await _apiService.RefreshAsync())
                    {
                        jwt = await _jsRuntime.InvokeAsync<string>("getCookie", "JWT");

                        if (isToServer && !string.IsNullOrEmpty(jwt))
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
                finally
                {
                    _refreshing = false;
                }
            }

            return response;
            /* OLD CODE
            var jwt = await _apiService.GetJwtAsync();
            var isToServer = request.RequestUri?.AbsoluteUri.StartsWith(_configuration["ServerUrl"] ?? "") ?? false;

            if (isToServer && !string.IsNullOrEmpty(jwt))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await base.SendAsync(request, cancellationToken);

            if (!_refreshing && !string.IsNullOrEmpty(jwt) && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                try
                {
                    _refreshing = true;

                    if (await _apiService.RefreshAsync())
                    {
                        jwt = await _apiService.GetJwtAsync();

                        if (isToServer && !string.IsNullOrEmpty(jwt))
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

                        response = await base.SendAsync(request, cancellationToken);
                    }
                }
                finally
                {
                    _refreshing = false;
                }
            }

            return response; 
            */
        }
    }
}