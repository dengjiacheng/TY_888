using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetManager
{
    class RedisService
    {
        static CSRedisClient redis = new CSRedis.CSRedisClient("sh-crs-8rir59dg.sql.tencentcdb.com:24088,password=Djc258456,defaultDatabase=8");

        public static void save_login_info(string info)
        {
            redis.LPush("登录信息", info);
        }

        public static long get_login_info_count()
        {
            return redis.LLen("登录信息");
        }
        public static string pop_login_info()
        {
            return redis.LPop("登录信息");
        }

        public static string get_version()
        {
            return redis.HGet("权限管理", "版本信息");
        }
        public static void save_platform_user_info(string platform, string phoneNumber, string user_info)
        {
            redis.HSet(platform, phoneNumber, user_info);
        }
        public static string get_platform_user_info(string platform, string phoneNumber)
        {
            return redis.HGet(platform, phoneNumber);
        }


        public static Dictionary<string, string> get_platform_users_info(string platform)
        {
            return redis.HGetAll(platform);
        }

        public static void del_platform_user_info(string platform, string phoneNumber)
        {
            redis.HDel(platform, phoneNumber);
        }
    }

}
