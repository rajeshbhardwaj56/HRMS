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

namespace HRMS.API.BusinessLayer
{
    public class BusinessLayer : IBusinessLayer
    {
        IDataLayer DataLayer { get; set; }
        public BusinessLayer(IConfiguration configuration, IDataLayer dataLayer)
        {
            DataLayer = dataLayer;
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
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
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
                                  LeavePolicyID = dataRow.Field<long>("LeavePolicyID"),
                                  InsertedDate = dataRow.Field<DateTime>("InsertedDate"),
                                  CarryForword = dataRow.Field<long>("CarryForword"),
                                  Gender = dataRow.Field<int>("Gender"),
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
                                  ReportingToID = dataRow.Field<long>("ReportingToID"),
                                  DesignationName = dataRow.Field<string>("DesignationName"),
                                  EmployeeTypeName = dataRow.Field<string>("EmployeeTypeName"),
                                  DepartmentName = dataRow.Field<string>("DepartmentName"),
                                  JobLocationName = dataRow.Field<string>("JobLocationName"),
                                  ReportingToName = dataRow.Field<string>("ReportingToName")

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

            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Employee, sqlParameter, ref pOutputParams);

            if (dataSet.Tables[0].Columns.Contains("Result"))
            {
                model = dataSet.Tables[0].AsEnumerable()
                   .Select(dataRow =>
                        new Result()
                        {
                            Message = dataRow.Field<string>("Result").ToString(),
                            PKNo = Convert.ToInt64(pOutputParams["@RetEmployeeID"].Value)
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
            sqlParameter.Add(new SqlParameter("@DepartmentID", employmentDetails.DepartmentID));
            sqlParameter.Add(new SqlParameter("@JobLocationID", employmentDetails.JobLocationID));
            sqlParameter.Add(new SqlParameter("@OfficialEmailID", employmentDetails.OfficialEmailID));
            sqlParameter.Add(new SqlParameter("@OfficialContactNo", employmentDetails.OfficialContactNo));
            sqlParameter.Add(new SqlParameter("@JoiningDate", employmentDetails.JoiningDate));
            sqlParameter.Add(new SqlParameter("@JobSeprationDate", employmentDetails.JobSeprationDate));
            sqlParameter.Add(new SqlParameter("@ReportingToID", employmentDetails.ReportingToID));
            sqlParameter.Add(new SqlParameter("@IsActive", employmentDetails.IsActive));
            sqlParameter.Add(new SqlParameter("@IsDeleted", employmentDetails.IsDeleted));
            sqlParameter.Add(new SqlParameter("@UserID", employmentDetails.UserID));
            sqlParameter.Add(new SqlParameter("@LeavePolicyID", employmentDetails.LeavePolicyID));

            SqlParameterCollection pOutputParams = null;

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_EmploymentDetails, sqlParameter, ref pOutputParams);
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
            EmploymentDetail employmentDetail = dataSet.Tables[5].AsEnumerable()
                            .Select(dataRow => new EmploymentDetail()
                            {
                                EmployeeID = dataRow.Field<long>("EmployeeID"),
                                EmployeNumber = dataRow.Field<string>("EmployeNumber"),
                                EmployeeTypeID = dataRow.Field<long>("EmployeeTypeID"),
                                EmploymentDetailID = dataRow.Field<long>("EmploymentDetailID"),
                                DesignationID = dataRow.Field<long>("DesignationID"),
                                DepartmentID = dataRow.Field<long>("DepartmentID"),
                                JobLocationID = dataRow.Field<long>("JobLocationID"),
                                ReportingToID = dataRow.Field<long>("ReportingToID"),
                                OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                OfficialContactNo = dataRow.Field<string>("OfficialContactNo"),
                                DesignationName = dataRow.Field<string>("Name"),
                                DepartmentName = dataRow.Field<string>("DepartmentName"),
                                JoiningDate = dataRow.Field<DateTime?>("JoiningDate"),
                                JobSeprationDate = dataRow.Field<DateTime?>("JobSeprationDate"),
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

            employmentDetail.Departments = dataSet.Tables[2].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();


            employmentDetail.Designations = dataSet.Tables[3].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Text = dataRow.Field<string>("Name"),
                                 Value = dataRow.Field<long>("ID").ToString()
                             }).ToList();


            employmentDetail.EmployeeList = dataSet.Tables[4].AsEnumerable()
                             .Select(dataRow => new SelectListItem
                             {
                                 Value = dataRow.Field<long>("EmployeeID").ToString(),
                                 Text = dataRow.Field<string>("Name")
                             }).ToList();

            employmentDetail.LeavePolicyList = dataSet.Tables[6].AsEnumerable()
                            .Select(dataRow => new SelectListItem
                            {
                                Value = dataRow.Field<long>("LeavePolicyID").ToString(),
                                Text = dataRow.Field<string>("LeavePolicyName")
                            }).ToList();

            return employmentDetail;
        }

        #endregion


        #region Leave Policies
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
        public DashBoardModel GetDashBoardodel(DashBoardModelInputParams model)
        {
            DashBoardModel dashBoardModel = new DashBoardModel();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
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
                                  ReportingToID = dataRow.Field<long>("ReportingToID"),
                                  OfficialEmailID = dataRow.Field<string>("OfficialEmailID"),
                                  OfficialContactNo = dataRow.Field<string>("OfficialContactNo"),
                                  JoiningDate = dataRow.Field<DateTime?>("JoiningDate"),
                                  JobSeprationDate = dataRow.Field<DateTime?>("JobSeprationDate")

                              }).ToList().FirstOrDefault();

            var OtherDetails = dataSet.Tables[1].AsEnumerable()
                              .Select(dataRow => new DashBoardModel
                              {
                                  NoOfEmployees = dataRow.Field<int>("NoOfEmployees"),
                                  NoOfCompanies = dataRow.Field<int>("NoOfCompanies")
                              }).ToList().FirstOrDefault();

            if (dashBoardModel == null)
            {
                dashBoardModel = new DashBoardModel();
            }

            dashBoardModel.NoOfEmployees = OtherDetails.NoOfEmployees;
            dashBoardModel.NoOfCompanies = OtherDetails.NoOfCompanies;

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

            myInfoResults.employmentDetail = GetEmploymentDetailsByEmployee(new EmploymentDetailInputParams() { EmployeeID = model.EmployeeID, CompanyID = model.CompanyID });

            return myInfoResults;
        }




        #endregion

    }
}
