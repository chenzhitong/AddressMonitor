using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace AddressMonitor
{
    public static class SMS
    {
        public static void Send(string phone, string address, decimal neo, decimal gas)
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));
            var accountSid = config["yuntongxun"]["ACCOUNT SID"].ToString();
            var authToken = config["yuntongxun"]["AUTH TOKEN"].ToString();
            var appId = config["yuntongxun"]["AppID"].ToString();
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var SigParameter = MD5Encrypt($"{accountSid}{authToken}{timestamp}");
            var url = $"https://app.cloopen.com:8883/2013-12-26/Accounts/{accountSid}/SMS/TemplateSMS?sig={SigParameter}";
            var authorization = Convert.ToBase64String(Encoding.Default.GetBytes($"{accountSid}:{timestamp}"));
            var headers = new List<HttpHeader>
            {
                new HttpHeader("Accept", "application/json"),
                new HttpHeader("Content-Type", "application/json;charset=utf-8"),
                new HttpHeader("Authorization", authorization)
            };

            var body = $"{{'templateId':477518,'to':'{phone}','appId':'{appId}', 'datas':['{address}','{neo}','{gas}']}}";
            var response = (string)HttpPost(url, body, headers);
            if (response == null) return;
            var xml = new XmlDocument();
            xml.LoadXml(response);
            var responseBody = xml.SelectSingleNode("Response");
            var statusCode = responseBody.SelectSingleNode("statusCode").InnerText;
            if (statusCode == "000000")
            {
                Console.WriteLine($"短信发送成功 {phone}");
            }
            else
            {
                Console.WriteLine($"短信发送失败 {phone}");
            }
        }

        static string MD5Encrypt(string strText)
        {
            byte[] result = Encoding.Default.GetBytes(strText.Trim());
            using MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToUpper();
        }
        static string HttpPost(string Url, string postData, List<HttpHeader> HttpHeaders = null, int timeOut = 5000)
        {
            try
            {
                WebRequest request = WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                request.Timeout = timeOut;
                if (HttpHeaders != null && HttpHeaders.Count > 0)
                {
                    foreach (var item in HttpHeaders)
                    {
                        switch (item.Name)
                        {
                            case "Accept": break;
                            case "Content-Type": request.ContentType = item.Value; break;
                            default: request.Headers.Add(item.Name, item.Value); break;
                        }
                    }
                }
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch (Exception e)
            {
                Console.WriteLine("HttpPost Exception: " + e.Message);
                return null;
            }
        }
    }
}
