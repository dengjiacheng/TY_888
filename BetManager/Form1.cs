using Fleck;
using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetManager
{
    public partial class Form1 : UIForm
    {
        HashSet<string> platform_set = new HashSet<string>() { "9393", "1717", "8868", "6686", "6566" };
        Dictionary<string, dynamic> platform_user = new Dictionary<string, dynamic>();
        int current_sid = 0; int current_tid = 0; int current_iid = 0; string current_market = "ah"; dynamic current_bet;
        int home_score_365 = 0; int away_score_365 = 0; string pre_bet_score = "";
        Dictionary<string, int> bet_team = new Dictionary<string, int>() {
            { "9393",0},
            { "8868",0},
            { "1717",0},
            { "6686",0},
            { "6566",0}
        };

        //--------------自动计算相关参数
        double current_k = 999;
        double home_optimal_k = 999;
        double away_optimal_k = 999;

        HashSet<double> home_cancel = new HashSet<double>();
        HashSet<double> away_cancel = new HashSet<double>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WebSocketServer server = new WebSocketServer("ws://127.0.0.1:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                };
                socket.OnClose = () =>
                {
                    home_score_365 = 0;
                    away_score_365 = 0;
                    Console.WriteLine("Close!");
                };
                socket.OnMessage = message =>
                {
                    try
                    {
                        // { "team1":{ "name":"福岛Fire Bonds","score":"47"},"team2":{ "name":"Yamagata Wyverns","score":"59"} }
                        var data = JsonConvert.DeserializeObject<dynamic>(message);
                        if (uiCheckBox_change_score.Checked)
                        {
                            away_score_365 = data.team1.score;
                            home_score_365 = data.team2.score;
                        }
                        else
                        {
                            home_score_365 = data.team1.score;
                            away_score_365 = data.team2.score;
                        }

                    }
                    catch (Exception)
                    {
                    }
                    // Console.WriteLine(message);
                };
            });
            Task.Factory.StartNew(() =>
            {
                auto_cal();
            }, TaskCreationOptions.LongRunning);

            if ("1.0".Equals(RedisService.get_version()))
            {
                OutputLog("当前版本:1.0");
                Task.Factory.StartNew(() =>
                {
                    auto_cart();
                }, TaskCreationOptions.LongRunning);

                Task.Factory.StartNew(() =>
                {
                    auto_bet("9393");
                }, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() =>
                {
                    auto_bet("8868");
                }, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() =>
                {
                    auto_bet("1717");
                }, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() =>
                {
                    auto_bet("6686");
                }, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(() =>
                {
                    auto_bet("6566");
                }, TaskCreationOptions.LongRunning);
            }

        }

        private void auto_cal()
        {
            while (true)
            {

                if (uiCheckBox_auto_cal.Checked && null != current_bet)
                {
                    int market_index = Convert.ToInt32(uiTextBox_bet_index.Text);
                    double k = current_bet["market"][current_market][market_index]["k"];
                    double odds_h = current_bet["market"][current_market][market_index]["h"];
                    double odds_a = current_bet["market"][current_market][market_index]["a"];
                    if (odds_h <= 0 || odds_a <= 0)
                    {
                        Thread.Sleep(200);
                        continue;

                    }

                    if (home_optimal_k == 999)
                    {
                        if (away_optimal_k == 999)
                        {
                            OutputLog($"主队：{k}    {odds_h}");
                            if (uiCheckBox_bet_home_9393.Checked)
                            {
                                bet_team["9393"] = 1;
                            }
                            if (uiCheckBox_bet_home_8868.Checked)
                            {
                                bet_team["8868"] = 1;
                            }
                            if (uiCheckBox_bet_home_1717.Checked)
                            {
                                bet_team["1717"] = 1;
                            }
                            if (uiCheckBox_bet_home_6686.Checked)
                            {
                                bet_team["6686"] = 1;
                            }
                            if (uiCheckBox_bet_home_6566.Checked)
                            {
                                bet_team["6566"] = 1;
                            }
                            home_optimal_k = k;
                        }
                        else if (k - away_optimal_k >= 3)
                        {
                            OutputLog($"主队：{k}    {odds_h}");
                            if (uiCheckBox_bet_home_9393.Checked)
                            {
                                bet_team["9393"] = 1;
                            }
                            if (uiCheckBox_bet_home_8868.Checked)
                            {
                                bet_team["8868"] = 1;
                            }
                            if (uiCheckBox_bet_home_1717.Checked)
                            {
                                bet_team["1717"] = 1;
                            }
                            if (uiCheckBox_bet_home_6686.Checked)
                            {
                                bet_team["6686"] = 1;
                            }
                            if (uiCheckBox_bet_home_6566.Checked)
                            {
                                bet_team["6566"] = 1;
                            }
                            home_optimal_k = k;
                        }
                    }

                    if (away_optimal_k == 999)
                    {
                        if (home_optimal_k == 999)
                        {
                            OutputLog($"客队：{k * -1d}    {odds_a}");
                            if (uiCheckBox_bet_away_9393.Checked)
                            {
                                bet_team["9393"] = 2;
                            }
                            if (uiCheckBox_bet_away_8868.Checked)
                            {
                                bet_team["8868"] = 2;
                            }
                            if (uiCheckBox_bet_away_1717.Checked)
                            {
                                bet_team["1717"] = 2;
                            }
                            if (uiCheckBox_bet_away_6686.Checked)
                            {
                                bet_team["6686"] = 2;
                            }
                            if (uiCheckBox_bet_away_6566.Checked)
                            {
                                bet_team["6566"] = 2;
                            }
                            away_optimal_k = k;
                        }
                        else if (home_optimal_k - k >= 3)
                        {
                            OutputLog($"客队：{k * -1d}    {odds_a}");
                            if (uiCheckBox_bet_away_9393.Checked)
                            {
                                bet_team["9393"] = 2;
                            }
                            if (uiCheckBox_bet_away_8868.Checked)
                            {
                                bet_team["8868"] = 2;
                            }
                            if (uiCheckBox_bet_away_1717.Checked)
                            {
                                bet_team["1717"] = 2;
                            }
                            if (uiCheckBox_bet_away_6686.Checked)
                            {
                                bet_team["6686"] = 2;
                            }
                            if (uiCheckBox_bet_away_6566.Checked)
                            {
                                bet_team["6566"] = 2;
                            }
                            away_optimal_k = k;
                        }
                    }

                    if (k != current_k)
                    {

                        //主队优势
                        if (k < current_k)
                        {

                            //替换最优值
                            if (home_optimal_k < k && k - home_optimal_k >= 2 && home_optimal_k != 999)
                            {
                                OutputLog($"主队：{k}    {odds_h}");
                                if (uiCheckBox_bet_home_9393.Checked)
                                {
                                    bet_team["9393"] = 1;
                                }
                                if (uiCheckBox_bet_home_8868.Checked)
                                {
                                    bet_team["8868"] = 1;
                                }
                                if (uiCheckBox_bet_home_1717.Checked)
                                {
                                    bet_team["1717"] = 1;
                                }
                                if (uiCheckBox_bet_home_6686.Checked)
                                {
                                    bet_team["6686"] = 1;
                                }
                                if (uiCheckBox_bet_home_6566.Checked)
                                {
                                    bet_team["6566"] = 1;
                                }
                                home_cancel.Add(home_optimal_k);
                                home_optimal_k = k;

                            }

                        }
                        //客队优势
                        else if (k > current_k)
                        {

                            //替换最优值
                            if (away_optimal_k > k && away_optimal_k - k >= 2 && away_optimal_k != 999)
                            {
                                OutputLog($"客队：{k * -1d}    {odds_a}");
                                if (uiCheckBox_bet_away_9393.Checked)
                                {
                                    bet_team["9393"] = 2;
                                }
                                if (uiCheckBox_bet_away_8868.Checked)
                                {
                                    bet_team["8868"] = 2;
                                }
                                if (uiCheckBox_bet_away_1717.Checked)
                                {
                                    bet_team["1717"] = 2;
                                }
                                if (uiCheckBox_bet_away_6686.Checked)
                                {
                                    bet_team["6686"] = 2;
                                }
                                if (uiCheckBox_bet_away_6566.Checked)
                                {
                                    bet_team["6566"] = 2;
                                }
                                away_cancel.Add(away_optimal_k);
                                away_optimal_k = k;

                            }


                        }
                        if (home_cancel.Contains(k))
                        {
                            OutputLog($"客队：{k * -1d}   对冲  {odds_a}");
                            if (uiCheckBox_bet_away_9393.Checked)
                            {
                                bet_team["9393"] = 2;
                            }
                            if (uiCheckBox_bet_away_8868.Checked)
                            {
                                bet_team["8868"] = 2;
                            }
                            if (uiCheckBox_bet_away_1717.Checked)
                            {
                                bet_team["1717"] = 2;
                            }
                            if (uiCheckBox_bet_away_6686.Checked)
                            {
                                bet_team["6686"] = 2;
                            }
                            if (uiCheckBox_bet_away_6566.Checked)
                            {
                                bet_team["6566"] = 2;
                            }
                            home_cancel.Remove(k);
                        }
                        if (away_cancel.Contains(k))
                        {
                            OutputLog($"主队：{k}   对冲  {odds_h}");
                            if (uiCheckBox_bet_home_9393.Checked)
                            {
                                bet_team["9393"] = 1;
                            }
                            if (uiCheckBox_bet_home_8868.Checked)
                            {
                                bet_team["8868"] = 1;
                            }
                            if (uiCheckBox_bet_home_1717.Checked)
                            {
                                bet_team["1717"] = 1;
                            }
                            if (uiCheckBox_bet_home_6686.Checked)
                            {
                                bet_team["6686"] = 1;
                            }
                            if (uiCheckBox_bet_home_6566.Checked)
                            {
                                bet_team["6566"] = 1;
                            }
                            away_cancel.Remove(k);
                        }
                        current_k = k;
                    }
                }

                Thread.Sleep(200);
            }
        }

        private void auto_bet(string platform)
        {
            while (true)
            {
                if (platform_user.ContainsKey(platform) && bet_team[platform] > 0)
                {
                    switch (bet_team[platform])
                    {
                        case 1:
                            bet_home(platform);
                            break;
                        case 2:
                            bet_away(platform);
                            break;
                        default:
                            break;
                    }
                    bet_team[platform] = -1;
                }
                Thread.Sleep(20);
            }
        }
        private void bet_home(string platform)
        {
            int market_index = Convert.ToInt32(uiTextBox_bet_index.Text);
            string beton = string.Empty;
            if (current_market.StartsWith("ah"))
            {
                beton = "h";
            }
            else if (current_market.StartsWith("ou"))
            {
                beton = "ov";
            }
            string k = current_bet["market"][current_market][market_index]["k"] ?? "";
            int ante = Convert.ToInt32(uiTextBox_bet_ante.Text);
            double odds = current_bet["market"][current_market][market_index][beton];
            string score = current_bet.detail.score;
            string thirdparty_token = platform_user[platform].thirdparty_token;
            if (PlatformHttpService.bet(platform, thirdparty_token, current_sid, current_iid, current_tid, current_market, beton, odds, k, market_index, ante, score, out dynamic resp_bet))
            {
                if (resp_bet.submitted.singles.Count > 0)
                {
                    OutputLog(String.Format("{0} {1} {2} {3}-{4}", platform, "订单提交成功", resp_bet.submitted.singles[0].orderno, k, score));
                }
                else if (resp_bet.failed.singles.Count > 0)
                {
                    OutputLog(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet.failed.singles[0].msg));
                }
            }
            else
            {
                OutputLog(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet));
                if (null != resp_bet && Convert.ToString(resp_bet).Equals("token expired"))
                {

                }
            }


        }

        private void bet_away(string platform)
        {
            int market_index = Convert.ToInt32(uiTextBox_bet_index.Text);
            string beton = string.Empty;
            if (current_market.StartsWith("ah"))
            {
                beton = "a";
            }
            else if (current_market.StartsWith("ou"))
            {
                beton = "ud";
            }
            string k = current_bet["market"][current_market][market_index]["k"] ?? "";
            if (k.StartsWith("+"))
            {
                k = k.Replace("+", "-");
            }
            else
            {
                if (k.StartsWith("-"))
                {
                    k = k.Replace("-", "+");
                }
            }

            int ante = Convert.ToInt32(uiTextBox_bet_ante.Text);
            double odds = current_bet["market"][current_market][market_index][beton];
            string score = current_bet.detail.score;
            string thirdparty_token = platform_user[platform].thirdparty_token;
            if (PlatformHttpService.bet(platform, thirdparty_token, current_sid, current_iid, current_tid, current_market, beton, odds, k, market_index, ante, score, out dynamic resp_bet))
            {
                if (resp_bet.submitted.singles.Count > 0)
                {
                    OutputLog(String.Format("{0} {1} {2} {3}-{4}", platform, "订单提交成功", resp_bet.submitted.singles[0].orderno, k, score));
                }
                else if (resp_bet.failed.singles.Count > 0)
                {
                    OutputLog(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet.failed.singles[0].msg));
                }
            }
            else
            {
                OutputLog(String.Format("{0} {1} {2}", platform, "订单提交失败", resp_bet));
                if (null != resp_bet && Convert.ToString(resp_bet).Equals("token expired"))
                {

                }
            }
        }
        private void ChangeBtnStatus(UIButton btn)
        {
            Action<UIButton> actionDelegate = (btn_) => { btn_.Enabled = !btn_.Enabled; ; };
            if (btn.InvokeRequired)
            {
                btn.Invoke(actionDelegate, btn);
            }
            else
            {
                actionDelegate(btn);
            }
        }

        private void OutputLog(string log_info)
        {
            Action<string> actionDelegate = (log_info_) =>
            {
                uiRichTextBox_log.Text = $"【{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}】  {log_info_}\n{uiRichTextBox_log.Text}";
            };
            if (uiRichTextBox_log.InvokeRequired)
            {
                uiRichTextBox_log.Invoke(actionDelegate, log_info);
            }
            else
            {
                actionDelegate(log_info);
            }
        }
        private void uiButton_login_Click(object sender, EventArgs e)
        {
            string phoneNumber = uiTextBox_account.Text.Trim();
            Task.Factory.StartNew(() =>
            {
                ChangeBtnStatus(uiButton_login);
                platform_user.Clear();
                foreach (string platform in platform_set)
                {
                    string user_info = RedisService.get_platform_user_info(platform, phoneNumber);
                    if (string.IsNullOrWhiteSpace(user_info))
                    {
                        OutputLog(String.Format("{0} {1} {2} ", platform, phoneNumber, "平台未登录"));
                        continue;
                    }
                    var user_info_json = JsonConvert.DeserializeObject<dynamic>(user_info);
                    string token = user_info_json.token;
                    if (PlatformHttpService.thirdparty(platform, token, out dynamic resp_thirdparty))
                    {
                        user_info_json.thirdparty_token = resp_thirdparty.token;
                        platform_user[platform] = user_info_json;
                        OutputLog(String.Format("{0} {1} {2} ", platform, phoneNumber, "第三方登录成功"));
                    }
                    else
                    {
                        OutputLog(String.Format("{0} {1} {2} {3}", platform, phoneNumber, "第三方登录失败", resp_thirdparty));
                    }
                }
                ChangeBtnStatus(uiButton_login);

            });
        }
        private void clear_tournaments_data()
        {
            Action actionDelegate = () => { uiDataGridView_tournaments.Rows.Clear(); };
            if (uiDataGridView_tournaments.InvokeRequired)
            {
                uiDataGridView_tournaments.Invoke(actionDelegate);
            }
            else
            {
                actionDelegate();
            }
        }
        private void add_tournaments_data(dynamic tournaments)
        {
            Action<dynamic> actionDelegate = (tournaments_) =>
            {
                foreach (var tournament in tournaments_)
                {
                    var matches = tournament.matches;
                    foreach (var matche in matches)
                    {
                        string tnName = matche.tnName;
                        string sid = matche.sid;
                        string tid = matche.tid;
                        string iid = matche.iid;
                        string vd = matche.vd;
                        string home_name = matche.home.name;
                        string away_name = matche.away.name;
                        uiDataGridView_tournaments.AddRow(tnName, sid, tid, iid, vd, home_name, away_name);
                    }
                }
            };

            if (uiDataGridView_tournaments.InvokeRequired)
            {
                uiDataGridView_tournaments.Invoke(actionDelegate, tournaments);
            }
            else
            {
                actionDelegate(tournaments);
            }
        }
        private void uiButton_load_basketball_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                ChangeBtnStatus(uiButton_load_basketball);
                clear_tournaments_data();
                if (PlatformHttpService.get_tournaments("9393", "2", out dynamic resp_get_tournaments))
                {
                    var tournaments = resp_get_tournaments.tournaments;
                    add_tournaments_data(tournaments);
                    OutputLog(String.Format("查询滚球篮球成功"));
                }
                else
                {
                    OutputLog(String.Format("{0} {1}", "查询滚球篮球失败", resp_get_tournaments));
                }
                ChangeBtnStatus(uiButton_load_basketball);
            });
        }

        private void uiDataGridView_tournaments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                current_sid = Convert.ToInt32(uiDataGridView_tournaments.Rows[e.RowIndex].Cells[1].Value.ToString());
                current_tid = Convert.ToInt32(uiDataGridView_tournaments.Rows[e.RowIndex].Cells[2].Value.ToString()); ;
                current_iid = Convert.ToInt32(uiDataGridView_tournaments.Rows[e.RowIndex].Cells[3].Value.ToString()); ;
            }
        }
        private void clear_cart_data()
        {
            Action actionDelegate = () => { uiDataGridView_cart_info.Rows.Clear(); };
            if (uiDataGridView_cart_info.InvokeRequired)
            {

                uiDataGridView_cart_info.Invoke(actionDelegate);
            }
            else
            {
                actionDelegate();
            }
        }

        private void add_cart_data(int index, string info, string score, string score_365, string ts)
        {
            Action<int, string, string, string, string> actionDelegate = (index_, info_, score_, score_365_, ts_) =>
            {
                uiDataGridView_cart_info.AddRow(index_, info_, score_, score_365_, ts_);
            };
            if (uiDataGridView_cart_info.InvokeRequired)
            {
                uiDataGridView_cart_info.Invoke(actionDelegate, index, info, score, score_365, ts);
            }
            else
            {
                actionDelegate(index, info, score, score_365, ts);
            }
        }
        private void auto_cart()
        {
            while (true)
            {
                try
                {
                    if (current_sid > 0 && !string.IsNullOrWhiteSpace(current_market))
                    {
                        if (PlatformHttpService.cart("9393", current_sid, current_iid, current_market, out dynamic resp_cart))
                        {
                            clear_cart_data();
                            string ts = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds.ToString();
                            var bets = resp_cart.bets;
                            foreach (var bet in bets)
                            {
                                current_bet = bet;
                                string k = current_bet["market"][current_market][Convert.ToInt32(uiTextBox_bet_index.Text)]["k"] ?? "";
                                var detail = bet.detail;
                                string score = detail.score;
                                var market = bet.market;
                                if (null != score && null != market[current_market])
                                {
                                    var handicaps = market[current_market];
                                    int handicap_index = 0;
                                    foreach (var handicap in handicaps)
                                    {
                                        var scores = score.Split("-");
                                        int home_score = Convert.ToInt32(scores[0]);
                                        int away_score = Convert.ToInt32(scores[1]);
                                        string score_365 = home_score_365 + "-" + away_score_365;
                                        if ((home_score_365 - home_score >= Convert.ToInt32(uiTextBox_score_difference.Text) || away_score_365 - away_score >= Convert.ToInt32(uiTextBox_score_difference.Text)) && !score.Equals(pre_bet_score))
                                        {
                                            pre_bet_score = score;
                                            OutputLog("score：" + score + ",score_365：" + score_365 + ",k：" + k);
                                            if (home_score_365 > home_score && uiCheckBox_auto_bet.Checked)
                                            {
                                                if (uiCheckBox_bet_home_9393.Checked)
                                                {
                                                    bet_team["9393"] = 1;
                                                }
                                                if (uiCheckBox_bet_home_8868.Checked)
                                                {
                                                    bet_team["8868"] = 1;
                                                }
                                                if (uiCheckBox_bet_home_1717.Checked)
                                                {
                                                    bet_team["1717"] = 1;
                                                }
                                                if (uiCheckBox_bet_home_6686.Checked)
                                                {
                                                    bet_team["6686"] = 1;
                                                }
                                                if (uiCheckBox_bet_home_6566.Checked)
                                                {
                                                    bet_team["6566"] = 1;
                                                }

                                            }
                                            else if (away_score_365 > away_score && uiCheckBox_auto_bet.Checked)
                                            {

                                                if (uiCheckBox_bet_away_9393.Checked)
                                                {
                                                    bet_team["9393"] = 2;
                                                }
                                                if (uiCheckBox_bet_away_8868.Checked)
                                                {
                                                    bet_team["8868"] = 2;
                                                }
                                                if (uiCheckBox_bet_away_1717.Checked)
                                                {
                                                    bet_team["1717"] = 2;
                                                }
                                                if (uiCheckBox_bet_away_6686.Checked)
                                                {
                                                    bet_team["6686"] = 2;
                                                }
                                                if (uiCheckBox_bet_away_6566.Checked)
                                                {
                                                    bet_team["6566"] = 2;
                                                }

                                            }
                                        }
                                        add_cart_data(handicap_index, JsonConvert.SerializeObject(handicap), score, score_365, ts);
                                        handicap_index++;
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.Left:
                    if (uiCheckBox_bet_home_9393.Checked)
                    {
                        bet_team["9393"] = 1;
                    }
                    if (uiCheckBox_bet_home_8868.Checked)
                    {
                        bet_team["8868"] = 1;
                    }
                    if (uiCheckBox_bet_home_1717.Checked)
                    {
                        bet_team["1717"] = 1;
                    }
                    if (uiCheckBox_bet_home_6686.Checked)
                    {
                        bet_team["6686"] = 1;
                    }
                    if (uiCheckBox_bet_home_6566.Checked)
                    {
                        bet_team["6566"] = 1;
                    }
                    break;
                case System.Windows.Forms.Keys.Right:
                    if (uiCheckBox_bet_away_9393.Checked)
                    {
                        bet_team["9393"] = 2;
                    }
                    if (uiCheckBox_bet_away_8868.Checked)
                    {
                        bet_team["8868"] = 2;
                    }
                    if (uiCheckBox_bet_away_1717.Checked)
                    {
                        bet_team["1717"] = 2;
                    }
                    if (uiCheckBox_bet_away_6686.Checked)
                    {
                        bet_team["6686"] = 2;
                    }
                    if (uiCheckBox_bet_away_6566.Checked)
                    {
                        bet_team["6566"] = 2;
                    }
                    break;
            }
        }

        private void uiButton_clear_Click(object sender, EventArgs e)
        {
            uiRichTextBox_log.Clear();
        }

        private void uiComboBox_market_SelectedIndexChanged(object sender, EventArgs e)
        {
            current_market = uiComboBox_market.SelectedItem.ToString();
        }

        private void uiCheckBox_auto_cal_CheckedChanged(object sender, EventArgs e)
        {
            if (uiCheckBox_auto_cal.Checked)
            {

                current_k = 999;
                home_optimal_k = 999;
                away_optimal_k = 999;
                home_cancel.Clear();
                away_cancel.Clear();

            }
        }
    }
}
