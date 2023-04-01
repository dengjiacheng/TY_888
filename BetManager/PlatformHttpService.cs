using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetManager
{
    class PlatformHttpService
    {
        static RestClient client = new RestClient();
        static RestClient client_cart = new RestClient();
        static Dictionary<string, string> host_dic = new Dictionary<string, string>() {
            { "9393", "vd002-we46hc-api.xiaozhi326.com" },
            { "1717", "vd008-gtksap-api.dnzka.com" },
            { "8868", "vd006-2zyayk-api.anguo114.com" },
            { "6686", "vd004-nfaa-api.juliettechenais.com" },
            { "6566", "vd009-bhtpeu4xrn-api.hpbpaas.com" },
        };
        static Dictionary<string, string> referer_dic = new Dictionary<string, string>() {
            { "9393", "089393.app" },
            { "1717", "201717.app" },
            { "8868", "88681.hk" },
            { "6686", "196686.hk" },
            { "6566", "196566.app" },
        };

        static Dictionary<string, RestClient> client_bet = new Dictionary<string, RestClient>() {
            { "9393",new RestClient()},
            { "8868",new RestClient()},
            { "1717",new RestClient()},
            { "6686",new RestClient()},
            { "6566",new RestClient()}
        };

        public static bool nonce(string platform, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/platform/user/nonce", Method.Post);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                dynamic bodyJson = new ExpandoObject();
                bodyJson.imageWidth = 280;
                bodyJson.imageHeight = 155;
                bodyJson.jigsawWidth = 52;
                bodyJson.jigsawHeight = 52;
                string bodyStr = JsonConvert.SerializeObject(bodyJson);
                request.AddBody(bodyStr, "application/json");
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool sendOtpSms(string platform, string phoneNumber, string clientNonce, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/platform/user/sendOtpSms", Method.Post);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                dynamic bodyJson = new ExpandoObject();
                bodyJson.account = "";
                bodyJson.otpType = 7;
                bodyJson.countryCallingCode = 86;
                bodyJson.phoneNumber = phoneNumber;
                bodyJson.clientNonce = clientNonce;
                string bodyStr = JsonConvert.SerializeObject(bodyJson);
                request.AddBody(bodyStr, "application/json");
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool login(string platform, string phoneNumber, string otpCode, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/platform/user/token/quick", Method.Post);


                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");

                dynamic bodyJson = new ExpandoObject();
                bodyJson.account = "";
                bodyJson.password = "";
                bodyJson.device = "pc";
                bodyJson.appType = 2;
                bodyJson.countryCallingCode = 86;
                bodyJson.phoneNumber = phoneNumber;
                bodyJson.otpCode = otpCode;
                string bodyStr = JsonConvert.SerializeObject(bodyJson);
                request.AddBody(bodyStr, "application/json");
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool query_orders(string platform, string token, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/platform/thirdparty-report/user/orders/withoutPage/sport", Method.Get);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");

                request.AddParameter("startDate", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "+00:00:00");
                request.AddParameter("endDate", DateTime.Now.ToString("yyyy-MM-dd") + "+23:59:59");
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool query_amount(string platform, string token, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/platform/payment/wallets/list", Method.Get);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                request.AddParameter("startDate", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + "+00:00:00");
                request.AddParameter("endDate", DateTime.Now.ToString("yyyy-MM-dd") + "+23:59:59");
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool bet(string platform, string token, int sid, int iid, int tid, string market, string beton, double odds, string k, int market_index, int ante, string score, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/product/game/bet", Method.Post);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                dynamic bodyJson = new ExpandoObject();
                bodyJson.marketType = "EU";
                bodyJson.singles = new List<dynamic>();
                dynamic single = new ExpandoObject();
                single.idx = 0;
                single.id = $"{sid}|{iid}|{market}|{beton}|{market_index}";
                single.ante = ante;
                single.transId = ScriptService.transId($"{sid}|{iid}|{market}|{beton}|{market_index}", token);
                bodyJson.singles.Add(single);
                bodyJson.outrights = new List<dynamic>();
                bodyJson.parlays = new List<dynamic>();
                bodyJson.tickets = new List<dynamic>();
                dynamic ticket = new ExpandoObject();
                ticket.sid = sid;
                ticket.tid = tid;
                ticket.iid = iid;
                ticket.market = market;
                ticket.beton = beton;
                ticket.odds = odds;
                ticket.k = k;
                ticket.inp = true;
                ticket.outright = false;
                ticket.score = score;
                ticket.orderPhase = 1;
                ticket.v = "a";
                bodyJson.tickets.Add(ticket);
                string bodyStr = JsonConvert.SerializeObject(bodyJson);
                Console.WriteLine(bodyStr);
                request.AddBody(bodyStr, "application/json");
                var response = client_bet[platform].ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool cart(string platform, int sid, int iid, string market, out dynamic data)
        {
            data = string.Empty;
            try
            {

                                        
                var request = new RestRequest($"https://{host_dic[platform]}/product/business/cart", Method.Post);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                dynamic bodyJson = new ExpandoObject();
                bodyJson.bets = new List<dynamic>();
                dynamic bet = new ExpandoObject();
                bet.sid = sid;
                bet.iid = iid;
                bet.inplay = true;
                bet.outright = false;
                bet.market = market;
                bodyJson.bets.Add(bet);
                string bodyStr = JsonConvert.SerializeObject(bodyJson);
                request.AddBody(bodyStr, "application/json");
                var response = client_cart.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool get_tournaments(string platform, string sid, out dynamic data)
        {
            data = string.Empty;
            try
            {

                var request = new RestRequest($"https://{host_dic[platform]}/product/business/sport/tournament/info", Method.Get);
                
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                request.AddParameter("sid", sid);
                request.AddParameter("sort", "tournament");
                request.AddParameter("inplay", "true");
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }
        public static bool thirdparty(string platform,string token, out dynamic data)
        {
            data = string.Empty;
            try
            {
                var request = new RestRequest($"https://{host_dic[platform]}/platform/thirdparty/game/entry", Method.Get);
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddHeader("Accept-Language", "zh-cn");
                request.AddHeader("time-zone", "GMT+08:00");
                request.AddHeader("device", "android");
                request.AddHeader("os", "Android 11");
                request.AddHeader("screen", "1080x1920");
                request.AddHeader("appType", "8");
                request.AddHeader("User-Agent", "okhttp/4.9.1");
                request.AddHeader("phonebrand", "google");
                request.AddHeader("phonemodel", "Pixel");
                request.AddHeader("appversion", "3.89.12");
                request.AddHeader("currency", "CNY");
                request.AddHeader("Referer", $"https://{referer_dic[platform]}/");
                request.AddParameter("device", "android");
                request.AddParameter("providerCode", 1);
                var response = client.ExecuteAsync(request).Result;
                if (response.IsSuccessful)
                {
                    var respJson = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    int code = respJson.code;
                    if (0 == code)
                    {
                        data = respJson.data;
                        return true;
                    }
                    else
                    {
                        data = respJson.msg;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                data = string.Format("Rquest Error{0}", e.Message); ;
                return false;
            }
        }

    }
}
