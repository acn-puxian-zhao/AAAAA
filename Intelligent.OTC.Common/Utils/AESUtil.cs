using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Intelligent.OTC.Common.Utils
{
    public class AESUtil
    {
        private static readonly string Key = ConfigurationManager.AppSettings["AESKey"].ToString();

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="data">被加密的明文</param>
        /// <param name="Key">密钥</param>
        /// <returns>密文</returns>
        public static string AESEncrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(data);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(Key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="Data">被解密的密文</param>
        /// <param name="Key">密钥</param>
        /// <returns>明文</returns>
        public static string AESDecrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(data);

            RijndaelManaged rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(Key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
