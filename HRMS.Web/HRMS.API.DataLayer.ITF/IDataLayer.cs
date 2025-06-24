using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HRMS.API.DataLayer.ITF
{
    public interface IDataLayer
    {
        public IConfiguration _configuration { get; set; }
        Task<DataSet> GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null);

        Task<DataSet> GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams, Action<SqlParameterCollection> pOutputParamsl);

        Task<bool> InsertUpdateByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null);
    }
}
