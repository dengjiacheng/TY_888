using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AccountManager
{
    public partial class Form1 : UIForm
    {
        HashSet<string> platform_set = new HashSet<string>() { "9393", "1717", "8868", "6686", "6566" };
        Dictionary<string, Dictionary<string, dynamic>> unSettlements_info = new Dictionary<string, Dictionary<string, dynamic>>() {
            { "9393",new Dictionary<string, dynamic>()},
            { "1717",new Dictionary<string, dynamic>()},
            { "8868",new Dictionary<string, dynamic>()},
            { "6686",new Dictionary<string, dynamic>()},
            { "6566",new Dictionary<string, dynamic>()},
        };
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
                 {
                     manager_accounts();
                 }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                query_accounts("9393");
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                query_accounts("1717");
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                query_accounts("8868");
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                query_accounts("6686");
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
                query_accounts("6566");
            }, TaskCreationOptions.LongRunning);

        }



        private void clear_users_info_view()
        {
            Action actionDelegate = () =>
            {
                uiDataGridView_users_info.Rows.Clear();
            };
            if (uiDataGridView_users_info.InvokeRequired)
            {
                uiDataGridView_users_info.Invoke(actionDelegate);
            }
            else
            {
                actionDelegate();
            }
        }
        private void add_users_info_view(string platform, string phoneNumber, string amount, string settlementCount, string unSettlementCount)
        {
            Action<string, string, string, string, string> actionDelegate = (platform_, phoneNumber_, amount_, settlementCount_, unSettlementCount_) =>
            {
                uiDataGridView_users_info.Rows.Add(platform_, phoneNumber_, amount_, settlementCount_, unSettlementCount_);
            };
            if (uiDataGridView_users_info.InvokeRequired)
            {
                uiDataGridView_users_info.Invoke(actionDelegate, platform, phoneNumber, amount, settlementCount, unSettlementCount);
            }
            else
            {
                actionDelegate(platform, phoneNumber, amount, settlementCount, unSettlementCount);
            }
        }
        private void query_users_info()
        {

            clear_users_info_view();
            foreach (string platform in platform_set)
            {
                Dictionary<string, string> users_info = RedisService.get_platform_users_info(platform);
                foreach (KeyValuePair<string, string> kvp in users_info)
                {
                    string phoneNumber = kvp.Key;
                    string user_info = kvp.Value;
                    var user_info_json = JsonConvert.DeserializeObject<dynamic>(user_info);
                    string amount = user_info_json.amount;
                    string settlementCount = user_info_json.settlementCount;
                    string unSettlementCount = user_info_json.unSettlementCount;
                    add_users_info_view(platform, phoneNumber, amount, settlementCount, unSettlementCount);
                }
            }

        }
        private void query_accounts(string platform)
        {
            while (true)
            {
                Dictionary<string, string> users_info = RedisService.get_platform_users_info(platform);
                foreach (KeyValuePair<string, string> kvp in users_info)
                {
                    string phoneNumber = kvp.Key;
                    string user_info = kvp.Value;
                    var user_info_json = JsonConvert.DeserializeObject<dynamic>(user_info);
                    string token = user_info_json.token;
                    if (PlatformHttpService.query_amount(platform, token, out dynamic resp_query_amount))
                    {
                        var wallets = resp_query_amount.wallets;
                        foreach (var wallet in wallets)
                        {
                            string currency = wallet.currency;
                            if ("USDT_TRC20".Equals(currency))
                            {
                                user_info_json.amount = wallet.amount;
                                break;
                            }
                        }
                    }
                    else
                    {

                        OutputLog(String.Format("{0} {1} {2} {3}", platform, phoneNumber, "查询余额失败", resp_query_amount));
                        if (null != resp_query_amount && (Convert.ToString(resp_query_amount).Contains("token") || Convert.ToString(resp_query_amount).Contains("Token")))
                        {
                            RedisService.del_platform_user_info(platform, phoneNumber);
                            continue;
                        }

                    }
                    Thread.Sleep(5000);
                    if (PlatformHttpService.query_orders(platform, token, out dynamic resp_query_orders))
                    {
                        unSettlements_info[platform][phoneNumber] = resp_query_orders.unSettlement.data;
                        user_info_json.settlementCount = resp_query_orders.settlement.summary.count;
                        user_info_json.totalNetWin = resp_query_orders.settlement.summary.totalNetWin;
                        user_info_json.unSettlementCount = resp_query_orders.unSettlement.summary.count;
                    }
                    else
                    {
                        OutputLog(String.Format("{0} {1} {2} {3}", platform, phoneNumber, "查询订单失败", resp_query_orders));
                        if (null != resp_query_orders && Convert.ToString(resp_query_orders).Contains("token"))
                        {
                            RedisService.del_platform_user_info(platform, phoneNumber);
                            continue;
                        }
                    }

                    RedisService.save_platform_user_info(platform, phoneNumber, JsonConvert.SerializeObject(user_info_json));
                    Thread.Sleep(5000);
                }
                Thread.Sleep(5000);
            }

        }
        private void manager_accounts()
        {
            while (true)
            {
                long info_count = RedisService.get_login_info_count();
                for (int i = 0; i < info_count; i++)
                {
                    string login_info = RedisService.pop_login_info();
                    var login_info_json = JsonConvert.DeserializeObject<dynamic>(login_info);
                    string platform = login_info_json.platform;
                    string phoneNumber = login_info_json.phoneNumber;
                    RedisService.save_platform_user_info(platform, phoneNumber, login_info);
                }
                Thread.Sleep(1000);
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

        private void uiButton_sendOpt_Click(object sender, EventArgs e)
        {
            string phoneNumber = uiTextBox_account.Text.Trim();
            string platform = uiComboBox_platform.Text.Trim();
            Task.Factory.StartNew(() =>
            {
                ChangeBtnStatus(uiButton_sendOpt);

                if (PlatformHttpService.nonce(platform, out dynamic resp_nonce))
                {
                    string clientNonce = resp_nonce.clientNonce;

                    if (PlatformHttpService.sendOtpSms(platform, phoneNumber, AlgorithmService.ChangeClientNonce(clientNonce), out dynamic resp_sendOtpSms))
                    {
                        OutputLog(String.Format("{0} {1} {2} ", platform, phoneNumber, "发送验证码成功"));
                    }
                    else
                    {
                        OutputLog(String.Format("{0} {1} {2} {3}", platform, phoneNumber, "发送验证码失败", resp_nonce));
                    }
                }
                else
                {
                    OutputLog(String.Format("{0} {1} {2} {3}", platform, phoneNumber, "滑块获取失败", resp_nonce));
                }

                ChangeBtnStatus(uiButton_sendOpt);

            });

        }

        private void uiButton_login_Click(object sender, EventArgs e)
        {
            string phoneNumber = uiTextBox_account.Text.Trim();
            string otpcode = uiTextBox_otpcode.Text.Trim();
            string platform = uiComboBox_platform.Text.Trim();
            Task.Factory.StartNew(() =>
            {
                ChangeBtnStatus(uiButton_login);
                if (PlatformHttpService.login(platform, phoneNumber, otpcode, out dynamic resp_login))
                {
                    string account = resp_login.account;
                    string token = resp_login.token;
                    string refreshToken = resp_login.refreshToken;
                    dynamic userJson = new ExpandoObject();
                    userJson.platform = platform;
                    userJson.account = account;
                    userJson.token = token;
                    userJson.refreshToken = refreshToken;
                    userJson.phoneNumber = phoneNumber;
                    RedisService.save_login_info(JsonConvert.SerializeObject(userJson));
                    OutputLog(String.Format("{0} {1} {2}", platform, phoneNumber, "登录成功"));
                }
                else
                {
                    OutputLog(String.Format("{0} {1} {2} {3}", platform, phoneNumber, "登录失败", resp_login));
                }

                ChangeBtnStatus(uiButton_login);

            });
        }

        private void uiButton_query_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                ChangeBtnStatus(uiButton_query);
                query_users_info();
                ChangeBtnStatus(uiButton_query);
            });
        }


        private void clear_unSettlements_info_view()
        {
            Action actionDelegate = () =>
            {
                uiDataGridView_unSettlements.Rows.Clear();
            };
            if (uiDataGridView_unSettlements.InvokeRequired)
            {
                uiDataGridView_unSettlements.Invoke(actionDelegate);
            }
            else
            {
                actionDelegate();
            }
        }
        private void add_unSettlements_info_view(dynamic unSettlements_info)
        {
            Action<dynamic> actionDelegate = (unSettlements_info_) =>
            {
                foreach (var unSettlement in unSettlements_info_)
                {
                    var gameContent = unSettlement.gameContent;
                    double ante = gameContent.ante;
                    var details = gameContent.detail;
                    foreach (var detail in details)
                    {
                        string betOn = detail.betOn;
                        string market = detail.market;
                        string homeName = detail.homeName;
                        string awayName = detail.awayName;
                        string homeScore = detail.homeScore;
                        string awayScore = detail.awayScore;
                        string k = detail.k;
                        string odds = detail.odds;
                        string betTeamName = betOn.Equals("h") ? homeName : awayName;
                        uiDataGridView_unSettlements.Rows.Add($"{homeName} VS {awayName} -{market}", $"{betTeamName}{k}@{odds}@{homeScore}-{awayScore}", ante);
                    }
                }

            };
            if (uiDataGridView_unSettlements.InvokeRequired)
            {
                uiDataGridView_unSettlements.Invoke(actionDelegate, unSettlements_info);
            }
            else
            {
                actionDelegate(unSettlements_info);
            }
        }

        private void uiDataGridView_users_info_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                string platform = Convert.ToString(uiDataGridView_users_info.Rows[e.RowIndex].Cells[0].Value.ToString());
                string phoneNumber = Convert.ToString(uiDataGridView_users_info.Rows[e.RowIndex].Cells[1].Value.ToString());
                clear_unSettlements_info_view();
                if (unSettlements_info[platform].ContainsKey(phoneNumber))
                {
                    var unSettlement_info = unSettlements_info[platform][phoneNumber];
                    add_unSettlements_info_view(unSettlement_info);
                }


            }
        }
    }
}
