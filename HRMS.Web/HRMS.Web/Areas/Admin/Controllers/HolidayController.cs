using HRMS.Models;
using HRMS.Models.Common;
using HRMS.Web.BusinessLayer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Results = HRMS.Models.Common.Results;

namespace HRMS.Web.Areas.Admin.Controllers
{
    [Area(Constants.ManageAdmin)]
    [Authorize]
    public class HolidayController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBusinessLayer _businessLayer;
        private readonly ICheckUserFormPermission _CheckUserFormPermission;

        public HolidayController(ICheckUserFormPermission CheckUserFormPermission,IConfiguration configuration, IBusinessLayer businessLayer)
        {
            _configuration = configuration;
            _businessLayer = businessLayer;
            _CheckUserFormPermission = CheckUserFormPermission;
        }
        private int GetSessionInt(string key)
        {
            return int.TryParse(HttpContext.Session.GetString(key), out var value) ? value : 0;
        }
        public IActionResult HolidayListing()
        {
            HRMS.Models.Common.Results results = new HRMS.Models.Common.Results();
            var EmployeeID = GetSessionInt(Constants.EmployeeID);
            var RoleId = GetSessionInt(Constants.RoleID);

            var FormPermission = _CheckUserFormPermission.GetFormPermission(EmployeeID, (int)PageName.HolidayListing);
            if (FormPermission.HasPermission == 0 && RoleId != (int)Roles.Admin && RoleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View(results);     
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult HolidayListings(string sEcho, int iDisplayStart, int iDisplayLength, string sSearch, long? locationId)
        {
            HolidayInputParams HolidayParams = new HolidayInputParams();
            HolidayParams.LocationID = locationId;
            HolidayParams.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            var data = _businessLayer.SendPostAPIRequest(HolidayParams, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidayList), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var results = JsonConvert.DeserializeObject<Results>(data);
            results.Holiday.ForEach(x => x.EncodedId = _businessLayer.EncodeStringBase64(x.HolidayID.ToString()));
            return Json(new
            {
                data = results.Holiday,
                locations = results.JobLocationList.Select(j => new
                {
                    jobLocationID = j.Value,   
                    jobLocationName = j.Text
                })
            });
        }

        public IActionResult Index(string id)
        {
            HolidayModel HolidayModel = new HolidayModel();
            HolidayModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
            
            HolidayModel.FromDate = DateTime.Now;
            HolidayModel.ToDate = DateTime.Now;
            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);
                HolidayModel.HolidayID = Convert.ToInt64(id);
                var data = _businessLayer.SendPostAPIRequest(HolidayModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidays), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                HolidayModel = JsonConvert.DeserializeObject<Results>(data).holidayModel;
                var holidayResult = JsonConvert.DeserializeObject<Results>(data);
                HolidayModel = holidayResult.holidayModel ?? new HolidayModel();
                HolidayModel.JobLocationList = holidayResult.JobLocationList;

            }
            else
            {

                var data = _businessLayer.SendPostAPIRequest(
                    HolidayModel,
                    _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.GetAllHolidays),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true
                ).Result.ToString();

                var holidayResult = JsonConvert.DeserializeObject<Results>(data);
                HolidayModel.JobLocationList = holidayResult.JobLocationList;
            }
            return View(HolidayModel);
        }

        [HttpPost]
        public IActionResult Index(HolidayModel HolidayModel)
        {
            if (ModelState.IsValid)
            {
                HolidayModel.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));
                HolidayModel.UserID = Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));
                var data = _businessLayer.SendPostAPIRequest(HolidayModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.Holiday, APIApiActionConstants.AddUpdateHoliday), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
                var result = JsonConvert.DeserializeObject<Result>(data);

                if (HolidayModel.HolidayID > 0)
                {
                    SetSuccessToast("Holiday data Modified successfully.");
                    return RedirectToActionPermanent(WebControllarsConstants.HolidayListing, WebControllarsConstants.Holiday);
                }
                else
                {
                    SetSuccessToast("Holiday details Added successfully");
                    return RedirectToActionPermanent(WebControllarsConstants.HolidayListing, WebControllarsConstants.Holiday);
                }
            }
            else
            {
                SetWarningToast("Please check all data and try again.");
                return View(HolidayModel);
            }
        }

        private void SetSuccessToast(string message)
        {
            TempData[Constants.toastType] = Constants.toastTypeSuccess;
            TempData[Constants.toastMessage] = message;
        }

        private void SetWarningToast(string message)
        {
            TempData[Constants.toastType] = Constants.toastTypetWarning;
            TempData[Constants.toastMessage] = message;
        }
        #region Designation 
        public IActionResult DesignationListings()
        {
            Results results = new Results();

            var employeeID = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            var formPermission = _CheckUserFormPermission.GetFormPermission(
                employeeID,
                (int)PageName.DesignationListing); // Change page name

            if (formPermission.HasPermission == 0
                && roleId != (int)Roles.Admin
                && roleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult DesignationListings(
            string sEcho,
            int iDisplayStart,
            int iDisplayLength,
            string sSearch)
        {
            DesignationInputParams designationParams = new DesignationInputParams
            {
                CompanyID = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID))
            };

            var data = _businessLayer.SendPostAPIRequest(
                designationParams,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.GetAllDesignationList),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result.ToString();

            var results = JsonConvert.DeserializeObject<Results>(data);

            if (results?.DesignationList != null)
            {
                results.DesignationList.ForEach(x =>
                    x.EncodedId = _businessLayer.EncodeStringBase64(
                        x.DesignationID.ToString()));
            }

            return Json(new
            {
                data = results?.DesignationList ?? new List<DesignationModel>()
            });
        }

        public IActionResult AddDesignation(string id)
        {
            DesignationInputParams model = new DesignationInputParams();
            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            DesignationModel designationModel = new DesignationModel();

            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);

                model.DesignationID = Convert.ToInt64(id);

                var data = _businessLayer.SendPostAPIRequest(
                    model,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.GetDesignationDetails),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Results>(data);

                designationModel = result.designationModel ?? new DesignationModel();
                designationModel.DepartmentList = result.DepartmentList;
            }
            else
            {
                var data = _businessLayer.SendPostAPIRequest(
                    model,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.GetDesignationDetails),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Results>(data);

                designationModel.DepartmentList = result.DepartmentList;
                designationModel.IsActive = true;
            }

            return View(designationModel);
        }
        [HttpPost]
        public IActionResult AddDesignation(DesignationModel designationModel)
        {
            if (ModelState.IsValid)
            {
                designationModel.CompanyID =
                    Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                designationModel.UserID =
                    Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                var data = _businessLayer.SendPostAPIRequest(
                    designationModel,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.AddUpdateDesignation),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);

                if (designationModel.DesignationID > 0)
                {
                    SetSuccessToast("Designation modified successfully.");
                }
                else
                {
                    SetSuccessToast("Designation added successfully.");
                }

                return RedirectToAction(
                    WebControllarsConstants.DesignationListings,
                    WebControllarsConstants.Holiday);
            }

            SetWarningToast("Please check all data and try again.");

            // Reload dropdown when validation fails
            DesignationInputParams input = new DesignationInputParams
            {
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID))
            };

            var response = _businessLayer.SendPostAPIRequest(
                input,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.GetDesignationDetails),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result.ToString();

            var designationResult = JsonConvert.DeserializeObject<Results>(response);

            designationModel.DepartmentList = designationResult.DepartmentList;

            return View(designationModel);
        }

        [HttpPost]
        public JsonResult CheckDuplicateDesignation(string designationName, long designationId = 0)
        {
            if (string.IsNullOrEmpty(designationName))
            {
                return Json(new { isDuplicate = false });
            }

            DesignationModel model = new DesignationModel
            {
                DesignationID = designationId,
                Name = designationName,
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID))
            };

            var data = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.CheckDuplicateDesignation),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true);

            var result = JsonConvert.DeserializeObject<DuplicateDesignationResponse>(
                data.Result.ToString());

            return Json(new
            {
                isDuplicate = result?.IsDuplicate ?? false
            });
        }
        #endregion
        #region LOB 
        public IActionResult LOBListings()
        {
            Results results = new Results();

            var employeeID = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            var formPermission = _CheckUserFormPermission.GetFormPermission(
                employeeID,
                (int)PageName.LOBListing);

            if (formPermission.HasPermission == 0
                && roleId != (int)Roles.Admin
                && roleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult LOBListings(
            string sEcho,
            int iDisplayStart,
            int iDisplayLength,
            string sSearch)
        {
            LOBInputParams lobParams = new LOBInputParams
            {
                CompanyID = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID))
            };

            var data = _businessLayer.SendPostAPIRequest(
                lobParams,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.GetAllLOBList),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result.ToString();

            var results = JsonConvert.DeserializeObject<Results>(data);

            if (results?.LOBList != null)
            {
                results.LOBList.ForEach(x =>
                    x.EncodedId = _businessLayer.EncodeStringBase64(
                        x.LOBID.ToString()));
            }

            return Json(new
            {
                data = results?.LOBList ?? new List<LOBModel>()
            });
        }
        public IActionResult AddLOB(string id)
        {
            LOBInputParams model = new LOBInputParams();
            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            LOBModel lobModel = new LOBModel();

            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);

                model.LOBID = Convert.ToInt64(id);

                var data = _businessLayer.SendPostAPIRequest(
                    model,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.GetLOBDetails),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Results>(data);

                lobModel = result.lobModel ?? new LOBModel();
            }
            else
            {
                lobModel.IsActive = true;
            }

            return View(lobModel);
        }

        [HttpPost]
        public IActionResult AddLOB(LOBModel lobModel)
        {
            if (ModelState.IsValid)
            {
                lobModel.CompanyID =
                    Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                lobModel.UserID =
                    Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                var data = _businessLayer.SendPostAPIRequest(
                    lobModel,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.AddUpdateLOB),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);

                if (lobModel.LOBID > 0)
                    SetSuccessToast("LOB modified successfully.");
                else
                    SetSuccessToast("LOB added successfully.");

                return RedirectToAction(
                    WebControllarsConstants.LOBListings,
                    WebControllarsConstants.Holiday);
            }

            SetWarningToast("Please check all data and try again.");
            return View(lobModel);
        }
        [HttpPost]
        public JsonResult CheckDuplicateLOB(string lobName, long lobId = 0)
        {
            if (string.IsNullOrEmpty(lobName))
            {
                return Json(new { isDuplicate = false });
            }

            LOBModel model = new LOBModel
            {
                LOBID = lobId,
                Name = lobName,
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID))
            };

            var data = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.CheckDuplicateLOB),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true);

            var result = JsonConvert.DeserializeObject<DuplicateLOBResponse>(
                data.Result.ToString());

            return Json(new
            {
                isDuplicate = result?.IsDuplicate ?? false
            });
        }
        #endregion
        #region  SubDepartment
        public IActionResult SubDepartmentListings()
        {
            Results results = new Results();

            var employeeID = GetSessionInt(Constants.EmployeeID);
            var roleId = GetSessionInt(Constants.RoleID);

            var formPermission = _CheckUserFormPermission.GetFormPermission(
                employeeID,
                (int)PageName.SubDepartmentListing);

            if (formPermission.HasPermission == 0
                && roleId != (int)Roles.Admin
                && roleId != (int)Roles.SuperAdmin)
            {
                HttpContext.Session.Clear();
                HttpContext.SignOutAsync();

                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View(results);
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult SubDepartmentListings(
            string sEcho,
            int iDisplayStart,
            int iDisplayLength,
            string sSearch)
        {
            SubDepartmentInputParams subDeptParams = new SubDepartmentInputParams
            {
                CompanyID = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID))
            };

            var data = _businessLayer.SendPostAPIRequest(
                subDeptParams,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.GetAllSubDepartmentList),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result.ToString();

            var results = JsonConvert.DeserializeObject<Results>(data);

            if (results?.SubDepartmentList != null)
            {
                results.SubDepartmentList.ForEach(x =>
                    x.EncodedId = _businessLayer.EncodeStringBase64(
                        x.SubDepartmentID.ToString()));
            }

            return Json(new
            {
                data = results?.SubDepartmentList ?? new List<SubDepartmentModel>()
            });
        }
        public IActionResult AddSubDepartment(string id)
        {
            SubDepartmentInputParams model = new SubDepartmentInputParams();
            model.CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

            SubDepartmentModel subDeptModel = new SubDepartmentModel();

            if (!string.IsNullOrEmpty(id))
            {
                id = _businessLayer.DecodeStringBase64(id);

                model.SubDepartmentID = Convert.ToInt64(id);

                var data = _businessLayer.SendPostAPIRequest(
                    model,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.GetSubDepartmentDetails),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Results>(data);

                subDeptModel = result.subDepartmentModel ?? new SubDepartmentModel();
                subDeptModel.DepartmentList = result.DepartmentList;
            }
            else
            {
                var data = _businessLayer.SendPostAPIRequest(
                    model,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.GetSubDepartmentDetails),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Results>(data);

                subDeptModel.DepartmentList = result.DepartmentList;
                subDeptModel.IsActive = true;
            }

            return View(subDeptModel);
        }
        [HttpPost]
        public IActionResult AddSubDepartment(SubDepartmentModel subDeptModel)
        {
            if (ModelState.IsValid)
            {
                subDeptModel.CompanyID =
                    Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID));

                subDeptModel.UserID =
                    Convert.ToInt64(HttpContext.Session.GetString(Constants.EmployeeID));

                var data = _businessLayer.SendPostAPIRequest(
                    subDeptModel,
                    _businessLayer.GetFormattedAPIUrl(
                        APIControllarsConstants.Holiday,
                        APIApiActionConstants.AddUpdateSubDepartment),
                    HttpContext.Session.GetString(Constants.SessionBearerToken),
                    true).Result.ToString();

                var result = JsonConvert.DeserializeObject<Result>(data);

                if (subDeptModel.SubDepartmentID > 0)
                    SetSuccessToast("Sub Department modified successfully.");
                else
                    SetSuccessToast("Sub Department added successfully.");

                return RedirectToAction(
                    WebControllarsConstants.SubDepartmentListings,
                    WebControllarsConstants.Holiday);
            }

            SetWarningToast("Please check all data and try again.");

            SubDepartmentInputParams input = new SubDepartmentInputParams
            {
                CompanyID = Convert.ToInt64(HttpContext.Session.GetString(Constants.CompanyID))
            };

            var response = _businessLayer.SendPostAPIRequest(
                input,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.GetSubDepartmentDetails),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true).Result.ToString();

            var resultData = JsonConvert.DeserializeObject<Results>(response);

            subDeptModel.DepartmentList = resultData.DepartmentList;

            return View(subDeptModel);
        }
        [HttpPost]
        public JsonResult CheckDuplicateSubDepartment(
            string subDepartmentName,
            long subDepartmentId = 0,
            long departmentId = 0)
        {
            if (string.IsNullOrEmpty(subDepartmentName))
            {
                return Json(new { isDuplicate = false });
            }

            SubDepartmentModel model = new SubDepartmentModel
            {
                SubDepartmentID = subDepartmentId,
                DepartmentID = departmentId,
                Name = subDepartmentName,
                CompanyID = Convert.ToInt64(
                    HttpContext.Session.GetString(Constants.CompanyID))
            };

            var data = _businessLayer.SendPostAPIRequest(
                model,
                _businessLayer.GetFormattedAPIUrl(
                    APIControllarsConstants.Holiday,
                    APIApiActionConstants.CheckDuplicateSubDepartment),
                HttpContext.Session.GetString(Constants.SessionBearerToken),
                true);

            var result = JsonConvert.DeserializeObject<DuplicateSubDepartmentResponse>(
                data.Result.ToString());

            return Json(new
            {
                isDuplicate = result?.IsDuplicate ?? false
            });
        }
        #endregion
    }
}
