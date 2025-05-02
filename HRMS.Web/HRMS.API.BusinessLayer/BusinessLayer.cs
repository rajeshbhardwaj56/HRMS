using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer.ITF;
using HRMS.Models.Common;
using HRMS.Models.Employee;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel.Design;
using HRMS.Models.Template;
using HRMS.Models.Company;
using HRMS.Models.Leave;
using HRMS.Models.LeavePolicy;
using HRMS.Models.MyInfo;
using HRMS.Models;
using HRMS.Models.AttendenceList;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using HRMS.Models.DashBoard;
using Microsoft.AspNetCore.Mvc;
using HRMS.Models.User;
using Newtonsoft.Json;
using HRMS.Models.ShiftType;
using HRMS.Models.ImportFromExcel;
using HRMS.Models.WhatsHappeningModel;

namespace HRMS.API.BusinessLayer
{
    public class BusinessLayer : IBusinessLayer
    {
        IDataLayer DataLayer { get; set; }
        IAttandanceDataLayer AttandanceDataLayer { get; set; }
        public BusinessLayer(IConfiguration configuration, IDataLayer dataLayer, IAttandanceDataLayer attandanceDataLayer)
        {
            DataLayer = dataLayer;
            AttandanceDataLayer = attandanceDataLayer;
            DataLayer._configuration = configuration;
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
                                   IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired"),
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
                                      IsResetPasswordRequired = dataRow.Field<bool>("IsResetPasswordRequired"),
                                      IsActive = dataRow.Field<bool>("IsActive"),
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
            new SqlParameter("@SortCol", "EmployeeID"), // 🔹 Default Sorting
            new SqlParameter("@SortDir", "ASC"),
            new SqlParameter("@Searching", string.IsNullOrEmpty(model.Searching) ? DBNull.Value : (object)model.Searching),
            new SqlParameter("@DisplayStart", model.DisplayStart),
            new SqlParameter("@DisplayLength", model.DisplayLength)
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
                                      CarryForword = dataRow.Field<long?>("CarryForword"),
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
                                      OfficialEmailID = dataRow.Field<string>("OfficialEmail"),
                                      ManagerName = dataRow.Field<string>("ManagerName"),
                                      Shift = dataRow.Field<string>("Shift"),
                                      PayrollTypeName = dataRow.Field<string>("PayrollType"),
                                      PanCardImage = dataRow.Field<string>("PanCardImage"),
                                      AadhaarCardImage = dataRow.Field<string>("AadhaarCardImage"),


                                  }).ToList();

