using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace 充值网关
{
    class MysqlBase
    {
        private MySqlConnection conn = null;
        private MySqlCommand command = null;
        private MySqlDataReader reader = null;

        /// <summary>
        /// 构造方法里建议连接
        /// </summary>
        /// <param name="connstr"></param>
        public MysqlBase(string connstr)
        {
            conn = new MySqlConnection(connstr);
        }
        /// <summary>
        /// 检测数据库连接状态
        /// </summary>
        /// <returns></returns>
        public bool CheckConnectStatus()
        {
            bool result = false;
            try
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    result = true;
                }

            }
            catch
            {
                result = false;
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
        /// <summary>
        /// 发送指令
        /// </summary>
        /// <param name="sql"></param>
        public void CreateCommand(string sql)
        {
            conn.Open();
            command = new MySqlCommand(sql, conn);
        }
        /// <summary>
        /// 增、删、改公共方法
        /// </summary>
        /// <returns></returns>
        public int commonExecute()
        {
            int res = -1;
            try
            {
                res = command.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("操作失败!" + ex.Message);
            }
            conn.Close();
            return res;
        }
        /// <summary>
        /// 查询方法
        /// 注意：尽量不要用select * from table表（返回的数据过长时，DataTable可能会出错），最好指定要查询的字段。
        /// </summary>
        /// <returns></returns>
        public DataTable selectExecute()
        {
            DataTable dt = new DataTable();
            using (reader = command.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(reader);
            }
            return dt;
        }

    }
}