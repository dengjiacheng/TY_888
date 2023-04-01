using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager
{
    class AlgorithmService
    {

        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>密文</returns>
        public static string EncodeBase64(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        /// <param name="base64EncodedData">密文</param>
        /// <returns>明文</returns>
        public static string DecodeBase64(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string ChangeClientNonce(string clientNonce)
        {
            try
            {
                var nonce = JsonConvert.DeserializeObject<dynamic>(DecodeBase64(clientNonce));
                int x = nonce.point.x;
                int y = nonce.point.y;
                nonce.point.x = x;
                nonce.point.y = y;
                return EncodeBase64(JsonConvert.SerializeObject(nonce));
            }
            catch (Exception)
            {

                return clientNonce;
            }
        }

    }
}
