using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.API.DataLayer.ITF
{
    public interface IAttandanceDataLayer
    {
        public IConfiguration _configuration { get; set; }
        DataSet GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null);

        DataSet GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams, ref SqlParameterCollection pOutputParamsl);

        bool InsertUpdateByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null);
    }
}
