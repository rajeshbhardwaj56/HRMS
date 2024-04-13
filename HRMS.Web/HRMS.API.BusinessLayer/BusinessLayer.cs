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

                                   //FirstName = dataRow.Field<string>("FirstName"),
                                   //LastName = dataRow.Field<string>("LastName"),
                                   //Email = dataRow.Field<string>("EmailId"),
                                   //Alias = dataRow.Field<string>("Alias"),
                                   Role = dataRow.Field<string>("Role"),
                                   RoleId = dataRow.Field<int>("RoleId"),
                               }).ToList().FirstOrDefault();
            }
            return loginUser;
        }

        public Results GetAllEmployees(EmployeeInputParans model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_EmployeeDetails, sqlParameter);
            result.Employees = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new EmployeeModel
                              {
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                                  guid = dataRow.Field<Guid>("guid"),
                                  EmployeeTypeID = dataRow.Field<long>("EmployeeTypeID"),
                                  CompanyID = dataRow.Field<long>("CompanyID"),
                                  DepartmentID = dataRow.Field<long>("DepartmentID"),
                                  EmployeeNumber = dataRow.Field<string>("EmployeeNumber"),
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


                //////////////////////// EmploymentDetails
                result.employeeModel.EmploymentDetails = dataSet.Tables[4].AsEnumerable()
                           .Select(dataRow => new EmploymentDetail
                           {
                               EmploymentDetailID = dataRow.Field<long>("EmploymentDetailID"),
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
                if (result.employeeModel.EmploymentDetails == null)
                {
                    result.employeeModel.EmploymentDetails = new List<EmploymentDetail>();
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
            }

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
            sqlParameter.Add(new SqlParameter("@EmployeeTypeID", employeeModel.EmployeeTypeID));
            sqlParameter.Add(new SqlParameter("@DepartmentID", employeeModel.DepartmentID));
            sqlParameter.Add(new SqlParameter("@CompanyID", employeeModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@EmployeeNumber", employeeModel.EmployeeNumber));
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
            sqlParameter.Add(new SqlParameter("@EmploymentDetails", this.ConvertObjectToXML(employeeModel.EmploymentDetails)));
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


        public Results GetAllTemplates(TemplateInputParans model)
        {
            Results result = new Results();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", model.CompanyID));
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

            if (model.CompanyID > 0)
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

        public Results GetAllCompanies(EmployeeInputParans model)
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
                                  TaxID = dataRow.Field<string>("TaxID"),
                                  IsGroup = dataRow.Field<bool>("IsGroup"),

                              }).ToList();

            if (model.CompanyID > 0)
            {
                result.companyModel = result.Companies.FirstOrDefault();
            }

            return result;
        }

        public Result AddUpdateCompany(CompanyModel companyModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@CompanyID", companyModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@RetCompanyID", companyModel.CompanyID));
            sqlParameter.Add(new SqlParameter("@Abbr", companyModel.Abbr));
            sqlParameter.Add(new SqlParameter("@CountryID", companyModel.CountryID));
            sqlParameter.Add(new SqlParameter("@DefaultCurrencyID", companyModel.DefaultCurrencyID));
            sqlParameter.Add(new SqlParameter("@TaxID", companyModel.TaxID));
            sqlParameter.Add(new SqlParameter("@Name", companyModel.Name));
            sqlParameter.Add(new SqlParameter("@DefaultLetterHead", companyModel.DefaultLetterHead));
            sqlParameter.Add(new SqlParameter("@Domain", companyModel.Domain));
            sqlParameter.Add(new SqlParameter("@DateOfEstablished", companyModel.DateOfEstablished));
            sqlParameter.Add(new SqlParameter("@IsGroup", companyModel.IsGroup));
            sqlParameter.Add(new SqlParameter("@ParentCompany", companyModel.ParentCompany));
            sqlParameter.Add(new SqlParameter("@CompanyLogo", companyModel.CompanyLogo));


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

        #region Leaves
        public Result AddUpdateLeave(LeaveSummayModel leaveSummayModel)
        {
            Result model = new Result();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();

            sqlParameter.Add(new SqlParameter("@LeaveSummaryID", leaveSummayModel.LeaveSummaryID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", leaveSummayModel.EmployeeID));
            sqlParameter.Add(new SqlParameter("@LeaveStatusID", leaveSummayModel.LeaveStatusID));
            sqlParameter.Add(new SqlParameter("@LeaveDurationTypeID", leaveSummayModel.LeaveDurationTypeID));
            sqlParameter.Add(new SqlParameter("@Reason", leaveSummayModel.Reason));
            sqlParameter.Add(new SqlParameter("@StartDate", leaveSummayModel.StartDate));
            sqlParameter.Add(new SqlParameter("@EndDate", leaveSummayModel.EndDate));
            sqlParameter.Add(new SqlParameter("@LeaveTypeID", leaveSummayModel.LeaveTypeID));
            sqlParameter.Add(new SqlParameter("@NoOfDays", leaveSummayModel.NoOfDays));
            sqlParameter.Add(new SqlParameter("@IsActive", leaveSummayModel.IsActive));
            sqlParameter.Add(new SqlParameter("@IsDeleted", leaveSummayModel.IsDeleted));
            sqlParameter.Add(new SqlParameter("@UserID", leaveSummayModel.UserID));
            SqlParameterCollection pOutputParams = null;
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_LeaveSummary, sqlParameter, ref pOutputParams);

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

        public LeaveResults GetlLeavesSummary(LeaveSummayModel model)
        {
            LeaveResults result = new LeaveResults();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@LeaveSummaryID", model.LeaveSummaryID));
            sqlParameter.Add(new SqlParameter("@EmployeeID", model.EmployeeID));
            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_Get_LeavesSummary, sqlParameter);
            result.leavesSummay = dataSet.Tables[0].AsEnumerable()
                              .Select(dataRow => new LeaveSummayModel
                              {
                                  LeaveSummaryID = dataRow.Field<long>("LeaveSummaryID"),
                                  LeaveStatusID = dataRow.Field<long>("LeaveStatusID"),
                                  LeaveTypeID = dataRow.Field<long>("LeaveTypeID"),
                                  LeaveDurationTypeID = dataRow.Field<long>("LeaveDurationTypeID"),
                                  LeaveStatusName = dataRow.Field<string>("LeaveStatusName"),
                                  Reason = dataRow.Field<string>("Reason"),
                                  RequestDate = dataRow.Field<DateTime?>("RequestDate"),
                                  StartDate = dataRow.Field<DateTime?>("StartDate"),
                                  EndDate = dataRow.Field<DateTime?>("EndDate"),
                                  LeaveTypeName = dataRow.Field<string>("LeaveTypeName"),
                                  LeaveDurationTypeName = dataRow.Field<string>("LeaveDurationTypeName"),
                                  NoOfDays = dataRow.Field<int>("NoOfDays"),
                                  IsActive = dataRow.Field<bool>("IsActive"),
                                  IsDeleted = dataRow.Field<bool>("IsDeleted"),
                                  UserID = dataRow.Field<long>("UserID"),
                                  EmployeeID = dataRow.Field<long>("EmployeeID"),
                              }).ToList();

            if (model.LeaveSummaryID > 0)
            {
                result.leaveSummayModel = result.leavesSummay.Where(x => x.LeaveSummaryID == model.LeaveSummaryID).FirstOrDefault();
            }

            return result;
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
        #endregion

    }
}
