using System.Data;
using System.Data.SqlClient;
using HRMSConsoleApp.Model;
using Microsoft.Extensions.Configuration;

namespace HRMSConsoleApp.Services


{
    public class AttendanceService
    {
        private readonly IConfiguration _configuration;

        public AttendanceService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    
        public AttendanceInputParams GetAttendance(AttendanceInputParams model)
        {
            string connectionString = _configuration["ConnectionStrings:conStr"];
        

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))

                using (SqlCommand cmd = new SqlCommand("usp_CalculateMonthlyAttendance_WithShifts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Set parameters
                    cmd.Parameters.AddWithValue("@Year", model.Year);        
                    cmd.Parameters.AddWithValue("@Month", model.Month);
                    cmd.Parameters.AddWithValue("@Day", model.Day);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    cmd.Parameters.AddWithValue("@IsManual", false);
                    cmd.Parameters.AddWithValue("@AttendanceStatus", "Approved");
                    cmd.CommandTimeout = 3600;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            model.Message = "Attendance calculated successfully.";
                            model.IsSuccess = true;
                           

                        }
                        else
                        {
                            model.Message = "No attendance data returned.";
                            model.IsSuccess = false;
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string date = $"{model.Year}{model.Month:D2}{model.Day:D2}";
                string errorMessage = $"Error during attendance fetching for date {date}: {ex.Message}";
              
                model.Message = "Error: " + ex.Message;
                model.IsSuccess = false;
            }


            return model;
        }

    }
}



