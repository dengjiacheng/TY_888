using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetManager
{
    class ScriptService
    {
        private static V8ScriptEngine engine_;
        private static void engine_init()
        {
            if (null == engine_)
            {
                engine_ = new V8ScriptEngine();
            }
            engine_.Execute(ResourceJS.transId);

        }

        public static string transId(string bet_info, string token)
        {
            if (null == engine_)
            {
                engine_init();
            }
            string str = engine_.Script.transId(bet_info + "|" + token);
            return str;
        }
    }
}
