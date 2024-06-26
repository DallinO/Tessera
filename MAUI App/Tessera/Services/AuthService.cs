using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera_Models;

namespace Tessera
{
    public interface IAuthService
    {
        bool IsAuthenticated { get; }
        Task<bool> Login(LoginDefaultModel model);
        Task<bool> Register(RegisterDefaultModel model);
    }

    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;

        public AuthService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public bool IsAuthenticated { get; private set; }

        public async Task<bool> Login(LoginDefaultModel model)
        {
            // Call API service to attempt login
            var result = await _apiService.Login(model);

            // If login was successful, update IsAuthenticated
            if (result == "User Access Granted" || result == "Generic Success")
            {
                IsAuthenticated = true;
                return true;
            }

            return false; // Handle failed login case
        }

        public async Task<bool> Register(RegisterDefaultModel model)
        {
            // Call API service to attempt registration
            var result = await _apiService.Register(model);

            // If registration was successful, update IsAuthenticated
            if (result == "User created successfully")
            {
                IsAuthenticated = true;
                return true;
            }

            return false; // Handle failed registration case
        }
    }

}
