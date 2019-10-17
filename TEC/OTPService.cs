using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace TEC
{
    public class OTPService
    {
        private string _connectionString;
        public OTPService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public MobileOTP GenerateOTP(OTPRequest request)
        {
            Random random = new Random();
            MobileOTP mobileOTP = new MobileOTP();
            mobileOTP.OTP = random.Next(100, 9999).ToString("0000");          

            using (SqlConnection _con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("CREATE_OTP", _con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@USERNAME", System.Data.SqlDbType.NVarChar).Value = request.UserName;
                    cmd.Parameters.Add("@OTP", System.Data.SqlDbType.NVarChar).Value = mobileOTP.OTP;
                    cmd.Parameters.Add("@IPADDRESS", System.Data.SqlDbType.NVarChar).Value = request.IPAddress;
                    cmd.Parameters.Add("@LATITUDE", System.Data.SqlDbType.Decimal).Value = request.Latitude;
                    cmd.Parameters.Add("@LONGITUDE", System.Data.SqlDbType.Decimal).Value = request.Longitude;
                    cmd.Parameters.Add("@ACTIONPAGEID", System.Data.SqlDbType.Int).Value = request.ActionPageID;
                    cmd.Parameters.Add("@MOBILENO", System.Data.SqlDbType.NVarChar, 50).Direction = System.Data.ParameterDirection.Output;

                    _con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();

                        mobileOTP.MobileNo = cmd.Parameters["@MOBILENO"].Value?.ToString();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        _con.Close();
                    }
                }

            }

            return mobileOTP;
        }

        public MobileOTP GetOTP(string username)
        {
            MobileOTP mobileOTP = new MobileOTP();

            using (SqlConnection _con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Get_OTP", _con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@USERNAME", System.Data.SqlDbType.NVarChar).Value = username;
                    cmd.Parameters.Add("@OTP", System.Data.SqlDbType.NVarChar,50).Direction = System.Data.ParameterDirection.Output;                   
                    cmd.Parameters.Add("@MOBILENO", System.Data.SqlDbType.NVarChar, 50).Direction = System.Data.ParameterDirection.Output;

                    _con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();

                        mobileOTP.MobileNo = cmd.Parameters["@MOBILENO"].Value?.ToString();
                        mobileOTP.OTP = cmd.Parameters["@OTP"].Value?.ToString();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        _con.Close();
                    }
                }

            }

            return mobileOTP;
        }

        public bool VerifyOTP(string username, string OTP)
        {
            bool isSuccess = false;

            using (SqlConnection _con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Verify_OTP", _con))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("@USERNAME", System.Data.SqlDbType.NVarChar).Value = username;
                    cmd.Parameters.Add("@OTP", System.Data.SqlDbType.NVarChar).Value = OTP;
                    cmd.Parameters.Add("@RETURNVAL", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.ReturnValue;

                    _con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();

                        if (cmd.Parameters["@RETURNVAL"].Value != DBNull.Value)
                        {
                            isSuccess = Convert.ToInt32(cmd.Parameters["@RETURNVAL"].Value) == 1;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        _con.Close();
                    }
                }

            }
            return isSuccess;
        }
    }
}
