using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;
using RandomPasswordGenerator.ViewModels;

namespace RandomPasswordGenerator.Test
{
    public class Tests
    {
        public IConfigurationRoot Configuration { get; set; }
        private readonly HttpClient _client;
        
        public Tests()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.BaseAddress = new Uri("http://localhost:5000/");

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets();

            Configuration = builder.Build();
        }

        [Fact]
        public async Task TestPassword() 
        {
            await Login("jt26@cfc.uk","ktbffh");

            // Post password
            var pass = new PasswordViewModel() 
            {
                Length = 5,
                LowerChars = true,
                UpperChars = true,
                Digits = true,
                Symbols = false,
                Hint = "ktbffh"
            };
            var id = 0;
            using (var response = await _client.PostAsJsonAsync("/api/password", pass))
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic res = JObject.Parse(responseString);
                Assert.Equal(true, (bool)res.success);
                id = (int)res.id;
            }

            // Get user's passwords
            using (var responseGet = await _client.GetAsync("/api/user/jt26/passwords"))
            {
                responseGet.EnsureSuccessStatusCode();
            }

            // Log out
            await _client.DeleteAsync("/api/logout/"+id);
        }

        [Fact]
        public async Task GetUser()
        {
            using (var responseGet = await _client.GetAsync("/api/user/jt26"))
            {
                responseGet.EnsureSuccessStatusCode();
            }
        }
        
        private async Task Login (string email, string password)
        {
            var login = new LoginViewModel() 
            {
                Email = email,
                Password = password
            };
            await _client.DeleteAsync("/api/logout");
            using (var responseLogin = await _client.PostAsJsonAsync("/api/login",login))
            {
                await responseLogin.Content.ReadAsStringAsync();
                responseLogin.EnsureSuccessStatusCode();
            }
        }
    }
}