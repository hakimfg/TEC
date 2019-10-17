using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TEC
{
    public class SMSHelper
    {
        IConfiguration configuration;

        public SMSHelper(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public Task<HttpResponseMessage> Send(string number, string udata)
        {
            //string _uData = configuration.GetValue<string>("udata");           
            string _url = configuration.GetValue<string>("SMSURL");
            string _uName = configuration.GetValue<string>("uname");
            string _password = configuration.GetValue<string>("passwd");
            string _urname = configuration.GetValue<string>("urname");

            string _finalUrl = _url + "uname=" + _uName + "&passwd=" + _password + "&urname=" + _urname + "&number=" + number
                + "&udata=" + udata + "&drmsgid=1";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_finalUrl);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            Task<HttpResponseMessage> _resp = client.GetAsync(_finalUrl);
            return _resp;
        }
    }
}
