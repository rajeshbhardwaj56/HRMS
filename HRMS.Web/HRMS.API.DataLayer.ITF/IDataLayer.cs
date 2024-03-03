using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HRMS.API.DataLayer.ITF
{
    public interface IDataLayer
    {
        public IConfiguration _configuration { get; set; }
        DataSet GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null);
        bool InsertUpdateByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null);
    }
}
