using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer.ITF;
using HRMS.Models;
using HRMS.Models.AttendenceList;
using HRMS.Models.Common;
using HRMS.Models.Company;
using HRMS.Models.DashBoard;
using HRMS.Models.Employee;
using HRMS.Models.ExportEmployeeExcel;
using HRMS.Models.FormPermission;
using HRMS.Models.ImportFromExcel;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
using HRMS.Models.PayRoll;
using HRMS.Models.ShiftType;
using HRMS.Models.Template;
using HRMS.Models.User;
using HRMS.Models.WhatsHappeningModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Core;

namespace HRMS.API.BusinessLayer
{
    public class BusinessLayer : IBusinessLayer
    {
        private readonly ILogger<BusinessLayer> _logger;
        IDataLayer DataLayer { get; set; }
        IAttandanceDataLayer AttandanceDataLayer { get; set; }
        private readonly IConfiguration _configuration;
        public BusinessLayer(ILogger<BusinessLayer> logger, IConfiguration configuration, IDataLayer dataLayer, IAttandanceDataLayer attandanceDataLayer, IConfiguration configurations)
        {
            _logger = logger;
            DataLayer = dataLayer;
            AttandanceDataLayer = attandanceDataLayer;
            DataLayer._configuration = configuration;
            _configuration = configurations;
        }
        public LoginUser LoginUser(LoginUser loginUser)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@UserName", loginUser.Email));
            sqlParameter.Add(new SqlParameter("@Password", loginUser.Password));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_LoginUser, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                loginUser = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow => new LoginUser
                   {
                       Result = dataRow.Field<int>("Result").ToString(),
                   }).ToList().FirstOrDefault();

            }
            else
            {
                loginUser = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new LoginUser
                               {
                                   UserID = dataRow.Field<long>("UserID"),
                                   CompanyID = dataRow.Field<long>("CompanyID"),
                                   EmployeeID = dataRow.Field<long>("EmployeeID"),
                                   GenderId = dataRow.Field<int>("Gender"),
                                   Role = dataRow.Field<string>("Role"),
                                   RoleId = dataRow.Field<int>("RoleId"),
                                   Manager1Name = dataRow.Field<string>("Manager1Name"),
                                   Manager1Email = dataRow.Field<string>("Manager1Email"),
                                   Manager2Email = dataRow.Field<string>("Manager2Email"),
                                   Manager2Name = dataRow.Field<string>("Manager2Name"),
                                   EmployeeNumber = dataRow.Field<string>("EmployeeNumber"),
                                   EmployeeNumberWithoutAbbr = dataRow.Field<string>("EmployeeNumberWithoutAbbr"),
                                   IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired"),
                                   JobLocationID = dataRow.Field<long>("JobLocationID"),
                                   DepartmentID = dataRow.Field<long>("DepartmentID"),
                                   IsFirstLoginPasswordReset = dataRow.Field<bool>("IsFirstLoginPasswordReset"),
                               }).ToList().FirstOrDefault();
            }
            return loginUser;
        }
        public Result ResetPassword(ResetPasswordModel model)
        {
            Result results = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            sqlParameter.Add(new SqlParameter("@Password", model.Password));
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_ResetPassword, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                results = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow => new Result
                   {
                       Message = dataRow.Field<string>("Result").ToString(),
                       UserID = dataRow.Field<long?>("EmployeeID"),
                   }).ToList().FirstOrDefault();

            }
            return results;
        }
        public Result GetFogotPasswordDetails(ChangePasswordModel model)
        {
            Result results = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmailId", model.EmailId));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_FogotPasswordDetails, sqlParameter);

            var data = dataSet.Tables[0].AsEnumerable()
                                  .Select(dataRow => new UserModel
                                  {
                                      EmployeeID = dataRow.Field<long>("EmployeeID"),
                                      UserID = dataRow.Field<long>("UserID"),
                                      guid = dataRow.Field<Guid>("guid"),
                                      CompanyID = dataRow.Field<long>("CompanyID"),
                                      UserName = dataRow.Field<string>("UserName"),
                                      FullName = dataRow.Field<string>("FullName"),
                                      DepartmentName = dataRow.Field<string>("DepartmentName"),
                                      ManagerName = dataRow.Field<string>("ManagerName"),
                                      IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired"),
                                      IsActive = dataRow.Field<bool>("IsActive"),
                                      RoleID = dataRow.Field<int>("RoleID"),
                                      Email = dataRow.Field<string>("OfficialEmailID"),
                                  }).ToList().LastOrDefault();
            results.Data = JsonConvert.SerializeObject(data);
            return results;
        }
        public Results GetAllEmployees(EmployeeInputParams model)
        {
            Results result = new Results();
            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>
        {
            new SqlParameter("@CompanyID", model.CompanyID),
            new SqlParameter("@EmployeeID", model.EmployeeID),
            new SqlParameter("@RoleID", model.RoleID),
            new SqlParameter("@SortCol", model.SortCol ),
            new SqlParameter("@SortDir", model.SortDir),
            new SqlParameter("@Searching", string.IsNullOrEmpty(model.Searching) ? DBNull.Value : (object)model.Searching),
            new SqlParameter("@DisplayStart", model.DisplayStart),
            new SqlParameter("@DisplayLength", model.DisplayLength),
            new SqlParameter("@LocationID", model.LocationID),
            new SqlParameter("@SubDepartmentID", model.SubDepartmentID),
            new SqlParameter("@EmployeeTypeID", model.EmployeeTypeID),
            new SqlParameter("@IsActive", model.IsActive)
        };
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeeDetails, sqlParameter);

                result.Employees = dataSet.Tables[0].AsEnumerable()
                                  .Select(dataRow =>
                                  new EmployeeModel
                                  {
                                      EmployeeID = dataRow.Field<long>("EmployeeID"),
                                      guid = dataRow.Field<Guid>("guid"),
                                      CompanyID = dataRow.Field<long>("CompanyID"),
                                      ProfilePhoto = dataRow.Field<string>("ProfilePhoto"),
                                      FirstName = dataRow.Field<string>("FirstName"),
                                      MiddleName = dataRow.Field<string>("MiddleName"),
                                      Surname = dataRow.Field<string>("Surname"),
                                      CorrespondenceAddress = dataRow.Field<string>("CorrespondenceAddress"),
                                      CorrespondenceCity = dataRow.Field<string>("CorrespondenceCity"),
                                      CorrespondencePinCode = dataRow.Field<string>("CorrespondencePinCode"),
                                      CorrespondenceState = dataRow.Field<string>("CorrespondenceState"),
                                      CorrespondenceCountryID = dataRow.Field<long>("CorrespondenceCountryID"),
                                      EmailAddress = dataRow.Field<string>("EmailAddress"),
                                      Landline = dataRow.Field<string>("Landline"),
                                      Mobile = dataRow.Field<string>("Mobile"),
                                      Telephone = dataRow.Field<string>("Telephone"),
                                      PersonalEmailAddress = dataRow.Field<string>("PersonalEmailAddress"),
                                      PermanentAddress = dataRow.Field<string>("PermanentAddress"),
                                      PermanentCity = dataRow.Field<string>("PermanentCity"),
                                      PermanentPinCode = dataRow.Field<string>("PermanentPinCode"),
                                      PermanentState = dataRow.Field<string>("PermanentState"),
                                      PermanentCountryID = dataRow.Field<long>("PermanentCountryID"),
                                      VerificationContactPersonName = dataRow.Field<string>("VerificationContactPersonName"),
                                      VerificationContactPersonContactNo = dataRow.Field<string>("VerificationContactPersonContactNo"),
                                      DateOfBirth = dataRow.Field<DateTime?>("DateOfBirth"),
                                      PlaceOfBirth = dataRow.Field<string>("PlaceOfBirth"),
                                      IsReferredByExistingEmployee = dataRow.Field<bool>("IsReferredByExistingEmployee"),
                                      IsRelativesWorkingWithCompany = dataRow.Field<bool>("IsRelativesWorkingWithCompany"),
                                      ReferredByEmployeeID = dataRow.Field<string>("ReferredByEmployeeID"),
                                      BloodGroup = dataRow.Field<string>("BloodGroup"),
                                      PANNo = dataRow.Field<string>("PANNo"),
                                      AadharCardNo = dataRow.Field<string>("AadharCardNo"),
                                      Allergies = dataRow.Field<string>("Allergies"),
                                      RelativesDetails = dataRow.Field<string>("RelativesDetails"),
                                      MajorIllnessOrDisability = dataRow.Field<string>("MajorIllnessOrDisability"),
                                      AwardsAchievements = dataRow.Field<string>("AwardsAchievements"),
                                      EducationGap = dataRow.Field<string>("EducationGap"),
                                      ExtraCuricuarActivities = dataRow.Field<string>("ExtraCuricuarActivities"),
                                      ForiegnCountryVisits = dataRow.Field<string>("ForiegnCountryVisits"),
                                      ContactPersonName = dataRow.Field<string>("ContactPersonName"),
                                      ContactPersonMobile = dataRow.Field<string>("ContactPersonMobile"),
                                      ContactPersonTelephone = dataRow.Field<string>("ContactPersonTelephone"),
                                      ContactPersonRelationship = dataRow.Field<string>("ContactPersonRelationship"),
                                      ITSkillsKnowledge = dataRow.Field<string>("ITSkillsKnowledge"),
                                      InsertedDate = dataRow.Field<DateTime>("InsertedDate"),
                                      Gender = dataRow.Field<int>("Gender"),
                                      CarryForword = dataRow.Field<double?>("CarryForword"),
                                      DepartmentID = dataRow["DepartmentID"] == DBNull.Value ? 0 : Convert.ToInt64(dataRow["DepartmentID"]),
                                      DesignationID = dataRow["DesignationID"] == DBNull.Value ? 0 : Convert.ToInt64(dataRow["DesignationID"]),
                                      LeavePolicyID = dataRow.Field<long?>("LeavePolicyID"),
                                      JoiningDate = dataRow.Field<DateTime?>("JoiningDate"),
                                      IsActive = dataRow.Field<bool>("IsActive"),
                                      ShiftTypeID = dataRow.Field<long>("ShiftTypeID"),
                                      TotalRecords = dataRow.Field<int>("TotalRecords"),
                                      FilteredRecords = dataRow.Field<int>("TotalRecords"),
                                      DesignationName = dataRow.Field<string>("Designation"),
                                      DepartmentName = dataRow.Field<string>("Department"),
                                      EmployeeNumber = dataRow.Field<string>("EmployeeNumber"),
                                      EmployeeNumberWithoutAbbr = dataRow.Field<string>("EmployeeNumberWithoutAbbr"),
                                      OfficialEmailID = dataRow.Field<string>("OfficialEmail"),
                                      ManagerName = dataRow.Field<string>("ManagerName"),
                                      PayrollTypeName = dataRow.Field<string>("PayrollType"),
                                      PanCardImage = dataRow.Field<string>("PanCardImage"),
                                      AadhaarCardImage = dataRow.Field<string>("AadhaarCardImage"),
                                      ShiftEndTime = dataRow.Field<string>("ShiftEndTime"),
                                      ShiftStartTime = dataRow.Field<string>("ShiftStartTime"),
                                      Shift = dataRow.Field<string>("Shift"),
                                      JobLocation = dataRow.Field<string>("JobLocation"),
                                      JobLocationID = dataRow.Field<long>("JobLocationID"),
                                  }).ToList();

                if (model.EmployeeID > 0)
                {
                    result.employeeModel = result.Employees.FirstOrDefault();


                    result.employeeModel.FamilyDetails = dataSet.Tables[1].AsEnumerable()
                                 .Select(dataRow => new FamilyDetail
                                 {
                                     EmployeesFamilyDetailID = dataRow.Field<long>("EmployeesFamilyDetailID"),
                                     Age = dataRow.Field<string>("Age"),
                                     FamilyName = dataRow.Field<string>("FamilyName"),
                                     Relationship = dataRow.Field<string>("Relationship"),
                                     Details = dataRow.Field<string>("Details"),
                                 }).ToList();
                    if (result.employeeModel.FamilyDetails == null)
                    {
                        result.employeeModel.FamilyDetails = new List<FamilyDetail>();
                    }

                    //////////////////////// EducationalDetails
                    result.employeeModel.EducationalDetails = dataSet.Tables[2].AsEnumerable()
                               .Select(dataRow => new EducationalDetail
                               {
                                   EducationDetailID = dataRow.Field<long>("EducationDetailID"),
                                   Major_OptionalSubjects = dataRow.Field<string>("Major_OptionalSubjects"),
                                   Percentage = dataRow.Field<string>("Percentage"),
                                   Qualification = dataRow.Field<string>("Qualification"),
                                   School_University = dataRow.Field<string>("School_University"),
                                   YearOfPassing = dataRow.Field<string>("YearOfPassing"),
                               }).ToList();
                    if (result.employeeModel.EducationalDetails == null)
                    {
                        result.employeeModel.EducationalDetails = new List<EducationalDetail>();
                    }

                    //////////////////////// LanguageDetails
                    result.employeeModel.LanguageDetails = dataSet.Tables[3].AsEnumerable()
                               .Select(dataRow => new LanguageDetail
                               {
                                   LanguageDetailID = dataRow.Field<long>("LanguageDetailID"),
                                   IsRead = dataRow.Field<bool>("IsRead"),
                                   IsSpeak = dataRow.Field<bool>("IsSpeak"),
                                   IsWrite = dataRow.Field<bool>("IsWrite"),
                                   LanguageID = dataRow.Field<long>("LanguageID")
                               }).ToList();
                    if (result.employeeModel.LanguageDetails == null)
                    {
                        result.employeeModel.LanguageDetails = new List<LanguageDetail>();
                    }


                    //////////////////////// EmploymentHistory
                    result.employeeModel.EmploymentHistory = dataSet.Tables[4].AsEnumerable()
                               .Select(dataRow => new EmploymentHistory
                               {
                                   EmploymentHistoryID = dataRow.Field<long>("EmploymentHistoryID"),
                                   Address = dataRow.Field<string>("Address"),
                                   City = dataRow.Field<string>("City"),
                                   CompanyName = dataRow.Field<string>("CompanyName"),
                                   CountryID = dataRow.Field<long>("CountryID"),
                                   Designition = dataRow.Field<string>("Designition"),
                                   EmployeeID = dataRow.Field<long>("EmployeeID"),
                                   EmploymentID = dataRow.Field<string>("EmploymentID"),
                                   From = dataRow.Field<DateTime>("From"),
                                   GrossSalary = dataRow.Field<string>("GrossSalary"),
                                   HRContactNo = dataRow.Field<string>("HRContactNo"),
                                   HREmail = dataRow.Field<string>("HREmail"),
                                   HRName = dataRow.Field<string>("HRName"),
                                   Phone = dataRow.Field<string>("Phone"),
                                   PostalCode = dataRow.Field<string>("PostalCode"),
                                   ReasionFoLeaving = dataRow.Field<string>("ReasionFoLeaving"),
                                   State = dataRow.Field<string>("State"),
                                   SupervisorContactNo = dataRow.Field<string>("SupervisorContactNo"),
                                   SupervisorDesignition = dataRow.Field<string>("SupervisorDesignition"),
                                   SupervisorName = dataRow.Field<string>("SupervisorName"),
                                   To = dataRow.Field<DateTime>("To"),

                               }).ToList();
                    if (result.employeeModel.EmploymentHistory == null)
                    {
                        result.employeeModel.EmploymentHistory = new List<EmploymentHistory>();
                    }


                    result.employeeModel.References = dataSet.Tables[5].AsEnumerable()
                               .Select(dataRow => new Reference
                               {
                                   ReferenceDetailID = dataRow.Field<long>("ReferenceDetailID"),
                                   Contact = dataRow.Field<string>("Contact"),
                                   Name = dataRow.Field<string>("Name"),
                                   OrgnizationName = dataRow.Field<string>("OrgnizationName"),
                                   RelationWithCandidate = dataRow.Field<string>("RelationWithCandidate")
                               }).ToList();
                    if (result.employeeModel.References == null)
                    {
                        result.employeeModel.References = new List<Reference>();
                    }


                    result.employeeModel.EmploymentDetail = dataSet.Tables[6].AsEnumerable()
                               .Select(dataRow => new EmploymentDetail
                               {
                                   EmployeeID = dataRow.Field<long>("EmployeeID"),
                                   JoiningDate = dataRow.Field<DateTime>("JoiningDate"),

                               }).ToList();
                    if (result.employeeModel.EmploymentDetail == null)
                    {
                        result.employeeModel.EmploymentDetail = new List<EmploymentDetail>();
                    }



                }
                result.employeeModel.SubDepartmentList = dataSet.Tables[1].AsEnumerable()
                           .Select(dataRow => new SubDepartmentList
                           {
                               SubDepartmentID = dataRow.Field<long>("SubDepartmentID"),
                               Name = dataRow.Field<string>("Name"),

                           }).ToList();
                if (result.employeeModel.SubDepartmentList == null)
                {
                    result.employeeModel.SubDepartmentList = new List<SubDepartmentList>();
                }

                result.employeeModel.LocationList = dataSet.Tables[2].AsEnumerable()
                           .Select(dataRow => new LocationList
                           {
                               JobLocationID = dataRow.Field<long>("JobLocationID"),
                               Name = dataRow.Field<string>("Name"),

                           }).ToList();
                if (result.employeeModel.LocationList == null)
                {
                    result.employeeModel.LocationList = new List<LocationList>();
                }

                result.employeeModel.EmploymentTypesList = dataSet.Tables[3].AsEnumerable()
                             .Select(dataRow => new EmploymentTypesList
                             {
                                 EmployeeTypeId = dataRow.Field<long>("EmployeeTypeID"),
                                 Name = dataRow.Field<string>("Name"),

                             }).ToList();
                if (result.employeeModel.EmploymentTypesList == null)
                {
                    result.employeeModel.EmploymentTypesList = new List<EmploymentTypesList>();
                }
            }
            catch (Exception ex)
            {

            }


            return result;
        }
        public Results GetAllActiveEmployees(EmployeeInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_ActiveEmployeeDetails, sqlParameter);
            result.Employees = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new EmployeeModel
                              {
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  guid = dataRow.Field<Guid>("guid"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  ProfilePhoto = dataRow.Field<string>("ProfilePhoto"),
                                  FirstName = dataRow.Field<string>("FirstName"),
                                  MiddleName = dataRow.Field<string>("MiddleName"),
                                  Surname = dataRow.Field<string>("Surname"),
                                  CorrespondenceAddress = dataRow.Field<string>("CorrespondenceAddress"),
                                  CorrespondenceCity = dataRow.Field<string>("CorrespondenceCity"),
                                  CorrespondencePinCode = dataRow.Field<string>("CorrespondencePinCode"),
                                  CorrespondenceState = dataRow.Field<string>("CorrespondenceState"),
                                  CorrespondenceCountryID = dataRow.Field<long>("CorrespondenceCountryID"),
                                  EmailAddress = dataRow.Field<string>("EmailAddress"),
                                  Landline = dataRow.Field<string>("Landline"),
                                  Mobile = dataRow.Field<string>("Mobile"),
                                  Telephone = dataRow.Field<string>("Telephone"),
                                  PersonalEmailAddress = dataRow.Field<string>("PersonalEmailAddress"),
                                  PermanentAddress = dataRow.Field<string>("PermanentAddress"),
                                  PermanentCity = dataRow.Field<string>("PermanentCity"),
                                  PermanentPinCode = dataRow.Field<string>("PermanentPinCode"),
                                  PermanentState = dataRow.Field<string>("PermanentPinCode"),
                                  PermanentCountryID = dataRow.Field<long>("PermanentCountryID"),
                                  VerificationContactPersonName = dataRow.Field<string>("VerificationContactPersonName"),
                                  VerificationContactPersonContactNo = dataRow.Field<string>("VerificationContactPersonContactNo"),
                                  DateOfBirth = dataRow.Field<DateTime?>("DateOfBirth"),
                                  PlaceOfBirth = dataRow.Field<string>("PlaceOfBirth"),
                                  IsReferredByExistingEmployee = dataRow.Field<bool>("IsReferredByExistingEmployee"),
                                  ReferredByEmployeeID = dataRow.Field<string>("ReferredByEmployeeID"),
                                  BloodGroup = dataRow.Field<string>("BloodGroup"),
                                  PANNo = dataRow.Field<string>("PANNo"),
                                  AadharCardNo = dataRow.Field<string>("AadharCardNo"),
                                  Allergies = dataRow.Field<string>("Allergies"),
                                  RelativesDetails = dataRow.Field<string>("RelativesDetails"),
                                  MajorIllnessOrDisability = dataRow.Field<string>("MajorIllnessOrDisability"),
                                  AwardsAchievements = dataRow.Field<string>("AwardsAchievements"),
                                  EducationGap = dataRow.Field<string>("EducationGap"),
                                  ExtraCuricuarActivities = dataRow.Field<string>("ExtraCuricuarActivities"),
                                  ForiegnCountryVisits = dataRow.Field<string>("ForiegnCountryVisits"),
                                  ContactPersonName = dataRow.Field<string>("ContactPersonName"),
                                  ContactPersonMobile = dataRow.Field<string>("ContactPersonMobile"),
                                  ContactPersonTelephone = dataRow.Field<string>("ContactPersonTelephone"),
                                  ContactPersonRelationship = dataRow.Field<string>("ContactPersonRelationship"),
                                  ITSkillsKnowledge = dataRow.Field<string>("ITSkillsKnowledge"),

                                  // Employment Details
                                  OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                  EmployeeNumber = dataRow.Field<string>("EmployeNumber"),
                                  DesignationID = dataRow.Field<long>("DesignationID"),
                                  EmployeeTypeID = dataRow.Field<long>("EmployeeTypeID"),
                                  DepartmentID = dataRow.Field<long>("DepartmentID"),
                                  JobLocationID = dataRow.Field<long>("JobLocationID"),
                                  ReportingToIDL1 = dataRow.Field<long>("ReportingToIDL1"),
                                  ReportingToIDL2 = dataRow.Field<long>("ReportingToIDL2"),
                                  DesignationName = dataRow.Field<string>("DesignationName"),
                                  EmployeeTypeName = dataRow.Field<string>("EmployeeTypeName"),
                                  DepartmentName = dataRow.Field<string>("DepartmentName"),
                                  JobLocationName = dataRow.Field<string>("JobLocationName"),
                                  PayrollTypeID = dataRow.Field<long>("PayrollTypeID"),
                                  PayrollTypeName = dataRow.Field<string>("PayrollTypeName"),
                                  ReportingToNameL1 = dataRow.Field<string>("ReportingToNameL1"),
                                  ReportingToNameL2 = dataRow.Field<string>("ReportingToNameL2"),
                                  ClientName = dataRow.Field<string>("ClientName"),
                              }).ToList();


            return result;
        }
        public string GetFullUrlByRole(int RoleID)
        {
            string RootName = "";
            switch (RoleID)
            {
                case (int)Roles.Admin:
                    RootName = string.Format(Models.Common.Constants.RootUrlFormat, Models.Common.Constants.ManageAdmin, Roles.Admin.ToString());
                    break;
                case (int)Roles.HR:
                    RootName = string.Format(Models.Common.Constants.RootUrlFormat, Models.Common.Constants.ManageHR, Roles.HR.ToString());
                    break;
                case (int)Roles.Employee:
                    RootName = string.Format(Models.Common.Constants.RootUrlFormat, Models.Common.Constants.ManageEmployee, Roles.Employee.ToString());
                    break;
                default:
                    break;
            }
            return RootName;
        }
        public Results GetAllCountries()
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Counteres, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                  .Select(dataRow => new Results
                  {
                      Result = new Result()
                      {
                          Message = dataRow.Field<int>("Result").ToString()
                      },
                  }).ToList().FirstOrDefault();

            }
            else
            {
                model.Countries = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("CountryID").ToString()
                               }).ToList();
            }
            return model;
        }
        public Results GetAllCurrencies(long companyID)
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            // sqlParameter.Add(new SqlParameter("@CompanyID", companyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Currencies, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                  .Select(dataRow => new Results
                  {
                      Result = new Result()
                      {
                          Message = dataRow.Field<int>("Result").ToString()
                      },
                  }).ToList().FirstOrDefault();

            }
            else
            {
                model.Currencies = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("CurrencyID").ToString()
                               }).ToList();
            }
            return model;
        }
        public Results GetAllLanguages()
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Languages, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow => new Results
                   {
                       Result = new Result()
                       {
                           Message = dataRow.Field<int>("Result").ToString()
                       },
                   }).ToList().FirstOrDefault();

            }
            else
            {
                model.Languages = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("LanguageID").ToString()
                               }).ToList();
            }
            return model;
        }
        public Results GetAllCompanyLanguages(long companyID)
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", companyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CompanyLanguages, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow => new Results
                   {
                       Result = new Result()
                       {
                           Message = dataRow.Field<int>("Result").ToString()
                       },
                   }).ToList().FirstOrDefault();
            }
            else
            {
                model.Languages = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("LanguageID").ToString()
                               }).ToList();
            }
            return model;
        }
        public Results GetAllCompanyDepartments(long companyID)
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", companyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CompanyDepartments, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow => new Results
                   {
                       Result = new Result()
                       {
                           Message = dataRow.Field<int>("Result").ToString()
                       },
                   }).ToList().FirstOrDefault();

            }
            else
            {
                model.Departments = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("DepartmentID").ToString()
                               }).ToList();
            }
            return model;
        }

        #region Employee
        public Results GetAllCompanyEmployeeTypes(long companyID)
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", companyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CompanyEmployeeTypes, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow => new Results
                   {
                       Result = new Result()
                       {
                           Message = dataRow.Field<int>("Result").ToString()
                       },
                   }).ToList().FirstOrDefault();
            }
            else
            {
                model.EmploymentTypes = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("EmployeeTypeID").ToString()
                               }).ToList();
            }
            return model;
        }
        public Result AddUpdateEmployee(EmployeeModel employeeModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@EmployeeID", employeeModel.EmployeeID));
            sqlParameter.Add(new SqlParameter("@RetEmployeeID", employeeModel.EmployeeID));
            sqlParameter.Add(new SqlParameter("@CompanyID", employeeModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@FirstName", employeeModel.FirstName));
            sqlParameter.Add(new SqlParameter("@MiddleName", employeeModel.MiddleName));
            sqlParameter.Add(new SqlParameter("@Surname", employeeModel.Surname));
            sqlParameter.Add(new SqlParameter("@CorrespondenceAddress", employeeModel.CorrespondenceAddress));
            sqlParameter.Add(new SqlParameter("@CorrespondenceCity", employeeModel.CorrespondenceCity));
            sqlParameter.Add(new SqlParameter("@CorrespondencePinCode", employeeModel.CorrespondencePinCode));
            sqlParameter.Add(new SqlParameter("@CorrespondenceState", employeeModel.CorrespondenceState));
            sqlParameter.Add(new SqlParameter("@CorrespondenceCountryID", employeeModel.CorrespondenceCountryID));
            sqlParameter.Add(new SqlParameter("@EmailAddress", employeeModel.EmailAddress));
            sqlParameter.Add(new SqlParameter("@Landline", employeeModel.Landline));
            sqlParameter.Add(new SqlParameter("@Mobile", employeeModel.Mobile));
            sqlParameter.Add(new SqlParameter("@Telephone", employeeModel.Telephone));
            sqlParameter.Add(new SqlParameter("@PersonalEmailAddress", employeeModel.PersonalEmailAddress));
            sqlParameter.Add(new SqlParameter("@PermanentAddress", employeeModel.PermanentAddress));
            sqlParameter.Add(new SqlParameter("@PermanentCity", employeeModel.PermanentCity));
            sqlParameter.Add(new SqlParameter("@PermanentPinCode", employeeModel.PermanentPinCode));
            sqlParameter.Add(new SqlParameter("@PermanentState", employeeModel.PermanentState));
            sqlParameter.Add(new SqlParameter("@PermanentCountryID", employeeModel.PermanentCountryID));
            sqlParameter.Add(new SqlParameter("@PeriodOfStay", employeeModel.PeriodOfStay));
            sqlParameter.Add(new SqlParameter("@VerificationContactPersonName", employeeModel.VerificationContactPersonName));
            sqlParameter.Add(new SqlParameter("@VerificationContactPersonContactNo", employeeModel.VerificationContactPersonContactNo));
            sqlParameter.Add(new SqlParameter("@DateOfBirth", employeeModel.DateOfBirth));
            sqlParameter.Add(new SqlParameter("@ProfilePhoto", employeeModel.ProfilePhoto));
            sqlParameter.Add(new SqlParameter("@PlaceOfBirth", employeeModel.PlaceOfBirth));
            sqlParameter.Add(new SqlParameter("@IsReferredByExistingEmployee", employeeModel.IsReferredByExistingEmployee));
            sqlParameter.Add(new SqlParameter("@ReferredByEmployeeID", employeeModel.ReferredByEmployeeID));
            sqlParameter.Add(new SqlParameter("@BloodGroup", employeeModel.BloodGroup));
            sqlParameter.Add(new SqlParameter("@PANNo", employeeModel.PANNo));
            sqlParameter.Add(new SqlParameter("@AadharCardNo", employeeModel.AadharCardNo));
            sqlParameter.Add(new SqlParameter("@Allergies", employeeModel.Allergies));
            sqlParameter.Add(new SqlParameter("@IsRelativesWorkingWithCompany", employeeModel.IsRelativesWorkingWithCompany));
            sqlParameter.Add(new SqlParameter("@RelativesDetails", employeeModel.RelativesDetails));
            sqlParameter.Add(new SqlParameter("@MajorIllnessOrDisability", employeeModel.MajorIllnessOrDisability));
            sqlParameter.Add(new SqlParameter("@AwardsAchievements", employeeModel.AwardsAchievements));
            sqlParameter.Add(new SqlParameter("@EducationGap", employeeModel.EducationGap));
            sqlParameter.Add(new SqlParameter("@ExtraCuricuarActivities", employeeModel.ExtraCuricuarActivities));
            sqlParameter.Add(new SqlParameter("@ForiegnCountryVisits", employeeModel.ForiegnCountryVisits));
            sqlParameter.Add(new SqlParameter("@ContactPersonName", employeeModel.ContactPersonName));
            sqlParameter.Add(new SqlParameter("@ContactPersonMobile", employeeModel.ContactPersonMobile));
            sqlParameter.Add(new SqlParameter("@ContactPersonTelephone", employeeModel.ContactPersonTelephone));
            sqlParameter.Add(new SqlParameter("@ContactPersonRelationship", employeeModel.ContactPersonRelationship));
            sqlParameter.Add(new SqlParameter("@ITSkillsKnowledge", employeeModel.ITSkillsKnowledge));
            sqlParameter.Add(new SqlParameter("@FamilyDetails", this.ConvertObjectToXML(employeeModel.FamilyDetails)));
            sqlParameter.Add(new SqlParameter("@EducationalDetails", this.ConvertObjectToXML(employeeModel.EducationalDetails)));
            sqlParameter.Add(new SqlParameter("@LanguageDetails", this.ConvertObjectToXML(employeeModel.LanguageDetails)));
            sqlParameter.Add(new SqlParameter("@EmploymentHistory", this.ConvertObjectToXML(employeeModel.EmploymentHistory)));
            sqlParameter.Add(new SqlParameter("@References", this.ConvertObjectToXML(employeeModel.References)));
            sqlParameter.Add(new SqlParameter("@IsActive", employeeModel.IsActive));
            sqlParameter.Add(new SqlParameter("@Gender", employeeModel.Gender));
            sqlParameter.Add(new SqlParameter("@PanCardImage", employeeModel.PanCardImage));
            sqlParameter.Add(new SqlParameter("@AadhaarCardImage", employeeModel.AadhaarCardImage));

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Employee, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            UserID = dataRow.Field<long>("UserID"),
                            PKNo = dataRow.Field<long>("UserID")
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }
        #endregion

        #region Templates
        public Results GetAllTemplates(TemplateInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter =
            [
                new SqlParameter("@CompanyID", model.CompanyID),
                new SqlParameter("@TemplateID", model.TemplateID),
            ];

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_TemplateDetails, sqlParameter);
            result.Template = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new TemplateModel
                              {
                                  TemplateID = dataRow.Field<long>("TemplateID"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  LetterHeadName = dataRow.Field<string>("LetterHeadName"),
                                  HeaderImage = dataRow.Field<string>("HeaderImage"),
                                  FooterImage = dataRow.Field<string>("FooterImage"),
                                  Description = dataRow.Field<string>("Description"),
                                  TemplateName = dataRow.Field<string>("TemplateName")
                              }).ToList();

            if (model.TemplateID > 0)
            {
                result.templateModel = result.Template.FirstOrDefault();
            }

            return result;
        }

        public Result AddUpdateTemplate(TemplateModel templateModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@TemplateID", templateModel.TemplateID));
            sqlParameter.Add(new SqlParameter("@LetterHeadName", templateModel.LetterHeadName));
            sqlParameter.Add(new SqlParameter("@HeaderImage", templateModel.HeaderImage));
            sqlParameter.Add(new SqlParameter("@CompanyID", templateModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@FooterImage", templateModel.FooterImage));
            sqlParameter.Add(new SqlParameter("@Description", templateModel.Description));
            sqlParameter.Add(new SqlParameter("@TemplateName", templateModel.TemplateName));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Template, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }
        #endregion

        #region Companies
        public Results GetCompaniesLogo(CompanyLoginModel model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Companies, sqlParameter);
            result.companyLoginModel = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new CompanyLoginModel
                              {
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  CompanyLogo = dataRow.Field<string>("CompanyLogo"),
                                  Abbr = dataRow.Field<string>("Abbr"),
                              }).ToList().FirstOrDefault();
            return result;
        }
        public Results GetAllCompanies(EmployeeInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Companies, sqlParameter);
            result.Companies = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new CompanyModel
                              {
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  CompanyLogo = dataRow.Field<string>("CompanyLogo"),
                                  Abbr = dataRow.Field<string>("Abbr"),
                                  CountryID = dataRow.Field<long?>("CountryID"),
                                  DateOfEstablished = dataRow.Field<DateTime?>("DateOfEstablished"),
                                  DefaultCurrencyID = dataRow.Field<long?>("DefaultCurrencyID"),
                                  DefaultLetterHead = dataRow.Field<string>("DefaultLetterHead"),
                                  Domain = dataRow.Field<string>("Domain"),
                                  Name = dataRow.Field<string>("Name"),
                                  ParentCompany = dataRow.Field<string>("ParentCompany"),
                                  GSTIN = dataRow.Field<string>("GSTIN"),
                                  IsGroup = dataRow.Field<bool>("IsGroup"),
                                  CIN = dataRow.Field<string>("CIN"),
                                  Address = dataRow.Field<string>("Address"),
                                  City = dataRow.Field<string>("City"),
                                  State = dataRow.Field<string>("State"),
                                  Phone = dataRow.Field<string>("Phone")

                              }).ToList();

            if (model.CompanyID > 0)
            {
                result.companyModel = result.Companies.FirstOrDefault();
            }

            return result;
        }

        public Results GetAllCompaniesList(EmployeeInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Companies, sqlParameter);
            result.Companies = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new CompanyModel
                              {
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  CompanyLogo = dataRow.Field<string>("CompanyLogo"),
                                  Abbr = dataRow.Field<string>("Abbr"),
                                  CountryID = dataRow.Field<long?>("CountryID"),
                                  DateOfEstablished = dataRow.Field<DateTime?>("DateOfEstablished"),
                                  DefaultCurrencyID = dataRow.Field<long?>("DefaultCurrencyID"),
                                  DefaultLetterHead = dataRow.Field<string>("DefaultLetterHead"),
                                  Domain = dataRow.Field<string>("Domain"),
                                  Name = dataRow.Field<string>("Name"),
                                  ParentCompany = dataRow.Field<string>("ParentCompany"),
                                  GSTIN = dataRow.Field<string>("GSTIN"),
                                  IsGroup = dataRow.Field<bool>("IsGroup"),
                                  CIN = dataRow.Field<string>("CIN"),
                                  Address = dataRow.Field<string>("Address"),
                                  City = dataRow.Field<string>("City"),
                                  State = dataRow.Field<string>("State"),
                                  Phone = dataRow.Field<string>("Phone")

                              }).ToList();

            return result;
        }

        public Result AddUpdateCompany(CompanyModel companyModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", companyModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@RetCompanyID", companyModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@Abbr", companyModel.Abbr));

            sqlParameter.Add(new SqlParameter("@DefaultCurrencyID", companyModel.DefaultCurrencyID));
            sqlParameter.Add(new SqlParameter("@Name", companyModel.Name));
            sqlParameter.Add(new SqlParameter("@DefaultLetterHead", companyModel.DefaultLetterHead));
            sqlParameter.Add(new SqlParameter("@Domain", companyModel.Domain));
            sqlParameter.Add(new SqlParameter("@DateOfEstablished", companyModel.DateOfEstablished));
            sqlParameter.Add(new SqlParameter("@IsGroup", companyModel.IsGroup));
            sqlParameter.Add(new SqlParameter("@ParentCompany", companyModel.ParentCompany));
            sqlParameter.Add(new SqlParameter("@CompanyLogo", companyModel.CompanyLogo));

            sqlParameter.Add(new SqlParameter("@GSTIN", companyModel.GSTIN));
            sqlParameter.Add(new SqlParameter("@CIN", companyModel.CIN));
            sqlParameter.Add(new SqlParameter("@Address", companyModel.Address));
            sqlParameter.Add(new SqlParameter("@City", companyModel.City));
            sqlParameter.Add(new SqlParameter("@State", companyModel.State));
            sqlParameter.Add(new SqlParameter("@Phone", companyModel.Phone));
            sqlParameter.Add(new SqlParameter("@CountryID", companyModel.CountryID));

            SqlParameterCollection pOutputParams = null;

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Company, sqlParameter, ref pOutputParams);
            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            PKNo = Convert.ToInt64(pOutputParams["@RetCompanyID"].Value)
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }
        #endregion

        #region EmploymentDetails
        public Result AddUpdateEmploymentDetails(EmploymentDetail employmentDetails)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmploymentDetailID", employmentDetails.EmploymentDetailID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", employmentDetails.EmployeeID));
            sqlParameter.Add(new SqlParameter("@DesignationID", employmentDetails.DesignationID));
            sqlParameter.Add(new SqlParameter("@EmployeeTypeID", employmentDetails.EmployeeTypeID));
            sqlParameter.Add(new SqlParameter("@PayrollTypeID", employmentDetails.PayrollTypeID));
            sqlParameter.Add(new SqlParameter("@DepartmentID", employmentDetails.DepartmentID));
            sqlParameter.Add(new SqlParameter("@JobLocationID", employmentDetails.JobLocationID));
            sqlParameter.Add(new SqlParameter("@OfficialEmailID", employmentDetails.OfficialEmailID));
            sqlParameter.Add(new SqlParameter("@OfficialContactNo", employmentDetails.OfficialContactNo));
            sqlParameter.Add(new SqlParameter("@JoiningDate", employmentDetails.JoiningDate));
            sqlParameter.Add(new SqlParameter("@JobSeprationDate", employmentDetails.JobSeprationDate));
            sqlParameter.Add(new SqlParameter("@ReportingToIDL1", employmentDetails.ReportingToIDL1));
            sqlParameter.Add(new SqlParameter("@IsActive", employmentDetails.IsActive));
            sqlParameter.Add(new SqlParameter("@IsDeleted", employmentDetails.IsDeleted));
            sqlParameter.Add(new SqlParameter("@UserID", employmentDetails.UserID));
            sqlParameter.Add(new SqlParameter("@LeavePolicyID", employmentDetails.LeavePolicyID));
            sqlParameter.Add(new SqlParameter("@ReportingToIDL2", employmentDetails.ReportingToIDL2));
            sqlParameter.Add(new SqlParameter("@ClientName", employmentDetails.ClientName));
            sqlParameter.Add(new SqlParameter("@SubDepartmentID", employmentDetails.SubDepartmentID));
            sqlParameter.Add(new SqlParameter("@ShiftTypeID", employmentDetails.ShiftTypeID));
            sqlParameter.Add(new SqlParameter("@EmployeeNumber", employmentDetails.EmployeNumber));
            sqlParameter.Add(new SqlParameter("@ESINumber", employmentDetails.ESINumber));
            sqlParameter.Add(new SqlParameter("@LOB", employmentDetails.LOB));
            sqlParameter.Add(new SqlParameter("@GrossSalary", employmentDetails.GrossSalary));
            sqlParameter.Add(new SqlParameter("@ESIRegistrationDate", employmentDetails.ESIRegistrationDate));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningOnroll", employmentDetails.DateOfJoiningOnroll));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningTraining", employmentDetails.DateOfJoiningTraining));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningFloor", employmentDetails.DateOfJoiningFloor));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningOJT", employmentDetails.DateOfJoiningOJT));
            sqlParameter.Add(new SqlParameter("@RoleID", employmentDetails.RoleId));
            sqlParameter.Add(new SqlParameter("@CompnayID", employmentDetails.CompanyID));

            SqlParameterCollection pOutputParams = null;

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmploymentDetails, sqlParameter, ref pOutputParams);
            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            UserID = dataRow.Field<long>("UserID"),
                            PKNo = Convert.ToInt64(pOutputParams["@EmploymentDetailID"].Value),
                            IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired")
                        }
                   ).ToList().FirstOrDefault();
            }

            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@RoleID", employmentDetails.RoleId));
            sqlParameters.Add(new SqlParameter("@UserID", model.UserID));

            SqlParameterCollection OutputParams = null;

            var datasSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_InsertUserRole, sqlParameters, ref OutputParams);
            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            PKNo = Convert.ToInt64(pOutputParams["@EmploymentDetailID"].Value),
                            IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired")
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }


        public EmploymentDetail GetEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {


            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeeDetailsFormDetails, sqlParameter);
            EmploymentDetail employmentDetail = dataSet.Tables[8].AsEnumerable()
                            .Select(dataRow => new EmploymentDetail()
                            {
                                EmployeeID = dataRow.Field<long>("EmployeeID"),
                                EmployeNumber = dataRow.Field<string>("EmployeNumber"),
                                EmployeeTypeID = dataRow.Field<long>("EmployeeTypeID"),
                                EmploymentDetailID = dataRow.Field<long>("EmploymentDetailID"),
                                DesignationID = dataRow.Field<long>("DesignationID"),
                                DepartmentID = dataRow.Field<long>("DepartmentID"),
                                JobLocationID = dataRow.Field<long>("JobLocationID"),
                                ReportingToIDL1 = dataRow.Field<long>("ReportingToIDL1"),
                                OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                OfficialContactNo = dataRow.Field<string>("OfficialContactNo"),
                                DesignationName = dataRow.Field<string>("DesignationName"),
                                DepartmentName = dataRow.Field<string>("DepartmentName"),
                                SubDepartmentName = dataRow.Field<string>("SubDepartmentName"),
                                JoiningDate = dataRow.Field<DateTime?>("JoiningDate"),
                                JobSeprationDate = dataRow.Field<DateTime?>("JobSeprationDate"),
                                ManagerEmail = dataRow.Field<string>("ManagerEmail"),
                                ManagerName = dataRow.Field<string>("ManagerName"),
                                OfficeLocation = dataRow.Field<string>("OfficeLocation"),
                                PayrollTypeID = dataRow.Field<long>("PayrollTypeID"),
                                EmployeeType = dataRow.Field<string>("EmployeeType"),
                                ReportingToIDL2 = dataRow.Field<long>("ReportingToIDL2"),
                                ClientName = dataRow.Field<string>("ClientName"),
                                SubDepartmentID = dataRow.Field<long>("SubDepartmentID"),
                                ShiftTypeID = dataRow.Field<long>("ShiftTypeID"),
                                ESINumber = dataRow.Field<string>("ESINumber"),
                                ESIRegistrationDate = dataRow.Field<DateTime?>("ESIRegistrationDate"),
                                DateOfJoiningOnroll = dataRow.Field<DateTime?>("DateOfJoiningOnroll"),
                                DateOfJoiningTraining = dataRow.Field<DateTime?>("DateOfJoiningTraining"),
                                DateOfJoiningFloor = dataRow.Field<DateTime?>("DateOfJoiningFloor"),
                                DateOfJoiningOJT = dataRow.Field<DateTime?>("DateOfJoiningOJT")
                            }).ToList().FirstOrDefault();

            if (employmentDetail == null)
            {
                employmentDetail = new EmploymentDetail();
            }
            if (employmentDetail.EmployeeID <= 0)
            {
                employmentDetail.EmployeeID = model.EmployeeID;
            }
            employmentDetail.UserID = model.UserID;
            employmentDetail.JobLocations = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("ID").ToString()
                               }).ToList();

            employmentDetail.EmploymentTypes = dataSet.Tables[1].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();
            employmentDetail.PayrollTypes = dataSet.Tables[2].AsEnumerable()
                         .Select(dataRow => new SelectListItem
                         {
                             Text = dataRow.Field<string>("Name"),
                             Value = dataRow.Field<long>("ID").ToString()
                         }).ToList();
            employmentDetail.Departments = dataSet.Tables[3].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();

            employmentDetail.SubDepartments = dataSet.Tables[4].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();
            employmentDetail.Designations = dataSet.Tables[5].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();

            employmentDetail.ShiftTypes = dataSet.Tables[6].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();
            employmentDetail.EmployeeList = dataSet.Tables[7].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Value = dataRow.Field<long>("EmployeeID").ToString(),
                                 Text = dataRow.Field<string>("Name")
                             }).ToList();

            employmentDetail.LeavePolicyList = dataSet.Tables[9].AsEnumerable()
                            .Select(dataRow => new SelectListItem
                            {
                                Value = dataRow.Field<long>("LeavePolicyID").ToString(),
                                Text = dataRow.Field<string>("LeavePolicyName")
                            }).ToList();
            employmentDetail.RoleList = dataSet.Tables[10].AsEnumerable()
                .Select(dataRow => new SelectListItem
                {
                    Value = dataRow.Field<int>("RoleID").ToString(),
                    Text = dataRow.Field<string>("UniqueName")
                }).ToList();
            return employmentDetail;
        }

        public EmploymentDetail GetFilterEmploymentDetailsByEmployee(EmploymentDetailInputParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            sqlParameter.Add(new SqlParameter("@DepartmentID", model.DepartmentID));
            sqlParameter.Add(new SqlParameter("@DesignationID", model.DesignationID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_FilterEmployeeDetailsFormDetails, sqlParameter);
            EmploymentDetail employmentDetail = dataSet.Tables[8].AsEnumerable()
                            .Select(dataRow => new EmploymentDetail()
                            {
                                EmployeeID = dataRow.Field<long>("EmployeeID"),
                                EmployeNumber = dataRow.Field<string>("EmployeNumber"),
                                EmployeeTypeID = dataRow.Field<long>("EmployeeTypeID"),
                                EmploymentDetailID = dataRow.Field<long>("EmploymentDetailID"),
                                DesignationID = dataRow.Field<long>("DesignationID"),
                                DepartmentID = dataRow.Field<long>("DepartmentID"),
                                JobLocationID = dataRow.Field<long>("JobLocationID"),
                                ReportingToIDL1 = dataRow.Field<long>("ReportingToIDL1"),
                                OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                OfficialContactNo = dataRow.Field<string>("OfficialContactNo"),
                                DesignationName = dataRow.Field<string>("DesignationName"),
                                DepartmentName = dataRow.Field<string>("DepartmentName"),
                                JoiningDate = dataRow.Field<DateTime?>("JoiningDate"),
                                JobSeprationDate = dataRow.Field<DateTime?>("JobSeprationDate"),
                                ManagerEmail = dataRow.Field<string>("ManagerEmail"),
                                ManagerName = dataRow.Field<string>("ManagerName"),
                                OfficeLocation = dataRow.Field<string>("OfficeLocation"),
                                EmployeeType = dataRow.Field<string>("EmployeeType"),
                                PayrollTypeID = dataRow.Field<long>("PayrollTypeID"),
                                LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                                ReportingToIDL2 = dataRow.Field<long>("ReportingToIDL2"),
                                ClientName = dataRow.Field<string>("ClientName"),
                                RoleId = dataRow.Field<int>("EmployeeRole"),
                                SubDepartmentID = dataRow.Field<long>("SubDepartmentID"),
                                ShiftTypeID = dataRow.Field<long>("ShiftTypeID"),
                                ESINumber = dataRow.Field<string>("ESINumber"),
                                LOB = dataRow.Field<string>("LOB"),
                                ESIRegistrationDate = dataRow.Field<DateTime?>("ESIRegistrationDate"),
                                DateOfJoiningOnroll = dataRow.Field<DateTime?>("DateOfJoiningOnroll"),
                                DateOfJoiningTraining = dataRow.Field<DateTime?>("DateOfJoiningTraining"),
                                DateOfJoiningFloor = dataRow.Field<DateTime?>("DateOfJoiningFloor"),
                                DateOfJoiningOJT = dataRow.Field<DateTime?>("DateOfJoiningOJT")
                            }).ToList().FirstOrDefault();
            if (employmentDetail == null)
            {
                employmentDetail = new EmploymentDetail();
                employmentDetail = dataSet.Tables[11].AsEnumerable()
                       .Select(dataRow => new EmploymentDetail
                       {
                           EmployeeID = model.EmployeeID,
                           EmployeNumber = dataRow.Field<string>("NewEmployeeNumber")
                       }).FirstOrDefault();
            }
            if (employmentDetail.EmployeeID <= 0)
            {
                employmentDetail.EmployeeID = model.EmployeeID;
            }

            employmentDetail.UserID = model.UserID;
            employmentDetail.JobLocations = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("ID").ToString()
                               }).ToList();

            employmentDetail.EmploymentTypes = dataSet.Tables[1].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();
            employmentDetail.PayrollTypes = dataSet.Tables[2].AsEnumerable()
                           .Select(dataRow => new SelectListItem
                           {
                               Text = dataRow.Field<string>("Name"),
                               Value = dataRow.Field<long>("ID").ToString()
                           }).ToList();

            employmentDetail.Departments = dataSet.Tables[3].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();

            employmentDetail.SubDepartments = dataSet.Tables[4].AsEnumerable()
                            .Select(dataRow => new SelectListItem
                            {
                                Text = dataRow.Field<string>("Name"),
                                Value = dataRow.Field<long>("ID").ToString()
                            }).ToList();

            employmentDetail.Designations = dataSet.Tables[5].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();

            employmentDetail.ShiftTypes = dataSet.Tables[6].AsEnumerable()
                           .Select(dataRow => new SelectListItem
                           {
                               Text = dataRow.Field<string>("Name"),
                               Value = dataRow.Field<long>("ID").ToString()
                           }).ToList();
            employmentDetail.EmployeeList = dataSet.Tables[7].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Value = dataRow.Field<long>("EmployeeID").ToString(),
                                 Text = dataRow.Field<string>("Name")
                             }).ToList();

            employmentDetail.LeavePolicyList = dataSet.Tables[9].AsEnumerable()
                            .Select(dataRow => new SelectListItem
                            {
                                Value = dataRow.Field<long>("LeavePolicyID").ToString(),
                                Text = dataRow.Field<string>("LeavePolicyName")
                            }).ToList();
            employmentDetail.RoleList = dataSet.Tables[10].AsEnumerable()
                            .Select(dataRow => new SelectListItem
                            {
                                Value = dataRow.Field<int>("RoleID").ToString(),
                                Text = dataRow.Field<string>("UniqueName")
                            }).ToList();


            return employmentDetail;
        }

        public L2ManagerDetail GetL2ManagerDetails(L2ManagerInputParams model)
        {
            // Prepare the SQL parameter for the stored procedure
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@L1EmployeeID", model.L1EmployeeID)
    };

            // Execute the stored procedure
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetManagerOfManager, sqlParameters);

            L2ManagerDetail managerDetail = new L2ManagerDetail();

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                managerDetail = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new L2ManagerDetail()
                    {
                        ManagerID = dataRow.Field<long>("EmployeeID"),
                        ManagerName = dataRow.Field<string>("ManagerName"),
                        EmployeNumber = dataRow.Field<string>("EmployeNumber")
                    })
                    .FirstOrDefault() ?? new L2ManagerDetail();
            }

            return managerDetail;
        }

        public Result AddUpdateEmploymentBankDetails(EmploymentBankDetail employmentBankDetails)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@BankDetailID", employmentBankDetails.BankDetailID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", employmentBankDetails.EmployeeID));
            sqlParameter.Add(new SqlParameter("@BankAccountNumber", employmentBankDetails.BankAccountNumber));
            sqlParameter.Add(new SqlParameter("@IFSCCode", employmentBankDetails.IFSCCode));
            sqlParameter.Add(new SqlParameter("@BankName", employmentBankDetails.BankName));
            sqlParameter.Add(new SqlParameter("@UANNumber", employmentBankDetails.UANNumber));
            sqlParameter.Add(new SqlParameter("@PANNo", employmentBankDetails.PANNo));
            sqlParameter.Add(new SqlParameter("@PanCardImage", employmentBankDetails.PanCardImage));
            sqlParameter.Add(new SqlParameter("@AadharCardNo", employmentBankDetails.AadharCardNo));
            sqlParameter.Add(new SqlParameter("@AadhaarCardImage", employmentBankDetails.AadhaarCardImage));
            sqlParameter.Add(new SqlParameter("@UserID", employmentBankDetails.UserID));
            SqlParameterCollection pOutputParams = null;

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmployeeBankDetails, sqlParameter, ref pOutputParams);
            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow =>
                         new Result()
                         {
                             Message = dataRow.Field<string>("Result").ToString(),
                             PKNo = Convert.ToInt64(dataRow["RetBankDetailID"] ?? 0) // Get the inserted/updated BankDetailID
                         }
                    ).FirstOrDefault();
            }



            return model;
        }

        public EmploymentBankDetail GetEmploymentBankDetails(EmploymentBankDetailInputParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeeBankDetails, sqlParameter);

            EmploymentBankDetail employmentBankDetail = dataSet.Tables[0].AsEnumerable()
                            .Select(dataRow => new EmploymentBankDetail()
                            {
                                BankDetailID = dataRow.Field<long>("BankDetailID"),
                                EmployeeID = dataRow.Field<long>("EmployeeID"),
                                BankAccountNumber = dataRow.Field<string>("BankAccountNumber"),
                                IFSCCode = dataRow.Field<string>("IFSCCode"),
                                BankName = dataRow.Field<string>("BankName"),
                                UANNumber = dataRow.Field<string>("UANNumber"),
                                AadharCardNo = dataRow.Field<string>("AadharCardNo"),
                                AadhaarCardImage = dataRow.Field<string>("AadhaarCardImage"),
                                PANNo = dataRow.Field<string>("PANNo"),
                                PanCardImage = dataRow.Field<string>("PanCardImage"),

                            }).ToList().FirstOrDefault();
            if (employmentBankDetail == null)
            {
                employmentBankDetail = new EmploymentBankDetail();

            }
            if (employmentBankDetail.EmployeeID <= 0)
            {
                employmentBankDetail.EmployeeID = model.EmployeeID;
            }
            employmentBankDetail.UserID = model.UserID;

            return employmentBankDetail;

        }

        public Result AddUpdateEmploymentSeparationDetails(EmploymentSeparationDetail employmentSeparationDetails)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeSeparationID", employmentSeparationDetails.EmployeeSeparationID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", employmentSeparationDetails.EmployeeID));
            sqlParameter.Add(new SqlParameter("@AgeOnNetwork", employmentSeparationDetails.AgeOnNetwork));
            sqlParameter.Add(new SqlParameter("@PreviousExperience", employmentSeparationDetails.PreviousExperience));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningTraining", employmentSeparationDetails.DateOfJoiningTraining));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningFloor", employmentSeparationDetails.DateOfJoiningFloor));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningOJT", employmentSeparationDetails.DateOfJoiningOJT));
            sqlParameter.Add(new SqlParameter("@DateOfJoiningOnroll", employmentSeparationDetails.DateOfJoiningOnroll));
            sqlParameter.Add(new SqlParameter("@DateOfResignation", employmentSeparationDetails.DateOfResignation));
            sqlParameter.Add(new SqlParameter("@DateOfLeaving", employmentSeparationDetails.DateOfLeaving));
            sqlParameter.Add(new SqlParameter("@BackOnFloorDate", employmentSeparationDetails.BackOnFloorDate));
            sqlParameter.Add(new SqlParameter("@LeavingRemarks", employmentSeparationDetails.LeavingRemarks));
            sqlParameter.Add(new SqlParameter("@MailReceivedFromAndDate", employmentSeparationDetails.MailReceivedFromAndDate));
            sqlParameter.Add(new SqlParameter("@EmailSentToITDate", employmentSeparationDetails.EmailSentToITDate));
            sqlParameter.Add(new SqlParameter("@LeavingType", employmentSeparationDetails.LeavingType));
            sqlParameter.Add(new SqlParameter("@NoticeServed", employmentSeparationDetails.NoticeServed));
            sqlParameter.Add(new SqlParameter("@UserID", employmentSeparationDetails.UserID));
            SqlParameterCollection pOutputParams = null;

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmployeeSeparation, sqlParameter, ref pOutputParams);
            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow =>
                         new Result()
                         {
                             Message = dataRow.Field<string>("Result").ToString(),
                             PKNo = Convert.ToInt64(dataRow["RetEmployeeSeparationID"] ?? 0)
                         }
                    ).FirstOrDefault();
            }



            return model;
        }

        public EmploymentSeparationDetail GetEmploymentSeparationDetails(EmploymentSeparationInputParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeeSeparationDetail, sqlParameter);

            EmploymentSeparationDetail employmentSeparationDetail = dataSet.Tables[0].AsEnumerable()
                            .Select(dataRow => new EmploymentSeparationDetail()
                            {
                                EmployeeSeparationID = dataRow.Field<long>("EmployeeSeparationID"),
                                EmployeeID = dataRow.Field<long>("EmployeeID"),
                                AgeOnNetwork = dataRow.Field<int?>("AgeOnNetwork"),
                                PreviousExperience = dataRow.Field<string>("PreviousExperience"),
                                DateOfJoiningTraining = dataRow.Field<DateTime?>("DateOfJoiningTraining"),
                                DateOfJoiningFloor = dataRow.Field<DateTime?>("DateOfJoiningFloor"),
                                DateOfJoiningOJT = dataRow.Field<DateTime?>("DateOfJoiningOJT"),
                                DateOfResignation = dataRow.Field<DateTime?>("DateOfResignation"),
                                DateOfLeaving = dataRow.Field<DateTime?>("DateOfLeaving"),
                                BackOnFloorDate = dataRow.Field<DateTime?>("BackOnFloorDate"),
                                LeavingRemarks = dataRow.Field<string>("LeavingRemarks"),
                                MailReceivedFromAndDate = dataRow.Field<string>("MailReceivedFromAndDate"),
                                EmailSentToITDate = dataRow.Field<DateTime?>("EmailSentToITDate"),
                                LeavingType = dataRow.Field<string>("LeavingType"),
                                NoticeServed = dataRow.Field<int?>("NoticeServed"),
                                DateOfJoiningOnroll = dataRow.Field<DateTime?>("DateOfJoiningOnroll"),

                            }).ToList().FirstOrDefault();
            if (employmentSeparationDetail == null)
            {
                employmentSeparationDetail = new EmploymentSeparationDetail();

            }
            if (employmentSeparationDetail.EmployeeID <= 0)
            {
                employmentSeparationDetail.EmployeeID = model.EmployeeID;
            }
            employmentSeparationDetail.UserID = model.UserID;

            return employmentSeparationDetail;

        }


        #endregion

        #region Leave Policies

        public LeavePolicyModel GetSelectLeavePolicies(LeavePolicyModel model)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameters.Add(new SqlParameter("@LeavePolicyID", model.LeavePolicyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavePolicyDetails, sqlParameters);
            var result = dataSet.Tables[0].AsEnumerable()
                                .Select(dataRow => new LeavePolicyModel
                                {
                                    LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                                    CompanyID = dataRow.Field<long>("CompanyID"),
                                    LeavePolicyName = dataRow.Field<string>("LeavePolicyName"),
                                    Annual_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Annual_MaximumLeaveAllocationAllowed"),
                                    Annual_ApplicableAfterWorkingDays = dataRow.Field<int>("Annual_ApplicableAfterWorkingDays"),
                                    Annual_MaximumConsecutiveLeavesAllowed = dataRow.Field<int>("Annual_MaximumConsecutiveLeavesAllowed"),
                                    Annual_IsCarryForward = dataRow.Field<bool>("Annual_IsCarryForward"),
                                    Annual_MedicalDocument = dataRow.Field<bool>("Annual_MedicalDocument"),
                                    Annual_MaximumEarnedLeaveAllowed = dataRow.Field<int>("Annual_MaximumEarnedLeaveAllowed"),
                                    Annual_MaximumMedicalLeaveAllocationAllowed = dataRow.Field<int>("Annual_MaximumMedicalLeaveAllocationAllowed"),
                                    Maternity_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Maternity_MaximumLeaveAllocationAllowed"),
                                    Maternity_ApplicableAfterWorkingDays = dataRow.Field<int>("Maternity_ApplicableAfterWorkingDays"),
                                    Maternity_ApplyBeforeHowManyDays = dataRow.Field<int>("Maternity_ApplyBeforeHowManyDays"),
                                    Maternity_MedicalDocument = dataRow.Field<bool>("Maternity_MedicalDocument"),
                                    Adoption_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Adoption_MaximumLeaveAllocationAllowed"),
                                    Adoption_ApplicableAfterWorkingDays = dataRow.Field<int>("Adoption_ApplicableAfterWorkingDays"),
                                    Adoption_MedicalDocument = dataRow.Field<bool>("Adoption_MedicalDocument"),
                                    Miscarriage_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Miscarriage_MaximumLeaveAllocationAllowed"),
                                    Miscarriage_MedicalDocument = dataRow.Field<bool>("Miscarriage_MedicalDocument"),
                                    CampOff_HoursOfWork = dataRow.Field<int>("CampOff_HoursOfWork"),
                                    CampOff_ExpiryDate = dataRow.Field<int>("CampOff_ExpiryDate"),
                                    CampOff_MedicalDocument = dataRow.Field<bool>("CampOff_MedicalDocument"),
                                    Paternity_maximumLeaveAllocationAllowed = dataRow.Field<int>("Paternity_maximumLeaveAllocationAllowed"),
                                    Paternity_applicableAfterWorkingMonth = dataRow.Field<int>("Paternity_applicableAfterWorkingMonth"),
                                    Paternity_active = dataRow.Field<bool>("Paternity_active"),
                                    Paternity_medicalDocument = dataRow.Field<bool>("Paternity_medicalDocument"),
                                }).FirstOrDefault();



            return result;
        }


        public Results GetAllLeavePolicies(LeavePolicyInputParans model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameters.Add(new SqlParameter("@LeavePolicyID", model.LeavePolicyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavePolicyDetails, sqlParameters);
            result.LeavePolicy = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeavePolicyModel
                              {
                                  LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  LeavePolicyName = dataRow.Field<string>("LeavePolicyName"),
                                  Annual_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Annual_MaximumLeaveAllocationAllowed"),
                                  Annual_ApplicableAfterWorkingDays = dataRow.Field<int>("Annual_ApplicableAfterWorkingDays"),
                                  Annual_MaximumConsecutiveLeavesAllowed = dataRow.Field<int>("Annual_MaximumConsecutiveLeavesAllowed"),
                                  Annual_IsCarryForward = dataRow.Field<bool>("Annual_IsCarryForward"),
                                  Annual_MedicalDocument = dataRow.Field<bool>("Annual_MedicalDocument"),
                                  Annual_MaximumEarnedLeaveAllowed = dataRow.Field<int>("Annual_MaximumEarnedLeaveAllowed"),
                                  Annual_MaximumMedicalLeaveAllocationAllowed = dataRow.Field<int>("Annual_MaximumMedicalLeaveAllocationAllowed"),
                                  Maternity_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Maternity_MaximumLeaveAllocationAllowed"),
                                  Maternity_ApplicableAfterWorkingDays = dataRow.Field<int>("Maternity_ApplicableAfterWorkingDays"),

                                  Maternity_ApplyBeforeHowManyDays = dataRow.Field<int>("Maternity_ApplyBeforeHowManyDays"),
                                  Maternity_MedicalDocument = dataRow.Field<bool>("Maternity_MedicalDocument"),
                                  Adoption_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Adoption_MaximumLeaveAllocationAllowed"),
                                  Adoption_ApplicableAfterWorkingDays = dataRow.Field<int>("Adoption_ApplicableAfterWorkingDays"),
                                  Adoption_MedicalDocument = dataRow.Field<bool>("Adoption_MedicalDocument"),
                                  Miscarriage_MaximumLeaveAllocationAllowed = dataRow.Field<int>("Miscarriage_MaximumLeaveAllocationAllowed"),
                                  Miscarriage_MedicalDocument = dataRow.Field<bool>("Miscarriage_MedicalDocument"),
                                  CampOff_HoursOfWork = dataRow.Field<int>("CampOff_HoursOfWork"),
                                  CampOff_ExpiryDate = dataRow.Field<int>("CampOff_ExpiryDate"),
                                  CampOff_MedicalDocument = dataRow.Field<bool>("CampOff_MedicalDocument"),
                                  Paternity_maximumLeaveAllocationAllowed = dataRow.Field<int>("Paternity_maximumLeaveAllocationAllowed"),
                                  Paternity_applicableAfterWorkingMonth = dataRow.Field<int>("Paternity_applicableAfterWorkingMonth"),
                                  Paternity_active = dataRow.Field<bool>("Paternity_active"),
                                  Paternity_medicalDocument = dataRow.Field<bool>("Paternity_medicalDocument"),
                              }).ToList();


            if (model.LeavePolicyID > 0)
            {
                result.leavePolicyModel = result.LeavePolicy.FirstOrDefault();

            }


            return result;
        }

        public Result AddUpdateLeavePolicy(LeavePolicyModel leavePolicyModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@LeavePolicyID", leavePolicyModel.LeavePolicyID));
            sqlParameters.Add(new SqlParameter("@CompanyID", leavePolicyModel.CompanyID));
            sqlParameters.Add(new SqlParameter("@LeavePolicyName", leavePolicyModel.LeavePolicyName));
            sqlParameters.Add(new SqlParameter("@Annual_MaximumLeaveAllocationAllowed", leavePolicyModel.Annual_MaximumLeaveAllocationAllowed));
            sqlParameters.Add(new SqlParameter("@Annual_ApplicableAfterWorkingDays", leavePolicyModel.Annual_ApplicableAfterWorkingDays));
            sqlParameters.Add(new SqlParameter("@Annual_MaximumConsecutiveLeavesAllowed", leavePolicyModel.Annual_MaximumConsecutiveLeavesAllowed));
            sqlParameters.Add(new SqlParameter("@Annual_MaximumEarnedLeaveAllowed", leavePolicyModel.Annual_MaximumEarnedLeaveAllowed));
            sqlParameters.Add(new SqlParameter("@Annual_MaximumMedicalLeaveAllocationAllowed", leavePolicyModel.Annual_MaximumMedicalLeaveAllocationAllowed));
            sqlParameters.Add(new SqlParameter("@Annual_IsCarryForward", leavePolicyModel.Annual_IsCarryForward));
            sqlParameters.Add(new SqlParameter("@Annual_MedicalDocument", leavePolicyModel.Annual_MedicalDocument));
            sqlParameters.Add(new SqlParameter("@Maternity_MaximumLeaveAllocationAllowed", leavePolicyModel.Maternity_MaximumLeaveAllocationAllowed));
            sqlParameters.Add(new SqlParameter("@Maternity_ApplicableAfterWorkingDays", leavePolicyModel.Maternity_ApplicableAfterWorkingDays));
            sqlParameters.Add(new SqlParameter("@Maternity_ApplyBeforeHowManyDays", leavePolicyModel.Maternity_ApplyBeforeHowManyDays));
            sqlParameters.Add(new SqlParameter("@Maternity_MedicalDocument", leavePolicyModel.Maternity_MedicalDocument));
            sqlParameters.Add(new SqlParameter("@Adoption_MaximumLeaveAllocationAllowed", leavePolicyModel.Adoption_MaximumLeaveAllocationAllowed));
            sqlParameters.Add(new SqlParameter("@Adoption_ApplicableAfterWorkingDays", leavePolicyModel.Adoption_ApplicableAfterWorkingDays));
            sqlParameters.Add(new SqlParameter("@Adoption_MedicalDocument", leavePolicyModel.Adoption_MedicalDocument));
            sqlParameters.Add(new SqlParameter("@Miscarriage_MaximumLeaveAllocationAllowed", leavePolicyModel.Miscarriage_MaximumLeaveAllocationAllowed));
            sqlParameters.Add(new SqlParameter("@Miscarriage_MedicalDocument", leavePolicyModel.Miscarriage_MedicalDocument));
            sqlParameters.Add(new SqlParameter("@CampOff_HoursOfWork", leavePolicyModel.CampOff_HoursOfWork));
            sqlParameters.Add(new SqlParameter("@CampOff_ExpiryDate", leavePolicyModel.CampOff_ExpiryDate));
            sqlParameters.Add(new SqlParameter("@CampOff_MedicalDocument", leavePolicyModel.CampOff_MedicalDocument));
            sqlParameters.Add(new SqlParameter("@Adoption_ApplyBeforeHowManyDays", leavePolicyModel.Adoption_ApplicableAfterWorkingDays));
            sqlParameters.Add(new SqlParameter("@Paternity_maximumLeaveAllocationAllowed", leavePolicyModel.Paternity_maximumLeaveAllocationAllowed));
            sqlParameters.Add(new SqlParameter("@Paternity_applicableAfterWorkingMonth", leavePolicyModel.Paternity_applicableAfterWorkingMonth));
            sqlParameters.Add(new SqlParameter("@Paternity_active", leavePolicyModel.Paternity_active));
            sqlParameters.Add(new SqlParameter("@Paternity_medicalDocument", leavePolicyModel.Paternity_medicalDocument));



            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_LeavePolicy, sqlParameters);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }
        #endregion

        #region Holidays

        public Results GetAllHolidays(HolidayInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter =
            [
                new SqlParameter("@CompanyID", model.CompanyID),
                new SqlParameter("@HolidayID", model.HolidayID),
            ];

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_HolidayDetails, sqlParameter);
            result.Holiday = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new HolidayModel
                              {
                                  HolidayID = dataRow.Field<long>("HolidayID"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  HolidayName = dataRow.Field<string>("HolidayName"),
                                  FromDate = dataRow.Field<DateTime>("FromDate"),
                                  ToDate = dataRow.Field<DateTime>("ToDate"),
                                  Description = dataRow.Field<string>("Description"),
                                  Status = dataRow.Field<bool>("Status"),
                                  JobLocationTypeID = dataRow.Field<long>("JobLocationTypeID")
                              }).ToList();

            if (model.HolidayID > 0)
            {
                result.holidayModel = result.Holiday.FirstOrDefault();
            }
            result.JobLocationList = dataSet.Tables[1].AsEnumerable()
                            .Select(dataRow => new SelectListItem
                            {
                                Value = dataRow.Field<long>("ID").ToString(),
                                Text = dataRow.Field<string>("Name")
                            }).ToList();
            return result;
        }

        public Result AddUpdateHoliday(HolidayModel HolidayModel)
        {
            Result model = new Result();

            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@HolidayID", HolidayModel.HolidayID));
            sqlParameter.Add(new SqlParameter("@HolidayName", HolidayModel.HolidayName));
            sqlParameter.Add(new SqlParameter("@FromDate", HolidayModel.FromDate));
            sqlParameter.Add(new SqlParameter("@ToDate", HolidayModel.ToDate));
            sqlParameter.Add(new SqlParameter("@CompanyID", HolidayModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@Status", HolidayModel.Status));
            sqlParameter.Add(new SqlParameter("@Description", HolidayModel.Description));
            sqlParameter.Add(new SqlParameter("@JobLocationTypeID", HolidayModel.JobLocationTypeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Holiday, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }

        public List<SelectListItem> GetHolidayList(HolidayInputParams model)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_HolidayList, sqlParameter);

            result = dataSet.Tables[0].AsEnumerable()
                          .Select(dataRow => new SelectListItem
                          {
                              Value = dataRow.Field<long>("HolidayID").ToString(),
                              Text = dataRow.Field<string>("HolidayName"),
                          }).ToList();
            return result;
        }
        public Results GetAllHolidayList(HolidayInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            if (model.LocationID.HasValue && model.LocationID.Value > 0)
            {
                sqlParameter.Add(new SqlParameter("@JobLocationID", model.LocationID.Value));
            }
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_HolidayList, sqlParameter);

            result.Holiday = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new HolidayModel
                               {
                                   HolidayID = dataRow.Field<long>("HolidayID"),
                                   CompanyID = dataRow.Field<long>("CompanyID"),
                                   HolidayName = dataRow.Field<string>("HolidayName"),
                                   FromDate = dataRow.Field<DateTime>("FromDate"),
                                   ToDate = dataRow.Field<DateTime>("ToDate"),
                                   Description = dataRow.Field<string>("Description"),
                                   Location = dataRow.Field<string>("JobLocationName"),
                                   Status = dataRow.Field<bool>("Status"),
                               }).ToList();
            result.JobLocationList = dataSet.Tables[1].AsEnumerable()
                           .Select(dataRow => new SelectListItem
                           {
                               Value = dataRow.Field<long>("JobLocationID").ToString(),
                               Text = dataRow.Field<string>("JobLocationName")
                           }).ToList();

            if (model.CompanyID > 0)
            {
                result.holidayModel = result.Holiday.FirstOrDefault();
            }

            return result;
        }
        #endregion

        #region Leaves

        public LeaveResults GetLeaveForApprovals(MyInfoInputParams model)
        {

            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@ReportingToEmployeeID", model.EmployeeID));
            sqlParameter.Add(new SqlParameter("@RoleId", model.RoleId));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_AgentLeavesSummaryForApproval, sqlParameter);
            result.leavesSummary = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeaveSummaryModel
                              {
                                  EmployeeNumber = dataRow.Field<string>("EmployeNumber"),
                                  EmployeeName = dataRow.Field<string>("EmployeeName"),
                                  AppliedByNumber = dataRow.Field<string>("AppliedByNumber"),
                                  AppliedByName = dataRow.Field<string>("AppliedByName"),
                                  LeaveSummaryID = dataRow.Field<long>("LeaveSummaryID"),
                                  LeaveStatusID = dataRow.Field<long>("LeaveStatusID"),
                                  LeaveTypeID = dataRow.Field<long>("LeaveTypeID"),
                                  LeaveTypeName = dataRow.Field<string>("LeaveTypeName"),
                                  LeaveDurationTypeID = dataRow.Field<long>("LeaveDurationTypeID"),
                                  LeaveStatusName = dataRow.Field<string>("LeaveStatusName"),
                                  Reason = dataRow.Field<string>("Reason"),
                                  RequestDate = dataRow.Field<DateTime>("RequestDate"),
                                  StartDate = dataRow.Field<DateTime>("StartDate"),
                                  EndDate = dataRow.Field<DateTime>("EndDate"),
                                  StartDateFormatted = dataRow.Field<DateTime>("StartDate").ToString("dd/M/yyyy"),
                                  EndDateFormatted = dataRow.Field<DateTime>("EndDate").ToString("dd/M/yyyy"),
                                  LeaveDurationTypeName = dataRow.Field<string>("LeaveDurationTypeName"),
                                  NoOfDays = dataRow.Field<decimal>("NoOfDays"),
                                  IsActive = dataRow.Field<bool>("IsActive"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  OfficialEmailID = dataRow.Field<string>("OfficialEmailID" ?? ""),
                                  ManagerOfficialEmailID = dataRow.Field<string>("ManagerOfficialEmailID") ?? "",
                                  EmployeeFirstName = dataRow.Field<string>("EmployeeFirstName") ?? "",
                                  ManagerFirstName = dataRow.Field<string>("ManagerFirstName") ?? "",
                                  ChildDOB = dataRow.Field<DateTime?>("ChildDOB"),
                                  LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                                  JoiningDate = dataRow.Field<DateTime>("JoiningDate"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),

                              }).ToList();

            result.leaveTypes = GetLeaveTypes(model).leaveTypes;
            //  result.leavePolicy = GetAllLeavePolicies(model).leaveTypes;
            result.leaveDurationTypes = GetLeaveDurationTypes(model).leaveDurationTypes;
            if (model.LeaveSummaryID > 0)
            {
                result.leaveSummaryModel = result.leavesSummary.Where(x => x.LeaveSummaryID == model.LeaveSummaryID).FirstOrDefault();
            }

            return result;
        }

        public Result AddUpdateLeave(LeaveSummaryModel leaveSummaryModel)
        {
            Result model = new Result();
            var oldData = GetLeaveSummaryByID(leaveSummaryModel.LeaveSummaryID);
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            var leaveSummaryIDParam = new SqlParameter("@LeaveSummaryID", SqlDbType.BigInt);
            leaveSummaryIDParam.Direction = ParameterDirection.Output;
            leaveSummaryIDParam.Value = leaveSummaryModel.LeaveSummaryID; // 0 for new insert
            sqlParameter.Add(leaveSummaryIDParam);

            
            sqlParameter.Add(new SqlParameter("@EmployeeID", leaveSummaryModel.EmployeeID));
            sqlParameter.Add(new SqlParameter("@LeaveStatusID", leaveSummaryModel.LeaveStatusID));
            sqlParameter.Add(new SqlParameter("@LeaveDurationTypeID", leaveSummaryModel.LeaveDurationTypeID));
            sqlParameter.Add(new SqlParameter("@Reason", leaveSummaryModel.Reason));
            sqlParameter.Add(new SqlParameter("@StartDate", leaveSummaryModel.StartDate));
            sqlParameter.Add(new SqlParameter("@EndDate", leaveSummaryModel.EndDate));
            sqlParameter.Add(new SqlParameter("@LeaveTypeID", leaveSummaryModel.LeaveTypeID));
            sqlParameter.Add(new SqlParameter("@NoOfDays", leaveSummaryModel.NoOfDays));
            sqlParameter.Add(new SqlParameter("@IsActive", leaveSummaryModel.IsActive));
            sqlParameter.Add(new SqlParameter("@IsDeleted", leaveSummaryModel.IsDeleted));
            sqlParameter.Add(new SqlParameter("@UserID", leaveSummaryModel.UserID));
            sqlParameter.Add(new SqlParameter("@LeavePolicyID", leaveSummaryModel.LeavePolicyID));
            sqlParameter.Add(new SqlParameter("@ApproveRejectComment", leaveSummaryModel.ApproveRejectComment));
            sqlParameter.Add(new SqlParameter("@UploadCertificate", leaveSummaryModel.UploadCertificate ?? ""));
            sqlParameter.Add(new SqlParameter("@ExpectedDeliveryDate", leaveSummaryModel.ExpectedDeliveryDate ?? (object)DBNull.Value));
            sqlParameter.Add(new SqlParameter("@ChildDOB", leaveSummaryModel.ChildDOB ?? (object)DBNull.Value));
            sqlParameter.Add(new SqlParameter("@CampOff", leaveSummaryModel.CampOff ?? Convert.ToInt32(CompOff.OtherLeaves)));
            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_LeaveSummary, sqlParameter, ref pOutputParams);
            var outputID = Convert.ToInt64(pOutputParams["@LeaveSummaryID"].Value);
            var newData = GetLeaveSummaryByID(
       Convert.ToInt64(pOutputParams["@LeaveSummaryID"].Value)
   );
            string editMode = leaveSummaryModel.LeaveSummaryID == 0 ? "Add" : "Edit";
            TrackLogAudit(
                oldData,
                newData,
                editMode,
                leaveSummaryModel.UserID,
                "LeaveSummary",
                "tbl_LeaveSummary",
                 Convert.ToInt64(pOutputParams["@LeaveSummaryID"].Value),
                "tbl_LeaveSummary_Log",
                "Leave Summary Details"
            );

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            PKNo = Convert.ToInt64(pOutputParams["@LeaveSummaryID"].Value)
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }

        public LeaveResults GetlLeavesSummary(MyInfoInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@LeaveSummaryID", model.LeaveSummaryID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            sqlParameter.Add(new SqlParameter("@JobLocationTypeID", model.JobLocationTypeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavesSummary, sqlParameter);
            result.leavesSummary = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeaveSummaryModel
                              {
                                  LeaveSummaryID = dataRow.Field<long>("LeaveSummaryID"),
                                  LeaveStatusID = dataRow.Field<long>("LeaveStatusID"),
                                  LeaveTypeID = dataRow.Field<long>("LeaveTypeID"),
                                  LeaveDurationTypeID = dataRow.Field<long>("LeaveDurationTypeID"),
                                  LeaveStatusName = dataRow.Field<string>("LeaveStatusName"),
                                  Reason = dataRow.Field<string>("Reason"),
                                  RequestDate = dataRow.Field<DateTime>("RequestDate"),
                                  StartDate = dataRow.Field<DateTime>("StartDate"),
                                  EndDate = dataRow.Field<DateTime>("EndDate"),
                                  LeaveTypeName = dataRow.Field<string>("LeaveTypeName"),
                                  LeaveDurationTypeName = dataRow.Field<string>("LeaveDurationTypeName"),
                                  UploadCertificate = dataRow.Field<string>("UploadCertificate"),
                                  ExpectedDeliveryDate = dataRow.Field<DateTime?>("ExpectedDeliveryDate"),
                                  NoOfDays = dataRow.Field<decimal>("NoOfDays"),
                                  IsActive = dataRow.Field<bool>("IsActive"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  ChildDOB = dataRow.Field<DateTime?>("ChildDOB"),
                              }).ToList();

            // result.leaveTypes = GetLeaveTypes(model).leaveTypes;
            result.leaveDurationTypes = GetLeaveDurationTypes(model).leaveDurationTypes;
            if (model.LeaveSummaryID > 0)
            {
                result.leaveSummaryModel = result.leavesSummary.Where(x => x.LeaveSummaryID == model.LeaveSummaryID).FirstOrDefault();
            }
            return result;
        }


        public LeaveResults GetAgentLeaveSummary(MyInfoInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@LeaveSummaryID", model.LeaveSummaryID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            sqlParameter.Add(new SqlParameter("@JobLocationTypeID", model.JobLocationTypeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_AgentLeavesSummary, sqlParameter);
            result.leavesSummary = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeaveSummaryModel
                              {
                                  LeaveSummaryID = dataRow.Field<long>("LeaveSummaryID"),
                                  LeaveStatusID = dataRow.Field<long>("LeaveStatusID"),
                                  LeaveTypeID = dataRow.Field<long>("LeaveTypeID"),
                                  LeaveDurationTypeID = dataRow.Field<long>("LeaveDurationTypeID"),
                                  LeaveStatusName = dataRow.Field<string>("LeaveStatusName"),
                                  Reason = dataRow.Field<string>("Reason"),
                                  RequestDate = dataRow.Field<DateTime>("RequestDate"),
                                  StartDate = dataRow.Field<DateTime>("StartDate"),
                                  EndDate = dataRow.Field<DateTime>("EndDate"),
                                  LeaveTypeName = dataRow.Field<string>("LeaveTypeName"),
                                  LeaveDurationTypeName = dataRow.Field<string>("LeaveDurationTypeName"),
                                  UploadCertificate = dataRow.Field<string>("UploadCertificate"),
                                  ExpectedDeliveryDate = dataRow.Field<DateTime?>("ExpectedDeliveryDate"),
                                  NoOfDays = dataRow.Field<decimal>("NoOfDays"),
                                  IsActive = dataRow.Field<bool>("IsActive"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  ChildDOB = dataRow.Field<DateTime?>("ChildDOB"),
                                  EmployeeNumber = dataRow.Field<string>("EmployeNumber"),
                                  EmployeeName = dataRow.Field<string>("EmployeeName"),
                                  AppliedByNumber = dataRow.Field<string>("AppliedByNumber"),
                                  AppliedByName = dataRow.Field<string>("AppliedByName"),
                                  JobLocationID = dataRow.Field<long>("JobLocationID"),
                                  Gender = dataRow.Field<int>("Gender")
                              }).ToList();

            // result.leaveTypes = GetLeaveTypes(model).leaveTypes;
            result.leaveDurationTypes = GetLeaveDurationTypes(model).leaveDurationTypes;
            if (model.LeaveSummaryID > 0)
            {
                result.leaveSummaryModel = result.leavesSummary.Where(x => x.LeaveSummaryID == model.LeaveSummaryID).FirstOrDefault();
            }
            return result;
        }

        public LeaveResults GetLeaveDurationTypes(MyInfoInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeaveDurationTypes, sqlParameter);
            result.leaveDurationTypes = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new SelectListItem
                              {
                                  Value = dataRow.Field<long>("LeaveDurationTypeID").ToString(),
                                  Text = dataRow.Field<string>("Name"),
                              }).ToList();

            return result;
        }

        public LeaveResults GetLeaveTypes(MyInfoInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@GenderId", model.GenderId));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeaveTypes, sqlParameter);
            result.leaveTypes = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new SelectListItem
                              {
                                  Value = dataRow.Field<long>("LeaveTypeID").ToString(),
                                  Text = dataRow.Field<string>("Name"),
                              }).ToList();

            return result;
        }

        public string DeleteLeavesSummary(MyInfoInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@LeaveSummaryID", model.LeaveSummaryID));
            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };
            sqlParameter.Add(outputMessage);
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_LeaveSummary, sqlParameter);
            string message = outputMessage.Value.ToString();
            return message;
        }

        #endregion

        #region Dashboard
        public DashBoardModel GetDashBoardModel(DashBoardModelInputParams model)
        {
            DashBoardModel dashBoardModel = new DashBoardModel();
            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>();
                sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
                sqlParameter.Add(new SqlParameter("@RoleId", model.RoleID));
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetDashBoardDetails, sqlParameter);
                dashBoardModel = dataSet.Tables[0].AsEnumerable()
                                  .Select(dataRow => new DashBoardModel
                                  {
                                      EmployeeID = dataRow.Field<long>("EmployeeID"),
                                      guid = dataRow.Field<Guid>("guid"),
                                      CompanyID = dataRow.Field<long>("CompanyID"),
                                      ProfilePhoto = dataRow.Field<string>("ProfilePhoto"),
                                      FirstName = dataRow.Field<string>("FirstName"),
                                      MiddleName = dataRow.Field<string>("MiddleName"),
                                      Surname = dataRow.Field<string>("Surname"),
                                      CorrespondenceAddress = dataRow.Field<string>("CorrespondenceAddress"),
                                      CorrespondenceCity = dataRow.Field<string>("CorrespondenceCity"),
                                      CorrespondencePinCode = dataRow.Field<string>("CorrespondencePinCode"),
                                      CorrespondenceState = dataRow.Field<string>("CorrespondenceState"),
                                      CorrespondenceCountryID = dataRow.Field<long>("CorrespondenceCountryID"),
                                      EmailAddress = dataRow.Field<string>("EmailAddress"),
                                      Landline = dataRow.Field<string>("Landline"),
                                      Mobile = dataRow.Field<string>("Mobile"),
                                      Telephone = dataRow.Field<string>("Telephone"),
                                      PersonalEmailAddress = dataRow.Field<string>("PersonalEmailAddress"),
                                      PermanentAddress = dataRow.Field<string>("PermanentAddress"),
                                      PermanentCity = dataRow.Field<string>("PermanentCity"),
                                      PermanentPinCode = dataRow.Field<string>("PermanentPinCode"),
                                      PermanentState = dataRow.Field<string>("PermanentPinCode"),
                                      PermanentCountryID = dataRow.Field<long>("PermanentCountryID"),
                                      VerificationContactPersonName = dataRow.Field<string>("VerificationContactPersonName"),
                                      VerificationContactPersonContactNo = dataRow.Field<string>("VerificationContactPersonContactNo"),
                                      DateOfBirth = dataRow.Field<DateTime?>("DateOfBirth"),
                                      PlaceOfBirth = dataRow.Field<string>("PlaceOfBirth"),
                                      IsReferredByExistingEmployee = dataRow.Field<bool>("IsReferredByExistingEmployee"),
                                      ReferredByEmployeeID = dataRow.Field<string>("ReferredByEmployeeID"),
                                      BloodGroup = dataRow.Field<string>("BloodGroup"),
                                      PANNo = dataRow.Field<string>("PANNo"),
                                      AadharCardNo = dataRow.Field<string>("AadharCardNo"),
                                      Allergies = dataRow.Field<string>("Allergies"),
                                      RelativesDetails = dataRow.Field<string>("RelativesDetails"),
                                      MajorIllnessOrDisability = dataRow.Field<string>("MajorIllnessOrDisability"),
                                      AwardsAchievements = dataRow.Field<string>("AwardsAchievements"),
                                      EducationGap = dataRow.Field<string>("EducationGap"),
                                      ExtraCuricuarActivities = dataRow.Field<string>("ExtraCuricuarActivities"),
                                      ForiegnCountryVisits = dataRow.Field<string>("ForiegnCountryVisits"),
                                      ContactPersonName = dataRow.Field<string>("ContactPersonName"),
                                      ContactPersonMobile = dataRow.Field<string>("ContactPersonMobile"),
                                      ContactPersonTelephone = dataRow.Field<string>("ContactPersonTelephone"),
                                      ContactPersonRelationship = dataRow.Field<string>("ContactPersonRelationship"),
                                      ITSkillsKnowledge = dataRow.Field<string>("ITSkillsKnowledge"),

                                      //EmploymentDetails
                                      EmployeeTypeID = dataRow.Field<long>("EmployeeTypeID"),
                                      EmploymentDetailID = dataRow.Field<long>("EmploymentDetailID"),
                                      DesignationID = dataRow.Field<long>("DesignationID"),
                                      DepartmentID = dataRow.Field<long>("DepartmentID"),
                                      JobLocationID = dataRow.Field<long>("JobLocationID"),
                                      ReportingToIDL1 = dataRow.Field<long>("ReportingToIDL1"),
                                      OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                      OfficialContactNo = dataRow.Field<string>("OfficialContactNo"),
                                      JoiningDate = dataRow.Field<DateTime?>("JoiningDate"),
                                      JobSeprationDate = dataRow.Field<DateTime?>("JobSeprationDate"),
                                      CarryForword = dataRow.Field<double>("CarryForword"),
                                      LeavePolicyId = dataRow.Field<long>("LeavePolicyId"),
                                      PayrollTypeID = dataRow.Field<long>("PayrollTypeID"),
                                      ReportingToIDL2 = dataRow.Field<long>("ReportingToIDL2"),
                                      ClientName = dataRow.Field<string>("ClientName"),
                                      EmployeNumber = dataRow.Field<string>("EmployeNumber"),

                                  }).ToList().FirstOrDefault();

                var OtherDetails = dataSet.Tables[1].AsEnumerable()
                                  .Select(dataRow => new DashBoardModel
                                  {
                                      NoOfEmployees = dataRow.Field<int>("NoOfEmployees"),
                                  }).ToList().FirstOrDefault();

                var attendanceTable = dataSet.Tables[2];
                dashBoardModel.AttendanceModel = new List<AttendanceModel>();

                bool isDetailedReport = attendanceTable.Columns.Contains("EmployeeName");
                if (isDetailedReport)
                {
                    bool includeLevel2 = model.RoleID == 2 || model.RoleID == 5;

                    foreach (DataRow row in attendanceTable.Rows)
                    {
                        int level = row.Field<int>("Level");
                        if (level == 1 || (includeLevel2 && level == 2))
                        {
                            var attendance = new AttendanceModel
                            {
                                EmployeeId = row.Field<long>("EmployeeID"),
                                EmployeeNumber = row.Field<string>("EmployeNumber"),
                                EmployeeName = row.Field<string>("EmployeeName"),
                                ManagerName = row.Field<string>("ManagerName"),
                                Designation = row.Field<string>("Designation"),
                                Department = row.Field<string>("Department"),
                                Level = level,
                                AttendanceStatus = row.Field<string>("AttendanceStatus"),
                                ReferenceDate = row.Field<DateTime>("ReferenceDate"),
                                TotalPresent = row.Field<int>("TotalPresent"),
                                TotalAbsent = row.Field<int>("TotalAbsent"),
                                TotalLeaves = row.Field<int>("TotalLeave"),
                                TotalHoliday = row.Field<int>("TotalHoliday"),
                                TotalWeekOff = row.Field<int>("TotalWeekOff")
                            };

                            dashBoardModel.AttendanceModel.Add(attendance);
                        }
                    }

                }
                else
                {
                    foreach (DataRow row in attendanceTable.Rows)
                    {
                        var attendance = new AttendanceModel
                        {
                            Day = row.Field<DateTime>("Day"),
                            Present = row.Field<int>("Present"),
                            Absent = row.Field<int>("Absent"),
                            Leaves = row.Field<int>("Leave"),
                            PresentByLocation = new Dictionary<string, int>(),
                            AbsentByLocation = new Dictionary<string, int>()
                        };

                        foreach (DataColumn column in attendanceTable.Columns)
                        {
                            if (column.ColumnName.EndsWith("_Present"))
                            {
                                var location = column.ColumnName.Replace("_Present", "");
                                int present = row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
                                attendance.PresentByLocation[location] = present;
                            }
                            else if (column.ColumnName.EndsWith("_Absent"))
                            {
                                var location = column.ColumnName.Replace("_Absent", "");
                                int absent = row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
                                attendance.AbsentByLocation[location] = absent;
                            }
                            else if (column.ColumnName.EndsWith("_Leave"))
                            {
                                var location = column.ColumnName.Replace("_Leave", "");
                                int absent = row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
                                attendance.LeaveByLocation[location] = absent;
                            }
                        }

                        dashBoardModel.AttendanceModel.Add(attendance);
                    }
                }

                dashBoardModel.EmployeeDetails = dataSet.Tables[3].AsEnumerable()
     .Select(dataRow => new EmployeeDetails
     {
         EmployeeId = dataRow.Field<long>("EmployeeId"),
         FirstName = dataRow.Field<string>("EmployeeFirstName"),
         LastName = dataRow.Field<string>("EmployeeLastName"),
         DOB = dataRow.Field<DateTime?>("EmployeeDOB"),
         EmployeePhoto = dataRow.Field<string>("EmployeePhoto"),
     }).ToList();


                var LeaveDetails = dataSet.Tables[4].AsEnumerable()
                              .Select(dataRow => new DashBoardModel
                              {
                                  TotalLeave = dataRow.Field<decimal>("TotalLeave"),
                              }).ToList().FirstOrDefault();

                var HolidayList = dataSet.Tables[5].AsEnumerable()
                              .Select(dataRow => new HolidayModel
                              {
                                  FromDate = dataRow.Field<DateTime>("FromDate"),
                                  HolidayName = dataRow.Field<string>("HolidayName"),
                              }).ToList();
                var WhatsHappening = dataSet.Tables[6].AsEnumerable()
                             .Select(dataRow => new WhatsHappening
                             {
                                 WhatsHappeningID = dataRow.Field<long>("WhatsHappeningID"),
                                 Title = dataRow.Field<string>("Title"),
                                 Description = dataRow.Field<string>("Description"),
                                 FromDate = dataRow.Field<DateTime>("FromDate"),
                                 ToDate = dataRow.Field<DateTime>("ToDate"),
                                 IconImage = dataRow.Field<string>("IconImage"),

                             }).ToList();

                var TotalRecordPresent = dataSet.Tables[7].AsEnumerable()
                       .Select(dataRow => new DashBoardModel
                       {
                           RecordPresent = dataRow.Field<int>("RecordPresent"),
                       }).ToList().FirstOrDefault();
                dashBoardModel.RecordPresent = TotalRecordPresent.RecordPresent;

                if (dataSet.Tables.Count > 8)
                {
                    var hierarchyTable = dataSet.Tables[8];
                    var flatList = hierarchyTable.AsEnumerable()
                        .Select(row => new HierarchyEmployee
                        {
                            EmployeNumber = row.Field<string>("EmployeNumber") ?? string.Empty,
                            EmployeeID = row.IsNull("EmployeeID") ? 0 : row.Field<long>("EmployeeID"),
                            EmployeeName = row.Field<string>("EmployeeName") ?? string.Empty,
                            ManagerName = row.Field<string>("ManagerName") ?? string.Empty,
                            Designation = row.Field<string>("Designation") ?? string.Empty,
                            Department = row.Field<string>("Department") ?? string.Empty,
                            ProfilePhoto = row.Field<string>("ProfilePhoto") ?? string.Empty,
                            Level = row.IsNull("Level") ? 0 : row.Field<int>("Level"),
                            Path = row.Field<string>("Path") ?? string.Empty,
                            Subordinate = row.Field<int?>("SubordinateCount") ?? 0,
                            TotalSubordinateCount = row.Field<int?>("TotalSubordinateCount") ?? 0,
                            RoleID = row.Field<long?>("RoleID") ?? 0
                        })
                        .ToList();

                    var hierarchy = BuildCombinedHierarchy(flatList);

                    if (model.RoleID == 2 || model.RoleID == 5)
                    {
                        var extraSubordinates = hierarchy
                            .SelectMany(e => e.Subordinates ?? new List<HierarchyEmployee>())
                            .ToList();
                        dashBoardModel.EmployeeHierarchy = hierarchy
                            .Concat(extraSubordinates)
                            .ToList();
                    }
                    else
                    {
                        dashBoardModel.EmployeeHierarchy = hierarchy;
                    }
                }
                if (model.RoleID == (int)Roles.SuperAdmin || model.RoleID == (int)Roles.HR || model.RoleID == (int)Roles.Admin)
                {
                    var CompanyDetails = dataSet.Tables[9].AsEnumerable()
                           .Select(dataRow => new DashBoardModel
                           {
                               CountsOfCompanies = dataRow.Field<int>("TotalCompanies"),
                           }).ToList().FirstOrDefault();
                    dashBoardModel.CountsOfCompanies = CompanyDetails.CountsOfCompanies;
                }

                if (model.RoleID == (int)Roles.SuperAdmin || model.RoleID == (int)Roles.Admin)
                {
                    var Employment = dataSet.Tables[10].AsEnumerable()
                           .Select(dataRow => new DashBoardModel
                           {
                               TotalSeniorCore = dataRow.Field<int?>("TotalSeniorCore"),
                               TotalCCE = dataRow.Field<int?>("TotalCCE")

                           }).ToList().FirstOrDefault();
                    dashBoardModel.TotalSeniorCore = Employment.TotalSeniorCore;
                    dashBoardModel.TotalCCE = Employment.TotalCCE;


                }
                if (model.RoleID == (int)Roles.SuperAdmin || model.RoleID == (int)Roles.Admin)
                {
                    var attendanceTablea = dataSet.Tables[11];

                    foreach (DataRow row in attendanceTablea.Rows)
                    {
                        var attendance = new AttendanceModel
                        {
                            Day = row.Field<DateTime>("Day"),
                            Present = row.Field<int>("Present"),
                            Absent = row.Field<int>("Absent"),
                            Leaves = row.Field<int>("Leave"),
                            PresentByLocation = new Dictionary<string, int>(),
                            AbsentByLocation = new Dictionary<string, int>()
                        };

                        foreach (DataColumn column in attendanceTablea.Columns)
                        {
                            if (column.ColumnName.EndsWith("_Present"))
                            {
                                var location = column.ColumnName.Replace("_Present", "");
                                int present = row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
                                attendance.PresentByLocation[location] = present;
                            }
                            else if (column.ColumnName.EndsWith("_Absent"))
                            {
                                var location = column.ColumnName.Replace("_Absent", "");
                                int absent = row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
                                attendance.AbsentByLocation[location] = absent;
                            }
                            else if (column.ColumnName.EndsWith("_Leave"))
                            {
                                var location = column.ColumnName.Replace("_Leave", "");
                                int absent = row.IsNull(column) ? 0 : Convert.ToInt32(row[column]);
                                attendance.LeaveByLocation[location] = absent;
                            }
                        }

                        dashBoardModel.AttendanceModel.Add(attendance);
                    }

                }

                if (dashBoardModel == null)
                {
                    dashBoardModel = new DashBoardModel();
                }

                dashBoardModel.NoOfEmployees = OtherDetails.NoOfEmployees;
                dashBoardModel.TotalLeave = LeaveDetails.TotalLeave;
                dashBoardModel.HolidayList = HolidayList;
                dashBoardModel.WhatsHappening = WhatsHappening;
                // dashBoardModel.NoOfCompanies = OtherDetails.NoOfCompanies;
            }
            catch (Exception ex)
            {

            }


            MyInfoInputParams objmodel = new MyInfoInputParams();
            objmodel.EmployeeID = model.EmployeeID;
            objmodel.JobLocationTypeID = model.JobLocationId;
            dashBoardModel.leaveResults = GetlLeavesSummary(objmodel);
            return dashBoardModel;
        }






        public List<HierarchyEmployee> BuildCombinedHierarchy(List<HierarchyEmployee> flatList)
        {
            if (flatList == null || flatList.Count == 0)
                return new List<HierarchyEmployee>();


            var combined = BuildHierarchyTree(flatList);

            return combined;
        }

        private List<HierarchyEmployee> BuildHierarchyTree(List<HierarchyEmployee> flatList)
        {
            var pathLookup = flatList.ToDictionary(e => e.Path, e => e);
            var hierarchy = new List<HierarchyEmployee>();

            foreach (var employee in flatList)
            {
                var parentPath = GetParentPath(employee.Path);

                if (!string.IsNullOrEmpty(parentPath) && pathLookup.ContainsKey(parentPath))
                {
                    var parent = pathLookup[parentPath];
                    if (parent.Subordinates == null)
                        parent.Subordinates = new List<HierarchyEmployee>();


                    if (!parent.Subordinates.Any(s => s.Path == employee.Path))
                    {
                        parent.Subordinates.Add(employee);
                    }
                }
                else
                {
                    if (!hierarchy.Any(h => h.Path == employee.Path))
                    {
                        hierarchy.Add(employee);
                    }
                }
            }

            return hierarchy;
        }

        private string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                return null;

            var parts = path.Trim('/').Split('/');
            if (parts.Length <= 1)
                return null;

            return "/" + string.Join("/", parts.Take(parts.Length - 1));
        }







        //        public List<HierarchyEmployee> BuildCombinedHierarchy(List<HierarchyEmployee> flatList)
        //        {
        //            var combined = new List<HierarchyEmployee>();


        //            combined.AddRange(BuildHierarchyTree(flatList));

        //            var level2Paths = flatList
        //         .Where(e => e.Path.Trim('/').Split('/').Length == 2)
        //         .ToList();

        //            var parentPaths = level2Paths
        //                .Where(level2Path =>
        //                    flatList.Any(x => GetParentPathLevel2(x.Path) == level2Path.Path) // has children
        //                    ||
        //                    !flatList.Any(x => GetParentPathLevel2(x.Path) == level2Path.Path) // no one reports
        //                )
        //                .Select(e => e.Path)
        //                .Distinct()
        //                .ToList();


        //            foreach (var path in parentPaths)
        //            {

        //                var subtreeNodes = flatList
        //                    .Where(e => e.Path == path || GetParentPath(e.Path) == path)
        //                    .ToList();

        //                var subtree = BuildHierarchyTree(subtreeNodes);


        //                if (subtree.Any())
        //                {
        //                    combined.AddRange(subtree);
        //                }
        //            }

        //            return combined;
        //        }

        //        private List<HierarchyEmployee> BuildHierarchyTree(List<HierarchyEmployee> flatList)
        //        {
        //            var pathLookup = flatList.ToDictionary(e => e.Path);
        //            var hierarchy = new List<HierarchyEmployee>();

        //            foreach (var employee in flatList)
        //            {
        //                var parentPath = GetParentPath(employee.Path);

        //                if (!string.IsNullOrEmpty(parentPath) && pathLookup.ContainsKey(parentPath))
        //                {
        //                    var parent = pathLookup[parentPath];
        //                    if (parent.Subordinates == null)
        //                        parent.Subordinates = new List<HierarchyEmployee>();

        //                    parent.Subordinates.Add(employee);
        //                }
        //                else
        //                {

        //                    hierarchy.Add(employee);
        //                }
        //            }

        //            return hierarchy;
        //        }

        //        private string GetParentPath(string path)
        //        {
        //            if (string.IsNullOrEmpty(path) || path == "/")
        //                return null;

        //            var parts = path.Trim('/').Split('/');
        //            if (parts.Length <= 1)
        //                return null;
        //            return "/" + string.Join("/", parts.Take(parts.Length - 1));
        //        }

        //       private string GetParentPathLevel2(string path)
        //{
        //    if (string.IsNullOrEmpty(path) || path == "/")
        //        return null;

        //    var parts = path.Trim('/').Split('/');
        //    if (parts.Length <= 1)
        //        return null;

        //    return "/" + string.Join("/", parts.Take(parts.Length - 1));
        //}

        #endregion

        #region AttendenceList
        public Result AddUpdateAttendenceList(AttendenceListModel AttendenceListModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@ID", AttendenceListModel.ID));
            sqlParameters.Add(new SqlParameter("@EmployeeID", AttendenceListModel.EmployeeID));
            sqlParameters.Add(new SqlParameter("@Series", AttendenceListModel.Series));
            sqlParameters.Add(new SqlParameter("@AttendenceDate", AttendenceListModel.AttendenceDate));
            sqlParameters.Add(new SqlParameter("@Status", AttendenceListModel.SelectedStatus));
            sqlParameters.Add(new SqlParameter("@ShiftSelection", AttendenceListModel.SelectedShift.ToString()));
            sqlParameters.Add(new SqlParameter("@LateEntry", AttendenceListModel.LateEntry));
            sqlParameters.Add(new SqlParameter("@EarlyExit", AttendenceListModel.EarlyExit));
            sqlParameters.Add(new SqlParameter("@CreatedAt", AttendenceListModel.CreatedAt));
            sqlParameters.Add(new SqlParameter("@UpdatedAt", AttendenceListModel.UpdatedAt));
            //sqlParameters.Add(new SqlParameter("@IsActive", AttendenceListModel.IsActive));
            //sqlParameters.Add(new SqlParameter("@IsDeleted", AttendenceListModel.IsDeleted));
            //sqlParameters.Add(new SqlParameter("@CreatedBy", AttendenceListModel.CreatedBy));
            //sqlParameters.Add(new SqlParameter("@UpdatedBy", AttendenceListModel.UpdatedBy));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_AttendenceList, sqlParameters);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }

        public List<SelectListItem> GetAllEmployeesList(WeekOfEmployeeId Employeemodel)
        {
            List<SelectListItem> model = new List<SelectListItem>();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", Employeemodel.EmployeeID));
            sqlParameter.Add(new SqlParameter("@ManagerID", Employeemodel.ManagerID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Employees, sqlParameter);
            model = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new SelectListItem
                           {
                               Text = dataRow.Field<string>("EmployeeName"),
                               Value = dataRow.Field<string>("EmployeeID").ToString()
                           }).ToList();

            return model;
        }

        public Results GetAllAttendenceList(AttendenceListInputParans model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@ID", model.ID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_AttendanceList, sqlParameters);
            result.AttendenceList = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new AttendenceListModel
                              {
                                  ID = dataRow.Field<long>("ID"),
                                  Series = dataRow.Field<string>("Series"),
                                  AttendenceDate = dataRow.Field<DateTime>("AttendenceDate"),
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  Status = dataRow.Field<string>("Status"),
                                  ShiftSelection = dataRow.Field<int>("ShiftSelection"),
                                  LateEntry = dataRow.Field<bool>("LateEntry"),
                                  EarlyExit = dataRow.Field<bool>("EarlyExit"),
                                  IsActive = dataRow.Field<bool>("IsActive"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                  CreatedAt = dataRow.Field<DateTime>("CreatedAt"),
                                  UpdatedAt = dataRow.Field<DateTime>("UpdatedAt"),
                                  CreatedBy = dataRow.Field<int>("CreatedBy"),
                                  UpdatedBy = dataRow.Field<int>("UpdatedBy"),
                                  EmployeeName = dataRow.Field<string>("EmployeeName"),

                              }).ToList();

            if (model.ID > 0)
            {
                var Attendence = result.AttendenceList.FirstOrDefault();
                if (Attendence != null)
                {
                    // Set the selected status and shift values for edit case
                    Attendence.SelectedStatus = Attendence.Status;

                    Attendence.SelectedShift = Attendence.ShiftSelection.ToString();
                }
                result.AttendenceListModel = Attendence;
            }

            return result;
        }
        #endregion

        #region Shift Types
        public Results GetAllShiftTypes(ShiftTypeInputParans model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameters.Add(new SqlParameter("@ShiftTypeID", model.ShiftTypeID));
            sqlParameters.Add(new SqlParameter("@SortCol", model.SortCol));
            sqlParameters.Add(new SqlParameter("@SortDir", model.SortDir));
            sqlParameters.Add(new SqlParameter("@Searching", string.IsNullOrEmpty(model.Searching) ? DBNull.Value : (object)model.Searching));
            sqlParameters.Add(new SqlParameter("@DisplayStart", model.DisplayStart));
            sqlParameters.Add(new SqlParameter("@DisplayLength", model.DisplayLength));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_ShiftTypeDetails, sqlParameters);
            result.ShiftType = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new ShiftTypeModel
                              {
                                  ShiftTypeID = dataRow.Field<long>("ShiftTypeID"),
                                  ShiftTypeName = dataRow.Field<string>("ShiftTypeName"),
                                  StartTime = dataRow.Field<TimeSpan>("StartTime"),
                                  EndTime = dataRow.Field<TimeSpan>("EndTime"),
                                  AutoAttendance = dataRow.Field<bool>("AutoAttendance"),
                                  IsCheckInAndOut = dataRow.Field<bool>("IsCheckInAndOut"),
                                  HalfDayWorkingHours = dataRow.Field<long>("HalfDayWorkingHours"),
                                  HolidayID = dataRow.Field<long>("HolidayID"),
                                  WorkingHoursCalculation = dataRow.Field<string>("WorkingHoursCalculation"),
                                  AbsentDayWorkingHours = dataRow.Field<long>("AbsentDayWorkingHours"),
                                  CheckInBeforeShift = dataRow.Field<bool>("CheckInBeforeShift"),
                                  CheckOutAfterShift = dataRow.Field<bool>("CheckOutAfterShift"),
                                  ProcessAttendanceAfter = dataRow.Field<long>("ProcessAttendanceAfter"),
                                  LastSyncDateTime = dataRow.Field<DateTime>("LastSyncDateTime"),
                                  AutoAttendanceOnHoliday = dataRow.Field<bool>("AutoAttendanceOnHoliday"),
                                  LastEntryGracePeriod = dataRow.Field<long>("LastEntryGracePeriod"),
                                  EarlyExitGracePeriod = dataRow.Field<long>("EarlyExitGracePeriod"),
                                  Comments = dataRow.Field<string>("Comments"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  TotalRecords = dataRow.Field<int>("TotalRecords"),
                                  FilteredRecords = dataRow.Field<int>("TotalRecords"),
                              }).ToList();

            if (model.ShiftTypeID > 0)
            {
                result.shiftTypeModel = result.ShiftType.FirstOrDefault();
            }

            return result;
        }

        public Result AddUpdateShiftType(ShiftTypeModel shiftTypeModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@ShiftTypeID", shiftTypeModel.ShiftTypeID));
            sqlParameters.Add(new SqlParameter("@CompanyID", shiftTypeModel.CompanyID));
            sqlParameters.Add(new SqlParameter("@ShiftTypeName", shiftTypeModel.ShiftTypeName));
            sqlParameters.Add(new SqlParameter("@StartTime", shiftTypeModel.StartTime));
            sqlParameters.Add(new SqlParameter("@EndTime", shiftTypeModel.EndTime));
            sqlParameters.Add(new SqlParameter("@AutoAttendance", shiftTypeModel.AutoAttendance));
            sqlParameters.Add(new SqlParameter("@IsCheckInAndOut", shiftTypeModel.IsCheckInAndOut));
            sqlParameters.Add(new SqlParameter("@HalfDayWorkingHours", shiftTypeModel.HalfDayWorkingHours));
            sqlParameters.Add(new SqlParameter("@HolidayID", shiftTypeModel.HolidayID));
            sqlParameters.Add(new SqlParameter("@WorkingHoursCalculation", shiftTypeModel.WorkingHoursCalculation));
            sqlParameters.Add(new SqlParameter("@AbsentDayWorkingHours", shiftTypeModel.AbsentDayWorkingHours));
            sqlParameters.Add(new SqlParameter("@CheckInBeforeShift", shiftTypeModel.CheckInBeforeShift));
            sqlParameters.Add(new SqlParameter("@CheckOutAfterShift", shiftTypeModel.CheckOutAfterShift));
            sqlParameters.Add(new SqlParameter("@ProcessAttendanceAfter", shiftTypeModel.ProcessAttendanceAfter));
            sqlParameters.Add(new SqlParameter("@LastSyncDateTime", shiftTypeModel.LastSyncDateTime));
            sqlParameters.Add(new SqlParameter("@AutoAttendanceOnHoliday", shiftTypeModel.AutoAttendanceOnHoliday));
            sqlParameters.Add(new SqlParameter("@LastEntryGracePeriod", shiftTypeModel.LastEntryGracePeriod));
            sqlParameters.Add(new SqlParameter("@EarlyExitGracePeriod", shiftTypeModel.EarlyExitGracePeriod));
            sqlParameters.Add(new SqlParameter("@Comments", shiftTypeModel.Comments));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_ShiftType, sqlParameters);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }
        #endregion

        #region XML Serialization
        public string ConvertObjectToXML(object obj)
        {
            // Remove Declaration  
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            // Remove Namespace  
            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(writer, obj, ns);
                return stream.ToString();
            }
        }


        public MyInfoResults GetMyAgentInfo(MyInfoInputParams model)
        {
            MyInfoResults myInfoResults = new MyInfoResults();
            HolidayInputParams holidayobj = new HolidayInputParams();
            myInfoResults.leaveResults = GetAgentLeaveSummary(model);
            EmployeeInputParams employeeInputParams = new EmployeeInputParams();
            employeeInputParams.EmployeeID = model.EmployeeID;
            employeeInputParams.CompanyID = model.CompanyID;
            holidayobj.CompanyID = model.CompanyID;
            holidayobj.EmployeeID = model.EmployeeID;
            var data = GetAllEmployees(employeeInputParams);
            myInfoResults.employeeModel = data.employeeModel;
            var holiday = GetAllHolidayList(holidayobj);
            myInfoResults.HolidayModel = holiday.Holiday;
            myInfoResults.employmentHistory = data.employeeModel.EmploymentHistory;
            LeavePolicyModel models = new LeavePolicyModel();
            models.CompanyID = model.CompanyID;
            models.LeavePolicyID = data.employeeModel.LeavePolicyID ?? 0;
            myInfoResults.LeavePolicyDetails = GetSelectLeavePolicies(models);
            myInfoResults.employeeBankDetail = GetEmploymentBankDetails(new EmploymentBankDetailInputParams
            {
                EmployeeID = model.EmployeeID,
                UserID = model.UserID
            });
            myInfoResults.employeeSeparationDetail = GetEmploymentSeparationDetails(new EmploymentSeparationInputParams
            {
                EmployeeID = model.EmployeeID,
                UserID = model.UserID
            });
            myInfoResults.employmentDetail = GetEmploymentDetailsByEmployee(new EmploymentDetailInputParams() { EmployeeID = model.EmployeeID, CompanyID = model.CompanyID });
            myInfoResults.CampOffLeaveCount = GetCampOffLeaveCount(model.EmployeeID, model.JobLocationTypeID);
            myInfoResults.leaveResults.leaveTypes = GetLeaveTypes(model).leaveTypes;
            return myInfoResults;
        }
        public MyInfoResults GetMyInfo(MyInfoInputParams model)
        {
            MyInfoResults myInfoResults = new MyInfoResults();
            HolidayInputParams holidayobj = new HolidayInputParams();
            myInfoResults.leaveResults = GetlLeavesSummary(model);
            EmployeeInputParams employeeInputParams = new EmployeeInputParams();
            employeeInputParams.EmployeeID = model.EmployeeID;
            employeeInputParams.CompanyID = model.CompanyID;
            holidayobj.CompanyID = model.CompanyID;
            holidayobj.EmployeeID = model.EmployeeID;
            var data = GetAllEmployees(employeeInputParams);
            myInfoResults.employeeModel = data.employeeModel;
            var holiday = GetAllHolidayList(holidayobj);
            myInfoResults.HolidayModel = holiday.Holiday;
            myInfoResults.employmentHistory = data.employeeModel.EmploymentHistory;
            LeavePolicyModel models = new LeavePolicyModel();
            models.CompanyID = model.CompanyID;
            models.LeavePolicyID = data.employeeModel.LeavePolicyID ?? 0;
            myInfoResults.LeavePolicyDetails = GetSelectLeavePolicies(models);
            myInfoResults.employeeBankDetail = GetEmploymentBankDetails(new EmploymentBankDetailInputParams
            {
                EmployeeID = model.EmployeeID,
                UserID = model.UserID
            });
            myInfoResults.employeeSeparationDetail = GetEmploymentSeparationDetails(new EmploymentSeparationInputParams
            {
                EmployeeID = model.EmployeeID,
                UserID = model.UserID
            });
            myInfoResults.employmentDetail = GetEmploymentDetailsByEmployee(new EmploymentDetailInputParams() { EmployeeID = model.EmployeeID, CompanyID = model.CompanyID });
            myInfoResults.CampOffLeaveCount = GetCampOffLeaveCount(model.EmployeeID, model.JobLocationTypeID);
            myInfoResults.leaveResults.leaveTypes = GetLeaveTypes(model).leaveTypes;
            return myInfoResults;
        }

        #endregion

        #region LeavePolicyDetails



        public Result AddUpdateLeavePolicyDetails(LeavePolicyDetailsModel LeavePolicyModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@Id", LeavePolicyModel.Id));
            sqlParameter.Add(new SqlParameter("@Title", LeavePolicyModel.Title));
            sqlParameter.Add(new SqlParameter("@Description", LeavePolicyModel.Description));
            sqlParameter.Add(new SqlParameter("@PolicyCategoryId", LeavePolicyModel.PolicyCategoryId));
            sqlParameter.Add(new SqlParameter("@PolicyDocument", LeavePolicyModel.PolicyDocument));
            sqlParameter.Add(new SqlParameter("@CompanyID", LeavePolicyModel.CompanyID));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_LeavePolicyDetails, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            PKNo = dataRow.Field<long>("PKNo")
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }

        public Results GetAllLeavePolicyDetails(LeavePolicyDetailsInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter =
            [
                new SqlParameter("@CompanyID", model.CompanyID),
                new SqlParameter("@Id", model.Id),
            ];

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavePrivacyPolicyDetails, sqlParameter);
            result.LeavePolicyDetailsList = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeavePolicyDetailsModel
                              {
                                  Id = dataRow.Field<long>("Id"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  Title = dataRow.Field<string>("Title"),
                                  Description = dataRow.Field<string>("Description"),
                                  PolicyCategoryId = dataRow.Field<long>("PolicyCategoryId"),
                                  PolicyDocument = dataRow.Field<string>("PolicyDocument"),
                              }).ToList();

            if (model.Id > 0)
            {
                result.LeavePolicyDetailsModel = result.LeavePolicyDetailsList.FirstOrDefault();
            }

            return result;
        }

        public Results GetLeavePolicyList(LeavePolicyDetailsInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavePolicyDetailsList, sqlParameter);

            result.LeavePolicyDetailsList = dataSet.Tables[0].AsEnumerable()
                          .Select(dataRow => new LeavePolicyDetailsModel
                          {
                              Id = dataRow.Field<long>("Id"),
                              Title = dataRow.Field<string>("Title"),
                          }).ToList();
            return result;
        }


        public string DeleteLeavePolicyDetails(LeavePolicyDetailsInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@Id", model.Id));
            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };
            sqlParameter.Add(outputMessage);
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_LeavePolicyDetails, sqlParameter);
            string message = outputMessage.Value.ToString();
            return message;
        }



        public Results GetAllLeavePolicyDetailsByCompanyId(LeavePolicyDetailsInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter =
            [
                new SqlParameter("@CompanyID", model.CompanyID),
                new SqlParameter("@Id", model.Id),
            ];

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavePrivacyPolicyDetails, sqlParameter);
            result.LeavePolicyDetailsModel = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeavePolicyDetailsModel
                              {
                                  Id = dataRow.Field<long>("Id"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  Title = dataRow.Field<string>("Title"),
                                  Description = dataRow.Field<string>("Description"),
                              }).ToList().FirstOrDefault();



            return result;
        }


        public EmployeeDashboardResponse GetEmployeeListByManagerID(EmployeeInputParams model)
        {
            var response = new EmployeeDashboardResponse();

            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>
        {
            new SqlParameter("@ReportingUserID", model.EmployeeID),
            new SqlParameter("@RoleID", model.RoleID),
             new SqlParameter("@LocationID", model.LocationID),
            new SqlParameter("@SubDepartmentID", model.SubDepartmentID),
            new SqlParameter("@EmployeeTypeID", model.EmployeeTypeID),
            new SqlParameter("@ManagerID", model.ManagerID)
        };

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetEmployeeListByManagerIDs, sqlParameter);

                // Table 0: Employee list
                if (dataSet.Tables.Count > 0)
                {
                    response.Employees = dataSet.Tables[0].AsEnumerable()
                        .Select(row => new EmployeeDetails
                        {
                            EmployeeId = row.Field<long>("EmployeeId"),
                            EmployeeNumber = row.Field<string>("EmployeNumber"),
                            ManagerName = row.Field<string>("ManagerName"),
                            FirstName = row.Field<string>("EmployeeFirstName"),
                            MiddelName = row.Field<string>("EmployeeMiddelName"),
                            LastName = row.Field<string>("EmployeeLastName"),
                            EmployeePhoto = row.Field<string>("EmployeePhoto"),
                            DepartmentName = row.Field<string>("DepartmentName"),
                            DesignationName = row.Field<string>("DesignationName")
                        }).ToList();
                }

                // Table 1: SubDepartments
                if (dataSet.Tables.Count > 1)
                {
                    response.SubDepartmentList = dataSet.Tables[1].AsEnumerable()
                        .Select(row => new SubDepartmentList
                        {
                            SubDepartmentID = row.Field<long>("SubDepartmentID"),
                            Name = row.Field<string>("Name")
                        }).ToList();
                }

                // Table 2: Locations
                if (dataSet.Tables.Count > 2)
                {
                    response.LocationList = dataSet.Tables[2].AsEnumerable()
                        .Select(row => new LocationList
                        {
                            JobLocationID = row.Field<long>("JobLocationID"),
                            Name = row.Field<string>("Name")
                        }).ToList();
                }

                // Table 3: Employment Types
                if (dataSet.Tables.Count > 3)
                {
                    response.EmploymentTypesList = dataSet.Tables[3].AsEnumerable()
                        .Select(row => new EmploymentTypesList
                        {
                            EmployeeTypeId = row.Field<long>("EmployeeTypeID"),
                            Name = row.Field<string>("Name")
                        }).ToList();
                }

                // Table 4: Managers List
                if (dataSet.Tables.Count > 4)
                {
                    response.ManagerList = dataSet.Tables[4].AsEnumerable()
                        .Select(row => new ManagersList
                        {
                            EmployeeID = row.Field<long>("EmployeeID"),
                            FullName = row.Field<string>("FullName")
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log exception
            }

            return response;
        }



        #endregion

        #region Policy Category



        public Result AddUpdatePolicyCategory(PolicyCategoryModel LeavePolicyModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@Id", LeavePolicyModel.Id));
            sqlParameter.Add(new SqlParameter("@Name", LeavePolicyModel.Name));
            sqlParameter.Add(new SqlParameter("@CompanyID", LeavePolicyModel.CompanyID));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_PolicyCategory, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }

        public Results GetAllPolicyCategory(PolicyCategoryInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter =
            [
                new SqlParameter("@CompanyID", model.CompanyID),
                new SqlParameter("@Id", model.Id),
            ];

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_PolicyCategory, sqlParameter);
            result.PolicyCategoryList = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new PolicyCategoryModel
                              {
                                  Id = dataRow.Field<long>("Id"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  Name = dataRow.Field<string>("Name"),
                              }).ToList();

            if (model.Id > 0)
            {
                result.PolicyCategoryModel = result.PolicyCategoryList.FirstOrDefault();
            }

            return result;
        }

        public Results GetPolicyCategoryList(PolicyCategoryInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_PolicyCategoryList, sqlParameter);

            result.PolicyCategoryList = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new PolicyCategoryModel
                           {
                               Id = dataRow.Field<long>("Id"),
                               CompanyID = dataRow.Field<long>("CompanyID"),
                               Name = dataRow.Field<string>("Name"),
                           }).ToList();
            return result;
        }


        public string DeletePolicyCategory(PolicyCategoryInputParams model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@Id", model.Id));
            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };
            sqlParameter.Add(outputMessage);
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_PolicyCategory, sqlParameter);
            string message = outputMessage.Value.ToString();
            return message;
        }


        public List<LeavePolicyDetailsModel> PolicyCategoryDetails(PolicyCategoryInputParams model)
        {
            List<LeavePolicyDetailsModel> result = new List<LeavePolicyDetailsModel>();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_DistinctPolicyCategoryDetails, sqlParameter);

            result = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new LeavePolicyDetailsModel
                           {
                               Id = dataRow.Field<long>("Id"),
                               CompanyID = dataRow.Field<long>("CompanyID"),
                               Title = dataRow.Field<string>("Title"),
                               PolicyCategoryName = dataRow.Field<string>("PolicyCategoryName"),
                               PolicyDocument = dataRow.Field<string>("PolicyDocument"),
                               Description = dataRow.Field<string>("Description"),
                               PolicyCategoryId = dataRow.Field<long>("PolicyCategoryId"),
                               IsAcknowledged = dataRow.Field<bool>("IsAcknowledged"),
                           }).ToList();
            return result;
        }

        public Result AddAcknowledgePolicy(AcknowledgePolicyModel pAck)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeId", pAck.EmployeeId));
            sqlParameter.Add(new SqlParameter("@PolicyId", pAck.Id));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Add_AcknowledgePolicy, sqlParameter);
            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var dataRow = dataSet.Tables[0].Rows[0];

                model = new Result
                {
                    PKNo = dataRow["PKNo"] != DBNull.Value ? Convert.ToInt64(dataRow["PKNo"]) : (long?)null,
                    UserID = dataRow["UserID"] != DBNull.Value ? Convert.ToInt64(dataRow["UserID"]) : (long?)null,
                    Message = dataRow["Message"]?.ToString(),
                    ErrorCode = dataRow["ErrorCode"]?.ToString(),
                    IsResetPasswordRequired = dataRow["IsResetPasswordRequired"] != DBNull.Value && Convert.ToBoolean(dataRow["IsResetPasswordRequired"]),
                    Data = dataRow["Data"] != DBNull.Value ? dataRow["Data"] : null
                };
            }
            return model;
        }
        #endregion


        #region Attandance Module

        public AttendanceLogResponse GetAttendanceDeviceLogs(AttendanceDeviceLog model)
        {
            var attendanceLogs = new List<AttendanceDeviceLog>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeId", model.EmployeeId ),
        new SqlParameter("@CreatedBy", model.CreatedBy )
    };
            var dataSet = DataLayer.GetDataSetByStoredProcedure("usp_GetAttendanceAuditLog", sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                attendanceLogs = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new AttendanceDeviceLog
                    {
                        ID = dataRow.Field<long>("ID"),
                        EmployeeId = dataRow.Field<string>("EmployeeId"),
                        EmployeeName = dataRow.Field<string>("EmployeeName"),
                        Description = dataRow.Field<string>("Description"),
                        CreatedBy = dataRow.Field<string>("CreatedBy"),
                        CreatedDate = dataRow.Field<DateTime>("CreatedDate"),
                        AttendanceStatus = dataRow.Field<string>("AttendanceStatus")
                    }).ToList();
            }

            return new AttendanceLogResponse { AttendanceLogs = attendanceLogs };
        }

        private static readonly object _logLock = new object();

        private void Logdata(string eventType, string message)
        {
            try
            {
                string projectRoot = _configuration["LogSettings:LogDirectory"];
                string logDirectory = Path.Combine(projectRoot, "Logs");
                string logFilePath = Path.Combine(logDirectory, "Logdata.txt");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                lock (_logLock)
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                        writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{eventType}] {message}");
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        string source = "HRMSAppLogger";
                        if (!EventLog.SourceExists(source))
                        {
                            EventLog.CreateEventSource(source, "Application");
                        }
                        EventLog.WriteEntry(source, $"Log write failed: {ex.Message}", EventLogEntryType.Error);
                    }
                }
                catch
                {
                    // Silent fail
                }
            }
        }



        public AttendanceInputParams GetAttendance(AttendanceInputParams model)
        {
            string connectionString = _configuration["ConnectionStrings:conStr"];
            Logdata("Attendance fetching started: {Date}", $"{model.Year}{model.Month}{model.Day}");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))

                using (SqlCommand cmd = new SqlCommand("usp_CalculateMonthlyAttendance_WithShifts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Set parameters
                    cmd.Parameters.AddWithValue("@Year", model.Year);
                    cmd.Parameters.AddWithValue("@Month", model.Month);
                    cmd.Parameters.AddWithValue("@Day", model.Day);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    cmd.Parameters.AddWithValue("@IsManual", false);
                    cmd.Parameters.AddWithValue("@AttendanceStatus", AttendanceStatus.Approved.ToString());

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            model.Message = "Attendance calculated successfully.";
                            model.IsSuccess = true;
                            Logdata("Attendance fetching completed: {Date}", $"{model.Year}{model.Month}{model.Day}");

                        }
                        else
                        {
                            model.Message = "No attendance data returned.";
                            model.IsSuccess = false;
                            Logdata("No attendance data: {Date}", $"{model.Year}{model.Month}{model.Day}");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string date = $"{model.Year}{model.Month:D2}{model.Day:D2}";
                string errorMessage = $"Error during attendance fetching for date {date}: {ex.Message}";
                Logdata("ERROR", errorMessage);
                model.Message = "Error: " + ex.Message;
                model.IsSuccess = false;
            }


            return model;
        }



        public MonthlyViewAttendance GetAttendanceForMonthlyViewCalendar([FromForm] AttendanceInputParams model)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@Year", model.Year),
        new SqlParameter("@Month", model.Month),
        new SqlParameter("@UserId", model.UserId)
    };
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetAttendanceDeviceLog, sqlParameters);
            var dailyStatuses = new List<DailyAttendanceStatus>();
            if (dataSet.Tables.Count > 0)
            {
                var table = dataSet.Tables[0];

                dailyStatuses = table.AsEnumerable()
                    .Select(row => new DailyAttendanceStatus
                    {
                        DayLabel = row.Field<string>(0),
                        Date = row.Field<DateTime>(1),
                        FirstLogDate = row.Field<DateTime?>(2),
                        LastLogDate = row.Field<DateTime?>(3),
                        Status = row.IsNull(4) ? null : row.Field<string>(4)
                    }).ToList();
            }

            return new MonthlyViewAttendance
            {
                DailyStatuses = dailyStatuses
            };
        }


        public AttendanceWithHolidaysVM GetTeamAttendanceForCalendar(AttendanceInputParams model)
        {
            List<AttendanceViewModel> attendanceList = new List<AttendanceViewModel>();
            int totalRecords = 0;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@Year", model.Year),
        new SqlParameter("@Month", model.Month),
        new SqlParameter("@UserId", model.UserId),
        new SqlParameter("@RoleId", model.RoleId),
        new SqlParameter("@PageNumber", model.Page),
        new SqlParameter("@PageSize", model.PageSize),
        new SqlParameter("@SearchTerm", model.SearchTerm),
        new SqlParameter("@JobLocationID", model.JobLocationID),
        new SqlParameter("@ManagerFilterID", model.ManagerID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetTeamAttendanceDeviceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                // First result set: Attendance data
                attendanceList = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow =>
                    {
                        var attendance = new AttendanceViewModel
                        {
                            EmployeeId = dataRow.Field<long?>("EmployeeID"),
                            EmployeNumber = dataRow.Field<string>("EmployeNumber"),
                            JobLocationName = dataRow.Field<string>("JobLocationName"),
                            EmployeeNumberWithoutAbbr = dataRow.Field<string>("EmployeeNumberWithoutAbbr"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            ManagerName = dataRow.Field<string>("ManagerName"),
                            ManagerManagerName = dataRow.Field<string>("ManagerManagerName"),
                            TotalWorkingDays = dataRow.Field<int>("TotalWorkingDays"),
                            PresentDays = dataRow.Field<decimal>("PresentDays"),
                            TotalLeaves = dataRow.Field<decimal>("TotalLeaves"),
                            AttendanceByDay = new Dictionary<string, string>()
                        };

                        foreach (DataColumn column in dataSet.Tables[0].Columns)
                        {
                            string columnName = column.ColumnName;
                            if (columnName.Contains("_"))
                            {
                                attendance.AttendanceByDay[columnName] = dataRow[columnName]?.ToString() ?? "N/A";
                            }
                        }

                        return attendance;
                    }).ToList();
            }

            // Second result set: TotalRecords
            if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
            {
                int.TryParse(dataSet.Tables[1].Rows[0]["TotalRecords"].ToString(), out totalRecords);
            }

            return new AttendanceWithHolidaysVM
            {
                Attendances = attendanceList,
                TotalRecords = totalRecords
            };
        }


        public AttendanceWithHolidaysVM GetExportAttendanceForCalendar(AttendanceInputParams model)
        {
            List<AttendanceViewModel> attendanceList = new List<AttendanceViewModel>();
            int totalRecords = 0;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@FromDate", model.FromDate),
        new SqlParameter("@ToDate", model.ToDate),
        new SqlParameter("@UserId", model.UserId),
        new SqlParameter("@RoleId", model.RoleId),
        new SqlParameter("@PageNumber", model.Page),
        new SqlParameter("@PageSize", model.PageSize),
        new SqlParameter("@SearchTerm", model.SearchTerm),
        new SqlParameter("@JobLocationID", model.JobLocationID),
        new SqlParameter("@ManagerFilterID", model.ManagerID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_ExportAttendanceDeviceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                // First result set: Attendance data
                attendanceList = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow =>
                    {
                        var attendance = new AttendanceViewModel
                        {
                            EmployeeId = dataRow.Field<long?>("EmployeeID"),
                            EmployeNumber = dataRow.Field<string>("EmployeNumber"),
                            JobLocationName = dataRow.Field<string>("JobLocationName"),
                            EmployeeNumberWithoutAbbr = dataRow.Field<string>("EmployeeNumberWithoutAbbr"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            TotalWorkingDays = dataRow.Field<int>("TotalWorkingDays"),
                            PresentDays = dataRow.Field<decimal>("PresentDays"),
                            TotalLeaves = dataRow.Field<decimal>("TotalLeaves"),
                            ManagerName = dataRow.Field<string>("ManagerName"),
                            ManagerManagerName = dataRow.Field<string>("ManagerManagerName"),
                            AttendanceByDay = new Dictionary<string, string>()
                        };

                        foreach (DataColumn column in dataSet.Tables[0].Columns)
                        {
                            string columnName = column.ColumnName;
                            if (columnName.Contains("-"))
                            {
                                attendance.AttendanceByDay[columnName] = dataRow[columnName]?.ToString() ?? "N/A";
                            }
                        }

                        return attendance;
                    }).ToList();
            }

            // Second result set: TotalRecords
            if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
            {
                int.TryParse(dataSet.Tables[1].Rows[0]["TotalRecords"].ToString(), out totalRecords);
            }

            return new AttendanceWithHolidaysVM
            {
                Attendances = attendanceList,
                TotalRecords = totalRecords
            };
        }
        public AttendanceDetailsVM FetchAttendanceHolidayAndLeaveInfo(AttendanceDetailsInputParams model)
        {
            AttendanceDetailsVM result = new AttendanceDetailsVM();
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@SelectedDate", model.SelectedDate),
        new SqlParameter("@EmployeeNumber", model.EmployeeNumber),
        new SqlParameter("@EmployeeId",model.EmployeeId),
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetDailyAttendanceDetails, sqlParameters);

            if (dataSet == null || dataSet.Tables.Count == 0)
                return result;


            if (dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[0];
                var attendanceStatus = new AttendanceDetailsVM
                {
                    RecordType = row.Field<string>("RecordType")
                };

                switch (attendanceStatus.RecordType)
                {
                    case "Attendance":
                        attendanceStatus.UserId = row.Field<string>("UserId");
                        attendanceStatus.WorkDate = row.Field<DateTime?>("WorkDate");
                        attendanceStatus.FirstLogDate = row.Field<DateTime?>("FirstLogDate");
                        attendanceStatus.LastLogDate = row.Field<DateTime?>("LastLogDate");
                        attendanceStatus.HoursWorked = row.Field<TimeSpan?>("HoursWorked");
                        attendanceStatus.AttendanceStatus = row.Field<string>("AttendanceStatus");
                        attendanceStatus.DialerTime = row.Field<string>("DialerTime");
                        attendanceStatus.Remarks = row.Field<string>("Remarks");

                        break;
                    case "WeekOff":

                        attendanceStatus.FromDate = row.Field<DateTime?>("MatchingDayOff");
                        attendanceStatus.ToDate = row.Field<DateTime?>("WeekStartDate");
                        break;
                    case "Holiday":
                        attendanceStatus.HolidayName = row.Field<string>("HolidayName");
                        attendanceStatus.Description = row.Field<string>("Description");
                        attendanceStatus.FromDate = row.Field<DateTime?>("FromDate");
                        attendanceStatus.ToDate = row.Field<DateTime?>("ToDate");
                        break;

                    case "Leave":
                        attendanceStatus.EmployeeID = row.Field<long?>("EmployeeID");
                        attendanceStatus.StartDate = row.Field<DateTime?>("StartDate");
                        attendanceStatus.EndDate = row.Field<DateTime?>("EndDate");
                        attendanceStatus.Reason = row.Field<string>("Reason");
                        attendanceStatus.LeaveStatusID = row.Field<string?>("LeaveStatusID");
                        break;

                    case "No Record Found":
                    default:

                        break;
                }

                result = attendanceStatus;
            }


            if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
            {
                result.StatusChanges = new List<StatusChangeVM>();

                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    var sc = new StatusChangeVM
                    {
                        RecordType = row.Field<string>("RecordType"),
                        EmployeeID = row.Field<long?>("EmployeeID"),
                        EmployeeNumber = row.Field<string>("EmployeeNumber"),
                        WorkDate = row.Field<DateTime?>("WorkDate"),
                        AttendanceStatus = row.Field<string>("AttendanceStatus"),
                        Remarks = row.Field<string>("Remarks"),
                        StatusState = row.Field<string>("StatusState"),
                        InsertedDate = row.Field<DateTime?>("InsertedDate"),
                        InsertedByUserName = row.Field<string>("InsertedByUserName"),
                        ModifiedDate = row.Field<DateTime?>("ModifiedDate"),
                        UpdatedByUserName = row.Field<string>("UpdatedByUserName"),
                        ApprovedByUserName = row.Field<string>("ApprovedByUserName")
                    };

                    result.StatusChanges.Add(sc);
                }
            }


            return result;
        }







        public MyAttendanceList GetMyAttendanceList(AttendanceInputParams model)
        {
            List<Attendance> attendanceList = new List<Attendance>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>  {
            new SqlParameter("@Year", model.Year),
            new SqlParameter("@Month", model.Month),
            new SqlParameter("@UserId", model.UserId)
            };
            // Get the dataset from the stored procedure
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetMyAttendanceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Attendance
                {
                    ID = dataRow.Field<long?>("ID") ?? 0,
                    UserId = dataRow.Field<long?>("EmployeeId"),
                    AttendanceStatus = dataRow.Field<string>("AttendanceStatus"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    Comments = dataRow.Field<string>("Comments"),
                    WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date,
                    FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"),
                    LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"),
                    HoursWorked = dataRow.IsNull("HoursWorked") ? 0 : dataRow.Field<int>("HoursWorked"),
                    AttendanceStatusId = dataRow.Field<int?>("AttendanceStatusId")
                })
                .ToList();
            }
            var result = new MyAttendanceList
            {
                Attendances = attendanceList,
            };
            return result;
        }

        public Result AddUpdateAttendace(Attendance att)
        {
            Result model = new Result();

            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>();

                sqlParameter.Add(new SqlParameter("@Id", att.ID));
                sqlParameter.Add(new SqlParameter("@EmployeeId", att.UserId));
                sqlParameter.Add(new SqlParameter("@AttendanceStatusId", att.AttendanceStatusId));
                sqlParameter.Add(new SqlParameter("@WorkDate", att.WorkDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@FirstLogDate", att.FirstLogDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@LastLogDate", att.LastLogDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@Comments", att.Comments));
                sqlParameter.Add(new SqlParameter("@ModifiedBy", att.ModifiedBy));
                sqlParameter.Add(new SqlParameter("@ModifiedDate", att.ModifiedDate));
                sqlParameter.Add(new SqlParameter("@CreatedDate", att.CreatedDate));
                sqlParameter.Add(new SqlParameter("@CreatedBy", att.CreatedBy));
                sqlParameter.Add(new SqlParameter("@IsDeleted", att.IsDeleted));
                sqlParameter.Add(new SqlParameter("@RoleId", att.RoleId));
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_SaveAttendanceManualLog, sqlParameter);
                if (dataSet.Tables[0].Columns.Contains("Result"))
                {
                    model = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow =>
                            new Result()
                            {
                                Message = dataRow.Field<string>("Result").ToString()
                            }
                        ).ToList().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

            }
            return model;
        }

        public Results GetAttendenceListID(Attendance model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@ID", model.ID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.GetAttendanceDeviceLogById, sqlParameters);
            var attendence = dataSet.Tables[0].AsEnumerable()
                                 .Select(dataRow => new Attendance
                                 {
                                     ID = dataRow.Field<long>("ID"),
                                     UserId = dataRow.Field<long?>("EmployeeId"),
                                     AttendanceStatusId = dataRow.Field<int?>("AttendanceStatusId"),
                                     WorkDate = dataRow.Field<DateTime>("WorkDate"),
                                     FirstLogDate = dataRow.Field<DateTime>("FirstLogDate"),
                                     LastLogDate = dataRow.Field<DateTime>("LastLogDate"),
                                     Comments = dataRow.Field<string>("Comments"),
                                     HoursWorked = dataRow.Field<int>("HoursWorked"),
                                 }).FirstOrDefault();
            result.AttendanceModel = attendence;

            return result;
        }

        public string DeleteAttendanceDetails(Attendance model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@Id", model.ID));
            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };
            sqlParameter.Add(outputMessage);
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_DeleteAttendanceDeviceLog, sqlParameter);
            string message = outputMessage.Value.ToString();
            return message;
        }


        public MyAttendanceList GetAttendanceForApproval(AttendanceInputParams model)
        {
            List<Attendance> attendanceList = new List<Attendance>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>  {
            new SqlParameter("@ReportingID", model.UserId)
            };
            // Get the dataset from the stored procedure
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetMyAttendanceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Attendance
                {
                    ID = dataRow.Field<long?>("ID") ?? 0,
                    UserId = dataRow.Field<long?>("UserId"),
                    AttendanceStatus = dataRow.Field<string>("AttendanceStatus"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date,
                    FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"),
                    LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"),
                    HoursWorked = dataRow.IsNull("HoursWorked") ? 0 : dataRow.Field<int>("HoursWorked")
                })
                .ToList();
            }
            var result = new MyAttendanceList
            {
                Attendances = attendanceList,
            };
            return result;
        }


        public List<Attendance> GetApprovedAttendance(AttendanceInputParams model)
        {
            var result = new List<Attendance>();

            int attStatus = Convert.ToInt32(model.AttendanceStatusId);
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
        {
            new SqlParameter("@ReportingUserID", model.UserId),
            new SqlParameter("@Year",model.Year),
            new SqlParameter("@Month",model.Month),
            new SqlParameter("@AttendanceStatusId",attStatus),
            new SqlParameter("@RoleId",model.RoleId)
        };

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_ApprovedAttendance, sqlParameters);

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    result = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow => new Attendance
                        {
                            ID = dataRow.Field<long?>("ID") ?? 0,
                            UserId = dataRow.Field<long?>("EmployeeId") ?? 0,
                            AttendanceStatus = dataRow.Field<string>("AttendanceStatusName"),
                            AttendanceStatusId = dataRow.Field<int>("AttendanceStatusId"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date,
                            FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"),
                            LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"),
                            HoursWorked = dataRow.IsNull("HoursWorked") ? 0 : dataRow.Field<int>("HoursWorked"),
                            Comments = dataRow.Field<string>("Comments"),
                            EmployeeNumber = dataRow.Field<string>("EmployeeNumber"),
                        }).OrderBy(x => x.ID)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetApprovedAttendance: " + ex.Message);
            }

            return result;
        }
        public List<Attendance> GetManagerApprovedAttendance(AttendanceInputParams model)
        {
            var result = new List<Attendance>();

            int attStatus = Convert.ToInt32(model.AttendanceStatusId);
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
        {
            new SqlParameter("@ReportingUserID", model.UserId),
            new SqlParameter("@AttendanceStatusId",attStatus)
        };

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Manager_ApprovedAttendance, sqlParameters);

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    result = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow => new Attendance
                        {
                            ID = dataRow.Field<long?>("ID") ?? 0,
                            UserId = dataRow.Field<long?>("employeeId"),
                            AttendanceStatus = dataRow.Field<string>("AttendanceStatus"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date,
                            FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"),
                            LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"),
                            HoursWorked = dataRow.IsNull("HoursWorked") ? 0 : dataRow.Field<int>("HoursWorked"),
                            Comments = dataRow.Field<string>("Comments"),
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetApprovedAttendance: " + ex.Message);
            }

            return result;
        }


        #endregion Attandance Module

        public Dictionary<string, long> GetCountryDictionary()
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Counteres, sqlParameter);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].AsEnumerable()
                            .ToDictionary(row => row.Field<string>("Name").ToLower(), // Convert Name to lowercase
                                          row => row.Field<long>("CountryID"));
            }

            return new Dictionary<string, long>(); // Return empty dictionary if no data
        }

        public Dictionary<string, CompanyInfo> GetCompaniesDictionary()
        {
            EmployeeInputParams model = new EmployeeInputParams();
            model.CompanyID = 0;
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Companies, sqlParameter);
            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].AsEnumerable()
                            .ToDictionary(row => row.Field<string>("Name").ToLower(),
                row => new CompanyInfo
                {
                    Abbr = row.Field<string?>("Abbr"),
                    CompanyID = row.Field<long?>("CompanyID")
                });
            }
            return new Dictionary<string, CompanyInfo>();
        }
        public Dictionary<string, long> GetSubDepartmentDictionary(EmployeeInputParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CompanySubDepartments, sqlParameter);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].AsEnumerable()
                            .ToDictionary(row => row.Field<string>("DepartmentName_SubDepartment").ToLower(), // Convert Name to lowercase
                                          row => row.Field<long>("ID"));
            }

            return new Dictionary<string, long>(); // Return empty dictionary if no data
        }


        public Dictionary<string, Dictionary<string, long>> GetEmploymentDetailsDictionaries(EmploymentDetailInputParams model)
        {
            Dictionary<string, Dictionary<string, long>> employmentDictionaries = new Dictionary<string, Dictionary<string, long>>();

            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@CompanyID", model.CompanyID),
        new SqlParameter("@EmployeeID", model.EmployeeID),
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_FilterEmployeeDetailsFormDetails, sqlParameter);

            if (dataSet.Tables.Count > 0)
            {
                employmentDictionaries["JobLocations"] = dataSet.Tables[0].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("Name"), row => row.Field<long>("ID"));

                employmentDictionaries["EmploymentTypes"] = dataSet.Tables[1].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("Name"), row => row.Field<long>("ID"));

                employmentDictionaries["PayrollTypes"] = dataSet.Tables[2].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("Name"), row => row.Field<long>("ID"));

                employmentDictionaries["Departments"] = dataSet.Tables[3].AsEnumerable()
         .OrderBy(row => row.Field<long>("ID"))
         .ToDictionary(
             row => row.Field<string>("Name"),
             row => row.Field<long>("ID")
         );



                employmentDictionaries["Designations"] = dataSet.Tables[5].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("Name"), row => row.Field<long>("ID"));

                employmentDictionaries["ShiftTypes"] = dataSet.Tables[6].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("Name"), row => row.Field<long>("ID"));


                employmentDictionaries["Employees"] = dataSet.Tables[7].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("EmployeNumber"), row => row.Field<long>("EmployeeID"));

                employmentDictionaries["LeavePolicies"] = dataSet.Tables[9].AsEnumerable()
                    .ToDictionary(row => row.Field<string>("LeavePolicyName"), row => row.Field<long>("LeavePolicyID"));

                employmentDictionaries["Roles"] = dataSet.Tables[10].AsEnumerable()
     .ToDictionary(
         row => row.Field<string>("UniqueName"),
         row => (long)row.Field<int>("RoleID") // Convert int to long
     );

            }

            return employmentDictionaries;
        }
        private object GetDbValue(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) ? DBNull.Value : value;
        }
        public Result AddUpdateEmployeeFromExecel(ImportEmployeeDetail employeeModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", Convert.ToInt64(employeeModel.CompanyName)));
            sqlParameter.Add(new SqlParameter("@FirstName", GetDbValue(employeeModel.FirstName)));
            sqlParameter.Add(new SqlParameter("@MiddleName", GetDbValue(employeeModel.MiddleName)));
            sqlParameter.Add(new SqlParameter("@Surname", GetDbValue(employeeModel.Surname)));
            sqlParameter.Add(new SqlParameter("@CorrespondenceAddress", GetDbValue(employeeModel.CorrespondenceAddress)));
            sqlParameter.Add(new SqlParameter("@CorrespondenceCity", GetDbValue(employeeModel.CorrespondenceCity)));
            sqlParameter.Add(new SqlParameter("@CorrespondencePinCode", GetDbValue(employeeModel.CorrespondencePinCode)));
            sqlParameter.Add(new SqlParameter("@CorrespondenceState", GetDbValue(employeeModel.CorrespondenceState)));
            sqlParameter.Add(new SqlParameter("@CorrespondenceCountryID", Convert.ToInt64(employeeModel.CorrespondenceCountryName)));
            sqlParameter.Add(new SqlParameter("@EmailAddress", GetDbValue(employeeModel.EmailAddress)));
            sqlParameter.Add(new SqlParameter("@Landline", GetDbValue(employeeModel.Landline)));
            sqlParameter.Add(new SqlParameter("@Mobile", GetDbValue(employeeModel.Mobile)));
            sqlParameter.Add(new SqlParameter("@Telephone", GetDbValue(employeeModel.Telephone)));
            sqlParameter.Add(new SqlParameter("@PersonalEmailAddress", GetDbValue(employeeModel.PersonalEmailAddress)));
            sqlParameter.Add(new SqlParameter("@PermanentAddress", GetDbValue(employeeModel.PermanentAddress)));
            sqlParameter.Add(new SqlParameter("@PermanentCity", GetDbValue(employeeModel.PermanentCity)));
            sqlParameter.Add(new SqlParameter("@PermanentPinCode", GetDbValue(employeeModel.PermanentPinCode)));
            sqlParameter.Add(new SqlParameter("@PermanentState", GetDbValue(employeeModel.PermanentState)));
            sqlParameter.Add(new SqlParameter("@PermanentCountryID", Convert.ToInt64(employeeModel.CorrespondenceCountryName)));
            sqlParameter.Add(new SqlParameter("@PeriodOfStay", GetDbValue(employeeModel.PeriodOfStay)));
            sqlParameter.Add(new SqlParameter("@VerificationContactPersonName", GetDbValue(employeeModel.VerificationContactPersonName)));
            sqlParameter.Add(new SqlParameter("@VerificationContactPersonContactNo", GetDbValue(employeeModel.VerificationContactPersonContactNo)));
            // DateOfBirth can be null, handle using DBNull.Value if it's null or empty
            sqlParameter.Add(new SqlParameter("@DateOfBirth",
                string.IsNullOrEmpty(employeeModel.DateOfBirth?.ToString()) ? DBNull.Value : (object)Convert.ToDateTime(employeeModel.DateOfBirth)));
            // If no profile photo is provided, set to default or DBNull.Value
            sqlParameter.Add(new SqlParameter("@ProfilePhoto", "/assets/img/avatars/m.png"));
            sqlParameter.Add(new SqlParameter("@PlaceOfBirth", GetDbValue(employeeModel.PlaceOfBirth)));
            sqlParameter.Add(new SqlParameter("@IsReferredByExistingEmployee", employeeModel.IsReferredByExistingEmployee));
            sqlParameter.Add(new SqlParameter("@ReferredByEmployeeID", GetDbValue(employeeModel.ReferredByEmployeeName)));
            sqlParameter.Add(new SqlParameter("@BloodGroup", GetDbValue(employeeModel.BloodGroup)));
            sqlParameter.Add(new SqlParameter("@PANNo", GetDbValue(employeeModel.PANNo)));
            sqlParameter.Add(new SqlParameter("@AadharCardNo", GetDbValue(employeeModel.AadharCardNo)));
            sqlParameter.Add(new SqlParameter("@Allergies", GetDbValue(employeeModel.Allergies)));
            sqlParameter.Add(new SqlParameter("@IsRelativesWorkingWithCompany", employeeModel.IsRelativesWorkingWithCompany));
            sqlParameter.Add(new SqlParameter("@RelativesDetails", GetDbValue(employeeModel.RelativesDetails)));
            sqlParameter.Add(new SqlParameter("@MajorIllnessOrDisability", GetDbValue(employeeModel.MajorIllnessOrDisability)));
            sqlParameter.Add(new SqlParameter("@AwardsAchievements", GetDbValue(employeeModel.AwardsAchievements)));
            sqlParameter.Add(new SqlParameter("@EducationGap", GetDbValue(employeeModel.EducationGap)));
            sqlParameter.Add(new SqlParameter("@ExtraCuricuarActivities", GetDbValue(employeeModel.ExtraCuricuarActivities)));
            sqlParameter.Add(new SqlParameter("@ForiegnCountryVisits", GetDbValue(employeeModel.ForiegnCountryVisits)));
            sqlParameter.Add(new SqlParameter("@ContactPersonName", GetDbValue(employeeModel.ContactPersonName)));
            sqlParameter.Add(new SqlParameter("@ContactPersonMobile", GetDbValue(employeeModel.ContactPersonMobile)));
            sqlParameter.Add(new SqlParameter("@ContactPersonTelephone", GetDbValue(employeeModel.ContactPersonTelephone)));
            sqlParameter.Add(new SqlParameter("@ContactPersonRelationship", GetDbValue(employeeModel.ContactPersonRelationship)));
            sqlParameter.Add(new SqlParameter("@ITSkillsKnowledge", GetDbValue(employeeModel.ITSkillsKnowledge)));
            // Handle Gender, EmployeeID, and RetEmployeeID with default null checks
            sqlParameter.Add(new SqlParameter("@Gender", employeeModel.Gender == null ? DBNull.Value : (object)Convert.ToInt32(employeeModel.Gender)));
            sqlParameter.Add(new SqlParameter("@EmployeeID", Convert.ToInt64(employeeModel.EmployeeID)));
            sqlParameter.Add(new SqlParameter("@RetEmployeeID", Convert.ToInt64(employeeModel.EmployeeID)));
            // IsActive default value set as false
            sqlParameter.Add(new SqlParameter("@IsActive", true));
            // Execute the stored procedure
            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Employee, sqlParameter, ref pOutputParams);

            // Handle result
            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow =>
                            new Result()
                            {
                                Message = dataRow.Field<string>("Result").ToString(),
                                UserID = dataRow.Field<long>("UserID"),
                                PKNo = dataRow.Field<long>("UserID")
                            }
                    ).ToList().FirstOrDefault();
            }

            long employmentDetailID = 0;



            List<SqlParameter> sqlParametersBank = new List<SqlParameter>();
            sqlParametersBank.Add(new SqlParameter("@EmployeeID", model.UserID));
            sqlParametersBank.Add(new SqlParameter("@BankAccountNumber", employeeModel.BankAccountNumber));
            sqlParametersBank.Add(new SqlParameter("@IFSCCode", employeeModel.IFSCCode));
            sqlParametersBank.Add(new SqlParameter("@BankName", employeeModel.BankName));
            sqlParametersBank.Add(new SqlParameter("@UserID", model.UserID));
            SqlParameterCollection pOutputParamsdataSetBankDetails = null;

            var dataSetBankDetails = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmployeeBankDetails, sqlParametersBank, ref pOutputParamsdataSetBankDetails);

            List<SqlParameter> sqlParameterSeparation = new List<SqlParameter>();

            sqlParameterSeparation.Add(new SqlParameter("@EmployeeID", model.UserID));
            sqlParameterSeparation.Add(new SqlParameter("@DateOfJoiningTraining", ConvertToDbValue(employeeModel.DOJInTraining)));
            sqlParameterSeparation.Add(new SqlParameter("@DateOfJoiningFloor", ConvertToDbValue(employeeModel.JoiningDate)));
            sqlParameterSeparation.Add(new SqlParameter("@DateOfJoiningOJT", ConvertToDbValue(employeeModel.DOJInOJTOnroll)));
            sqlParameterSeparation.Add(new SqlParameter("@DateOfResignation", ConvertToDbValue(employeeModel.DateOfResignation)));
            sqlParameterSeparation.Add(new SqlParameter("@DateOfLeaving", ConvertToDbValue(employeeModel.DateOfLeaving)));
            sqlParameterSeparation.Add(new SqlParameter("@BackOnFloorDate", ConvertToDbValue(employeeModel.BackOnFloor)));
            sqlParameterSeparation.Add(new SqlParameter("@LeavingRemarks", ConvertToDbValue(employeeModel.LeavingRemarks, isString: true)));
            sqlParameterSeparation.Add(new SqlParameter("@MailReceivedFromAndDate", ConvertToDbValue(employeeModel.MailReceivedFromAndDate)));
            sqlParameterSeparation.Add(new SqlParameter("@LeavingType", ConvertToDbValue(employeeModel.LeavingType, isString: true)));
            sqlParameterSeparation.Add(new SqlParameter("@NoticeServed", ConvertToDbValues(employeeModel.NoticeServed, isString: true)));
            sqlParameterSeparation.Add(new SqlParameter("@UserID", model.UserID));
            sqlParameterSeparation.Add(new SqlParameter("@AgeOnNetwork", employeeModel.AON));
            //  sqlParameterSeparation.Add(new SqlParameter("@PreviousExperience", employeeModel.PreviousExperience));

            SqlParameterCollection pOutputParamSeparation = null;


            var dataSetSeparation = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmployeeSeparation, sqlParameterSeparation, ref pOutputParamSeparation);
            if (dataSetSeparation.Tables[0].Columns.Contains("Result"))
            {

            }
            long SuperAdminId = Convert.ToInt64(employeeModel.InsertedBy);
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@EmploymentDetailID", employmentDetailID));
            sqlParameters.Add(new SqlParameter("@EmployeeID", model.UserID));
            sqlParameters.Add(new SqlParameter("@DesignationID", employeeModel.DesignationName));
            sqlParameters.Add(new SqlParameter("@EmployeeTypeID", employeeModel.EmployeeType));
            sqlParameters.Add(new SqlParameter("@PayrollTypeID", employeeModel.PayrollTypeName));
            sqlParameters.Add(new SqlParameter("@DepartmentID", employeeModel.DepartmentName));
            sqlParameters.Add(new SqlParameter("@JobLocationID", employeeModel.JobLocationName));
            sqlParameters.Add(new SqlParameter("@OfficialEmailID", employeeModel.OfficialEmailID));
            sqlParameters.Add(new SqlParameter("@OfficialContactNo", employeeModel.OfficialContactNo));
            if (employeeModel.JoiningDate != null)
            {
                sqlParameters.Add(new SqlParameter("@JoiningDate", ConvertToDbValue(employeeModel.JoiningDate)));

            }
            else
            {
                sqlParameters.Add(new SqlParameter("@JoiningDate", DateTime.Now));

            }
            sqlParameters.Add(new SqlParameter("@ReportingToIDL1", employeeModel.ReportingToIDL1Name));
            sqlParameters.Add(new SqlParameter("@IsActive", false));
            sqlParameters.Add(new SqlParameter("@IsDeleted", false));
            sqlParameters.Add(new SqlParameter("@UserID", SuperAdminId));
            sqlParameters.Add(new SqlParameter("@LeavePolicyID", employeeModel.LeavePolicyName));
            sqlParameters.Add(new SqlParameter("@ReportingToIDL2", employeeModel.ReportingToIDL2Name));
            sqlParameters.Add(new SqlParameter("@ClientName", employeeModel.ClientName));
            sqlParameters.Add(new SqlParameter("@SubDepartmentID", employeeModel.SubDepartmentName));
            sqlParameters.Add(new SqlParameter("@ShiftTypeID", employeeModel.ShiftTypeName));
            sqlParameters.Add(new SqlParameter("@EmployeeNumber", employeeModel.EmployeeNumber));
            sqlParameters.Add(new SqlParameter("@CompnayID", Convert.ToInt64(employeeModel.CompanyName)));
            sqlParameters.Add(new SqlParameter("@ESINumber", employeeModel.ESINumber));
            sqlParameters.Add(new SqlParameter("@ESIRegistrationDate", ConvertToDbValue(employeeModel.RegistrationDateInESIC)));

            SqlParameterCollection pOutputParamss = null;

            var dataSets = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmploymentDetails, sqlParameters, ref pOutputParamss);
            if (dataSets.Tables[0].Columns.Contains("Result"))
            {
                model = dataSets.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            UserID = dataRow.Field<long>("UserID"),
                            PKNo = dataRow.Field<long>("UserID")
                        }
                   ).ToList().FirstOrDefault();
            }

            List<SqlParameter> sqlParameterss = new List<SqlParameter>();

            sqlParameterss.Add(new SqlParameter("@RoleID", Convert.ToInt32(employeeModel.RoleName)));
            sqlParameterss.Add(new SqlParameter("@UserID", model.UserID));

            SqlParameterCollection OutputParams1 = null;

            var datasSet2 = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_InsertUserRole, sqlParameterss, ref OutputParams1);
            if (datasSet2.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            PKNo = Convert.ToInt64(pOutputParams["@EmploymentDetailID"].Value),
                            IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired")
                        }
                   ).ToList().FirstOrDefault();
            }



            return model;
        }
        private object ConvertToDbValue(object value, bool isString = false)
        {
            if (value == null || value.ToString().Trim().Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                return isString ? string.Empty : DBNull.Value;
            }

            if (!isString)
            {
                if (DateTime.TryParse(value.ToString(), out DateTime dateValue))
                {
                    return dateValue;
                }
                return DBNull.Value; // If date parsing fails, store null in SQL
            }

            return value.ToString().Trim();
        }
        private object ConvertToDbValues(object value, bool isString = false)
        {
            if (value == null || value.ToString().Trim().Equals("NA", StringComparison.OrdinalIgnoreCase))
            {
                return isString ? string.Empty : 0; // Return 0 if not a string
            }

            if (!isString)
            {
                if (DateTime.TryParse(value.ToString(), out DateTime dateValue))
                {
                    return dateValue;
                }
                return DBNull.Value; // If it's not a valid date, return DBNull
            }

            return value.ToString().Trim(); // For string, return trimmed string
        }


        #region What's happening

        public Result AddUpdateWhatsHappeningDetails(WhatsHappeningModels Model)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@WhatsHappeningID", Model.WhatsHappeningID));
            sqlParameter.Add(new SqlParameter("@RetWhatsHappeningID", Model.WhatsHappeningID));
            sqlParameter.Add(new SqlParameter("@Title", Model.Title));
            sqlParameter.Add(new SqlParameter("@Description", Model.Description));
            sqlParameter.Add(new SqlParameter("@FromDate", Model.FromDate));
            sqlParameter.Add(new SqlParameter("@ToDate", Model.ToDate));
            sqlParameter.Add(new SqlParameter("@IconImage", Model.IconImage));
            sqlParameter.Add(new SqlParameter("@CompanyID", Model.CompanyID));
            sqlParameter.Add(new SqlParameter("@IsDeleted", Model.IsDeleted));
            sqlParameter.Add(new SqlParameter("@UserID", Model.CreatedBy));

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_WhatsHappening, sqlParameter);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString()
                        }
                   ).ToList().FirstOrDefault();
            }
            return model;
        }

        public Results GetAllWhatsHappeningDetails(WhatsHappeningModelParans model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter =
            [
                new SqlParameter("@CompanyID", model.CompanyID),
                new SqlParameter("@WhatsHappeningID", model.WhatsHappeningID),
            ];

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_WhatsHappeningS, sqlParameter);
            result.WhatsHappeningList = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new WhatsHappeningModels
                              {
                                  WhatsHappeningID = dataRow.Field<long>("WhatsHappeningID"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  Title = dataRow.Field<string>("Title"),
                                  Description = dataRow.Field<string>("Description"),
                                  FromDate = dataRow.Field<DateTime>("FromDate"),
                                  ToDate = dataRow.Field<DateTime>("ToDate"),
                                  IconImage = dataRow.Field<string>("IconImage"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),

                              }).ToList();

            if (model.WhatsHappeningID > 0)
            {
                result.WhatsHappeningModel = result.WhatsHappeningList.FirstOrDefault();
            }

            return result;
        }

        public string DeleteWhatsHappening(WhatsHappeningModelParans model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@WhatsHappeningID", model.WhatsHappeningID));
            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };
            sqlParameter.Add(outputMessage);
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_WhatsHappening, sqlParameter);
            string message = outputMessage.Value.ToString();
            return message;
        }

        #endregion What's happening

        private static DateTime? TryParseDate(string? dateString)
        {
            if (DateTime.TryParse(dateString, out var date))
            {
                return date;
            }
            return null;
        }
        private static int? TryParseInt(string? intString)
        {
            if (int.TryParse(intString, out var number))
            {
                return number;
            }
            return null;
        }
        public Result AddUpdateEmployeeFromExecelBulk(BulkEmployeeImportModel bulkImportModel)
        {
            try
            {
                var employeeList = bulkImportModel.Employees.Select(item => new ImportExcelDataTableType
                {
                    EmployeeID = 0,
                    CompanyID = Convert.ToInt64(item.CompanyName),
                    FirstName = item.FirstName,
                    MiddleName = item.MiddleName,
                    Surname = item.Surname,
                    CorrespondenceAddress = item.PresentAddress,
                    CorrespondenceCity = item.PresentCity,
                    CorrespondencePinCode = item.PresentPinCode,
                    CorrespondenceState = item.PresentState,
                    CorrespondenceCountryID = Convert.ToInt64(item.PresentCountryName),
                    EmailAddress = item.EmailId,
                    Landline = item.Landline,
                    Mobile = item.Mobile,
                    Telephone = item.Telephone,
                    PersonalEmailAddress = item.EmailId,
                    PermanentAddress = item.PermanentAddress,
                    PermanentCity = item.PermanentCity,
                    PermanentPinCode = item.PermanentPinCode,
                    PermanentState = item.PermanentState,
                    PermanentCountryID = Convert.ToInt64(item.PermanentCountryName),
                    PeriodOfStay = item.PeriodOfStay,
                    VerificationContactPersonName = item.VerificationContactPersonName,
                    VerificationContactPersonContactNo = item.VerificationContactPersonContactNo,
                    DateOfBirth = TryParseDate(item.DateOfBirth),
                    PlaceOfBirth = item.PlaceOfBirth,
                    IsReferredByExistingEmployee = TryParseBool(item.IsReferredByExistingEmployee),
                    ReferredByEmployeeID = item.ReferredByEmployeeName,
                    BloodGroup = item.BloodGroup,
                    PANNo = item.PANNo,
                    AadharCardNo = item.AadharCardNo,
                    Allergies = item.Allergies,
                    IsRelativesWorkingWithCompany = TryParseBool(item.IsRelativesWorkingWithCompany),
                    RelativesDetails = item.RelativesDetails,
                    MajorIllnessOrDisability = item.MajorIllnessOrDisability,
                    AwardsAchievements = item.AwardsAchievements,
                    EducationGap = item.EducationGap,
                    ExtraCuricuarActivities = item.ExtraCurricularActivities,
                    ForiegnCountryVisits = item.ForeignCountryVisits,
                    ContactPersonName = item.EmergencyContactPersonName,
                    ContactPersonMobile = item.EmergencyContactPersonMobile,
                    ContactPersonTelephone = item.EmergencyContactPersonTelephone,
                    ContactPersonRelationship = item.EmergencyContactPersonRelationship,
                    ITSkillsKnowledge = item.ITSkillsKnowledge,
                    InsertedByUserID = Convert.ToInt64(item.InsertedByUserID),
                    LeavePolicyID = Convert.ToInt64(item.LeavePolicyName),
                    Gender = TryParseInt(item.Gender),
                    UserName = item.EMPID,
                    Email = item.EmailId,
                    EmployeNumber = item.EMPID,
                    DesignationID = Convert.ToInt64(item.DesignationName),
                    EmployeeTypeID = Convert.ToInt64(item.Category),
                    DepartmentID = Convert.ToInt64(item.DepartmentName),
                    JobLocationID = Convert.ToInt64(item.Location),
                    OfficialEmailID = item.ProtalkId,
                    OfficialContactNo = item.OfficialContactNo,
                    JoiningDate = TryParseDate(item.JoiningDate),
                    JobSeprationDate = TryParseDate(item.DateOfResignation),
                    ReportingToIDL1 = Convert.ToInt64(item.ReportingToIDL2Name),
                    PayrollTypeID = Convert.ToInt64(item.PayrollTypeName),
                    ReportingToIDL2 = Convert.ToInt64(item.ReportingToIDL2Name),
                    ClientName = item.ClientName,
                    SubDepartmentID = Convert.ToInt64(item.SubDepartmentName),
                    ShiftTypeID = Convert.ToInt64(item.ShiftTypeName),
                    ESINumber = item.ESINumber,
                    ESIRegistrationDate = TryParseDate(item.RegistrationDateInESIC),
                    BankAccountNumber = item.BankAccountNumber,
                    UANNumber = item.UANNumber,
                    IFSCCode = item.IFSCCode,
                    BankName = item.BankName,
                    AgeOnNetwork = Convert.ToInt32(item.AON),
                    NoticeServed = Convert.ToInt32(item.NoticeServed),
                    LeavingType = item.LeavingType,
                    PreviousExperience = item.PreviousExperience,
                    DateOfJoiningTraining = TryParseDate(item.DOJInTraining),
                    DateOfJoiningFloor = TryParseDate(item.DOJOnFloor),
                    DateOfJoiningOJT = TryParseDate(item.DOJInOJT),
                    DateOfJoiningOnroll = TryParseDate(item.DOJInOnroll),
                    DateOfResignation = TryParseDate(item.DateOfResignation),
                    DateOfLeaving = TryParseDate(item.DateOfLeaving),
                    BackOnFloorDate = TryParseDate(item.BackOnFloor),
                    LeavingRemarks = item.LeavingRemarks,
                    MailReceivedFromAndDate = item.MailReceivedFromAndDate,
                    EmailSentToITDate = TryParseDate(item.DateOfEmailSentToITForIDDeletion),
                    IsActive = item.Status == "1",
                    ReportingToIDL1EmployeeNumber = item.EmpCodeofReportingManager,
                    SourcingType = item.SourcingType,
                    RefereeName = item.RefereeName,
                    MobileNumberReferee = item.MobileNumberOfReferee,
                    DocumentationStatus = item.DocumentationStatus,
                    LOB = item.LOB
                }).ToList();
                var employeeDataTable = ConvertToDataTable(employeeList);
                var parameters = new List<SqlParameter>
{
    new SqlParameter("@EmployeeTVP", SqlDbType.Structured)
    {
        TypeName = "dbo.EmployeeTVP",
        Value = employeeDataTable
    }
};
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdateExcelImport, parameters);

                return new Result
                {
                    Message = "Employee data imported successfully.",
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Message = $"Error while importing employee data."
                };
            }
        }







        private DataTable ConvertToDataTable(List<ImportExcelDataTableType> employees)
        {
            var table = new DataTable();
            table.Columns.Add("EmployeeID", typeof(long));
            table.Columns.Add("CompanyID", typeof(long));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("MiddleName", typeof(string));
            table.Columns.Add("Surname", typeof(string));
            table.Columns.Add("CorrespondenceAddress", typeof(string));
            table.Columns.Add("CorrespondenceCity", typeof(string));
            table.Columns.Add("CorrespondencePinCode", typeof(string));
            table.Columns.Add("CorrespondenceState", typeof(string));
            table.Columns.Add("CorrespondenceCountryID", typeof(long));
            table.Columns.Add("EmailAddress", typeof(string));
            table.Columns.Add("Landline", typeof(string));
            table.Columns.Add("Mobile", typeof(string));
            table.Columns.Add("Telephone", typeof(string));
            table.Columns.Add("PersonalEmailAddress", typeof(string));
            table.Columns.Add("PermanentAddress", typeof(string));
            table.Columns.Add("PermanentCity", typeof(string));
            table.Columns.Add("PermanentPinCode", typeof(string));
            table.Columns.Add("PermanentState", typeof(string));
            table.Columns.Add("PermanentCountryID", typeof(long));
            table.Columns.Add("PeriodOfStay", typeof(string));
            table.Columns.Add("VerificationContactPersonName", typeof(string));
            table.Columns.Add("VerificationContactPersonContactNo", typeof(string));
            table.Columns.Add("DateOfBirth", typeof(DateTime));
            table.Columns.Add("PlaceOfBirth", typeof(string));
            table.Columns.Add("IsReferredByExistingEmployee", typeof(bool));
            table.Columns.Add("ReferredByEmployeeID", typeof(string));
            table.Columns.Add("BloodGroup", typeof(string));
            table.Columns.Add("PANNo", typeof(string));
            table.Columns.Add("AadharCardNo", typeof(string));
            table.Columns.Add("Allergies", typeof(string));
            table.Columns.Add("IsRelativesWorkingWithCompany", typeof(bool));
            table.Columns.Add("RelativesDetails", typeof(string));
            table.Columns.Add("MajorIllnessOrDisability", typeof(string));
            table.Columns.Add("AwardsAchievements", typeof(string));
            table.Columns.Add("EducationGap", typeof(string));
            table.Columns.Add("ExtraCuricuarActivities", typeof(string));
            table.Columns.Add("ForiegnCountryVisits", typeof(string));
            table.Columns.Add("ContactPersonName", typeof(string));
            table.Columns.Add("ContactPersonMobile", typeof(string));
            table.Columns.Add("ContactPersonTelephone", typeof(string));
            table.Columns.Add("ContactPersonRelationship", typeof(string));
            table.Columns.Add("ITSkillsKnowledge", typeof(string));
            table.Columns.Add("InsertedByUserID", typeof(long));
            table.Columns.Add("LeavePolicyID", typeof(long));
            table.Columns.Add("CarryForword", typeof(long));
            table.Columns.Add("Gender", typeof(int));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("PasswordHash", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("EmployeNumber", typeof(string));
            table.Columns.Add("DesignationID", typeof(long));
            table.Columns.Add("EmployeeTypeID", typeof(long));
            table.Columns.Add("DepartmentID", typeof(long));
            table.Columns.Add("JobLocationID", typeof(long));
            table.Columns.Add("OfficialEmailID", typeof(string));
            table.Columns.Add("OfficialContactNo", typeof(string));
            table.Columns.Add("JoiningDate", typeof(DateTime));
            table.Columns.Add("JobSeprationDate", typeof(DateTime));
            table.Columns.Add("ReportingToIDL1", typeof(long));
            table.Columns.Add("PayrollTypeID", typeof(long));
            table.Columns.Add("ReportingToIDL2", typeof(long));
            table.Columns.Add("ClientName", typeof(string));
            table.Columns.Add("SubDepartmentID", typeof(long));
            table.Columns.Add("ShiftTypeID", typeof(long));
            table.Columns.Add("ESINumber", typeof(string));
            table.Columns.Add("ESIRegistrationDate", typeof(DateTime));
            table.Columns.Add("BankAccountNumber", typeof(string));
            table.Columns.Add("UANNumber", typeof(string));
            table.Columns.Add("IFSCCode", typeof(string));
            table.Columns.Add("BankName", typeof(string));
            table.Columns.Add("AgeOnNetwork", typeof(int));
            table.Columns.Add("NoticeServed", typeof(int));
            table.Columns.Add("LeavingType", typeof(string));
            table.Columns.Add("PreviousExperience", typeof(string));
            table.Columns.Add("DateOfJoiningTraining", typeof(DateTime));
            table.Columns.Add("DateOfJoiningFloor", typeof(DateTime));
            table.Columns.Add("DateOfJoiningOJT", typeof(DateTime));
            table.Columns.Add("DateOfJoiningOnroll", typeof(DateTime));
            table.Columns.Add("DateOfResignation", typeof(DateTime));
            table.Columns.Add("DateOfLeaving", typeof(DateTime));
            table.Columns.Add("BackOnFloorDate", typeof(DateTime));
            table.Columns.Add("LeavingRemarks", typeof(string));
            table.Columns.Add("MailReceivedFromAndDate", typeof(string));
            table.Columns.Add("EmailSentToITDate", typeof(DateTime));
            table.Columns.Add("IsActive", typeof(bool));
            table.Columns.Add("ReportingToIDL1EmployeeNumber", typeof(string));
            table.Columns.Add("SourcingType", typeof(string));
            table.Columns.Add("RefereeName", typeof(string));
            table.Columns.Add("MobileNumberReferee", typeof(string));
            table.Columns.Add("DocumentationStatus", typeof(string));
            table.Columns.Add("LOB", typeof(string));
            foreach (var emp in employees)
            {
                table.Rows.Add(
                    emp.EmployeeID,
                    emp.CompanyID,
                    emp.FirstName,
                    emp.MiddleName,
                    emp.Surname,
                    emp.CorrespondenceAddress,
                    emp.CorrespondenceCity,
                    emp.CorrespondencePinCode,
                    emp.CorrespondenceState,
                    emp.CorrespondenceCountryID,
                    emp.EmailAddress,
                    emp.Landline,
                    emp.Mobile,
                    emp.Telephone,
                    emp.PersonalEmailAddress,
                    emp.PermanentAddress,
                    emp.PermanentCity,
                    emp.PermanentPinCode,
                    emp.PermanentState,
                    emp.PermanentCountryID,
                    emp.PeriodOfStay,
                    emp.VerificationContactPersonName,
                    emp.VerificationContactPersonContactNo,
                    emp.DateOfBirth,
                    emp.PlaceOfBirth,
                    emp.IsReferredByExistingEmployee,
                    emp.ReferredByEmployeeID,
                    emp.BloodGroup,
                    emp.PANNo,
                    emp.AadharCardNo,
                    emp.Allergies,
                    emp.IsRelativesWorkingWithCompany,
                    emp.RelativesDetails,
                    emp.MajorIllnessOrDisability,
                    emp.AwardsAchievements,
                    emp.EducationGap,
                    emp.ExtraCuricuarActivities,
                    emp.ForiegnCountryVisits,
                    emp.ContactPersonName,
                    emp.ContactPersonMobile,
                    emp.ContactPersonTelephone,
                    emp.ContactPersonRelationship,
                    emp.ITSkillsKnowledge,
                    emp.InsertedByUserID,
                    emp.LeavePolicyID,
                    emp.CarryForword,
                    emp.Gender,
                    emp.UserName,
                    emp.PasswordHash,
                    emp.Email,
                    emp.EmployeNumber,
                    emp.DesignationID,
                    emp.EmployeeTypeID,
                    emp.DepartmentID,
                    emp.JobLocationID,
                    emp.OfficialEmailID,
                    emp.OfficialContactNo,
                    emp.JoiningDate,
                    emp.JobSeprationDate,
                    emp.ReportingToIDL1,
                    emp.PayrollTypeID,
                    emp.ReportingToIDL2,
                    emp.ClientName,
                    emp.SubDepartmentID,
                    emp.ShiftTypeID,
                    emp.ESINumber,
                    emp.ESIRegistrationDate,
                    emp.BankAccountNumber,
                    emp.UANNumber,
                    emp.IFSCCode,
                    emp.BankName,
                    emp.AgeOnNetwork,
                    emp.NoticeServed,
                    emp.LeavingType,
                    emp.PreviousExperience,
                    emp.DateOfJoiningTraining,
                    emp.DateOfJoiningFloor,
                    emp.DateOfJoiningOJT,
                    emp.DateOfJoiningOnroll,
                    emp.DateOfResignation,
                    emp.DateOfLeaving,
                    emp.BackOnFloorDate,
                    emp.LeavingRemarks,
                    emp.MailReceivedFromAndDate,
                    emp.EmailSentToITDate,
                    emp.IsActive,
                    emp.ReportingToIDL1EmployeeNumber,
                    emp.SourcingType,
                    emp.RefereeName,
                    emp.MobileNumberReferee,
                    emp.DocumentationStatus,
                    emp.LOB

                );
            }

            return table;
        }







        private static bool? TryParseBool(string? boolString)
        {
            if (boolString == null) return null;
            return boolString.Trim().ToLower() switch
            {
                "yes" => true,
                "no" => false,
                "true" => true,
                "false" => false,
                _ => null
            };
        }

        public EmployeePersonalDetails GetEmployeeDetails(EmployeePersonalDetailsById objmodel)
        {
            EmployeePersonalDetails model = new EmployeePersonalDetails();

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeId", objmodel.EmployeeID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Employees_details, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[0];

                model.EmployeeID = Convert.ToInt64(row["EmployeeID"]);
                model.EmployeeName = Convert.ToString(row["EmployeeName"]);
                model.PersonalEmailAddress = Convert.ToString(row["PersonalEmailAddress"]);
                model.DepartmentName = Convert.ToString(row["DepartmentName"]);
                model.EmployeNumber = Convert.ToString(row["EmployeNumber"]);
            }

            return model;
        }


        #region CheckEmployeeReporting
        public ReportingStatus CheckEmployeeReporting(ReportingStatus obj)
        {
            ReportingStatus result = new ReportingStatus();
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeId", obj.EmployeeId),
        new SqlParameter("@IsActive", obj.Status)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_CheckEmployeeReporting, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[0];
                result = new ReportingStatus
                {
                    Status = Convert.ToInt32(row["Status"]),
                    Message = Convert.ToString(row["Message"])
                };
            }

            return result;
        }
        #endregion CheckEmployeeReporting


        public List<ExportEmployeeDetailsExcel> FetchExportEmployeeExcelSheet(EmployeeInputParams model)
        {
            List<ExportEmployeeDetailsExcel> employeeDetails = new List<ExportEmployeeDetailsExcel>();
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@CompnayId", model.CompanyID),  // keep as is if SP param has typo, else correct to @CompanyId
    };

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_ExportEmployeeFullDetails, sqlParameters);

                if (dataSet.Tables.Count > 0)
                {
                    employeeDetails = dataSet.Tables[0].AsEnumerable()
                        .Select(row => new ExportEmployeeDetailsExcel
                        {
                            EmployeeNumber = row.Field<string>("EmployeNumber"),
                            CompanyName = row.Field<string>("CompanyName"),
                            FirstName = row.Field<string>("FirstName"),
                            MiddleName = row.Field<string>("MiddleName"),
                            Surname = row.Field<string>("Surname"),
                            Gender = row.Field<string>("Gender"),
                            DateOfBirth = DateTime.TryParse(row["DateOfBirth"]?.ToString(), out var dob) ? dob : (DateTime?)null,
                            PlaceOfBirth = row.Field<string>("PlaceOfBirth"),
                            EmailAddress = row.Field<string>("EmailAddress"),
                            PersonalEmailAddress = row.Field<string>("PersonalEmailAddress"),
                            Mobile = row.Field<string>("Mobile"),
                            Landline = row.Field<string>("Landline"),
                            Telephone = row.Field<string>("Telephone"),
                            CorrespondenceAddress = row.Field<string>("CorrespondenceAddress"),
                            CorrespondenceCity = row.Field<string>("CorrespondenceCity"),
                            CorrespondenceState = row.Field<string>("CorrespondenceState"),
                            CorrespondencePinCode = row.Field<string>("CorrespondencePinCode"),
                            PermanentAddress = row.Field<string>("PermanentAddress"),
                            PermanentCity = row.Field<string>("PermanentCity"),
                            PermanentState = row.Field<string>("PermanentState"),
                            PermanentPinCode = row.Field<string>("PermanentPinCode"),
                            PANNo = row.Field<string>("PANNo"),
                            AadharCardNo = row.Field<string>("AadharCardNo"),
                            BloodGroup = row.Field<string>("BloodGroup"),
                            Allergies = row.Field<string>("Allergies"),
                            MajorIllnessOrDisability = row.Field<string>("MajorIllnessOrDisability"),
                            AwardsAchievements = row.Field<string>("AwardsAchievements"),
                            EducationGap = row.Field<string>("EducationGap"),
                            ExtraCuricuarActivities = row.Field<string>("ExtraCuricuarActivities"),
                            ForiegnCountryVisits = row.Field<string>("ForiegnCountryVisits"),
                            ContactPersonName = row.Field<string>("ContactPersonName"),
                            ContactPersonMobile = row.Field<string>("ContactPersonMobile"),
                            ContactPersonTelephone = row.Field<string>("ContactPersonTelephone"),
                            ContactPersonRelationship = row.Field<string>("ContactPersonRelationship"),
                            ITSkillsKnowledge = row.Field<string>("ITSkillsKnowledge"),
                            Designation = row.Field<string>("Designation"),
                            EmployeeType = row.Field<string>("EmployeeType"),
                            Department = row.Field<string>("Department"),
                            SubDepartment = row.Field<string>("SubDepartment"),
                            JobLocation = row.Field<string>("JobLocation"),
                            ShiftType = row.Field<string>("ShiftType"),
                            OfficialEmailID = row.Field<string>("OfficialEmailID"),
                            OfficialContactNo = row.Field<string>("OfficialContactNo"),
                            JoiningDate = DateTime.TryParse(row["JoiningDate"]?.ToString(), out var jd) ? jd : (DateTime?)null,
                            ReportingManager = row.Field<string>("ReportingManager"),
                            PolicyName = row.Field<string>("PolicyName"),
                            PayrollType = row.Field<string>("PayrollType"),
                            ClientName = row.Field<string>("ClientName"),
                            ESINumber = row.Field<string>("ESINumber"),
                            ESIRegistrationDate = row.Field<DateTime?>("ESIRegistrationDate"),
                            BankAccountNumber = row.Field<string>("BankAccountNumber"),
                            IFSCCode = row.Field<string>("IFSCCode"),
                            BankName = row.Field<string>("BankName"),
                            UANNumber = row.Field<string>("UANNumber"),
                            DateOfResignation = DateTime.TryParse(row["DateOfResignation"]?.ToString(), out var dr) ? dr : (DateTime?)null,
                            DateOfLeaving = DateTime.TryParse(row["DateOfLeaving"]?.ToString(), out var dateOfLeaving) ? dateOfLeaving : (DateTime?)null,
                            LeavingType = row.Field<string>("LeavingType"),
                            NoticeServed = row.Field<int?>("NoticeServed"),
                            AgeOnNetwork = row.Field<int?>("AgeOnNetwork"),
                            PreviousExperience = row.Field<string?>("PreviousExperience"),
                            DateOfJoiningTraining = DateTime.TryParse(row["DateOfJoiningTraining"]?.ToString(), out var dojTraining) ? dojTraining : (DateTime?)null,
                            DateOfJoiningFloor = DateTime.TryParse(row["DateOfJoiningFloor"]?.ToString(), out var dojFloor) ? dojFloor : (DateTime?)null,
                            DateOfJoiningOJT = DateTime.TryParse(row["DateOfJoiningOJT"]?.ToString(), out var dojOjt) ? dojOjt : (DateTime?)null,
                            DateOfJoiningOnroll = DateTime.TryParse(row["DateOfJoiningOnroll"]?.ToString(), out var djo) ? djo : (DateTime?)null,
                            BackOnFloorDate = DateTime.TryParse(row["BackOnFloorDate"]?.ToString(), out var backOnFloor) ? backOnFloor : (DateTime?)null,
                            LeavingRemarks = row.Field<string>("LeavingRemarks"),
                            MailReceivedFromAndDate = DateTime.TryParse(row["MailReceivedFromAndDate"]?.ToString(), out var mailReceived) ? mailReceived : (DateTime?)null,
                            EmailSentToITDate = DateTime.TryParse(row["EmailSentToITDate"]?.ToString(), out var emailSent) ? emailSent : (DateTime?)null,
                            Status = row.Field<string>("Status"),


                        }).ToList();
                }
            }
            catch (Exception ex)
            {

            }
            return employeeDetails;
        }

        //public int GetCampOffLeaveCount(long EmployeeID, long JobLocationTypeID)
        //{
        //    LeaveResults result = new LeaveResults();
        //    List<SqlParameter> sqlParameter = new List<SqlParameter>();
        //    sqlParameter.Add(new SqlParameter("@EmployeeID", EmployeeID));
        //    sqlParameter.Add(new SqlParameter("@JobLocationTypeID", JobLocationTypeID));
        //    var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CampOffLeaves, sqlParameter);
        //    int totalRecords = 0;
        //    if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
        //    {
        //        int.TryParse(dataSet.Tables[0].Rows[0]["AvailableCompOffDays"].ToString(), out totalRecords);
        //    }

        //    return totalRecords;
        //}
        public decimal GetCampOffLeaveCount(long employeeID, long jobLocationTypeID)
        {
            decimal availableCompOffDays = 0;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeID", employeeID),
        new SqlParameter("@JobLocationTypeID", jobLocationTypeID)
    };

            DataSet dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CampOffLeaves, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                object resultValue = dataSet.Tables[0].Rows[0]["AvailableCompOffDays"];
                if (resultValue != DBNull.Value)
                {
                    decimal.TryParse(resultValue.ToString(), out availableCompOffDays);
                }
            }

            return availableCompOffDays;
        }


        public CompOffValidationResult GetValidateCompOffLeave(CampOffEligible model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeID", model.EmployeeID),
        new SqlParameter("@JobLocationTypeID", model.JobLocationTypeID),
        new SqlParameter("@StartDate", model.StartDate),
        new SqlParameter("@EndDate", model.EndDate),
        new SqlParameter("@EmployeeNumber", model.EmployeeNumber),
        new SqlParameter("@RequestedLeaveDays", model.RequestedLeaveDays)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Is_CampOff_Eligible, sqlParameter);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[0];
                return new CompOffValidationResult
                {
                    IsEligible = Convert.ToInt32(row["IsEligible"]),
                    Message = row["Message"].ToString(),
                    EligibleDays = Convert.ToInt32(row["EligibleDays"]),
                    RequestedDays = Convert.ToInt32(row["RequestedDays"]),
                    AvailableCompOffDays = Convert.ToInt32(row["AvailableCompOffDays"])
                };
            }

            return null; // or return a default object indicating failure
        }
        public UpdateLeaveStatus UpdateLeaveStatus(UpdateLeaveStatus model)
        {
            var sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeID", model.EmployeeID),
        new SqlParameter("@NewLeaveStatusID", model.NewLeaveStatusID),
        new SqlParameter("@LeaveSummaryID", model.LeaveSummaryID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_UpdateLeaveStatus, sqlParameters);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[0];
                model.Message = row["Message"]?.ToString();
            }
            else
            {
                model.Message = "No response from stored procedure.";
            }

            return model;
        }

        #region EmployeeAdditonalDetails

        public List<EducationalDetail> GetEducationDetails(EducationDetailParams model)
        {
            List<SqlParameter> parameters = new()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EducationDetailsByEmployee, parameters);
            if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                return new List<EducationalDetail>();

            return dataSet.Tables[0].AsEnumerable()
                .Select(row => new EducationalDetail
                {
                    EducationDetailID = row.Field<long>("EducationDetailID"),
                    EmployeeID = row.Field<long>("EmployeeID"),
                    School_University = row.Field<string>("School_University"),
                    Qualification = row.Field<string>("Qualification"),
                    YearOfPassing = row.Field<string>("YearOfPassing"),
                    Percentage = row.Field<string>("Percentage"),
                    Major_OptionalSubjects = row.Field<string>("Major_OptionalSubjects"),
                    CertificateImage = row.Field<string>("CertificateImage")
                }).ToList();
        }

        public Result AddUpdateEducationDetail(EducationalDetail eduDetail)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@EducationDetailID", eduDetail.EducationDetailID),
        new SqlParameter("@EmployeeID", eduDetail.EmployeeID),
        new SqlParameter("@School_University", eduDetail.School_University),
        new SqlParameter("@Qualification", eduDetail.Qualification),
        new SqlParameter("@YearOfPassing", eduDetail.YearOfPassing),
        new SqlParameter("@Percentage", eduDetail.Percentage),
        new SqlParameter("@Major_OptionalSubjects", eduDetail.Major_OptionalSubjects),
        new SqlParameter("@CertificateImage", eduDetail.CertificateImage),
        new SqlParameter("@IsActive", true),
        new SqlParameter("@IsDeleted", false),
        new SqlParameter("@UserID", eduDetail.UserID)
    };

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EducationDetails, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new Result
                    {
                        Message = dataRow.Field<string>("Result"),
                        PKNo = dataRow.Field<long?>("RetEducationDetailID") ?? 0
                    })
                    .FirstOrDefault();
            }

            return model;
        }

        public string DeleteEducationDetail(EducationDetailParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EducationDetailID", model.EducationDetailID));
            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };
            sqlParameter.Add(outputMessage);
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_EducationDetails, sqlParameter);
            string message = outputMessage.Value.ToString();
            return message;
        }

        public List<EmploymentHistory> GetEmploymentHistory(EmploymentHistoryParams model)
        {
            List<SqlParameter> parameters = new()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmploymentHistoryByEmployee, parameters);
            if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                return new List<EmploymentHistory>();

            return dataSet.Tables[0].AsEnumerable()
                .Select(row => new EmploymentHistory
                {
                    EmploymentHistoryID = row.Field<long>("EmploymentHistoryID"),
                    EmployeeID = row.Field<long>("EmployeeID"),
                    CountryID = row.Field<long>("CountryID"),
                    EmploymentID = row.Field<string>("EmploymentID"),
                    CompanyName = row.Field<string>("CompanyName"),
                    From = row.Field<DateTime?>("From"),
                    To = row.Field<DateTime?>("To"),
                    Address = row.Field<string>("Address"),
                    Phone = row.Field<string>("Phone"),
                    City = row.Field<string>("City"),
                    State = row.Field<string>("State"),
                    PostalCode = row.Field<string>("PostalCode"),
                    ReasionFoLeaving = row.Field<string>("ReasionFoLeaving"),
                    Designition = row.Field<string>("Designition"),
                    GrossSalary = row.Field<string>("GrossSalary"),
                    SupervisorName = row.Field<string>("SupervisorName"),
                    SupervisorDesignition = row.Field<string>("SupervisorDesignition"),
                    SupervisorContactNo = row.Field<string>("SupervisorContactNo"),
                    HRName = row.Field<string>("HRName"),
                    HREmail = row.Field<string>("HREmail"),
                    HRContactNo = row.Field<string>("HRContactNo")
                }).ToList();
        }

        public Result AddUpdateEmploymentHistory(EmploymentHistory emp)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@EmploymentHistoryID", emp.EmploymentHistoryID),
        new SqlParameter("@EmployeeID", emp.EmployeeID),
        new SqlParameter("@CountryID", emp.CountryID),
        new SqlParameter("@EmploymentID", emp.EmploymentID),
        new SqlParameter("@CompanyName", emp.CompanyName),
        new SqlParameter("@From", (object?)emp.From ?? DBNull.Value),
        new SqlParameter("@To", (object?)emp.To ?? DBNull.Value),
        new SqlParameter("@Address", emp.Address),
        new SqlParameter("@Phone", emp.Phone),
        new SqlParameter("@City", emp.City),
        new SqlParameter("@State", emp.State),
        new SqlParameter("@PostalCode", emp.PostalCode),
        new SqlParameter("@ReasionFoLeaving", emp.ReasionFoLeaving),
        new SqlParameter("@Designition", emp.Designition),
        new SqlParameter("@GrossSalary", emp.GrossSalary),
        new SqlParameter("@SupervisorName", emp.SupervisorName),
        new SqlParameter("@SupervisorDesignition", emp.SupervisorDesignition),
        new SqlParameter("@SupervisorContactNo", emp.SupervisorContactNo),
        new SqlParameter("@HRName", emp.HRName),
        new SqlParameter("@HREmail", emp.HREmail),
        new SqlParameter("@HRContactNo", emp.HRContactNo),
        new SqlParameter("@IsActive", true),
        new SqlParameter("@IsDeleted", false),
        new SqlParameter("@UserID", emp.UserID)
    };

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmploymentHistory, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new Result
                    {
                        Message = dataRow.Field<string>("Result"),
                        PKNo = dataRow.Field<long?>("RetEmploymentHistoryID") ?? 0
                    })
                    .FirstOrDefault();
            }

            return model;
        }
        public string DeleteEmploymentHistory(EmploymentHistoryParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@EmploymentHistoryID", model.EmploymentHistoryID)
    };

            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };

            sqlParameter.Add(outputMessage);

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_EmploymentHistory, sqlParameter);

            return outputMessage.Value.ToString();
        }

        public List<Reference> GetReferenceDetails(ReferenceParams model)
        {
            List<SqlParameter> parameters = new()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_ReferenceDetailsByEmployee, parameters);
            if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                return new List<Reference>();

            return dataSet.Tables[0].AsEnumerable()
                .Select(row => new Reference
                {
                    ReferenceDetailID = row.Field<long>("ReferenceDetailID"),
                    EmployeeID = row.Field<long>("EmployeeID"),
                    Name = row.Field<string>("Name"),
                    Contact = row.Field<string>("Contact"),
                    OrgnizationName = row.Field<string>("OrgnizationName"),
                    RelationWithCandidate = row.Field<string>("RelationWithCandidate")
                }).ToList();
        }

        public Result AddUpdateReferenceDetail(Reference reference)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@ReferenceDetailID", reference.ReferenceDetailID),
        new SqlParameter("@EmployeeID", reference.EmployeeID),
        new SqlParameter("@Name", reference.Name),
        new SqlParameter("@Contact", reference.Contact),
        new SqlParameter("@OrgnizationName", reference.OrgnizationName),
        new SqlParameter("@RelationWithCandidate", reference.RelationWithCandidate),
        new SqlParameter("@IsActive", true),
        new SqlParameter("@IsDeleted", false),
        new SqlParameter("@UserID", reference.UserID) // Replace with actual UserID if available
    };

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_ReferenceDetails, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new Result
                    {
                        Message = dataRow.Field<string>("Result"),
                        PKNo = dataRow.Field<long?>("RetReferenceDetailID") ?? 0
                    })
                    .FirstOrDefault();
            }

            return model;
        }

        public string DeleteReferenceDetail(ReferenceParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@ReferenceDetailID", model.ReferenceDetailID)
    };

            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };

            sqlParameter.Add(outputMessage);

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_ReferenceDetails, sqlParameter);
            return outputMessage.Value.ToString();
        }

        public List<FamilyDetail> GetFamilyDetails(FamilyDetailParams model)
        {
            List<SqlParameter> parameters = new()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeesFamilyDetailsByEmployee, parameters);
            if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                return new List<FamilyDetail>();

            return dataSet.Tables[0].AsEnumerable()
                .Select(row => new FamilyDetail
                {
                    EmployeesFamilyDetailID = row.Field<long>("EmployeesFamilyDetailID"),
                    EmployeeID = row.Field<long>("EmployeeID"),
                    FamilyName = row.Field<string>("FamilyName"),
                    Relationship = row.Field<string>("Relationship"),
                    Age = row.Field<string>("Age"),
                    Details = row.Field<string>("Details")
                }).ToList();
        }

        public Result AddUpdateFamilyDetail(FamilyDetail familyDetail)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@EmployeesFamilyDetailID", familyDetail.EmployeesFamilyDetailID),
        new SqlParameter("@EmployeeID", familyDetail.EmployeeID),
        new SqlParameter("@FamilyName", familyDetail.FamilyName),
        new SqlParameter("@Relationship", familyDetail.Relationship),
        new SqlParameter("@Age", familyDetail.Age),
        new SqlParameter("@Details", familyDetail.Details),
        new SqlParameter("@IsActive", true),
        new SqlParameter("@IsDeleted", false),
        new SqlParameter("@UserID", familyDetail.UserID)
    };

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_FamilyDetails, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new Result
                    {
                        Message = dataRow.Field<string>("Result"),
                        PKNo = dataRow.Field<long?>("RetEmployeesFamilyDetailID") ?? 0
                    })
                    .FirstOrDefault();
            }

            return model;
        }

        public string DeleteFamilyDetail(FamilyDetailParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@EmployeesFamilyDetailID", model.EmployeesFamilyDetailID)
    };

            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };

            sqlParameter.Add(outputMessage);

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_EmployeesFamilyDetails, sqlParameter);
            return outputMessage.Value.ToString();
        }

        public List<LanguageDetail> GetLanguageDetails(LanguageDetailParams model)
        {
            List<SqlParameter> parameters = new()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LanguageDetailsByEmployee, parameters);
            if (dataSet == null || dataSet.Tables.Count == 0 || dataSet.Tables[0].Rows.Count == 0)
                return new List<LanguageDetail>();

            return dataSet.Tables[0].AsEnumerable()
                .Select(row => new LanguageDetail
                {
                    LanguageDetailID = row.Field<long>("LanguageDetailID"),
                    EmployeeID = row.Field<long>("EmployeeID"),
                    LanguageID = row.Field<long>("LanguageID"),
                    LanguageName = row.Field<string>("LanguageName"), // Assuming you joined this in SP
                    IsSpeak = row.Field<bool>("IsSpeak"),
                    IsRead = row.Field<bool>("IsRead"),
                    IsWrite = row.Field<bool>("IsWrite")
                }).ToList();
        }
        public Result AddUpdateLanguageDetail(LanguageDetail languageDetail)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new()
    {
        new SqlParameter("@LanguageDetailID", languageDetail.LanguageDetailID),
        new SqlParameter("@EmployeeID", languageDetail.EmployeeID),
        new SqlParameter("@LanguageID", languageDetail.LanguageID),
        new SqlParameter("@IsSpeak", languageDetail.IsSpeak),
        new SqlParameter("@IsRead", languageDetail.IsRead),
        new SqlParameter("@IsWrite", languageDetail.IsWrite),
        new SqlParameter("@UserID", languageDetail.UserID)
    };

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_LanguageDetails, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new Result
                    {
                        Message = dataRow.Field<string>("Result"),
                        PKNo = dataRow.Field<long?>("RetLanguageDetailID") ?? 0
                    })
                    .FirstOrDefault();
            }

            return model;
        }
        public string DeleteLanguageDetail(LanguageDetailParams model)
        {
            List<SqlParameter> sqlParameter = new()
    {
        new SqlParameter("@LanguageDetailID", model.EmployeeLanguageDetailID)
    };

            SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 250)
            {
                Direction = ParameterDirection.Output
            };

            sqlParameter.Add(outputMessage);

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Delete_LanguageDetails, sqlParameter);
            return outputMessage.Value.ToString();
        }



        #endregion EmployeeAdditonalDetails


        #region Page Permission
        public Results GetAllCompanyFormsPermission(long companyID)
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", companyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CompanyForms, sqlParameter);


            model.FormsPermission = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new SelectListItem
                           {
                               Text = dataRow.Field<string>("FormName"),
                               Value = dataRow.Field<long>("FormID").ToString()
                           }).ToList();

            return model;
        }


        public long AddFormPermissions(FormPermissionViewModel objmodel)
        {
            long retVal = 0;

            // Convert list to comma-separated string
            string formIdsCsv = string.Join(",", objmodel.SelectedFormIds); // e.g., "1,2,3"

            List<SqlParameter> sqlParameter = new List<SqlParameter>
    {
        new SqlParameter("@DepartmentID", objmodel.SelectedDepartment),
        new SqlParameter("@FormIDs", formIdsCsv),
        new SqlParameter("@LoggedUserID", objmodel.CreatedByID)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.ups_InsupdGroupFormPermission, sqlParameter);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var firstRow = dataSet.Tables[0].Rows[0];
                if (long.TryParse(firstRow[0].ToString(), out retVal))
                {
                    return retVal;
                }
            }
            return -1;
        }

        public List<FormPermissionViewModel> GetFormByDepartmentID(long DepartmentId)
        {
            List<FormPermissionViewModel> model = new List<FormPermissionViewModel>();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@DepartmentID", DepartmentId));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetFormByDepartmentID, sqlParameter);


            model = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new FormPermissionViewModel
                           {
                               FormName = dataRow.Field<string>("FormName"),
                               FormID = dataRow.Field<long>("FormID"),
                               SelectedDepartment = dataRow.Field<long>("FormID"),
                               GroupFormID = dataRow.Field<long?>("GroupFormID"),
                           }).ToList();

            return model;
        }
        public List<FormPermissionViewModel> GetUserFormByDepartmentID(FormPermissionVM obj)
        {
            List<FormPermissionViewModel> model = new List<FormPermissionViewModel>();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@DepartmentID", obj.DepartmentId));
            sqlParameter.Add(new SqlParameter("@EmployeeID", obj.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetUserFormByDepartmentID, sqlParameter);


            model = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new FormPermissionViewModel
                           {
                               FormName = dataRow.Field<string>("FormName"),
                               FormID = dataRow.Field<long>("FormID"),
                               SelectedDepartment = dataRow.Field<long>("FormID"),
                               GroupFormID = dataRow.Field<long?>("GroupFormID"),
                               IsSelected = dataRow.Field<int?>("IsSelected"),
                           }).ToList();

            return model;
        }


        public List<FormPermissionViewModel> GetUserFormPermissions(FormPermissionVM objmodel)
        {
            List<FormPermissionViewModel> model = new List<FormPermissionViewModel>();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@DepartmentID", objmodel.DepartmentId));
            sqlParameter.Add(new SqlParameter("@EmployeeID", objmodel.EmployeeID));
            sqlParameter.Add(new SqlParameter("@RoleID", objmodel.RoleID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetUserFormPermissions, sqlParameter);


            model = dataSet.Tables[0].AsEnumerable()
       .Select(dataRow =>
       {
           var modelItem = new FormPermissionViewModel();
           try { modelItem.FormName = dataRow.Field<string>("FormName"); } catch { Console.WriteLine("FormName error"); }
           try { modelItem.FormID = dataRow.Field<long>("FormID"); } catch { Console.WriteLine("FormID error"); }
           try { modelItem.FormLevel = dataRow.Field<long>("FormLevel"); } catch { Console.WriteLine("FormLevel error"); }
           try { modelItem.ParentId = dataRow.Field<long>("ParentId"); } catch { Console.WriteLine("ParentId error"); }
           try { modelItem.Status = dataRow.Field<bool>("Status"); } catch { Console.WriteLine("Status error"); }
           try { modelItem.IsFormPermissions = dataRow["IsFormPermission"] != DBNull.Value && Convert.ToBoolean(dataRow["IsFormPermission"]); }
           catch { Console.WriteLine("IsFormPermission error"); }

           return modelItem;
       }).ToList();

            return model;
        }




        public long AddUserFormPermissions(FormPermissionVM objmodel)
        {
            if (objmodel == null || objmodel.SelectedFormIds == null || !objmodel.SelectedFormIds.Any())
                return 0;

            string formIdsCsv = string.Join(",", objmodel.SelectedFormIds);

            // If formIdsCsv is null or empty, return without calling the SP
            if (string.IsNullOrWhiteSpace(formIdsCsv))
                return 0;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeID", objmodel.EmployeeID),
        new SqlParameter("@FormIDs", formIdsCsv)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(
                StoredProcedures.usp_InsertFormPermissions,
                sqlParameters
            );

            return (dataSet != null && dataSet.Tables.Count > 0) ? 1 : 0;
        }


        public EmployeePermissionVM CheckUserFormPermissionByEmployeeID(FormPermissionVM obj)
        {
            EmployeePermissionVM model = new EmployeePermissionVM();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", obj.EmployeeID));
            sqlParameter.Add(new SqlParameter("@FormId", obj.FormId));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_CheckUserFormPermissionByEmployeeID, sqlParameter);


            model = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new EmployeePermissionVM
                           {
                               HasPermission = dataRow.Field<int>("HasPermission"),

                           }).FirstOrDefault();
            return model;
        }


        #endregion Page Permission

        #region CompOff Attendance
        public List<CompOffAttendanceRequestModel> GetCompOffAttendanceList(CompOffAttendanceInputParams model)
        {
            List<CompOffAttendanceRequestModel> result = new List<CompOffAttendanceRequestModel>();

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeID", model.EmployeeID),
        new SqlParameter("@JobLocationTypeID", model.JobLocationTypeID),
                    new SqlParameter("@AttendanceStatus",model.AttendanceStatus)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetHolidayOrSundayWorkLog, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                result = dataSet.Tables[0].AsEnumerable().Select(row => new CompOffAttendanceRequestModel
                {
                    AttendanceId = row.Field<long>("AttendanceId"),
                    EmployeeId = row.Field<long>("EmployeeId"),
                    EmployeeName = row.Field<string>("EmployeeName"),
                    AttendanceDate = row.Field<DateTime?>("AttendanceDate"),
                    FirstLogDate = row.Field<DateTime?>("FirstLogDate"),
                    LastLogDate = row.Field<DateTime?>("LastLogDate"),
                    HoursWorked = row.Field<TimeSpan?>("HoursWorked"),
                    ManagerName = row.Field<string>("ManagerName"),
                    ManagerId = row.Field<long>("ManagerId"),
                    ApprovalStatus = row.Field<int>("ApprovalStatus"),
                    AttendanceStatus = row.Field<string>("AttendanceStatus"),
                    Comments = row.Field<string>("Remarks"),

                }).ToList();
            }

            return result;
        }

        public List<CompOffAttendanceRequestModel> GetApprovedCompOff(CompOffInputParams model)
        {
            var result = new List<CompOffAttendanceRequestModel>();

            int attStatus = Convert.ToInt32(model.AttendanceStatusId);
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
        {
            new SqlParameter("@ReportingUserID", model.UserId),
            new SqlParameter("@AttendanceStatusId",attStatus),
            new SqlParameter("@RoleId",model.RoleId)
        };

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetCompOffLeaveRequestsForManagers, sqlParameters);

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    result = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow => new CompOffAttendanceRequestModel
                        {
                            ID = dataRow.Field<long?>("RequestID") ?? 0,
                            // AttendanceId = dataRow.Field<long?>("AttendanceId") ,
                            UserId = dataRow.Field<long>("employeeId"),
                            AttendanceStatus = dataRow.Field<string>("AttendanceStatusName"),
                            AttendanceStatusId = dataRow.Field<int>("AttendanceStatusId"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date,
                            FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"),
                            LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"),
                            HoursWorked = dataRow.IsNull("HoursWorked") ? TimeSpan.Zero : dataRow.Field<TimeSpan>("HoursWorked"),
                            Comments = dataRow.Field<string>("Remarks"),
                            ManagerName = dataRow.Field<string>("ManagerName"),
                            EmployeeNumber = dataRow.Field<string>("EmployeeNumber"),
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetApprovedCompOffAttendance: " + ex.Message);
            }

            return result;
        }

        public Result AddUpdateCompOffAttendace(CompOffAttendanceRequestModel att)
        {
            Result model = new Result();

            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>();
                sqlParameter.Add(new SqlParameter("@RequestID", att.ID));
                sqlParameter.Add(new SqlParameter("@AttendanceId", att.AttendanceId));
                sqlParameter.Add(new SqlParameter("@EmployeeId", att.EmployeeId));
                sqlParameter.Add(new SqlParameter("@AttendanceStatusId", att.AttendanceStatusId));
                sqlParameter.Add(new SqlParameter("@WorkDate", att.WorkDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@FirstLogDate", att.FirstLogDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@LastLogDate", att.LastLogDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@HoursWorked", att.HoursWorked));
                sqlParameter.Add(new SqlParameter("@Remarks", att.Comments));
                sqlParameter.Add(new SqlParameter("@ModifiedBy", att.ModifiedBy));
                sqlParameter.Add(new SqlParameter("@CreatedBy", att.CreatedBy));
                sqlParameter.Add(new SqlParameter("@RoleId", att.RoleId));

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_SaveOrUpdateCompOffLeaveRequest, sqlParameter);
                if (dataSet.Tables[0].Columns.Contains("Result"))
                {
                    model = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow =>
                            new Result()
                            {
                                Message = dataRow.Field<string>("Result").ToString(),
                                PKNo = Convert.ToInt64(dataRow.Field<int>("PKNo"))
                            }
                        ).ToList().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AddUpdateCompOffAttendace: " + ex.Message);
            }
            return model;
        }




        public List<Joblcoations> GetJobLocationsByCompany(Joblcoations model)
        {
            List<Joblcoations> obj = new List<Joblcoations>();
            var sqlParameters = new List<SqlParameter>
             {
                 new SqlParameter("@CompanyID", model.CompanyId)
             };
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetJobLocationsByCompany, sqlParameters);
            if (dataSet.Tables.Count > 0)
            {
                obj = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Joblcoations
                {
                    JobLocationID = dataRow.Field<long>("JobLocationID"),
                    JobLocationName = dataRow.Field<string>("JobLocationName"),

                })
                .ToList();
            }
            return obj;
        }




        #endregion CompOff Attendance


        #region Exception Handling
        public void InsertException(ExceptionLogModel model)
        {


            List<SqlParameter> sqlParams = new List<SqlParameter>
        {
            new SqlParameter("@ControllerName", model.ControllerName ?? (object)DBNull.Value),
            new SqlParameter("@AreaName", model.AreaName ?? (object)DBNull.Value),
            new SqlParameter("@ActionName", model.ActionName ?? (object)DBNull.Value),
            new SqlParameter("@Url", model.Url ?? (object)DBNull.Value),
            new SqlParameter("@Message", model.Message ?? (object)DBNull.Value),
            new SqlParameter("@StackTrace", model.StackTrace ?? (object)DBNull.Value),
            new SqlParameter("@Source", model.Source ?? (object)DBNull.Value),
            new SqlParameter("@EmployeeId", model.EmployeeId ?? (object)DBNull.Value)
        };

            SqlParameterCollection outputParams = null;


            DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_InsertExceptionLog, sqlParams, ref outputParams);


        }
        #endregion Exception Handling


        public bool UploadRosterWeekOff(WeekOffUploadModelList model)
        {
            var data = CreateWeekOffDataTable(model.WeekOffList);
            UploadWeekOffRoster(data, model.CreatedBy);
            return true;
        }


        public DataTable CreateWeekOffDataTable(List<WeekOffUploadModel> models)
        {
            var dt = new DataTable();
            dt.Columns.Add("EmployeeNumber", typeof(string));
            dt.Columns.Add("WeekStartDate", typeof(DateTime));
            dt.Columns.Add("DayOff1", typeof(DateTime));
            dt.Columns.Add("DayOff2", typeof(DateTime));
            dt.Columns.Add("DayOff3", typeof(DateTime));
            dt.Columns.Add("ShiftId", typeof(long));

            foreach (var item in models)
            {
                var row = dt.NewRow();

                var employeeNumber = !string.IsNullOrWhiteSpace(item.EmployeeNumber)
                    ? item.EmployeeNumber
                    : item.EmployeeNumberWithOutAbbr ?? string.Empty;

                row["EmployeeNumber"] = employeeNumber;

                row["WeekStartDate"] = item.WeekStartDate ?? (object)DBNull.Value;
                row["DayOff1"] = item.DayOff1 ?? (object)DBNull.Value;
                row["DayOff2"] = item.DayOff2 ?? (object)DBNull.Value;
                row["DayOff3"] = item.DayOff3 ?? (object)DBNull.Value;
                row["ShiftId"] = item.ShiftTypeId ?? 0;
                dt.Rows.Add(row);
            }

            return dt;
        }


        public async Task UploadWeekOffRoster(DataTable dt, long createdBy)
        {
            try
            {
                string connStr = _configuration["ConnectionStrings:conStr"];

                using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("dbo.usp_BulkUploadWeekOffRoster", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                var tvpParam = cmd.Parameters.AddWithValue("@WeekOffs", dt);
                tvpParam.SqlDbType = SqlDbType.Structured;
                tvpParam.TypeName = "dbo.WeekOffUploadType";

                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {

            }
        }


        public List<WeekOffUploadModel> GetEmployeesWeekOffRoster(WeekOfInputParams model)
        {
            List<WeekOffUploadModel> result = new List<WeekOffUploadModel>();
            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>
        {
            new SqlParameter("@ReportingToID", model.EmployeeID),
            new SqlParameter("@RecordID", model.Id),
            new SqlParameter("@SearchTerm", string.IsNullOrEmpty(model.SearchTerm) ? DBNull.Value : (object)model.SearchTerm),
            new SqlParameter("@WeekStartDate", model.WeekStartDate),
            new SqlParameter("@Year", model.Year),
            new SqlParameter("@PageNumber", model.PageNumber),
            new SqlParameter("@PageSize", model.PageSize),
            new SqlParameter("@RoleId", model.RoleId),
         new SqlParameter("@SortCol", model.SortCol ?? "WeekStartDate"),
new SqlParameter("@SortDir", model.SortDir ?? "DESC")

        };
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetWeekOffRoster, sqlParameter);

                result = dataSet.Tables[0].AsEnumerable()
                                  .Select(dataRow =>
                                  new WeekOffUploadModel
                                  {
                                      Id = dataRow.Field<long?>("Id"),
                                      EmployeeNumber = dataRow.Field<string>("EmployeeNumber"),
                                      WeekStartDate = dataRow.Field<DateTime?>("WeekStartDate"),
                                      DayOff1 = dataRow.Field<DateTime?>("DayOff1"),
                                      DayOff2 = dataRow.Field<DateTime?>("DayOff2"),
                                      DayOff3 = dataRow.Field<DateTime?>("DayOff3"),
                                      ModifiedDate = dataRow.Field<DateTime?>("ModifiedDate"),
                                      ModifiedName = dataRow.Field<string?>("ModifiedName"),
                                      EmployeeName = dataRow.Field<string?>("EmployeeName"),
                                      TotalCount = dataRow.Field<int?>("TotalCount"),
                                      EmployeeId = dataRow.Field<long?>("EmployeeId"),
                                      EmployeeNumberWithOutAbbr = dataRow.Field<string?>(columnName: "EmployeeNumberWithOutAbbr")

                                  }).ToList();


            }
            catch (Exception ex)
            {

            }
            return result;
        }


        public string DeleteWeekOffRoster(WeekOffUploadDeleteModel model)
        {
            // Prepare parameters
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@Id", model.RecordId),
        new SqlParameter("@ModifiedBy", model.ModifiedBy)
    };

            // Call stored procedure and get dataset
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_DeleteWeekOffRoster, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[index: 0];
                int status = Convert.ToInt32(row["Status"]);
                string message = row["Message"].ToString();

                // Return in format like "1|Success message" or "0|Failure message"
                return message;
            }

            // Fallback if no result returned
            return "0|Delete failed: No response from stored procedure.";
        }

        public Dictionary<string, long> GetEmployeesHierarchyUnderManager(WeekOfInputParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeesHierarchyUnderManager, sqlParameter);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].AsEnumerable()
                            .ToDictionary(row => row.Field<string>("EmployeNumber").ToLower(),
                                          row => row.Field<long>("JobLocationID"));
            }

            return new Dictionary<string, long>();
        }
        public List<UpcomingWeekOffRoster> GetEmployeesWithoutUpcomingWeekOffRoster(UpcomingWeekOffRosterParams model)
        {
            List<UpcomingWeekOffRoster> upcomingWeekOffRosters = new List<UpcomingWeekOffRoster>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
    {
        new SqlParameter("@WeekStartDate", model.WeekStartDate)
    };

            DataSet dataSet = DataLayer.GetDataSetByStoredProcedure(
                StoredProcedures.usp_Get_EmployeesWithoutWeekOffRoster,
                sqlParameters
            );

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var hierarchyTable = dataSet.Tables[0];


                var flatList = hierarchyTable.AsEnumerable()
                    .Select(row => new UpcomingWeekOffRoster
                    {
                        EmployeeNumber = row.Field<string>("EmployeeNumber") ?? string.Empty,
                        EmployeeID = row.IsNull("EmployeeID") ? 0 : row.Field<long>("EmployeeID"),
                        EmployeeName = row.Field<string>("EmployeeName") ?? string.Empty,
                        ManagerName = row.Field<string>("ManagerName") ?? string.Empty,
                        ImmediateManagerName = row.Field<string>("ImmediateManagerName") ?? string.Empty,
                        ManagerEmailID = row.Field<string>("ManagerEmailID") ?? string.Empty,
                        ManagerID = row.IsNull("ManagerID") ? 0 : row.Field<long>("ManagerID"),
                        Level = row.IsNull("Level") ? 0 : row.Field<int>("Level"),
                        Path = row.Field<string>("Path") ?? string.Empty
                    })
                    .ToList();


                upcomingWeekOffRosters = flatList
                    .GroupBy(x => new { x.ManagerID, x.ManagerName, x.ManagerEmailID })
                    .Select(g => new UpcomingWeekOffRoster
                    {
                        ManagerID = g.Key.ManagerID,
                        ManagerName = g.Key.ManagerName,
                        ManagerEmailID = g.Key.ManagerEmailID,
                        Employees = g.Select(emp => new UpcomingWeekOffRoster
                        {
                            EmployeeID = emp.EmployeeID,
                            EmployeeNumber = emp.EmployeeNumber,
                            EmployeeName = emp.EmployeeName,
                            ImmediateManagerName = emp.ImmediateManagerName,
                            Level = emp.Level,
                            Path = emp.Path
                        }).ToList()
                    })
                    .ToList();
            }

            return upcomingWeekOffRosters;
        }
        public long GetShiftTypeId(string ShiftTypeName)

        {
            long ShiftTypeID = 0;

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@ShiftTypeName", ShiftTypeName),
    };

            DataSet dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetShiftTypeByName, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                object resultValue = dataSet.Tables[0].Rows[0]["ShiftTypeID"];
                if (resultValue != DBNull.Value)
                {
                    long.TryParse(resultValue.ToString(), out ShiftTypeID);
                }
            }

            return ShiftTypeID;
        }



        public List<EmployeeShiftModel> GetShiftTypeList(string employeeNumber)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeNumber", employeeNumber), // Note: Parameter name should match SP: @CompanyID
    };

            List<EmployeeShiftModel> model = new List<EmployeeShiftModel>();

            DataSet dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetShiftTypesByCompany, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                model = dataSet.Tables[0].AsEnumerable()
                          .Select(dataRow => new EmployeeShiftModel
                          {
                              ShiftName = dataRow.Field<string>("ShiftName"), // <-- updated to match SP column alias
                              ShiftTypeID = dataRow.Field<long>("ShiftTypeID"),
                              IsSelected = dataRow.Field<int>("IsSelected")
                          }).ToList();
            }

            return model;
        }

        public List<DateTime> GetLeaveWeekOffDates(LeaveWeekOfInputParams model)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@EmployeeID", model.EmployeeID),
        new SqlParameter("@FromDate", model.FromDate),
        new SqlParameter("@ToDate", model.ToDate)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(
                StoredProcedures.usp_GetWeekOffDatesForEmployee,
                sqlParameters
            );

            return dataSet?.Tables?[0]?
                .AsEnumerable()
                .Select(row => row.Field<DateTime?>("WeekOffDate"))
                .Where(d => d.HasValue)
                .Select(d => d.Value)
                .ToList() ?? new List<DateTime>();
        }

        #region Attendance Approval
        public AttendanceWithHolidaysVM GetTeamAttendanceForApproval(AttendanceInputParams model)
        {
            var attendanceList = new List<AttendanceViewModel>();
            int totalRecords = 0;

            var sqlParameters = new List<SqlParameter>
    {
       new SqlParameter("@ReportingToID", model.UserId),
new SqlParameter("@RoleID", model.RoleId),
new SqlParameter("@SortCol", model.SortCol ?? "WorkDate"),
new SqlParameter("@SortDir", model.SortDir ?? "DESC"),
new SqlParameter("@Searching", model.SearchTerm ?? (object)DBNull.Value),
new SqlParameter("@DisplayStart", model.DisplayStart),
new SqlParameter("@DisplayLength", model.DisplayLength)
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(
                StoredProcedures.usp_GetAttendanceForApprovalImmediateApprove,
                sqlParameters
            );

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                    .Select(row => new AttendanceViewModel
                    {
                        EmployeeId = row.Field<long?>("EmployeeID"),
                        ID = row.Field<long?>("ID"),
                        EmployeNumber = row.Field<string>("EmployeNumber"),
                        EmployeeName = row.Field<string>("EmployeeName"),
                        WorkDate = row.Field<DateTime>("WorkDate"),
                        Status = row.Field<string>("AttendanceStatus"),
                        Remarks = row.Field<string>("Remarks")
                    })
                    .ToList();
            }

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                int.TryParse(dataSet.Tables[0].Rows[0]["TotalRecords"]?.ToString(), out totalRecords);
            }

            return new AttendanceWithHolidaysVM
            {
                Attendances = attendanceList,
                TotalRecords = totalRecords
            };
        }


        public Result SaveOrUpdateAttendanceStatus(SaveTeamAttendanceStatus att)
        {
            Result model = new Result();
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
      {
          new SqlParameter("@ID", att.ID),
          new SqlParameter("@EmployeeId", att.EmployeeId),
          new SqlParameter("@ManagerID", att.ManagerId),
          new SqlParameter("@WorkDate", att.WorkDate),
          new SqlParameter("@Status", att.AttendanceStatus),
          new SqlParameter("@UserID ", att.UserID),
          new SqlParameter("@ApprovedByAdmin ", att.ApprovedByAdmin),
          new SqlParameter("@ApprovedStatus ", att.ApprovedStatus),
          new SqlParameter("@Remarks", string.IsNullOrEmpty(att.Remarks) ? (object)DBNull.Value : att.Remarks),
      };


                var dataSet = DataLayer.GetDataSetByStoredProcedure(
                    StoredProcedures.usp_SaveOrUpdateAttendanceStatus, sqlParameters
                );

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    model.PKNo = Convert.ToInt64(row["Id"]);
                    model.Message = row["Message"].ToString();
                    model.Data = new EmployeeData
                    {
                        EmployeeNumber = row.Table.Columns.Contains("EmployeeNumber") && row["EmployeeNumber"] != DBNull.Value
                                   ? row["EmployeeNumber"].ToString()
                                   : string.Empty,
                        EmployeeName = row.Table.Columns.Contains("EmployeeName") && row["EmployeeName"] != DBNull.Value
                                   ? row["EmployeeName"].ToString()
                                   : string.Empty,
                        IsManager = row.Table.Columns.Contains("IsManager") && row["IsManager"] != DBNull.Value
                                   && Convert.ToBoolean(row["IsManager"])
                    };

                }
                else
                {
                    model.PKNo = 0;
                    model.Message = "No result returned from database.";
                }
            }
            catch (Exception ex)
            {
                model.PKNo = -1;
                model.Message = "Error: " + ex.Message;
            }

            return model;
        }
        public Result SaveOrUpdateBulk(List<SaveAttendanceStatus> entries)
        {
            var result = new Result();

            try
            {
                var dt = ToAttendanceDataTable(entries);

                using (var conn = new SqlConnection(_configuration["ConnectionStrings:conStr"]))
                {
                    conn.Open();

                    // 1. Bulk copy to staging
                    using (var bulk = new SqlBulkCopy(conn))
                    {
                        bulk.DestinationTableName = "tbl_AttendanceDeviceLog_Staging";
                        bulk.WriteToServer(dt);
                    }

                    // 2. Merge from staging to main table
                    using (var cmd = new SqlCommand("usp_MergeAttendanceFromStaging", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }


                result.Message = $"Successfully saved attendance records.";
            }
            catch (Exception ex)
            {
                result.Message = "Bulk insert failed: " + ex.Message;
            }

            return result;
        }

        private DataTable ToAttendanceDataTable(List<SaveAttendanceStatus> entries)
        {
            var dt = new DataTable();
            dt.Columns.Add("UserId", typeof(string));
            dt.Columns.Add("AttendanceStatus", typeof(string));
            dt.Columns.Add("DialerTime", typeof(string));
            dt.Columns.Add("Remarks", typeof(string));
            dt.Columns.Add("WorkDate", typeof(DateTime));
            dt.Columns.Add("EmployeeId", typeof(long));
            dt.Columns.Add("UpdatedDate", typeof(DateTime));
            dt.Columns.Add("UpdatedByUserID", typeof(long));

            foreach (var e in entries)
            {
                dt.Rows.Add(
     e.UserId,
     e.AttendanceStatus ?? string.Empty,
     e.DialerTime ?? string.Empty,
     e.Remarks ?? string.Empty,
     e.WorkDate,
     e.EmployeeId,
     e.UpdatedDate,
     e.UpdatedByUserID
 );

            }

            return dt;
        }


        #endregion Attendance Approval

        #region LastLevelEmployeeDropdown

        public List<LastLevelEmployeeDropdown> GetLastLevelEmployeeDropdown(LastLevelEmployeeDropdownParams model)
        {
            List<LastLevelEmployeeDropdown> lastLevel = new List<LastLevelEmployeeDropdown>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID),
        new SqlParameter("@SearchTerm", string.IsNullOrEmpty(model.SearchTerm) ? DBNull.Value : model.SearchTerm)
    };


            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetFirstLevelEmployees, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    lastLevel.Add(new LastLevelEmployeeDropdown
                    {
                        EmployeeID = row.Field<long>("EmployeeID"),
                        EmployeeNumber = row.Field<string>("EmployeNumber"),
                        EmployeeName = row.Field<string>("EmployeeName"),
                        Gender = row.Field<int>("Gender"),
                        JobLocationID = row.Field<long>("JobLocationID"),


                    });
                }
            }

            return lastLevel;


        }

        public LastLevelEmployeeDropdown GetEmployeeForLeaveEdit(LastLevelEmployeeDropdownParams model)
        {
            LastLevelEmployeeDropdown employee = new LastLevelEmployeeDropdown();

            var sqlParameters = new List<SqlParameter>()
    {
        new SqlParameter("@EmployeeID", model.EmployeeID )
    };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetEmployeeForLeaveEdit, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var row = dataSet.Tables[0].Rows[0];
                employee = new LastLevelEmployeeDropdown
                {
                    EmployeeID = row.Field<long>("EmployeeID"),
                    EmployeeNumber = row.Field<string>("EmployeeNumber"),
                    EmployeeName = row.Field<string>("EmployeeName"),
                    Gender = row.Field<int>("Gender"),
                    JobLocationID = row.Field<long>("JobLocationID")
                };
            }

            return employee;
        }

        public List<Managers> GetManagerDropdown(Managers model)
        {
            List<Managers> obj = new List<Managers>();
            var sqlParameters = new List<SqlParameter>
            {
                 new SqlParameter("@ManagerID", model.ManagerID )
             };
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Manager_Hierarchy_List, sqlParameters);
            if (dataSet.Tables.Count > 0)
            {
                obj = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Managers
                {
                    EmployeeID = dataRow.Field<long>("EmployeeID"),
                    EmployeNumber = dataRow.Field<string>("EmployeNumber"),
                    EmployeeName = dataRow.Field<string>("EmployeeName")


                })
                .ToList();
            }
            return obj;
        }



        public List<HolidayCompanyList> GetCompanyHolidayList(HolidayInputparams model)
        {
            List<HolidayCompanyList> holidayList = new List<HolidayCompanyList>();

            var sqlParameters = new List<SqlParameter>
            {
              new SqlParameter("@CompanyID",model.CompanyID)
            };

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_HolidaysByCompany, sqlParameters);
            if (dataSet.Tables.Count > 0)
            {
                holidayList = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new HolidayCompanyList
                {
                    HolidayID = dataRow.Field<long>("HolidayID"),
                    Status = dataRow.Field<bool>("Status"),
                    FromDate = dataRow.Field<DateTime>("FromDate"),
                    ToDate = dataRow.Field<DateTime>("ToDate"),
                    JobLocationTypeID = dataRow.Field<long>("JobLocationTypeID")


                })
                .ToList();
            }

            return holidayList;

        }




        #endregion



        #region Payroll
        public List<SalaryDetails> GetEmployeesMonthlySalary(SalaryInputParams model)
        {
            List<SalaryDetails> result = new List<SalaryDetails>();
            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>
        {
            new SqlParameter("@CompanyID", model.CompanyID),
            new SqlParameter("@EmployeeID", model.EmployeeID ?? 0),
            new SqlParameter("@MonthlySalaryID", model.MonthlySalaryID),
            new SqlParameter("@Year", model.Year),
            new SqlParameter("@Month", model.Month),
            new SqlParameter("@SortCol", model.SortCol ?? "EmployeeID"),
            new SqlParameter("@SortDir", model.SortDir ?? "DESC"),
            new SqlParameter("@Searching", string.IsNullOrEmpty(model.Searching) ? DBNull.Value : (object)model.Searching),
            new SqlParameter("@DisplayStart", model.DisplayStart ?? 0),
            new SqlParameter("@DisplayLength", model.DisplayLength ?? 10)
        };

                var dataSet = DataLayer.GetDataSetByStoredProcedure("usp_GetMonthlySalary", sqlParameter);
                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    result = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow => new SalaryDetails
                        {
                            MonthlySalaryID = dataRow.Field<long>("MonthlySalaryID"),
                            EmployeeNumber = dataRow.Field<string>("EmployeNumber"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            EmployeeID = dataRow.Field<long>("EmployeeID"),
                            CompanyID = dataRow.Field<long>("CompanyID"),
                            PayrollTypeID = dataRow.Field<long>("PayrollTypeID"),
                            PayrollTypeName = dataRow.Field<string?>("PayrollTypeName"),
                            SalaryMonth = dataRow.Field<int>("SalaryMonth"),
                            SalaryYear = dataRow.Field<int>("SalaryYear"),
                            GrossSalary = dataRow.Field<decimal>("GrossSalary"),
                            BasicSalary = dataRow.Field<decimal>("BasicSalary"),
                            HRA = dataRow.Field<decimal>("HRA"),
                            ConveyanceAllowance = dataRow.Field<decimal>("ConveyanceAllowance"),
                            SpecialAllowance = dataRow.Field<decimal>("SpecialAllowance"),
                            PF = dataRow.Field<decimal>("PF"),
                            ESI = dataRow.Field<decimal>("ESI"),
                            LWF = dataRow.Field<decimal>("LWF"),
                            PTax = dataRow.Field<decimal>("PTax"),
                            TDS = dataRow.Field<decimal>("TDS"),
                            EmployerPF = dataRow.Field<decimal>("EmployerPF"),
                            EmployerESI = dataRow.Field<decimal>("EmployerESI"),
                            EmployerLWF = dataRow.Field<decimal>("EmployerLWF"),
                            Gratuity = dataRow.Field<decimal>("Gratuity"),
                            TotalEarnings = dataRow.Field<decimal>("TotalEarnings"),
                            TotalDeductions = dataRow.Field<decimal>("TotalDeductions"),
                            InHandSalary = dataRow.Field<decimal>("InHandSalary"),
                            CostToCompany = dataRow.Field<decimal>("CostToCompany"),
                            Status = dataRow.Field<string?>("Status"),
                            Remarks = dataRow.Field<string?>("Remarks"),
                            IsActive = dataRow.Field<bool>("IsActive"),
                        }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return result;
        }

        public Result AddUpdateEmployeeMonthlySalary(EmployeeMonthlySalaryModel salaryModel)
        {
            Result result = new Result();

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@MonthlySalaryID", salaryModel.MonthlySalaryID),
        new SqlParameter("@GrossSalary", salaryModel.GrossSalary),
        new SqlParameter("@BasicSalary", salaryModel.BasicSalary),
        new SqlParameter("@HRA", salaryModel.HRA),
        new SqlParameter("@ConveyanceAllowance", salaryModel.ConveyanceAllowance),
        new SqlParameter("@SpecialAllowance", salaryModel.SpecialAllowance),
        new SqlParameter("@PF", salaryModel.PF),
        new SqlParameter("@ESI", salaryModel.ESI),
        new SqlParameter("@LWF", salaryModel.LWF),
        new SqlParameter("@PTax", salaryModel.PTax),
        new SqlParameter("@TDS", salaryModel.TDS),
        new SqlParameter("@EmployerPF", salaryModel.EmployerPF),
        new SqlParameter("@EmployerESI", salaryModel.EmployerESI),
        new SqlParameter("@EmployerLWF", salaryModel.EmployerLWF),
        new SqlParameter("@Gratuity", salaryModel.Gratuity),
        new SqlParameter("@TotalEarnings", salaryModel.TotalEarnings),
        new SqlParameter("@TotalDeductions", salaryModel.TotalDeductions),
        new SqlParameter("@InHandSalary", salaryModel.InHandSalary),
        new SqlParameter("@CostToCompany", salaryModel.CostToCompany),
        new SqlParameter("@Status", salaryModel.Status ?? (object)DBNull.Value),
        new SqlParameter("@Remarks", salaryModel.Remarks ?? (object)DBNull.Value),
        new SqlParameter("@UpdatedByUserID", salaryModel.UpdatedByUserID)
    };


            var dataSet = DataLayer.GetDataSetByStoredProcedure(
                StoredProcedures.usp_UpdateEmployeeMonthlySalary,
                sqlParameters
            );
            if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                result = dataSet.Tables[0].AsEnumerable()
                         .Select(row => new Result
                         {
                             Message = row.Field<string>("Result")
                         })
                         .FirstOrDefault();
            }

            return result;
        }






        #endregion Payroll


        #region Logs
        public void LogChangeAsJson(
     long userId,
     string module,
     string table,
     long primaryKey,
     string columnName,
     string oldValue,
     string newValue,
     string mode,
     string newTableName,
     string sectionName)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@UserID", userId),
        new SqlParameter("@ModuleName", module),
        new SqlParameter("@TableName", table),
        new SqlParameter("@PrimaryKey", primaryKey),
        new SqlParameter("@ColumnName", (object)columnName ?? DBNull.Value),
        new SqlParameter("@OldValue", (object)oldValue ?? DBNull.Value),
        new SqlParameter("@NewValue", (object)newValue ?? DBNull.Value),
        new SqlParameter("@EditMode", mode),
        new SqlParameter("@NewTableName", newTableName),
        new SqlParameter("@SectionName", sectionName)
    };

           
            DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_InsertAuditLog, sqlParameters);
        }

        public void TrackLogAudit(
            DataTable oldData,
            DataTable newData,
            string editMode,
            long userId,
            string moduleName,
            string tableName,
            long recordId,
            string logTable,
            string description)
        {
            if (newData == null || newData.Rows.Count == 0)
                return;

            DataRow newRow = newData.Rows[0];
            DataRow oldRow = (oldData != null && oldData.Rows.Count > 0) ? oldData.Rows[0] : null;

            var changeLog = new List<Dictionary<string, object>>();
            foreach (DataColumn col in newData.Columns)
            {
                string colName = col.ColumnName;
                string newVal = newRow[colName] == DBNull.Value ? null : Convert.ToString(newRow[colName]);
                string oldVal = oldRow == null ? null : (oldRow[colName] == DBNull.Value ? null : Convert.ToString(oldRow[colName]));

                if (editMode == "Add" || oldVal != newVal || (colName == "ModifiedByName" || colName == "ModifiedByID"))
                {
                    changeLog.Add(new Dictionary<string, object>
            {
                { "ColumnName", colName },
                { "OldValue", oldVal },
                { "NewValue", newVal }
            });
                }
            }

            if (changeLog.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(changeLog);
                LogChangeAsJson(userId, moduleName, tableName, recordId, null, null, jsonData, editMode, logTable, description);
            }
        }

        public DataTable GetLeaveSummaryByID(long? leaveSummaryID)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@LeaveSummaryID", leaveSummaryID)
    };

            DataSet dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetLeaveSummaryByID, sqlParameters);

            if (dataSet != null && dataSet.Tables.Count > 0)
                return dataSet.Tables[0];

            return new DataTable();
        }
        #endregion Logs

        #region LeaveLogs


        #endregion LeaveLogs
    }




}
