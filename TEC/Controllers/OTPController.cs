using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TEC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTPController : ControllerBase
    {
        IConfiguration configuration;

        public OTPController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        [HttpPost("GenerateOTP")]
        public IActionResult GenerateOTP([FromBody] OTPRequest request)
        {
            OTPService service = new OTPService(configuration.GetConnectionString("MAF"));
            MobileOTP oTP;
            try
            {
                oTP = service.GenerateOTP(request);
                SMSHelper helper = new SMSHelper(configuration);
                helper.Send(oTP.MobileNo, oTP.OTP);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            return Ok();
        }

        [HttpGet("ResendOTP")]
        public IActionResult ResendOTP([FromQuery] string username)
        {
            OTPService service = new OTPService(configuration.GetConnectionString("MAF"));
            MobileOTP oTP;
            try
            {
                oTP = service.GetOTP(username);
                SMSHelper helper = new SMSHelper(configuration);
                helper.Send(oTP.MobileNo, oTP.OTP);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet("VerifyOTP")]
        public IActionResult VerifyOTP([FromQuery] string username, string otp)
        {
            OTPService service = new OTPService(configuration.GetConnectionString("MAF"));           
            try
            {
               if(!service.VerifyOTP(username, otp))
                {
                    return BadRequest();
                }               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}