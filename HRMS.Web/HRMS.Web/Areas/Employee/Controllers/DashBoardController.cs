using HRMS.Models.Common;
using HRMS.Models.DashBoard;
using HRMS.Models.LeavePolicy;
using HRMS.Web.BusinessLayer;
using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HRMS.Web.Areas.Employee.Controllers
{
    [Authorize]
    [Area(Constants.ManageEmployee )]
  //  [Authorize(Roles = (RoleConstants.HR + "," + RoleConstants.Admin + "," + RoleConstants.Employee + "," + RoleConstants.Manager))]
    public class DashBoardController : Controller
    {
        IConfiguration _configuration;
        IBusinessLayer _businessLayer;
        IHttpContextAccessor _context;
        private readonly IS3Service _s3Service;
        public DashBoardController(IConfiguration configuration, IBusinessLayer businessLayer,   IHttpContextAccessor context, IS3Service s3Service)
        {
            _configuration = configuration;
            _context = context;
            _businessLayer = businessLayer;
            _s3Service = s3Service;
         
        } 

        public async Task<IActionResult> Index()
        {
          
            var session = HttpContext.Session;
            var companyId = Convert.ToInt64(session.GetString(Constants.CompanyID));
            var employeeId = Convert.ToInt64(session.GetString(Constants.EmployeeID));
            var roleId = Convert.ToInt64(session.GetString(Constants.RoleID));
            var jobLocationId = Convert.ToInt64(session.GetString(Constants.JobLocationID));
            var token = session.GetString(Constants.SessionBearerToken);

            var inputParams = new DashBoardModelInputParams
            {
                EmployeeID = employeeId,
                RoleID = roleId,
                JobLocationId = jobLocationId
            };

            var apiUrl = _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.DashBoard, APIApiActionConstants.GetDashBoardModel);
            var apiResponse = await _businessLayer.SendPostAPIRequest(inputParams, apiUrl, token, true);
            var model = JsonConvert.DeserializeObject<DashBoardModel>(apiResponse?.ToString());

            // Update Employee Photos URLs
            if (model?.EmployeeDetails != null)
            {
                foreach (var employee in model.EmployeeDetails.Where(x => !string.IsNullOrEmpty(x.EmployeePhoto)))
                {
                    employee.EmployeePhoto = _s3Service.GetFileUrl(employee.EmployeePhoto);
                }
            }

            // Update WhatsHappening Icon URLs
            if (model?.WhatsHappening != null)
            {
                foreach (var item in model.WhatsHappening.Where(x => !string.IsNullOrEmpty(x.IconImage)))
                {
                    item.IconImage = _s3Service.GetFileUrl(item.IconImage);
                }
            }

            if (model != null)
            {
                var leavePolicy = GetLeavePolicyData(companyId, model.LeavePolicyId ?? 0);

                // Fiscal year start date (March 21)
                DateTime today = DateTime.Today;
                DateTime fiscalYearStart = (today.Month > 3 || (today.Month == 3 && today.Day >= 21))
                    ? new DateTime(today.Year, 3, 21)
                    : new DateTime(today.Year - 1, 3, 21);

                // Filter approved Annual and Medical leaves from current fiscal year
                var approvedLeaves = model.leaveResults?.leavesSummary?
                    .Where(x => x.StartDate >= fiscalYearStart &&
                                x.LeaveStatusID == (int)LeaveStatus.Approved &&
                                (x.LeaveTypeID == (int)LeaveType.AnnualLeavel || x.LeaveTypeID == (int)LeaveType.MedicalLeave))
                    .ToList();

                decimal approvedLeaveDays = approvedLeaves?.Sum(x => x.NoOfDays) ?? 0.0m;
                double approvedLeaveTotal = (double)approvedLeaveDays;
                const double maxAnnualLeaveLimit = 30;

                if (leavePolicy != null && model.JoiningDate != null)
                {
                    DateTime joinDate = model.JoiningDate.Value;

                    double accruedLeave = 0;
                    double carryForward = 0;

                    // Only calculate accrued leave if approved leave < max limit
                    if (approvedLeaveTotal < maxAnnualLeaveLimit)
                    {
                        accruedLeave = CalculateAccruedLeaveForCurrentFiscalYear(joinDate, leavePolicy.Annual_MaximumLeaveAllocationAllowed);
                        if (leavePolicy.Annual_IsCarryForward)
                        {
                            carryForward = Convert.ToDouble(model.CarryForword);
                        }
                    }

                    // Total earned leave capped by max limit
                    double totalEarnedLeave = Math.Min(accruedLeave + carryForward, maxAnnualLeaveLimit);

                    // Remaining leave = total earned minus approved leaves (minimum 0)
                    double remainingLeave = Math.Max(totalEarnedLeave - approvedLeaveTotal, 0);

                    // Assign values to model for the View
                    //model.TotalLeave = (decimal)approvedLeaveTotal;            // Leaves already taken
                    model.NoOfLeaves = Convert.ToInt64(remainingLeave);        // Leaves remaining (available)
                    ViewBag.NoOfLeaves = remainingLeave;
                    // Optional: Pass consecutive allowed days to ViewBag for UI use
                    ViewBag.ConsecutiveAllowedDays = Convert.ToDecimal(leavePolicy.Annual_MaximumConsecutiveLeavesAllowed);
                }
            }

            return View(model);
        }

         
        private LeavePolicyModel GetLeavePolicyData(long companyId, long leavePolicyId)
        {
            var leavePolicyModel = new LeavePolicyModel { CompanyID = companyId, LeavePolicyID = leavePolicyId };
            var leavePolicyDataJson = _businessLayer.SendPostAPIRequest(leavePolicyModel, _businessLayer.GetFormattedAPIUrl(APIControllarsConstants.LeavePolicy, APIApiActionConstants.GetAllLeavePolicies), HttpContext.Session.GetString(Constants.SessionBearerToken), true).Result.ToString();
            var leavePolicyModelResult = JsonConvert.DeserializeObject<HRMS.Models.Common.Results>(leavePolicyDataJson).leavePolicyModel;
            return leavePolicyModelResult;
        }
        private double CalculateAccruedLeaveForCurrentFiscalYear(DateTime joinDate, int annualMaxLeaveAllocation)
        {
            DateTime today = DateTime.Today;

            DateTime fiscalYearStart = new DateTime(
                (today.Month > 3 || (today.Month == 3 && today.Day >= 21)) ? today.Year : today.Year - 1,
                3,
                21);

            DateTime fiscalYearEnd = fiscalYearStart.AddYears(1).AddDays(-1);

            double monthlyLeaveAccrual = annualMaxLeaveAllocation / 12.0;
            double totalAccruedLeave = 0;

            int minimumDaysRequired = Convert.ToInt32(_configuration["DaysWorkedInMonth:DaysWorkedInMonth"]);

            DateTime cycleStart = GetAccrualPeriodStart(fiscalYearStart);
            DateTime cycleEnd = cycleStart.AddMonths(1).AddDays(-1);

            while (cycleStart <= today && cycleStart <= fiscalYearEnd)
            {
                // If employee joined AFTER this cycle → skip it
                if (joinDate > cycleEnd)
                {
                    cycleStart = cycleStart.AddMonths(1);
                    cycleEnd = cycleStart.AddMonths(1).AddDays(-1);
                    continue;
                }

                bool isJoiningCycle = joinDate >= cycleStart && joinDate <= cycleEnd;

                if (isJoiningCycle)
                {
                    // joining month → always credit once (after cycle completes)
                    if (today >= cycleStart.AddMonths(1))
                    {
                        totalAccruedLeave += monthlyLeaveAccrual;
                    }
                }
                else
                {
                    DateTime effectiveStart = joinDate > cycleStart ? joinDate : cycleStart;
                    DateTime effectiveEnd = cycleEnd < today ? cycleEnd : today;

                    int daysWorked = (effectiveEnd - effectiveStart).Days + 1;

                    if (daysWorked >= minimumDaysRequired)
                    {
                        totalAccruedLeave += monthlyLeaveAccrual;
                    }
                }

                cycleStart = cycleStart.AddMonths(1);
                cycleEnd = cycleStart.AddMonths(1).AddDays(-1);
            }

            return totalAccruedLeave;
        }


        private DateTime GetAccrualPeriodStart(DateTime date)
        {
            if (date.Day >= 21)
                return new DateTime(date.Year, date.Month, 21);
            else
            {
                DateTime prevMonth = date.AddMonths(-1);
                return new DateTime(prevMonth.Year, prevMonth.Month, 21);
            }
        }
    }
}
