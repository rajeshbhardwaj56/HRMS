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


        public Results GetAllLangueges()
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
                model.Langueges = dataSet.Tables[0].AsEnumerable()
                               .Select(dataRow => new SelectListItem
                               {
                                   Text = dataRow.Field<string>("Name"),
                                   Value = dataRow.Field<long>("LanguageID").ToString()
                               }).ToList();
            }
            return model;
        }


        public Results GetAllCompanyLangueges(long companyID)
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
                model.Langueges = dataSet.Tables[0].AsEnumerable()
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

            var dataSet = DataLayer.GetDataSetByStoredProcedure(StoredProcedures.usp_AddUpdate_Employee, sqlParameter);

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
