using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TEC
{
    public class MobileOTP
    {
        public string MobileNo { get; set; }
        public string OTP { get; set; }
    }

    public class OTPRequest
    {
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public int ActionPageID { get; set; }
    }
}
