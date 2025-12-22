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


        private void setConectionString()
        {
            if (string.IsNullOrEmpty(conStr))
            {
#if DEBUG
                conStr = _configuration.GetConnectionString("conStr");
#else
            conStr = _configuration.GetConnectionString("conStrProd");
#endif
            }
        }
        /// <summary>
        /// This function is used to return dataset with tables return by stored procedure
        /// </summary>
        /// <param name="pStoredProcedureName"></param>
        /// <param name="pParams"></param>
        /// <returns></returns>
        public DataSet GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null)
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
                            cmdSql.CommandTimeout = 0; 
                            daSql.SelectCommand = cmdSql;
                            daSql.Fill(dataSet);
                            return dataSet;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(conLocal + ex.Message);
                            throw new Exception(conLocal + conStr + ex.Message);
                       
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


        public DataSet GetDataSetByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams, ref SqlParameterCollection pOutputParams)
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
                            pOutputParams = cmdSql.Parameters;
                            return dataSet;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(conLocal + ex.Message);
                            
                            throw new Exception(conLocal + conStr + ex.Message);

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

        public bool InsertUpdateByStoredProcedure(string pStoredProcedureName, List<SqlParameter> pParams = null)
        {
            setConectionString();
            StringBuilder queryBuilder = new StringBuilder(pStoredProcedureName);
            using (SqlConnection conLocal = new SqlConnection(conStr))
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
                        return cmdSql.ExecuteNonQuery() > 0;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (conLocal.State == ConnectionState.Open)
                        {
                            conLocal.Close();
                        }
                        cmdSql.Dispose();
                        queryBuilder = null;
                    }
                }
            }
        }
    }
}
