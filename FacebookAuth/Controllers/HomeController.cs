using FacebookAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Diagnostics;

namespace FacebookAuth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRestClient _restClient;
        private readonly IOptions<OAuthConfig> _oauthConfig;
        private readonly IOptions<AppConfig> _appConfig;

        private const string callbackUrl = "https://localhost:44300/Home/AuthCallback";

        public HomeController(ILogger<HomeController> logger, IRestClient restClient, IOptions<OAuthConfig> oauthConfig, IOptions<AppConfig> appConfig)
        {
            _logger = logger;
            _restClient = restClient;
            _oauthConfig = oauthConfig;
            _appConfig = appConfig;
        }

        public IActionResult Index()
        {            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        // Auth endpoint
        public IActionResult Auth()
        {
            // Redirecting call to Facebook endpoint to get authorization code
            return Redirect($"{_appConfig.Value.Code}?" +
                $"client_id={_oauthConfig.Value.ClientId}" +
                $"&redirect_uri={callbackUrl}" +
                $"&response_type=code");
        }


        // Auth callback endpoint
        public IActionResult AuthCallback()
        {
            // Retrieving code from querystring
            string code = HttpContext.Request.Query["code"];

            // Building token request
            IRestRequest tokenRequest = new RestRequest(_appConfig.Value.Token);
            tokenRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            tokenRequest.Method = Method.POST;

            // Parameters
            tokenRequest.AddParameter("code", code);
            tokenRequest.AddParameter("client_id", _oauthConfig.Value.ClientId);
            tokenRequest.AddParameter("client_secret", _oauthConfig.Value.ClientSecret);
            tokenRequest.AddParameter("grant_type", "authorization_code");
            tokenRequest.AddParameter("redirect_uri", callbackUrl);

            // Retrieving token from response
            IRestResponse tokenResponse = _restClient.Execute(tokenRequest);
            TokenResponse tokenResponseData = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse.Content);

            // Building request with access token to get user data
            IRestRequest userRequest = new RestRequest(_appConfig.Value.Me);
            userRequest.Method = Method.GET;
            userRequest.AddHeader("Authorization", $"Bearer {tokenResponseData.Token}");

            IRestResponse userResponse = _restClient.Execute(userRequest);

            // Retrieving user from response
            UserResponse user = JsonConvert.DeserializeObject<UserResponse>(userResponse.Content);

            // Do something with user data
            return View("UserDetails", user);
        }
    }
}
