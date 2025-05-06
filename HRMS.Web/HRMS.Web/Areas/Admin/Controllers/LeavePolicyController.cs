using DocumentFormat.OpenXml.EMMA;
using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.WhatsHappeningModel;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Results = HRMS.Models.Common.Results;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.HR + "," + RoleConstants.SuperAdmin)]
    public class LeavePolicyController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IS3Service _s3Service;
        private readonly IBusinessLayer _businessLayer;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        public LeavePolicyController(IConfiguration configuration, IBusinessLayer businessLayer, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IS3Service s3Service)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            Environment = environment;
            _s3Service = s3Service;        
        }

        public IActionResult LeavePolicyListing()
        {
            Results results = new Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult LeavePolicyListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            LeavePolicyInputParans leavePolicyParams = new LeavePolicyInputParans();
            leavePolicyParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            var data = _businessLayer.SendPostAPIRequest(leavePolicyParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);

            return Json(new { data = results.LeavePolicy });

        }

        public IActionResult Index(string id)
        {
            LeavePolicyModel leavePolicyModel = new LeavePolicyModel();
            leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            
                 
            if (!string.IsNullOrEmpty(id))
            {
                leavePolicyModel.LeavePolicyID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                leavePolicyModel = JsonConvert.DeserializeObject<Results>(data).leavePolicyModel;
            }

            return View(leavePolicyModel);
        }

        [HttpPost]
        public IActionResult Index(LeavePolicyModel leavePolicyModel)
        {
            //// Leaves less than maximum leaves
            //if (leavePolicyModel.MaximumEarnedLeaveAllowed + leavePolicyModel.MaximumMedicalLeaveAllocationAllowed + leavePolicyModel.MaximumCompOffLeaveAllocationAllowed > leavePolicyModel.MaximumLeaveAllocationAllowed)
            //{
            //    TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeError;
            //    TempData[HRMS.Models.Common.Constants.toastMessage] = "The Casual,Medical and Annual leaves must be equal or less than to Maximum leaves allowed.";
            //    return View();
            //}
             
                leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.AddUpdateLeavePolicy), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave Policy created successfully.";
                return RedirectToActionPermanent(WebControllarsConstants.LeavePolicyListing, WebControllarsConstants.LeavePolicy);
             
        }







        #region Leave Policy Details

        public IActionResult LeavePolicyDetailsListing()
        {
            Results results = new Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult LeavePolicyDetailsListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            LeavePolicyInputParans leavePolicyParams = new LeavePolicyInputParans();
            leavePolicyParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(leavePolicyParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetLeavePolicyList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            results.LeavePolicyDetailsList.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.Id.ToString()));

            return Json(new { data = results.LeavePolicyDetailsList });

        }

        public IActionResult LeavePolicyDetails(string id)
        {
            LeavePolicyDetailsModel leavePolicyModel = new LeavePolicyDetailsModel();
            leavePolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));


            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                leavePolicyModel.Id = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicyDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                leavePolicyModel = JsonConvert.DeserializeObject<Results>(data).LeavePolicyDetailsModel;
            }
            EmployeeInputParams employee = new EmployeeInputParams();
            var compay = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompaniesList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(compay);
            leavePolicyModel.Companies = results.Companies;


            LeavePolicyInputParans PolicyParams = new LeavePolicyInputParans();
            PolicyParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            var Policy = _businessLayer.SendPostAPIRequest(PolicyParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetPolicyCategoryList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var Policyresults = JsonConvert.DeserializeObject<Results>(Policy);
            leavePolicyModel.PolicyList = Policyresults.PolicyCategoryList;


            return View(leavePolicyModel);
        }

        [HttpPost]
        public IActionResult LeavePolicyDetails(LeavePolicyDetailsModel leavePolicyModel, List<IFormFile> postedFiles)
        {
            string fileName = null;

            if(leavePolicyModel.Description==null)
            {
                leavePolicyModel.Description = string.Empty;
            }
            if (postedFiles.Count > 0)
            {
                string wwwPath = Environment.WebRootPath;
                string contentPath = this.Environment.ContentRootPath;

                foreach (IFormFile postedFile in postedFiles)
                {
                    fileName = postedFile.FileName.Replace(" ", "");
                }
                if (fileName != null)
                {
                    leavePolicyModel.PolicyDocument = fileName;
                }
                else
                {
                    leavePolicyModel.PolicyDocument = "";

                }
            }
        
            var data = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.AddUpdateLeavePolicyDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<Result>(data);

            if (postedFiles.Count > 0)
            {
                string path = Path.Combine(this.Environment.WebRootPath, Constants.UploadCertificate + result.PKNo.ToString());

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (IFormFile postedFile in postedFiles)
                {
                    using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        postedFile.CopyTo(stream);
                    }
                }
            }


            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
            TempData[HRMS.Models.Common.Constants.toastMessage] = "Leave Policy created successfully.";
            return RedirectToActionPermanent(WebControllarsConstants.LeavePolicyDetailsListing, WebControllarsConstants.LeavePolicy);

        }


        [HttpGet]
        public IActionResult DeleteLeavesDetails(string id)
        {
            id = _businessLayer.DecodeStringBase64(id);
            LeavePolicyDetailsInputParams model = new LeavePolicyDetailsInputParams()
            {
                Id = Convert.ToInt64(id),
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.DeleteLeavePolicyDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.LeavePolicyDetailsListing, WebControllarsConstants.LeavePolicy);
        }

        #endregion Leave Policy Details



        #region Policy Category

        public IActionResult PolicyCategoryListing()
        {
            Results results = new Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult PolicyCategoryListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            LeavePolicyInputParans PolicyParams = new LeavePolicyInputParans();
            PolicyParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            var data = _businessLayer.SendPostAPIRequest(PolicyParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetPolicyCategoryList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            return Json(new { data = results.PolicyCategoryList });

        }

        public IActionResult PolicyCategoryDetails(string id)
        {
            PolicyCategoryModel  PolicyModel = new PolicyCategoryModel();
            PolicyModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            if (!string.IsNullOrEmpty(id))
            {
                PolicyModel.Id = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(PolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.GetAllPolicyCategory), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                PolicyModel = JsonConvert.DeserializeObject<Results>(data).PolicyCategoryModel;
            }
            EmployeeInputParams employee = new EmployeeInputParams();
            var compay = _businessLayer.SendPostAPIRequest(employee, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Company, APIApiActionConstants.GetAllCompaniesList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(compay);
            PolicyModel.Companies = results.Companies;
            return View(PolicyModel);
        }

        [HttpPost]
        public IActionResult PolicyCategoryDetails(PolicyCategoryModel PolicyModel)
        {

            var data = _businessLayer.SendPostAPIRequest(PolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.AddUpdatePolicyCategory), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<Result>(data);

            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
            TempData[HRMS.Models.Common.Constants.toastMessage] = "  Policy category created successfully.";
            return RedirectToActionPermanent(WebControllarsConstants.PolicyCategoryListing, WebControllarsConstants.LeavePolicy);

        }


        [HttpGet]
        public IActionResult DeletePolicyCategory(int id)
        {
            LeavePolicyDetailsInputParams model = new LeavePolicyDetailsInputParams()
            {
                Id = id,
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Employee, APIApiActionConstants.DeletePolicyCategory), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.PolicyCategoryListing, WebControllarsConstants.LeavePolicy);
        }

        #endregion   Policy Category




        #region Whatshappening Details

        public IActionResult WhatshappeningListing()
        {
            Results results = new Results();
            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult WhatshappeningListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch)
        {
            WhatsHappeningModelParans WhatsHappeningModelParams = new WhatsHappeningModelParans();
            WhatsHappeningModelParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(WhatsHappeningModelParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllWhatsHappeningDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            results.WhatsHappeningList.ForEach(x => x.EncodedWhatsHappeningID = _businessLayer.EncodeStringBase64(x.WhatsHappeningID.ToString()));
            results.WhatsHappeningList.ForEach(x => x.IconImage = _s3Service.GetFileUrl(x.IconImage));
            return Json(new { data = results.WhatsHappeningList });
        }

        public IActionResult AddWhatshappening(string id)
        {
            WhatsHappeningModels objModelParams = new WhatsHappeningModels();
            WhatsHappeningModelParans WhatsHappeningModelParams = new WhatsHappeningModelParans();
            WhatsHappeningModelParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                WhatsHappeningModelParams.WhatsHappeningID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(WhatsHappeningModelParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllWhatsHappeningDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                objModelParams = JsonConvert.DeserializeObject<Results>(data).WhatsHappeningModel;
                objModelParams.IconImage = _s3Service.GetFileUrl(objModelParams.IconImage);
            }
            return View(objModelParams);
        }

        [HttpPost]
        public IActionResult AddWhatshappening(WhatsHappeningModels objModel, List<IFormFile> postedFiles)
        {
            string s3uploadUrl = _configuration["AWS:S3UploadUrl"];
           

            if (objModel.Description == null)
            {
                objModel.Description = string.Empty;
            }
            string keyToDelete = objModel.IconImage;
            string uploadedKey = string.Empty;
            if (postedFiles != null && postedFiles.Count > 0)
            {
                foreach (IFormFile postedFile in postedFiles)
                {
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        uploadedKey = _s3Service.UploadFile(postedFile, postedFile.FileName);
                        if (!string.IsNullOrEmpty(uploadedKey))
                        {
                            if (keyToDelete != null)
                            {
                                _s3Service.DeleteFile(keyToDelete);
                            }
                            objModel.IconImage = uploadedKey;
                        }
                    }

                }
            }
            else
            {
                string fileWithQuery = objModel.IconImage.Substring(objModel.IconImage.LastIndexOf('/') + 1);
                objModel.IconImage = fileWithQuery.Split('?')[0];
            }
                objModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            objModel.CreatedBy = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

            var data = _businessLayer.SendPostAPIRequest(objModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.AddUpdateWhatsHappeningDetails), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var result = JsonConvert.DeserializeObject<Result>(data);         
            TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
            TempData[HRMS.Models.Common.Constants.toastMessage] = "Whats happening  created successfully.";
            return RedirectToActionPermanent(WebControllarsConstants.WhatshappeningListing, WebControllarsConstants.LeavePolicy);

        }


        [HttpGet]
        public IActionResult DeleteWhatshappening (string id)
        {
            id = _businessLayer.DecodeStringBase64(id);
         
            WhatsHappeningModelParans model = new WhatsHappeningModelParans()
            {
                WhatsHappeningID = Convert.ToInt64(id),
            };
            var data = _businessLayer.SendPostAPIRequest(model, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.DeleteWhatsHappening), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            if (data != null)
            {
                TempData[HRMS.Models.Common.Constants.toastType] = HRMS.Models.Common.Constants.toastTypeSuccess;
                TempData[HRMS.Models.Common.Constants.toastMessage] = data;
            }
            return RedirectToActionPermanent(WebControllarsConstants.WhatshappeningListing, WebControllarsConstants.LeavePolicy);
        }
        #endregion Whatshappening Details


    }
}
