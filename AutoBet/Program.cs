using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoBet
{
    class Program
    {
        static dynamic user_info_json;
        static double cache_k = 0;
        static double home_optimal_k = 0;
        static double away_optimal_k = 0;
        static double cache_amount = 0;
        static int expansion_times = 1;
        static bool auto = false;
        static int sid = 2;
        static int tid;
        static int iid;
        static string tnName;
        static string market;
        static string home_name;
        static string away_name;
        static Dictionary<double, double> home_bet = new Dictionary<double, double>();
        static Dictionary<double, double> away_bet = new Dictionary<double, double>();
        static Dictionary<double, double> home_del_bet = new Dictionary<double, double>();
        static Dictionary<double, double> away_del_bet = new Dictionary<double, double>();

        static void Main(string[] args)
        {
            RestClient client = new RestClient(new RestClientOptions() { Proxy = new WebProxy("tps516.kdlapi.com:15818") });
            client.Options.Proxy.Credentials = new NetworkCredential("t15494907899660", "i0rjb3fd");
            PlatformHttpService.client_cart = client;


            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFilePath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            // 获取appSettings部分
            AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");

            // 读取键值对
            string platform = appSettings.Settings["platform"].Value;
            string host = appSettings.Settings["host"].Value;
            string referer = appSettings.Settings["referer"].Value;
            string currency = appSettings.Settings["currency"].Value;
            int ante = Convert.ToInt32(appSettings.Settings["ante"].Value);
            PlatformHttpService.host_dic[platform] = host;
            PlatformHttpService.referer_dic[platform] = referer;
            string account_home = appSettings.Settings["account_home"].Value;
            string account_away = appSettings.Settings["account_away"].Value;
            Console.WriteLine("platform: " + platform);
            Console.WriteLine("account: " + account_home);
            #region 登录
            string user_info_home = RedisService.get_platform_user_info(platform, account_home);
            if (string.IsNullOrWhiteSpace(user_info_home))
            {
                Console.WriteLine(String.Format("{0} {1} {2} ", platform, account_home, "平台未登录"));
                Console.ReadLine();
                return;
            }
            var user_info_home_json = JsonConvert.DeserializeObject<dynamic>(user_info_home);
            string token_home = user_info_home_json.token;
            if (PlatformHttpService.Thirdparty(platform, currency, token_home, out dynamic resp_thirdparty_home))
            {
                user_info_home_json.thirdparty_token = resp_thirdparty_home.token;
                user_info_json = user_info_home_json;
                Console.WriteLine(String.Format("{0} {1} {2} ", platform, account_home, "第三方登录成功"));
            }
            else
            {
                Console.WriteLine(String.Format("{0} {1} {2} {3}", platform, account_home, "第三方登录失败", resp_thirdparty_home));
                Console.ReadLine();
                return;
            }
            while (cache_amount <= 0)
            {
                if (PlatformHttpService.query_amount(platform, currency, token_home, out dynamic resp_query_amount_home))
                {
                    var wallets = resp_query_amount_home.wallets;
                    foreach (var wallet in wallets)
                    {
                        string currency_ = wallet.currency;
                        if (currency.Equals(currency_))
                        {
                            cache_amount = wallet.amount;
                            Console.WriteLine($"{currency}:{cache_amount}");
                        }
                    }
                }
                Thread.Sleep(200);
            }
            #endregion
           
            Task.Factory.StartNew(() =>
            {
                auto_find(platform, currency);
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                auto_cart(platform, currency, ante);
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                auto_login(platform, currency, account_home, account_away);
            }, TaskCreationOptions.LongRunning);

            Console.ReadLine();
        }



        private static void auto_login(string platform, string currency, string account_home, string account_away)
        {

            while (true)
            {
                #region 登录

                string token_home = user_info_json.token;
                if (PlatformHttpService.Thirdparty(platform, currency, token_home, out dynamic resp_thirdparty_home))
                {
                    user_info_json.thirdparty_token = resp_thirdparty_home.token;
                }
                else
                {
                    Console.WriteLine(String.Format("{0} {1} {2} {3}", platform, account_home, "第三方登录失败", resp_thirdparty_home));

                }
                #endregion
                Thread.Sleep(1000 * 60 * 2);
            }

        }


        private static async Task AutoFindAsync(string platform, string currency)
        {
            while (true)
            {
                if (auto)
                {
                    await Task.Delay(500);
                    continue;
                }

                if (!PlatformHttpService.GetTournaments(platform, currency, sid, out dynamic resp_tournaments))
                {
                    await Task.Delay(500);
                    continue;
                }

                foreach (var tournament in resp_tournaments.tournaments)
                {
                    foreach (var match in tournament.matches)
                    {
                        var homeName = match.home.name;
                        var awayName = match.away.name;
                        var iid = match.iid;
                        var tnName = match.tnName;
                        var matches = match.matches;
                        if (PlatformHttpService.Cart(platform, currency, sid, iid, "ah", out dynamic resp_cart))
                        {
                            foreach (var bet in resp_cart.bets)
                            {
                                var detail = bet.detail;
                                var (period, time) = (detail.period, detail.time);
                                var seconds = ConvertTimeStringToSeconds(time);
                                var isQ1Match = period.Equals("q1") && seconds >= 60 * 3;
                                var isQ3Match = period.Equals("q3") && seconds >= 60 * 3;

                                if (isQ1Match || isQ3Match)
                                {
                                    market = isQ1Match ? "ah_1st" : "ah";
                                    auto = true;
                                    Console.Title = $"{tnName}-{homeName} VS {awayName} -{(isQ1Match ? "上半场" : "全场")}";
                                    break;
                                }
                            }
                        }

                        if (auto) break;
                        await Task.Delay(200);
                    }

                    if (auto) break;
                }
                await Task.Delay(500);
            }
        }

        private static void auto_find(string platform, string currency)
        {

            while (true)
            {
                if (auto)
                {
                    Thread.Sleep(500);
                    continue;
                }
                if (PlatformHttpService.GetTournaments(platform, currency, sid, out dynamic resp_tournaments))
                {
                    var tournaments = resp_tournaments.tournaments;
                    foreach (var tournament in tournaments)
                    {
                        var matches = tournament.matches;
                        foreach (var matche in matches)
                        {
                            tnName = matche.tnName;
                            tid = matche.tid;
                            iid = matche.iid;
                            string vd = matche.vd;
                            home_name = matche.home.name;
                            away_name = matche.away.name;
                            if (PlatformHttpService.Cart(platform, currency, sid, iid, "ah", out dynamic resp_cart))
                            {
                                var bets = resp_cart.bets;

                                foreach (var bet in bets)
                                {
                                    var detail = bet.detail;
                                    string period = detail.period;
                                    string time = detail.time;
                                    int seconds = ConvertTimeStringToSeconds(time);
                                    if ("q1".Equals(period) && seconds >= 60 * 3)
                                    {
                                        market = "ah_1st";
                                        auto = true;
                                        Console.Title = $"{tnName}-{home_name} VS {away_name} -上半场";
                                        break;
                                    }
                                    else if ("q3".Equals(period) && seconds >= 60 * 3)
                                    {
                                        market = "ah";
                                        auto = true;
                                        Console.Title = $"{tnName}-{home_name} VS {away_name} -全场";
                                        break;
                                    }
                                }

                            }
                            if (auto)
                            {
                                break;
                            }
                            Thread.Sleep(200);
                        }
                        if (auto)
                        {
                            break;
                        }
                    }
                };
                Thread.Sleep(500);

            }
        }
        private static void auto_cart(string platform, string currency, double ante)
        {
            string token_ = user_info_json.token;
            //string token_away = user_info_json_away.token;
            while (true)
            {
                if (!auto)
                {
                    Thread.Sleep(300);
                    continue;
                }
                try
                {
                    if (PlatformHttpService.Cart(platform, currency, sid, iid, market, out dynamic resp_cart))
                    {
                        var bets = resp_cart.bets;

                        foreach (var bet in bets)
                        {
                            var detail = bet.detail;
                            string period = detail.period;
                            string time = detail.time;
                            int seconds = ConvertTimeStringToSeconds(time);
                            string score = detail.score;
                            if (("q2".Equals(period) || "q4".Equals(period)) && seconds <= 60 * 6 + 30)
                            {
                                while (true)
                                {
                                    bool v_amount = false;
                                    int unSettlement_count = -1;
                                    if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                    {
                                        var wallets = resp_query_amount.wallets;
                                        foreach (var wallet in wallets)
                                        {
                                            string currency_ = wallet.currency;
                                            if (currency.Equals(currency_))
                                            {
                                                double amount = wallet.amount;
                                                cache_amount = amount;
                                                v_amount = true;
                                                Thread.Sleep(2000);
                                                if (PlatformHttpService.query_orders(platform, currency, token_, out dynamic resp_query_orders))
                                                {
                                                    unSettlement_count = resp_query_orders.unSettlement.summary.count;
                                                    if (unSettlement_count == 0)
                                                    {
                                                        home_optimal_k = 0;
                                                        away_optimal_k = 0;
                                                        expansion_times = 0;
                                                        home_bet.Clear();
                                                        away_bet.Clear();
                                                        home_del_bet.Clear();
                                                        away_del_bet.Clear();
                                                        cache_k = 0;
                                                        break;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    if (v_amount && unSettlement_count == 0)
                                    {
                                        Console.WriteLine("---------------------------切换比赛---------------------------");
                                        auto = false;
                                        break;
                                    }

                                    Thread.Sleep(3000);
                                }

                            }
                            Console.Title = $"{tnName}-{home_name} VS {away_name} -{period}:{time} -{score}";
                            var market_arr = bet.market;
                            if (null != market_arr[market])
                            {
                                var handicaps = market_arr[market];
                                if (handicaps.Count > 0)
                                {
                                    var handicap = handicaps[0];
                                    double k = handicap["k"];
                                    double odds_h = handicap["h"];
                                    double odds_a = handicap["a"];
                                    var team = string.Empty;
                                    if (odds_h != 0 && odds_a != 0)
                                    {
                                        //if (cache_k != 0 && cache_k != k)
                                        //{
                                        if (home_optimal_k == 0 && away_optimal_k == 0)
                                        {
                                            if (((k >= -3.5 && k < 0) || k > 3.5) && odds_h < odds_a - 0.03)
                                            {
                                                //下主
                                                if (bet_home(platform, currency, market, sid, iid, tid, k, ante, odds_h, score))
                                                {
                                                    long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                    bool bet_success = false;
                                                    Console.WriteLine($"主队下单检测：{k}  {odds_h}  {cache_amount}");
                                                    while (!bet_success)
                                                    {
                                                        if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                        {
                                                            var wallets = resp_query_amount.wallets;
                                                            foreach (var wallet in wallets)
                                                            {
                                                                string currency_ = wallet.currency;
                                                                if (currency.Equals(currency_))
                                                                {
                                                                    double amount = wallet.amount;
                                                                    if (cache_amount > amount)
                                                                    {
                                                                        cache_amount = amount;
                                                                        home_optimal_k = k;
                                                                        home_bet[k] = ante;
                                                                        Console.WriteLine($"主队：{k}  {odds_h}  {cache_amount}");
                                                                        bet_success = true;
                                                                        break;
                                                                    }

                                                                }
                                                            }
                                                        }
                                                        long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                        if (d_t >= 6800 || bet_success)
                                                        {
                                                            bet_success = true;
                                                            break;
                                                        }
                                                        Thread.Sleep(500);
                                                    }
                                                }

                                            }
                                            else if (((k <= 3.5 && k > 0) || k < -3.5) && odds_a < odds_h - 0.03)
                                            {
                                                //下客
                                                if (bet_away(platform, currency, market, sid, iid, tid, k, ante, odds_a, score))
                                                {
                                                    long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                    bool bet_success = false;
                                                    Console.WriteLine($"客队下单检测：{k * -1d}  {odds_a}  {cache_amount}");
                                                    while (!bet_success)
                                                    {
                                                        if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                        {
                                                            var wallets = resp_query_amount.wallets;
                                                            foreach (var wallet in wallets)
                                                            {
                                                                string currency_ = wallet.currency;
                                                                if (currency.Equals(currency_))
                                                                {
                                                                    double amount = wallet.amount;
                                                                    if (cache_amount > amount)
                                                                    {
                                                                        cache_amount = amount;
                                                                        away_optimal_k = k;
                                                                        away_bet[k] = ante;
                                                                        Console.WriteLine($"客队：{k * -1d}  {odds_a}  {cache_amount}");
                                                                        bet_success = true;
                                                                        break;
                                                                    }

                                                                }
                                                            }
                                                        }
                                                        long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                        if (d_t >= 6800 || bet_success)
                                                        {
                                                            bet_success = true;
                                                            break;
                                                        }
                                                        Thread.Sleep(500);
                                                    }
                                                }
                                            }

                                        }
                                        //闭环
                                        if (home_optimal_k - k >= 4 && home_optimal_k != 0 && away_optimal_k == 0)
                                        {
                                            //下注客闭环
                                            if (bet_away(platform, currency, market, sid, iid, tid, k, ante, odds_a, score))
                                            {
                                                long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                bool bet_success = false;
                                                Console.WriteLine($"客队下单检测：{k * -1d}  {odds_a}  {cache_amount}  闭环主 {home_optimal_k}");
                                                while (!bet_success)
                                                {
                                                    if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                    {
                                                        var wallets = resp_query_amount.wallets;
                                                        foreach (var wallet in wallets)
                                                        {
                                                            string currency_ = wallet.currency;
                                                            if (currency.Equals(currency_))
                                                            {
                                                                double amount = wallet.amount;
                                                                if (cache_amount > amount)
                                                                {
                                                                    cache_amount = amount;
                                                                    away_bet[k] = ante;
                                                                    Console.WriteLine($"客队：{k * -1d}  {odds_a}  {cache_amount}  闭环主 {home_optimal_k}");
                                                                    bet_success = true;
                                                                    away_optimal_k = k;
                                                                    break;
                                                                }

                                                            }
                                                        }
                                                    }
                                                    long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                    if (d_t >= 6800 || bet_success)
                                                    {
                                                        bet_success = true;
                                                        break;
                                                    }
                                                    Thread.Sleep(500);
                                                }
                                            }
                                        }
                                        //闭环
                                        if (k - away_optimal_k >= 4 && away_optimal_k != 0 && home_optimal_k == 0)
                                        {
                                            //下主闭环
                                            if (bet_home(platform, currency, market, sid, iid, tid, k, ante, odds_h, score))
                                            {
                                                long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                bool bet_success = false;
                                                Console.WriteLine($"主队下单检测：{k}  {odds_h}  {cache_amount}  闭环客 {away_optimal_k * -1d}");
                                                while (!bet_success)
                                                {
                                                    if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                    {
                                                        var wallets = resp_query_amount.wallets;
                                                        foreach (var wallet in wallets)
                                                        {
                                                            string currency_ = wallet.currency;
                                                            if (currency.Equals(currency_))
                                                            {
                                                                double amount = wallet.amount;
                                                                if (cache_amount > amount)
                                                                {
                                                                    cache_amount = amount;
                                                                    home_bet[k] = ante;
                                                                    Console.WriteLine($"主队：{k}  {odds_h}  {cache_amount}  闭环客 {away_optimal_k * -1d}");
                                                                    home_optimal_k = k;
                                                                    bet_success = true;
                                                                    break;
                                                                }

                                                            }
                                                        }
                                                    }
                                                    long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                    if (d_t >= 6800 || bet_success)
                                                    {
                                                        bet_success = true;
                                                        break;
                                                    }
                                                    Thread.Sleep(500);
                                                }
                                            }
                                        }
                                        //主队扩容
                                        if (k >= home_optimal_k + 5 * (1 + home_del_bet.Count) && home_del_bet.Count < 2 && home_optimal_k != 0)
                                        {
                                            if (bet_home(platform, currency, market, sid, iid, tid, k, ante + ((ante * 0.2 * expansion_times) / 0.8), odds_h, score))
                                            {
                                                long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                bool bet_success = false;
                                                Console.WriteLine($"主队下单检测：{k}  {odds_h}  {cache_amount}  扩容 {home_optimal_k}");
                                                while (!bet_success)
                                                {
                                                    if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                    {
                                                        var wallets = resp_query_amount.wallets;
                                                        foreach (var wallet in wallets)
                                                        {
                                                            string currency_ = wallet.currency;
                                                            if (currency.Equals(currency_))
                                                            {
                                                                double amount = wallet.amount;
                                                                if (cache_amount > amount)
                                                                {
                                                                    expansion_times++;
                                                                    cache_amount = amount;
                                                                    home_del_bet[home_optimal_k] = home_bet[home_optimal_k];
                                                                    home_bet.Remove(home_optimal_k);
                                                                    home_bet[k] = ante + ((ante * 0.2 * expansion_times) / 0.8);
                                                                    Console.WriteLine($"主队：{k}  {odds_h}  {cache_amount}  扩容 {home_optimal_k}");
                                                                    home_optimal_k = k;
                                                                    bet_success = true;
                                                                    break;
                                                                }

                                                            }
                                                        }
                                                    }
                                                    long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                    if (d_t >= 6800 || bet_success)
                                                    {
                                                        bet_success = true;
                                                        break;
                                                    }
                                                    Thread.Sleep(500);
                                                }
                                            }
                                        }
                                        //客队扩容
                                        if (k <= away_optimal_k - 5 * (1 + away_del_bet.Count) && away_del_bet.Count < 2 && away_optimal_k != 0)
                                        {

                                            if (bet_away(platform, currency, market, sid, iid, tid, k, ante + ((ante * 0.2 * expansion_times) / 0.8), odds_a, score))
                                            {
                                                long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                bool bet_success = false;
                                                Console.WriteLine($"客队下单检测：{k * -1d}  {odds_a}  {cache_amount}  扩容 {away_optimal_k * -1d}");
                                                while (!bet_success)
                                                {
                                                    if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                    {
                                                        var wallets = resp_query_amount.wallets;
                                                        foreach (var wallet in wallets)
                                                        {
                                                            string currency_ = wallet.currency;
                                                            if (currency.Equals(currency_))
                                                            {
                                                                double amount = wallet.amount;
                                                                if (cache_amount > amount)
                                                                {
                                                                    expansion_times++;
                                                                    cache_amount = amount;
                                                                    away_del_bet[away_optimal_k] = away_bet[away_optimal_k];
                                                                    away_bet.Remove(away_optimal_k);
                                                                    away_bet[k] = ante + ((ante * 0.2 * expansion_times) / 0.8);
                                                                    Console.WriteLine($"客队：{k * -1d}  {odds_a}  {cache_amount}  扩容 {away_optimal_k * -1d}");
                                                                    away_optimal_k = k;
                                                                    bet_success = true;
                                                                    break;
                                                                }

                                                            }
                                                        }
                                                    }
                                                    long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                    if (d_t >= 6800 || bet_success)
                                                    {
                                                        bet_success = true;
                                                        break;
                                                    }
                                                    Thread.Sleep(500);
                                                }
                                            }

                                        }
                                        //主队对冲
                                        foreach (var del_k in home_del_bet.Keys.Reverse())
                                        {
                                            if (k <= del_k)
                                            {
                                                if (bet_away(platform, currency, market, sid, iid, tid, k, home_del_bet[del_k], odds_a, score))
                                                {
                                                    long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                    bool bet_success = false;
                                                    Console.WriteLine($"客队下单检测：{k * -1d}  {odds_a}  {cache_amount}  对冲主 {del_k}");
                                                    while (!bet_success)
                                                    {
                                                        if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                        {
                                                            var wallets = resp_query_amount.wallets;
                                                            foreach (var wallet in wallets)
                                                            {
                                                                string currency_ = wallet.currency;
                                                                if (currency.Equals(currency_))
                                                                {
                                                                    double amount = wallet.amount;
                                                                    if (cache_amount > amount)
                                                                    {
                                                                        cache_amount = amount;
                                                                        home_del_bet.Remove(del_k);
                                                                        Console.WriteLine($"客队：{k * -1d}  {odds_a}  {cache_amount}  对冲主 {del_k}");
                                                                        bet_success = true;
                                                                        break;
                                                                    }

                                                                }
                                                            }
                                                        }
                                                        long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                        if (d_t >= 6800 || bet_success)
                                                        {
                                                            bet_success = true;
                                                            break;
                                                        }
                                                        Thread.Sleep(500);
                                                    }
                                                }


                                            }
                                        }
                                        //客队对冲
                                        foreach (var del_k in away_del_bet.Keys.Reverse())
                                        {
                                            if (k >= del_k)
                                            {
                                                if (bet_home(platform, currency, market, sid, iid, tid, k, away_del_bet[del_k], odds_h, score))
                                                {
                                                    long s_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                    bool bet_success = false;
                                                    Console.WriteLine($"主队下单检测：{k}  {odds_h}  {cache_amount}  对冲客 {del_k * -1d}");
                                                    while (!bet_success)
                                                    {
                                                        if (PlatformHttpService.query_amount(platform, currency, token_, out dynamic resp_query_amount))
                                                        {
                                                            var wallets = resp_query_amount.wallets;
                                                            foreach (var wallet in wallets)
                                                            {
                                                                string currency_ = wallet.currency;
                                                                if (currency.Equals(currency_))
                                                                {
                                                                    double amount = wallet.amount;
                                                                    if (cache_amount > amount)
                                                                    {
                                                                        cache_amount = amount;
                                                                        away_del_bet.Remove(del_k);
                                                                        Console.WriteLine($"主队：{k}  {odds_h}  {cache_amount}  对冲客 {del_k * -1d}");
                                                                        bet_success = true;
                                                                        break;
                                                                    }

                                                                }
                                                            }
                                                        }
                                                        long d_t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - s_time;
                                                        if (d_t >= 6800 || bet_success)
                                                        {
                                                            bet_success = true;
                                                            break;
                                                        }
                                                        Thread.Sleep(500);
                                                    }
                                                }

                                            }

                                        }

                                        //}
                                        cache_k = k;
                                    }

                                }
                            }
                        }
                    }

                }
                catch (Exception)
                {

                }
                Thread.Sleep(150);
            }
        }


        private static bool bet_home(string platform, string currency, string market, int sid, int iid, int tid, double k, double ante, double odds, string score)
        {

            string thirdparty_token = user_info_json.thirdparty_token;
            if (PlatformHttpService.bet(platform, currency, thirdparty_token, sid, iid, tid, market, "h", odds, k, 0, ante, score, out dynamic resp_bet))
            {
                if (resp_bet.submitted.singles.Count > 0)
                {
                    return true;
                }
                else if (resp_bet.failed.singles.Count > 0)
                {
                    Console.WriteLine(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet.failed.singles[0].msg));
                }

            }
            else
            {
                Console.WriteLine(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet));

            }
            return false;
        }

        private static bool bet_away(string platform, string currency, string market, int sid, int iid, int tid, double k, double ante, double odds, string score)
        {

            string thirdparty_token = user_info_json.thirdparty_token;
            if (PlatformHttpService.bet(platform, currency, thirdparty_token, sid, iid, tid, market, "a", odds, k * -1d, 0, ante, score, out dynamic resp_bet))
            {
                if (resp_bet.submitted.singles.Count > 0)
                {
                    return true;
                }
                else if (resp_bet.failed.singles.Count > 0)
                {
                    Console.WriteLine(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet.failed.singles[0].msg));
                }
            }
            else
            {
                Console.WriteLine(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet));

            }
            return false;
        }

        public static int ConvertTimeStringToSeconds(string timeString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(timeString))
                {
                    return 0;
                }
                string[] timeParts = timeString.Split(':');
                if (timeParts.Length != 2)
                {
                    return 0;
                }

                int minutes = int.Parse(timeParts[0]);
                int seconds = int.Parse(timeParts[1]);

                return minutes * 60 + seconds;
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}