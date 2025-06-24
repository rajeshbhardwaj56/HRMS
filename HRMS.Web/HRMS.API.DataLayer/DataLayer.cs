using System.Data.SqlClient;
using System.Data;
using System.Text;
using Microsoft.Extensions.Configuration;
using HRMS.API.DataLayer.ITF;

namespace HRMS.API.DataLayer
{
    public class DataLayer : IDataLayer
    {
        public IConfiguration _configuration { get; set; }
        private static string conStr { get; set; }


        private async Task setConectionString()
        {
            if (string.IsNullOrEmpty(conStr))
            {
#if DEBUG
                conStr = _configuration.GetConnectionString("conStr");
#else
        conStr = _configuration.GetConnectionString("conStrProd");
#endif
            }

            await Task.CompletedTask; // Just to satisfy async
        }

        /// <summary>
        /// This function is used to return dataset with tables return by stored procedure
        /// </summary>
        /// <param name="pStoredProcedureName"></param>
        /// <param name="pParams"></param>
        /// <returns></returns>
        public async Task<DataSet> GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null)
        {

            setConectionString();

            DataSet dataSet = new DataSet();
            StringBuilder queryBuilder = new StringBuilder(pStoredProcedureName);
            using (SqlConnection conLocal = new SqlConnection(conStr))
            {
                using (SqlDataAdapter daSql = new SqlDataAdapter())
                {
                    using (SqlCommand cmdSql = new SqlCommand(queryBuilder.ToString(), conLocal))
                    {
                        try
                        {
                            if (conLocal.State == ConnectionState.Closed)
                            {
                                conLocal.Open();
                            }
                            if (pParams != null)
                            {
                                cmdSql.Parameters.AddRange(pParams.ToArray());
                            }
                            cmdSql.CommandType = CommandType.StoredProcedure;
                            cmdSql.CommandTimeout = 0; //--- 120 for 2 min and 0 for infinite
                            daSql.SelectCommand = cmdSql;
                            daSql.Fill(dataSet);
                            return dataSet;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(conLocal + ex.Message);
                            //ex.Message = ex.Message+ conLocal;
                            throw new Exception(conLocal + conStr + ex.Message);
                            // throw ex;
                        }
                        finally
                        {
                            if (conLocal.State == ConnectionState.Open)
                            {
                                conLocal.Close();
                            }
                            cmdSql.Dispose();
                            daSql.Dispose();
                            queryBuilder = null;
                            dataSet = null;
                        }
                    }
                }
            }
        }


        public async Task<DataSet> GetDataSetByStoredProcedure(string pStoredProcedureName,List<SqlParameter> pParams, Action<SqlParameterCollection> setOutputParams)
        {
            setConectionString();
            var dataSet = new DataSet();

            using (SqlConnection connection = new SqlConnection(conStr))
            using (SqlCommand command = new SqlCommand(pStoredProcedureName, connection))
            {
                try
                {
                    if (pParams != null && pParams.Any())
                        command.Parameters.AddRange(pParams.ToArray());

                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        int tableIndex = 0;
                        do
                        {
                            if (reader.IsClosed) break;

                            var table = new DataTable($"Table{tableIndex++}");
                            table.Load(reader); // Loads current result set into DataTable
                            dataSet.Tables.Add(table);
                        }
                        while (!reader.IsClosed && await reader.NextResultAsync());
                    }

                    // Handle output parameters (if any)
                    setOutputParams?.Invoke(command.Parameters);
                }
                catch (Exception ex)
                {
                    throw new Exception("Database Error: " + ex.Message, ex);
                }
            }

            return dataSet;
        }



        public async Task<bool> InsertUpdateByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null)
        {
            setConectionString();

            using (SqlConnection conLocal = new SqlConnection(conStr))
            using (SqlCommand cmdSql = new SqlCommand(pStoredProcedureName, conLocal))
            {
                try
                {
                    await conLocal.OpenAsync();

                    if (pParams != null && pParams.Any())
                    {
                        cmdSql.Parameters.AddRange(pParams.ToArray());
                    }

                    cmdSql.CommandType = CommandType.StoredProcedure;
                    cmdSql.CommandTimeout = 0;

                    return await cmdSql.ExecuteNonQueryAsync() > 0;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

    }
}
