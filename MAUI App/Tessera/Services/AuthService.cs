using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using Tessera.Constants;
using Tessera.Models;

namespace Tessera
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        Task<JObject> Login(LoginDefaultModel model);
        Task<string> Register(RegisterDefaultModel model);
    }

    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;

        public AuthService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public bool IsAuthenticated { get; private set; }

        public async Task<JObject> Login(LoginDefaultModel model)
        {
            // Call API service to attempt login
            var result = await _apiService.Login(model);
            if (result["result"]?.ToString() == Keys.API_LOGIN_SUCC)
                IsAuthenticated = true;
            return result;
        }

        public async Task<string> Register(RegisterDefaultModel model)
        {
            // Call API service to attempt registration
            var result = await _apiService.Register(model);

            // If registration was successful, update IsAuthenticated
            //if (result != null)
            //{
            //    if
            try
            {
                return result;
            }
            catch (NullReferenceException nullex)
            {
                return null;
            }
        }
    }

}
