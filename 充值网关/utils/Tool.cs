using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Data;

namespace 充值网关.utils
{
     public static class Tool
    {
        /// <summary>
        /// 判断DataSet是否为空
        /// </summary>
        /// <param name="ds">需要判断的DataSet</param>
        /// <returns>如果ds为空，返回true</returns>
         public static bool DataSetIsNullOrEmpty(DataSet ds)
        {
            bool Flag = false;
            if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables.Count == 1 && ds.Tables[0].Rows.Count == 0))
                Flag = true;
            return Flag;
        }
        /// <summary>
        /// 检测字符串是否为空
        /// </summary>
        /// <param name="textbox"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool checkStr(TextBox textbox, string msg)
        {
            string text = textbox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                textbox.Focus();
                return false;
            }
            return true;
        }
         /// <summary>
         /// 转换为json
         /// </summary>
         /// <param name="code"></param>
         /// <param name="msg"></param>
         /// <returns></returns>
         public static string toJson(int code,string msg){
             JObject postedJObject = new JObject();
             postedJObject.Add("code", code);
             postedJObject.Add("msg", msg);
             String paramString = postedJObject.ToString(Newtonsoft.Json.Formatting.None, null);
             return paramString;
         }
         /// <summary>
         /// 将URL中的参数转换成字典Dictionary<string, string>
         /// </summary>
         /// <param name="formData"></param>
         /// <returns></returns>
         public static Dictionary<String, String> getFormData(string formData)
         {
             //定义字典,将参数按照键值对存入字典中
             Dictionary<String, String> dataDic = new Dictionary<string, string>();
             try
             {
                 //将参数存入字符数组
                 String[] dataArry = formData.Split('&');
                
                 //遍历字符数组
                 for (int i = 0; i <= dataArry.Length - 1; i++)
                 {
                     //当前参数值
                     String dataParm = dataArry[i];
                     //"="的索引值
                     int dIndex = dataParm.IndexOf("=");
                     //参数名作为key
                     String key = dataParm.Substring(0, dIndex);
                     //参数值作为Value
                     String value = dataParm.Substring(dIndex + 1, dataParm.Length - dIndex - 1);
                     if (key != "__VIEWSTATE")
                     {
                         //将参数以键值对存入字典
                         dataDic.Add(key, value);
                     }
                 }
             }
             catch (Exception)
             {
                 
             }
             return dataDic;
         }
    }
}
