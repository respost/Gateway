using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace 充值网关
{
    /// <summary>
    /// 数据库连接池
    /// </summary>
    class SqlDbHelper
    {
        private const bool Asyn_Process = true;//设置异步访问数据库
        private string connectionString = "";//连接字符串
        private SqlConnection conn = null;//连接对象

        public SqlDbHelper(string serverStr)//构造函数
        {
            try
            {
                connectionString = serverStr;
                conn = new SqlConnection(connectionString);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 检测数据库连接状态
        /// </summary>
        /// <returns></returns>
        public bool CheckConnectStatus()
        {
            bool IsCanConnectioned = false;
            try
            {
                conn.Open();
                if (conn.State == ConnectionState.Open)
                {
                    IsCanConnectioned = true;
                }

            }
            catch
            {
                //打开不成功 则连接不成功
                IsCanConnectioned = false;
            }
            finally
            {

                if (conn != null)
                {
                    conn.Close();
                }
            }
            return IsCanConnectioned;
        }
        /// <summary>
        /// 查询sql语句是否有结果返回
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public object GetSingle(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.SqlClient.SqlException){
                        connection.Close();
                        return null;
                    }
                }
            }
        }
        /// <summary>
        /// 增删改公用方法
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /*
         * 调用方式：
           SqlDbHelper sqlDbHelper=new SqlDbHelper();     
           SqlParameter[] pars = new SqlParameter[] {
                new SqlParameter("@a",password),
                new SqlParameter("@b",name),
                new SqlParameter("@c",id)
            };       
            list.Add("update User set password=@a , name=@b where id = @c", pars);
         */
        public bool AduExecute(string sqlText, params  SqlParameter[] parameters)
        {
            bool flag = false;

            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                //建立连接，并执行sql语句命令
                SqlCommand command = new SqlCommand(sqlText, conn);
                //如果参数集合不为空则进行遍历
                if (parameters != null)
                {
                    foreach (SqlParameter p in parameters)
                    {
                        command.Parameters.Add(p);
                    }
                }
                int num = command.ExecuteNonQuery();
                if (num > 0)
                    flag = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                conn.Close();
            }
            return flag;
        }
        /// <summary>
        /// 查询,返回DataSet
        /// </summary>
        /// <param name="sqlText">查询语句</param>
        /// <returns>DataSet</returns>
        /*
         * 调用示例：
            SqlDbHelper sqlDbHelper=new SqlDbHelper(); 
            string sql = "select id,name,age from  student";
            DataSet ds = sqlDbHelper.QueryDataSet(sql);
         */
        public DataSet GetDataSet(string sqlText)
        {
            DataSet ds = new DataSet();
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlText, conn);
                dataAdapter.Fill(ds, "ds");
            }
            catch (Exception)
            {
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public int ExecuteSql(string SQLString)
        {
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                //建立连接，并执行sql语句命令
                SqlCommand command = new SqlCommand(SQLString, conn);
                int num = command.ExecuteNonQuery();
                  return num;
            }
            catch (Exception)
            {
                return 0;
            }
            finally
            {
                conn.Close();
            }
            /*
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.SqlClient.SqlException)
                    {
                        connection.Close();
                        return 0;
                    }
                }
            }
             */
        }
        /// <summary>
        /// 普通查询
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        /*
         * 调用示例：
            string sql = "select id,name,age from  student";
            SqlDataReader dr = sqlDbHelper.Query(sql);
            while (dr.Read())
            {               
                int id=Convert.ToInt32(dr[0].ToString());
                string name = dr[1].ToString();
                int age=Convert.ToInt32(dr[2].ToString());
            }
         */
        public SqlDataReader Query(string sqlText)
        {
            SqlDataReader dr = null;
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                //建立连接，并执行sql语句命令
                SqlCommand command = new SqlCommand(sqlText, conn);
                //CommandBehavior.CloseConnection会自动关闭连接
                dr = command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception)
            {
                dr = null;
            }
            return dr;
        }
        /// <summary>
        /// 查询一个结果
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /*
         * 调用示例：
         * SqlDbHelper sqlDbHelper=new SqlDbHelper();
         * string sql = "select id,name,age from  student";
         * string name=sqlDbHelper.QueryOne(sql,1);
         */
        public string SelectOne(string sqlText, int index)
        {
            string result = "";
            SqlDataReader dr = this.Query(sqlText);
            while (dr.Read())
            {
                result = dr[index].ToString();
                break;
            }
            return result;
        }
        /// <summary>
        /// 获取记录数
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        /*
         * 调用示例：
            SqlDbHelper sqlDbHelper=new SqlDbHelper();
            string sql = "select count(*) from  student";
            int count = sqlDbHelper.Count(sql);
            MessageBox.Show(string.Format("数据库里共有{0}条记录", count), "提示");
         */
        public int Count(string sqlText)
        {
            int result = 0;
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                //建立连接，并执行sql语句命令
                SqlCommand command = new SqlCommand(sqlText, conn);
                result = Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception)
            {
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
        /// <summary>
        /// 实现多SQL语句执行的数据库事务
        /// </summary>
        /// <param name="SQLStringList">SQL语句集合（多条语句）</param>
        public bool SqlTran(List<string> SQLStringList)
        {
            bool flag = false;
            try
            {
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                //开启事务
                SqlTransaction tran = this.conn.BeginTransaction();
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
            }
            catch (Exception)
            {
            }
            finally
            {
                conn.Close();
            }
            return flag;
        }
    }
}

