using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.LeavePolicy;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace HRMS.Web.Areas.HR.Controllers
{
    [Area(Constants.ManageHR)]
    [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.SuperAdmin))]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        private IHostingEnvironment Environment;
        IHttpContextAccessor _context;
        private readonly IS3Service _s3Service;

        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer, IHostingEnvironment _environment, IHttpContextAccessor context, IS3Service s3Service)
        {
            Environment = _environment;
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
            _s3Service = s3Service;
        }
        public async Task<IActionResult> Index()
        {
            var session = _context.HttpContext.Session;

            var companyId = Convert.ToInt64(session.GetString(Constants.CompanyID));
            var employeeId = Convert.ToInt64(session.GetString(Constants.EmployeeID));
            var roleId = Convert.ToInt64(session.GetString(Constants.RoleID));
            var token = session.GetString(Constants.SessionBearerToken);

            var inputParams = new DashBoardModelInputParams
            {
                EmployeeID = employeeId,
                RoleID = roleId
            };

            var apiUrl = _businessLayer.GetFormattedAPIUrl(
                APIControllarsConstants.DashBoard,
                APIApiActionConstants.GetDashBoardModel
            );

            var apiResponse = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, token, true);
            var model = JsonConvert.DeserializeObject<DashBoardModel>(apiResponse?.ToString());

            if (model == null)
                return View();

            // Update employee photos if profile photo exists
            if (!string.IsNullOrEmpty(model.ProfilePhoto) && model.EmployeeDetails != null)
            {
                foreach (var employee in model.EmployeeDetails.Where(e => !string.IsNullOrEmpty(e.EmployeePhoto)))
                {
                    employee.EmployeePhoto = _s3Service.GetFileUrl(employee.EmployeePhoto);
                }
            }

            // Update WhatsHappening icons
            if (model.WhatsHappening != null)
            {
                foreach (var item in model.WhatsHappening.Where(w => !string.IsNullOrEmpty(w.IconImage)))
                {
                    item.IconImage = _s3Service.GetFileUrl(item.IconImage);
                }
            }

            // Leave calculation
            var leavePolicy = GetLeavePolicyData(companyId, model.LeavePolicyId ?? 0);
            var joiningDate = model.JoiningDate.GetValueOrDefault();
            var maxLeaves = leavePolicy.Annual_MaximumLeaveAllocationAllowed;

            var accruedLeaves = CalculateAccruedLeaveForCurrentFiscalYear(joiningDate, maxLeaves);
            var usedLeaves = Convert.ToDouble(model.TotalLeave);
            var carryForward = leavePolicy.Annual_IsCarryForward ? Convert.ToDouble(model.CarryForword) : 0.0;

            var totalAvailableLeaves = accruedLeaves - usedLeaves + carryForward;
            model.NoOfLeaves = Convert.ToInt64(totalAvailableLeaves);

            // Optionally store profile photo in session
            // session.SetString(Constants.ProfilePhoto, model.ProfilePhoto);

            return View(model);
        }



        private LeavePolicyModel GetLeavePolicyData(long companyId, long leavePolicyId)
        {
            var leavePolicyModel = new LeavePolicyModel { CompanyID = companyId, LeavePolicyID = leavePolicyId };
            var leavePolicyDataJson = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var leavePolicyModelResult = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicyDataJson).leavePolicyModel;
            return leavePolicyModelResult;
        }
        private double CalculateAccruedLeaveForCurrentFiscalYear(DateTime joinDate, int Annual_MaximumLeaveAllocationAllowed)
        {
            // Define financial year start and end based on the current date
            DateTime today = DateTime.Today;
            //DateTime today = new DateTime(2024, 6, 14);
            // DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1); // Start from April 1st of current financial year
            DateTime fiscalYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1); // Start from April 1st of current financial year
            DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-11); // March 31st of the next year

            // Annual entitlement and accrual per month

            double annualLeaveEntitlement = Annual_MaximumLeaveAllocationAllowed;
            double monthlyAccrual = annualLeaveEntitlement / 12;

            double totalAccruedLeave = 0;

            // If the join date is before the fiscal year start, adjust it to the start of the fiscal year
            if (joinDate < fiscalYearStart)
            {
                joinDate = fiscalYearStart;
            }

            // Start from the month of joining and calculate leave for completed months up to today
            DateTime current = new DateTime(joinDate.Year, joinDate.Month, 1);

            while (current <= today)
            {
                // Get the last day of the current month
                int daysInMonth = DateTime.DaysInMonth(current.Year, current.Month);
                DateTime lastDayOfMonth = new DateTime(current.Year, current.Month, daysInMonth).AddDays(-11);

                // Adjust the comparison in the current month
                if (current.Month == today.Month && current.Year == today.Year)
                {
                    // Special case: Compare the days worked in the current month up to today
                    int daysWorkedInMonth = (today - joinDate).Days + 1;

                    // Accrue leave if more than 10 days worked
                    if (daysWorkedInMonth > Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]))
                    {
                        totalAccruedLeave += monthlyAccrual;
                    }


                }
                else
                {
                    // For past months, compare the join date with the last day of the month
                    if (joinDate <= lastDayOfMonth)
                    {
                        int daysWorkedInMonth = (lastDayOfMonth - joinDate).Days + 1;
                        if (daysWorkedInMonth > Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]))
                        {
                            totalAccruedLeave += monthlyAccrual;
                        }
                    }
                }

                // Move to the next month
                current = current.AddMonths(1);

                // Adjust join date to the 1st of the next month after the first iteration
                if (current.Month > joinDate.Month || current.Year > joinDate.Year)
                {
                    joinDate = new DateTime(current.Year, current.Month, 1);
                }
            }

            return totalAccruedLeave;
        }
    }
}
