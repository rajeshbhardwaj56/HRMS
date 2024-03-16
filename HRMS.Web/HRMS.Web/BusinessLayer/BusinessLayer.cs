using HRMS.Models.Common;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace HRMS.Web.BusinessLayer
{

    public interface IBusinessLayer
    {
        public string bearerToken { get; set; }
        public IConfiguration _configuration { get; set; }
        public Task<object> SendPostAPIRequest(object body, string ActionUrl, string BearerToken, bool isTokenRequired = true);
        public Task<object> SendGetAPIRequest(string ActionUrl, string BearerToken, bool isTokenRequired = true);
        public string GetControllarNameByRole(int RoleID);
        public string GetAreaNameByRole(int RoleID);

        public string GetFormattedAPIUrl(string ApiControllarName, string APIActionName);
    }


    public partial class BusinessLayer : IBusinessLayer
    {
        public string bearerToken { get; set; }
        private static readonly object Locker = new object();
        private HttpClient _httpClient;
        public string BaseAPIUrl { get; set; }
        public IConfiguration _configuration { get; set; }
        public BusinessLayer(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            BaseAPIUrl = _configuration.GetSection("AppSettings").GetSection("BaseAPIUrl").Value;
        }

        public string GetFormattedAPIUrl(string ApiControllarName, string APIActionName)
        {
            return string.Format("{0}/{1}", ApiControllarName, APIActionName);
        }
        public async Task<object> SendPostAPIRequest(object body, string ActionUrl, string BearerToken, bool isTokenRequired = true)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            string apiUrl = GetFullAPIUrl(ActionUrl);
            var requestData = JsonConvert.SerializeObject(body);
            if (isTokenRequired && (_httpClient.DefaultRequestHeaders == null || _httpClient.DefaultRequestHeaders.Count() == 0))
            {
                lock (Locker)
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + BearerToken);
                }
            }
            var requestContent = new StringContent(requestData, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(apiUrl, requestContent);

            response.EnsureSuccessStatusCode();
            _httpClient.DefaultRequestHeaders.Clear();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<object> SendGetAPIRequest(string ActionUrl, string BearerToken, bool isTokenRequired = true)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            if (isTokenRequired && (_httpClient.DefaultRequestHeaders == null || _httpClient.DefaultRequestHeaders.Count() == 0))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + BearerToken);
            }

            string apiUrl = GetFullAPIUrl(ActionUrl);
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            _httpClient.DefaultRequestHeaders.Clear();
            return await response.Content.ReadAsStringAsync();
        }

        private string GetFullAPIUrl(string ActionUrl)
        {
            return BaseAPIUrl + ActionUrl;
        }

        public string GetAreaNameByRole(int RoleID)
        {
            string RootName = "";
            switch (RoleID)
            {
                case (int)Roles.Admin:
                    RootName = HRMS.Models.Common.Constants.ManageAdmin;
                    break;
                case (int)Roles.HR:
                    RootName = HRMS.Models.Common.Constants.ManageHR;
                    break;
                case (int)Roles.Employee:
                    RootName = HRMS.Models.Common.Constants.ManageEmployee;
                    break;
                default:
                    break;
            }
            return RootName;
        }


        public string GetControllarNameByRole(int RoleID)
        {
            string RootName = "DashBoard";

            return RootName;
        }
    }
}
