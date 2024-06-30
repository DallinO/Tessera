using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Tessera.Constants;
using Tessera.Models;

namespace Tessera
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        bool HasOrganization { get; }
        Task<JObject> Login(LoginDefaultModel model);
        Task<JObject> Register(RegisterDefaultModel model);
        Task<JObject> CreateOrg(OrganizationModel model);
    }

    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;

        public AuthService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public bool IsAuthenticated { get; private set; } = false;
        public bool HasOrganization { get; private set; } = false;

        public async Task<JObject> Login(LoginDefaultModel model)
        {
            // Call API service to attempt login
            var result = await _apiService.Login(model);
            if (result["result"]?.ToString() == Keys.API_LOGIN_SUCC)
                IsAuthenticated = true;
            return result;
        }

        public async Task<JObject> Register(RegisterDefaultModel model)
        {
            // Call API service to attempt registration
            var result = await _apiService.Register(model);
            if (result["result"]?.ToString() == Keys.API_REG_SUCC)
                IsAuthenticated = true;
            return result;
        }

        public async Task<JObject> CreateOrg(OrganizationModel model)
        {
            var result = await _apiService.CreateOrg(model);
            return result;
        }
    }

}
