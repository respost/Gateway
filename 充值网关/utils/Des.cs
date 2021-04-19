using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace 充值网关
{
    public static class Des
    {
        /// DES算法，加密
        /// param message  待加密字符串
        /// param key      解密私钥
        /// return         加密后的Base64编码字符串
        public static string encrypt(string message, string key)
        {
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    byte[] inputByteArray = Encoding.UTF8.GetBytes(message);
                    des.Key = UTF8Encoding.UTF8.GetBytes(key);
                    des.IV = UTF8Encoding.UTF8.GetBytes(key);
                    des.Mode = System.Security.Cryptography.CipherMode.ECB;
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    string str = Convert.ToBase64String(ms.ToArray());
                    ms.Close();
                    return str;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// DES算法，解密
        /// param message  待解密字符串
        /// param key      解密私钥
        /// return         解密后的字符串
        public static string decrypt(string message, string key)
        {
            try
            {
                byte[] inputByteArray = Convert.FromBase64String(message);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    des.Key = UTF8Encoding.UTF8.GetBytes(key);
                    des.IV = UTF8Encoding.UTF8.GetBytes(key);
                    des.Mode = System.Security.Cryptography.CipherMode.ECB;
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    string str = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    return str;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
