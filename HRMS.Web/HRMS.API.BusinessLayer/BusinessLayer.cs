using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer.ITF;
using HRMS.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                       Result = dataRow.Field<int>("Result").ToString(),
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
                       Result = dataRow.Field<int>("Result").ToString(),
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
    }
}
