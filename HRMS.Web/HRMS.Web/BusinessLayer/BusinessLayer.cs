﻿using DocumentFormat.OpenXml.EMMA;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Buffers.Text;
using System.Data;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Drawing;
using System.Net;


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
        public string ConvertIFormFileToBase64(IFormFile file);
        public string EncodeStringBase64(string plainText);
        public string DecodeStringBase64(string base64EncodedData);
        public string GetSatutation();
        public string GetProfilePhoto();
   
    }


    public partial class BusinessLayer : IBusinessLayer
    {
        public string bearerToken { get; set; }
        private static readonly object Locker = new object();
        private HttpClient _httpClient;
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor;
        public string BaseAPIUrl { get; set; }
        public IConfiguration _configuration { get; set; }
        public BusinessLayer(IConfiguration configuration, Microsoft.AspNetCore.Http.IHttpContextAccessor HttpContextAccessor)
        {
            httpContextAccessor = HttpContextAccessor;
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
            //_httpClient.DefaultRequestHeaders.Clear();
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
                lock (Locker)
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + BearerToken);
                }
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
                case (int)Roles.Manager:
                    RootName = HRMS.Models.Common.Constants.ManageEmployee;
                    break;
                case (int)Roles.SuperAdmin:
                    RootName = HRMS.Models.Common.Constants.ManageAdmin;
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


        public string ConvertIFormFileToBase64(IFormFile file)
        {
            // Check if the file is not null and has content
            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Copy the file content into the memory stream
                    file.CopyTo(memoryStream);

                    // Convert the memory stream to byte array
                    byte[] fileBytes = memoryStream.ToArray();

                    // Convert byte array to Base64 string
                    string base64String = Convert.ToBase64String(fileBytes);

                    return base64String;
                }
            }

            return null;
        }


        public string EncodeStringBase64(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string DecodeStringBase64(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public string GetSatutation()
        {
            String Satutation = string.Empty;
            if (DateTime.Now.Hour <= 12)
            {
                Satutation = "Good Morning";
            }
            else if (DateTime.Now.Hour <= 16)
            {
                Satutation = "Good Afternoon";
            }
            else if (DateTime.Now.Hour <= 20)
            {

                Satutation = "Good Evening";
            }
            else
            {
                Satutation = "Good Evening";
            }
            return Satutation;
        }

        public string GetProfilePhoto()
        {
            var ProfilePhoto = "";
            if (!string.IsNullOrEmpty(httpContextAccessor.HttpContext.Session.GetString(HRMS.Models.Common.Constants.ProfilePhoto)))
            {
                ProfilePhoto =httpContextAccessor.HttpContext.Session.GetString(HRMS.Models.Common.Constants.ProfilePhoto);
            }
            else
            {
                ProfilePhoto = HRMS.Models.Common.Constants.NoImagePath;
            }
            return ProfilePhoto;
        }
     

    }



}
