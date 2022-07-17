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
    class MysqlHelper
    {
        private MySqlConnection conn = null;
        private MySqlCommand command = null;
        private MySqlDataReader reader = null;

        /// <summary>
        /// 构造方法里建议连接
        /// </summary>
        /// <param name="connstr"></param>
        public MysqlHelper(string connstr)
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
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
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
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return result;
        }
        /// <summary>
        /// 存储过程的调用
        /// </summary>
        /// <param name="name">存储过程的名字</param>
        /// <param name="paras">需要传入的参数列表</param>
        /// <returns></returns>
        public DataTable ExecSql(string name, MySqlParameter[] paras = null)
        {
            DataTable dt = new DataTable();
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(name, conn);
                //给命令对象指定 要执行的是存储过程
                cmd.CommandType = CommandType.StoredProcedure;
                if (paras != null)
                {
                    //将参数列表添加到命令对象的参数列表中
                    cmd.Parameters.AddRange(paras);
                }
                MySqlDataAdapter sda = new MySqlDataAdapter(cmd);
                sda.Fill(dt);
            }
            catch (Exception)
            {
            }
            finally
            {
                conn.Close();
            }
            return dt;
        }
        /// <summary>
        /// 增、删、改公共方法
        /// </summary>
        /// <returns></returns>
        public int commonExecute(string sql)
        {
            int res = 0;
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                command = new MySqlCommand(sql, conn);
                if (sql.ToUpper().Contains("CALL"))
                {
                    //可以是“存储过程名称”，也可以是“EXEC 存储过程语句；
                    command.CommandType = CommandType.Text;
                }
                res = command.ExecuteNonQuery();
            }
            catch (MySqlException)
            {
                res = 0;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return res;
        }
        /// <summary>
        /// 查询方法
        /// 注意：尽量不要用select * from table表（返回的数据过长时，DataTable可能会出错），最好指定要查询的字段。
        /// </summary>
        /// <returns></returns>
        public DataTable query(string sql)
        {
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                command = new MySqlCommand(sql, conn);
                DataTable dt = new DataTable();
                using (reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    dt.Load(reader);
                }
                return dt;
            }
            catch (MySqlException)
            {
                return null;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 获取DataSet数据集
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public DataSet GetDataSet(string sql, string tablename)
        {
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                command = new MySqlCommand(sql, conn);
                DataSet dataset = new DataSet();
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                adapter.Fill(dataset, tablename);
                conn.Close();
                return dataset;
            }
            catch (MySqlException)
            {
                return null;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 实现多SQL语句执行的数据库事务
        /// </summary>
        /// <param name="SQLStringList">SQL语句集合（多条语句）</param>
        public bool ExecuteSqlTran(List<string> SQLStringList)
        {
            bool flag = false;
            //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
            if (conn.State == ConnectionState.Open)
                conn.Close();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            //开启事务
            MySqlTransaction tran = this.conn.BeginTransaction();
            cmd.Transaction = tran;//将事务应用于CMD
            try
            {
                foreach (string strsql in SQLStringList)
                {
                    if (strsql.Trim() != "")
                    {
                        cmd.CommandText = strsql;
                        cmd.ExecuteNonQuery();
                    }
                }
                tran.Commit();//提交事务（不提交不会回滚错误）
                flag = true;
            }
            catch (Exception)
            {
                tran.Rollback();
                flag = false;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return flag;
        }

    }
}