                if (model.EmployeeID > 0)
                {
                    result.employeeModel = result.Employees.FirstOrDefault();

                    //////////////////////// FamilyDetails
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


                    //////////////////////// References
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

                    //////////////////////// References
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
                    RootName = string.Format(Constants.RootUrlFormat, Constants.ManageAdmin, Roles.Admin.ToString());
                    break;
                case (int)Roles.HR:
                    RootName = string.Format(Constants.RootUrlFormat, Constants.ManageHR, Roles.HR.ToString());
                    break;
                case (int)Roles.Employee:
                    RootName = string.Format(Constants.RootUrlFormat, Constants.ManageEmployee, Roles.Employee.ToString());
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
        public Results GetAllCompanies(EmployeeInputParams model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Companies, sqlParameter);
            result.companyModel = dataSet.Tables[0].AsEnumerable()
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

                              }).ToList().FirstOrDefault();

            //if (model.CompanyID > 0)
            //{
            //    result.companyModel = result.Companies.FirstOrDefault();
            //}

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
            sqlParameter.Add(new SqlParameter("@ESIRegistrationDate", employmentDetails.ESIRegistrationDate));
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
                                ESIRegistrationDate = dataRow.Field<DateTime?>("ESIRegistrationDate")
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
                                ESIRegistrationDate = dataRow.Field<DateTime?>("ESIRegistrationDate")
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


        public Result AddUpdateEmploymentBankDetails(EmploymentBankDetail employmentBankDetails)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@BankDetailID", employmentBankDetails.BankDetailID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", employmentBankDetails.EmployeeID));
            sqlParameter.Add(new SqlParameter("@BankAccountNumber", employmentBankDetails.BankAccountNumber));
            sqlParameter.Add(new SqlParameter("@IFSCCode", employmentBankDetails.IFSCCode));
            sqlParameter.Add(new SqlParameter("@BankName", employmentBankDetails.BankName));
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
                                PreviousExperience = dataRow.Field<int?>("PreviousExperience"),
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
                                NoticeServed = dataRow.Field<int?>("NoticeServed")

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
                              }).ToList();

            if (model.HolidayID > 0)
            {
                result.holidayModel = result.Holiday.FirstOrDefault();
            }

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
                                   Status = dataRow.Field<bool>("Status"),
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
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeaveForApprovals, sqlParameter);
            result.leavesSummary = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeaveSummaryModel
                              {
                                  EmployeeNumber = dataRow.Field<string>("EmployeNumber"),
                                  EmployeeName = dataRow.Field<string>("EmployeeName"),
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
                                  OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                  ManagerOfficialEmailID = dataRow.Field<string>("ManagerOfficialEmailID"),
                                  EmployeeFirstName = dataRow.Field<string>("EmployeeFirstName"),
                                  ManagerFirstName = dataRow.Field<string>("ManagerFirstName"),
                                  ChildDOB = dataRow.Field<DateTime>("ChildDOB"),
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
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@LeaveSummaryID", leaveSummaryModel.LeaveSummaryID));
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
            sqlParameter.Add(new SqlParameter("@UploadCertificate", leaveSummaryModel.UploadCertificate));
            sqlParameter.Add(new SqlParameter("@ExpectedDeliveryDate", leaveSummaryModel.ExpectedDeliveryDate));
            sqlParameter.Add(new SqlParameter("@ChildDOB", leaveSummaryModel.ChildDOB));
            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_LeaveSummary, sqlParameter, ref pOutputParams);

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
                                  ExpectedDeliveryDate = dataRow.Field<DateTime>("ExpectedDeliveryDate"),
                                  NoOfDays = dataRow.Field<decimal>("NoOfDays"),
                                  IsActive = dataRow.Field<bool>("IsActive"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  ChildDOB = dataRow.Field<DateTime>("ChildDOB"),
                              }).ToList();

            result.leaveTypes = GetLeaveTypes(model).leaveTypes;
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
                                      CarryForword = dataRow.Field<long>("CarryForword"),
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

                dashBoardModel.AttendanceModel = dataSet.Tables[2].AsEnumerable()
                              .Select(dataRow => new AttendanceModel
                              {
                                  Day = dataRow.Field<DateTime>("Day"),
                                  Present = dataRow.Field<int>("Present"),
                                  Absent = dataRow.Field<int>("Absent"),

                              }).ToList();

                dashBoardModel.EmployeeDetails = dataSet.Tables[3].AsEnumerable()
     .Select(dataRow => new EmployeeDetails
     {
         EmployeeId = dataRow.Field<long>("EmployeeId"),
         FirstName = dataRow.Field<string>("EmployeeFirstName"),
         LastName = dataRow.Field<string>("EmployeeLastName"),
         DOB = dataRow.Field<DateTime?>("EmployeeDOB"),  // Nullable DateTime
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

                if (model.RoleID == (int)Roles.SuperAdmin)
                {
                    var CompanyDetails = dataSet.Tables[5].AsEnumerable()
                           .Select(dataRow => new DashBoardModel
                           {
                               CountsOfCompanies = dataRow.Field<int>("CountsOfCompanies"),
                           }).ToList().FirstOrDefault();
                    dashBoardModel.CountsOfCompanies = CompanyDetails.CountsOfCompanies;

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
            return dashBoardModel;
        }
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

        public Results GetAllEmployees()
        {
            Results model = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Employees, sqlParameter);

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
                model.Employee = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("EmployeeName"),
                                   Value = dataRow.Field<long>("EmployeeID").ToString()
                               }).ToList();
            }
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
                                  CompanyID = dataRow.Field<long>("CompanyID")
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

        public MyInfoResults GetMyInfo(MyInfoInputParams model)
        {
            MyInfoResults myInfoResults = new MyInfoResults();
            HolidayInputParams holidayobj = new HolidayInputParams();
            myInfoResults.leaveResults = GetlLeavesSummary(model);
            EmployeeInputParams employeeInputParams = new EmployeeInputParams();
            employeeInputParams.EmployeeID = model.EmployeeID;
            employeeInputParams.CompanyID = model.CompanyID;
            holidayobj.CompanyID = model.CompanyID;

            var data = GetAllEmployees(employeeInputParams);
            myInfoResults.employeeModel = data.employeeModel;

            var holiday = GetAllHolidayList(holidayobj);
            myInfoResults.HolidayModel = holiday.Holiday;

            myInfoResults.employmentHistory = data.employeeModel.EmploymentHistory;
            LeavePolicyModel models = new LeavePolicyModel();
            models.CompanyID = model.CompanyID;
            models.LeavePolicyID = data.employeeModel.LeavePolicyID ?? 0;
            myInfoResults.LeavePolicyDetails = GetSelectLeavePolicies(models);
            myInfoResults.employmentDetail = GetEmploymentDetailsByEmployee(new EmploymentDetailInputParams() { EmployeeID = model.EmployeeID, CompanyID = model.CompanyID });
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
                            Message = dataRow.Field<string>("Result").ToString()
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


        public List<EmployeeDetails> GetEmployeeListByManagerID(EmployeeInputParams model)
        {
            List<EmployeeDetails> dashBoardModel = new List<EmployeeDetails>();

            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>();
                sqlParameter.Add(new SqlParameter("@ReportingUserID", model.EmployeeID));
                sqlParameter.Add(new SqlParameter("@RoleID", model.RoleID));
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetEmployeeListByManagerID, sqlParameter);


                dashBoardModel = dataSet.Tables[0].AsEnumerable()
                                  .Select(dataRow => new EmployeeDetails
                                  {
                                      EmployeeId = dataRow.Field<long>("EmployeeId"),
                                      FirstName = dataRow.Field<string>("EmployeeFirstName"),
                                      MiddelName = dataRow.Field<string>("EmployeeMiddelName"),
                                      LastName = dataRow.Field<string>("EmployeeLastName"),
                                      DOB = dataRow.Field<DateTime>("EmployeeDOB"),
                                      EmployeePhoto = dataRow.Field<string>("EmployeePhoto"),
                                      DepartmentName = dataRow.Field<string>("DepartmentName"),
                                      DesignationName = dataRow.Field<string>("DesignationName"),
                                  }).ToList();

            }
            catch(Exception ex)
            {

            }
            return dashBoardModel;
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
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_DistinctPolicyCategoryDetails, sqlParameter);

            result = dataSet.Tables[0].AsEnumerable()
                           .Select(dataRow => new LeavePolicyDetailsModel
                           {
                               Id = dataRow.Field<long>("Id"),
                               CompanyID = dataRow.Field<long>("CompanyID"),
                               Title = dataRow.Field<string>("Title"),
                               PolicyCategoryName = dataRow.Field<string>("PolicyCategoryName"),
                               PolicyDocument = dataRow.Field<string>("PolicyDocument"),
                               PolicyCategoryId = dataRow.Field<long>("PolicyCategoryId"),
                           }).ToList();
            return result;
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
            var dataSet = DataLayer.GetDataSetByStoredProcedure("sp_GetAttendanceAuditLog", sqlParameters);

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


        public AttendanceInputParams GetAttendance(AttendanceInputParams model)
        {
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
        {
            new SqlParameter("@Year", model.Year),
            new SqlParameter("@Month", model.Month),
            new SqlParameter("@Day", model.Day),
            new SqlParameter("@UserId", model.UserId),
            new SqlParameter("@IsManual", false),
            new SqlParameter("@AttendanceStatus", AttendanceStatus.Approved.ToString())
        };

                var dataSet = AttandanceDataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_GetDeviceLogsByMonth, sqlParameters);

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    string status = row["Status"].ToString();
                    string message = row["Message"].ToString();

                    model.Status = status;
                    model.Message = message;
                    model.IsSuccess = status.Equals("Success", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    model.Message = "No response from the stored procedure.";
                    model.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                model.Message = "Error: " + ex.Message;
                model.IsSuccess = false;
            }

            return model;
        }

        public AttendanceWithHolidays GetAttendanceForCalendar(AttendanceInputParams model)
        {
            List<Attendance> attendanceList = new List<Attendance>();
            List<HolidayModel> holidayList = new List<HolidayModel>();
            List<LeaveSummaryModel> leaveList = new List<LeaveSummaryModel>();
            // Define SQL parameters for the stored procedure
            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@Year", model.Year),
                new SqlParameter("@Month", model.Month),
                new SqlParameter("@UserId", model.UserId)
            };
            // Call the stored procedure and get the result as a DataSet
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.sp_GetAttendanceDeviceLog, sqlParameters);
            // Parse the attendance data from the first table
            if (dataSet.Tables.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                    .Select(dataRow => new Attendance
                    {
                        UserId = dataRow.Field<string>("UserId"),
                        EmployeeName = dataRow.Field<string>("EmployeeName"),
                        WorkDate = dataRow.Field<DateTime>("WorkDate").Date,
                        FirstLogDate = dataRow.Field<DateTime>("FirstLogDate"),
                        LastLogDate = dataRow.Field<DateTime>("LastLogDate"),
                        HoursWorked = dataRow.Field<int>("HoursWorked")
                    }).ToList();
            }
            // Parse the holiday data from the second table
            if (dataSet.Tables.Count > 1)
            {
                holidayList = dataSet.Tables[1].AsEnumerable()
                    .Select(dataRow => new HolidayModel
                    {
                        HolidayID = dataRow.Field<long>("HolidayID"),
                        CompanyID = dataRow.Field<long>("CompanyID"),
                        HolidayName = dataRow.Field<string>("HolidayName"),
                        FromDate = dataRow.Field<DateTime>("FromDate"),
                        ToDate = dataRow.Field<DateTime>("ToDate"),
                        Description = dataRow.Field<string>("Description"),
                        Status = dataRow.Field<bool>("Status"),
                        IsDeleted = dataRow.Field<bool>("IsDeleted"),
                        CreatedDate = dataRow.Field<DateTime>("CreatedDate"),
                        UpdatedDate = dataRow.Field<DateTime>("UpdatedDate"),
                        CreatedBy = dataRow.Field<long>("CreatedBy"),
                        UpdatedBy = dataRow.Field<long>("UpdatedBy")
                    }).ToList();
            }
            if (dataSet.Tables.Count > 2)
            {
                leaveList = dataSet.Tables[2].AsEnumerable()
                    .Select(dataRow => new LeaveSummaryModel
                    {
                        LeaveSummaryID = dataRow.Field<long>("LeaveSummaryID"),
                        EmployeeID = dataRow.Field<long>("EmployeeID"),
                        LeaveStatusID = dataRow.Field<long>("LeaveStatusID"),
                        Reason = dataRow.Field<string>("Reason"),
                        RequestDate = dataRow.Field<DateTime>("RequestDate"),
                        StartDate = dataRow.Field<DateTime>("StartDate"),
                        EndDate = dataRow.Field<DateTime>("EndDate"),
                        LeaveTypeID = dataRow.Field<long>("LeaveTypeID"),
                        LeaveDurationTypeID = dataRow.Field<long>("LeaveDurationTypeID"),
                        NoOfDays = dataRow.Field<decimal>("NoOfDays"),
                        LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                        ApproveRejectComment = dataRow.Field<string>("ApproveRejectComment"),
                        ExpectedDeliveryDate = dataRow.Field<DateTime>("ExpectedDeliveryDate"),
                    }).ToList();
            }
            var result = new AttendanceWithHolidays
            {
                Attendances = attendanceList,
                Holidays = holidayList,
                Leaves = leaveList
            };
            return result;
        }

        public AttendanceWithHolidays GetTeamAttendanceForCalendar(AttendanceInputParams model)
        {
            List<Attendance> attendanceList = new List<Attendance>();
            List<HolidayModel> holidayList = new List<HolidayModel>();
            List<LeaveSummaryModel> leaveList = new List<LeaveSummaryModel>();
            // Define the SQL parameters
            List<SqlParameter> sqlParameters = new List<SqlParameter>  {
            new SqlParameter("@Year", model.Year),
            new SqlParameter("@Month", model.Month),
            new SqlParameter("@UserId", model.UserId)
            };
            // Get the dataset from the stored procedure
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.sp_GetTeamAttendanceDeviceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Attendance
                {
                    ID = dataRow.Field<long?>("ID") ?? 0,
                    UserId = dataRow.Field<string>("UserId"),
                    EmployeeName = dataRow.Field<string>("EmployeeName"),
                    WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date, // Handle WorkDate as nullable
                    FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"), // Handle FirstLogDate as nullable
                    LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"), // Handle LastLogDate as nullable
                    HoursWorked = dataRow.IsNull("HoursWorked") ? 0 : dataRow.Field<int>("HoursWorked") // Handle null HoursWorked (if needed)
                })
                .ToList();
            }
            // Parse the holiday data from the second table
            if (dataSet.Tables.Count > 1)
            {
                holidayList = dataSet.Tables[1].AsEnumerable()
                    .Select(dataRow => new HolidayModel
                    {
                        HolidayID = dataRow.Field<long>("HolidayID"),
                        CompanyID = dataRow.Field<long>("CompanyID"),
                        HolidayName = dataRow.Field<string>("HolidayName"),
                        FromDate = dataRow.Field<DateTime>("FromDate"),
                        ToDate = dataRow.Field<DateTime>("ToDate"),
                        Description = dataRow.Field<string>("Description"),
                        Status = dataRow.Field<bool>("Status"),
                        IsDeleted = dataRow.Field<bool>("IsDeleted"),
                        CreatedDate = dataRow.Field<DateTime>("CreatedDate"),
                        UpdatedDate = dataRow.Field<DateTime>("UpdatedDate"),
                        CreatedBy = dataRow.Field<long>("CreatedBy"),
                        UpdatedBy = dataRow.Field<long>("UpdatedBy")
                    }).ToList();
            }
            if (dataSet.Tables.Count > 2)
            {
                leaveList = dataSet.Tables[2].AsEnumerable()
                    .Select(dataRow => new LeaveSummaryModel
                    {
                        LeaveSummaryID = dataRow.Field<long>("LeaveSummaryID"),
                        EmployeeID = dataRow.Field<long>("EmployeeID"),
                        LeaveStatusID = dataRow.Field<long>("LeaveStatusID"),
                        Reason = dataRow.Field<string>("Reason"),
                        RequestDate = dataRow.Field<DateTime>("RequestDate"),
                        StartDate = dataRow.Field<DateTime>("StartDate"),
                        EndDate = dataRow.Field<DateTime>("EndDate"),
                        LeaveTypeID = dataRow.Field<long>("LeaveTypeID"),
                        LeaveDurationTypeID = dataRow.Field<long>("LeaveDurationTypeID"),
                        NoOfDays = dataRow.Field<decimal>("NoOfDays"),
                        LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                        ApproveRejectComment = dataRow.Field<string>("ApproveRejectComment"),
                        ExpectedDeliveryDate = dataRow.Field<DateTime>("ExpectedDeliveryDate"),
                    }).ToList();
            }
            var result = new AttendanceWithHolidays
            {
                Attendances = attendanceList,
                Holidays = holidayList,
                Leaves = leaveList
            };
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
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.sp_GetMyAttendanceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Attendance
                {
                    ID = dataRow.Field<long?>("ID") ?? 0,
                    UserId = dataRow.Field<string>("EmployeeId"),
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

        public Result AddUpdateAttendace(Attendance att)
        {
            Result model = new Result();

            try
            {
                List<SqlParameter> sqlParameter = new List<SqlParameter>();

                sqlParameter.Add(new SqlParameter("@Id", att.ID));
                sqlParameter.Add(new SqlParameter("@EmployeeId", att.UserId));
                sqlParameter.Add(new SqlParameter("@AttendanceStatusId", att.AttendanceStatusId));
                sqlParameter.Add(new SqlParameter("@AttendanceStatus", att.AttendanceStatus));
                sqlParameter.Add(new SqlParameter("@WorkDate", att.WorkDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@FirstLogDate", att.FirstLogDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@LastLogDate", att.LastLogDate.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                sqlParameter.Add(new SqlParameter("@Comments", att.Comments));
                sqlParameter.Add(new SqlParameter("@ModifiedBy", att.ModifiedBy));
                sqlParameter.Add(new SqlParameter("@ModifiedDate", att.ModifiedDate));
                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.sp_SaveAttendanceManualLog, sqlParameter);
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
                                     UserId = dataRow.Field<string>("EmployeeId"),
                                     AttendanceStatus = dataRow.Field<string>("AttendanceStatus"),
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
            sqlParameter.Add(new SqlParameter("@ID", model.ID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.sp_DeleteAttendanceDeviceLog, sqlParameter);
            return "Deleted successfully";

        }


        public MyAttendanceList GetAttendanceForApproval(AttendanceInputParams model)
        {
            List<Attendance> attendanceList = new List<Attendance>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>  {
            new SqlParameter("@ReportingID", model.UserId)
            };
            // Get the dataset from the stored procedure
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.sp_GetMyAttendanceLog, sqlParameters);

            if (dataSet.Tables.Count > 0)
            {
                attendanceList = dataSet.Tables[0].AsEnumerable()
                .Select(dataRow => new Attendance
                {
                    ID = dataRow.Field<long?>("ID") ?? 0,
                    UserId = dataRow.Field<string>("UserId"),
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


        public MyAttendanceList GetApprovedAttendance(AttendanceInputParams model)
        {
            var result = new MyAttendanceList
            {
                Attendances = new List<Attendance>()
            };
            int attStatus = Convert.ToInt32(model.Status);
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>
        {
            new SqlParameter("@ReportingUserID", model.UserId),
            new SqlParameter("@AttendanceStatusId",attStatus)
        };

                var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_ApprovedAttendance, sqlParameters);

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    result.Attendances = dataSet.Tables[0].AsEnumerable()
                        .Select(dataRow => new Attendance
                        {
                            ID = dataRow.Field<long?>("ID") ?? 0,
                            UserId = dataRow.Field<string>("employeeId"),
                            AttendanceStatus = dataRow.Field<string>("AttendanceStatus"),
                            EmployeeName = dataRow.Field<string>("EmployeeName"),
                            WorkDate = dataRow.IsNull("WorkDate") ? (DateTime?)null : dataRow.Field<DateTime>("WorkDate").Date,
                            FirstLogDate = dataRow.IsNull("FirstLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("FirstLogDate"),
                            LastLogDate = dataRow.IsNull("LastLogDate") ? (DateTime?)null : dataRow.Field<DateTime>("LastLogDate"),
                            HoursWorked = dataRow.IsNull("HoursWorked") ? 0 : dataRow.Field<int>("HoursWorked")
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

        public Dictionary<string, long> GetCompaniesDictionary()
        {
            EmployeeInputParams model = new EmployeeInputParams();
            model.CompanyID = 0;
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_Companies, sqlParameter);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].AsEnumerable()
                            .ToDictionary(row => row.Field<string>("Name").ToLower(), // Convert Name to lowercase
                                          row => row.Field<long>("CompanyID"));
            }

            return new Dictionary<string, long>(); // Return empty dictionary if no data
        }
        public Dictionary<string, long> GetSubDepartmentDictionary(EmployeeInputParams model)
        {
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_CompanySubDepartments, sqlParameter);

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                return dataSet.Tables[0].AsEnumerable()
                            .ToDictionary(row => row.Field<string>("Name").ToLower(), // Convert Name to lowercase
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
                    .ToDictionary(row => row.Field<string>("Name"), row => row.Field<long>("EmployeeID"));

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
            // Add SQL parameters
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
            long SuperAdminId =Convert.ToInt64(employeeModel.InsertedBy);
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
            if (employeeModel.JoiningDate != null )
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



    }
}
