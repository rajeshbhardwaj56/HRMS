using HRMS.API.BusinessLayer.ITF;
using HRMS.API.DataLayer.ITF;
using HRMS.Models.Common;
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
    public class BusinessLayer: IBusinessLayer
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
            sqlParameter.Add(new SqlParameter("@Email", loginUser.Email));
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
                                   FirstName = dataRow.Field<string>("FirstName"),
                                   LastName = dataRow.Field<string>("LastName"),
                                   Email = dataRow.Field<string>("EmailId"),
                                   Alias = dataRow.Field<string>("Alias"),
                                   Role = dataRow.Field<string>("Role"),
                                   RoleId = dataRow.Field<int>("RoleId"),
                               }).ToList().FirstOrDefault();
            }
            return loginUser;
        }

    }
}
