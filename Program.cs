using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace AddressMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var address = "Aenzj5NnVgPUo3hAVy9cyFN9CbrgREyyJ1";
            var request = $"https://neoscan-testnet.io/api/test_net/v1/get_balance/{address}";
            var neo = 0m;
            var gas = 0m;

            using WebClient wc = new WebClient();
            JObject obj = JObject.Parse(wc.DownloadString(request));
            foreach (var item in obj["balance"])
            {
                if (item["asset"].ToString() == "NEO")
                {
                    neo = (decimal)item["amount"];
                }
                else if (item["asset"].ToString() == "GAS")
                {
                    gas = (decimal)item["amount"];
                }
            }

            if (neo < 5000 || gas < 5000)
            {
                var config = JObject.Parse(File.ReadAllText("config.json"));
                var smsList = config["sms"];
                foreach (string to in smsList)
                {
                    SMS.Send(to, address, neo, gas);
                }
                var emailList = config["email-list"];
                foreach (string to in emailList)
                {
                    Email.Send(to, "neowish 测试币余额不足", $"地址{address}测试币余额不足，{neo} NEO，{gas} GAS");
                }
            }

            Console.WriteLine($"{neo} NEO\r\n{gas} GAS");
            Console.ReadLine();
        }
    }
}
