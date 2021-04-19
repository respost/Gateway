using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using 充值网关.utils;

namespace 充值网关
{
    public partial class Form1 : Form
    {
        //套接字
        private SocketHelper socket = new SocketHelper();
        //代理
        private delegate void SetPos(string info);
        //客户端连接对象
        private Session socketClient;
        //数据库操作对象
        private MysqlBase Db;

        public Form1()
        {
            //加载嵌入资源
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
            ReadAppConfig();
            //右下角显示【注意：代码要放到InitializeComponent()后面】
            //int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Width;
            //int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Height;
            //Point p = new Point(x, y);
            //this.PointToScreen(p);
            //this.Location = p;
        }

        private void ReadAppConfig()
        {
            this.txtPort.Text = AppSettings.GetValue("port");         
            this.txtDbHost.Text = AppSettings.GetValue("DbHost");
            this.txtDbPort.Text = AppSettings.GetValue("DbPort");
            this.txtDbUser.Text = AppSettings.GetValue("DbUser");
            this.txtDbPassword.Text = AppSettings.GetValue("DbPassword");         
            this.txtAppid.Text = AppSettings.GetValue("appid");
            this.txtAppkey.Text = AppSettings.GetValue("appkey");
            this.txtKey.Text = AppSettings.GetValue("key");
            this.txtSql.Text = AppSettings.GetValue("sql");
        }
        /// <summary>
        /// 加载嵌入资源中的全部dll文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }
        /// <summary>
        /// 输出信息
        /// </summary>
        /// <param name="msg"></param>
        private void SetTextMesssage(string msg)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(SetTextMesssage);
                this.Invoke(setpos, new object[] { msg });
            }
            else
            {
                this.txtLog.AppendText(string.Format("[{0}]{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg));
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
           
            if (btnStart.Text == "启动")
            {
                //非空验证
                if (!Tool.checkStr(this.txtPort, "网关端口不能为空"))
                    return;          
                if (!Tool.checkStr(this.txtAppid, "商户编号不能为空"))
                    return;
                if (!Tool.checkStr(this.txtAppkey, "商户密钥不能为空"))
                    return;
                if (!Tool.checkStr(this.txtKey, "算法密钥不能为空"))
                    return;     
                //获取参数
                string txtPort = this.txtPort.Text.Trim();
                string txtKey = this.txtKey.Text.Trim();
                try
                {                  
                    AppSettings.SetValue("port", txtPort);                   
                }
                catch (Exception){}
                //启动Socket
                int port = int.Parse(txtPort);
                if (socket.OpenServer(port))
                {
                    labStatus.Text = "启动中";
                    labStatus.ForeColor = Color.Green;
                }
                else
                {
                    labStatus.Text = "失败";
                    labStatus.ForeColor = Color.Red;
                }
                socket.OnClientSocketChanged += Socket_OnClientSocketChanged;
                socket.getSocketText += Socket_getSocketText;
                btnStart.Text = "停止";
                btnStart.BackColor = Color.Firebrick;
            }
            else
            {
                try
                {
                    socket.CloseServer();
                    socket.OnClientSocketChanged -= Socket_OnClientSocketChanged;
                    socket.getSocketText -= Socket_getSocketText;
                }
                catch (Exception){}
                btnStart.Text = "启动";
                btnStart.BackColor = Color.Green;
                labStatus.Text = "已关闭";
                labStatus.ForeColor = Color.Gray;
            }
        }

        private void Socket_OnClientSocketChanged(int status, Session sesson)
        {
            //获取客户端IP
            sesson.GetIp();
            //获取连接对象
            socketClient = sesson;
        }


        private void Socket_getSocketText(Session sesson,string text)
        {
            //算法密钥
            string key = this.txtKey.Text.Trim();
            //DES解密
            string decode = Des.decrypt(text, key);
            if (decode == string.Empty)
            {
                Error("算法密钥不正确");
                return;
            }
            Dictionary<String, String> data = Tool.getFormData(decode);
            if (data.Count <= 0)
                return;
            string appkey = this.txtAppkey.Text.Trim();
            if (!appkey.Equals(data["appkey"]))
            {
                Error("商户密钥不正确");
                return;
            }
            //创建数据库连接
            string txtDbHost = this.txtDbHost.Text.Trim();
            string txtDbPort = this.txtDbPort.Text.Trim();
            string txtDbUser = this.txtDbUser.Text.Trim();
            string txtDbPassword = this.txtDbPassword.Text.Trim();
            string database = data["database"];
            if (database == string.Empty)
            {
                Error("分区数据库不能为空，请前往平台修改分区设置");
                return;
            }
            string serverStr = String.Format("Database={0};Data Source={1};User Id={2};Password={3};pooling=false;CharSet=utf8;port={4}", database, txtDbHost, txtDbUser, txtDbPassword, txtDbPort);
            Db = new MysqlBase(serverStr);
            //检测数据库连接
            if (!Db.CheckConnectStatus())
            {
                Error("数据库连接失败，请检查网关数据库配置是否正确");
                return;
            }
            //执行充值SQL语句
            string txtSql = this.txtSql.Text.Trim();
            //账号
            string account = data["account"];
            //金额
            double money = Convert.ToDouble(data["money"]);
            //充值比例
            int scale = Convert.ToInt32(data["scale"]);
            //游戏币
            double gold = money * scale;
            //分区
            string quname = data["quname"];
            //游戏币别名
            string alias = data["alias"];
            //订单编号
            string ordernumber = data["ordernumber"];
            //替换SQL的字符
            txtSql = txtSql.Replace("{account}", account);
            txtSql = txtSql.Replace("{money}", money.ToString());
            txtSql = txtSql.Replace("{gold}", gold.ToString());
            txtSql = txtSql.Replace("{order_no}", gold.ToString());
            //执行SQL
            Db.CreateCommand(txtSql);
            int res = Db.commonExecute();
            if (socketClient!=null)
            {             
                if (res>0)
                {
                    //充值成功返回
                    string yxb = gold >= 10000 ? (gold / 10000).ToString() + '万' : gold.ToString();
                    string msg = String.Format("{0}玩家[{1}]成功充值{2}元，获得{3}{4}个。", quname, account, money, alias, yxb);
                    SetTextMesssage(msg);
                    String paramString = Tool.toJson(1, msg);
                    socketClient.Send(System.Text.Encoding.Default.GetBytes(paramString));
                }
                else
                {
                    //充值失败返回
                    string msg = String.Format("{0}玩家[{1}]充值失败，充值金额{2}元，订单编号：{3}", quname, account, money, ordernumber);
                    SetTextMesssage(msg);
                    String paramString = Tool.toJson(0, msg);
                    socketClient.Send(System.Text.Encoding.Default.GetBytes(paramString));
                }
            }
            
        }

        private void Error(string msg)
        {
            SetTextMesssage(msg);
            if (socketClient != null)
            {
                socketClient.Send(System.Text.Encoding.Default.GetBytes(Tool.toJson(0, msg)));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            //数据库IP
            string txtDbHost = this.txtDbHost.Text.Trim();
            if (txtDbHost == string.Empty)
            {
                txtDbHost = "127.0.0.1";
            }
            //数据库端口
            string txtDbPort = this.txtDbPort.Text.Trim();
            if (txtDbPort == string.Empty)
            {
                txtDbHost = "3306";
            }
            //数据库用户
            string txtDbUser = this.txtDbUser.Text.Trim();
            if (txtDbUser == string.Empty)
            {
                txtDbUser = "root";
            }
            //数据库密码
            string txtDbPassword = this.txtDbPassword.Text.Trim();
            if (txtDbPassword == string.Empty)
            {
                txtDbPassword = "root";
            }
            if (!Tool.checkStr(this.txtAppid, "商户编号不能为空"))
                return;
            string txtAppid = this.txtAppid.Text.Trim();
            if (!Tool.checkStr(this.txtAppkey, "商户密钥不能为空"))
                return;
            string txtAppkey = this.txtAppkey.Text.Trim();
            if (!Tool.checkStr(this.txtKey, "算法密钥不能为空"))
                return;
            string txtKey = this.txtKey.Text.Trim();
            if (!Tool.checkStr(this.txtSql, "充值SQL语句不能为空"))
                return;
            string txtSql = this.txtSql.Text.Trim();
            try
            {
                AppSettings.SetValue("DbHost", txtDbHost);
                AppSettings.SetValue("DbPort", txtDbPort);
                AppSettings.SetValue("DbUser", txtDbUser);
                AppSettings.SetValue("DbPassword", txtDbPassword);
                AppSettings.SetValue("appid", txtAppid);
                AppSettings.SetValue("appkey", txtAppkey);
                AppSettings.SetValue("key", txtKey);
                AppSettings.SetValue("sql", txtSql);
                MessageBox.Show("设置成功");
            }
            catch (Exception)
            {
                MessageBox.Show("设置失败");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void labelClearLog_Click(object sender, EventArgs e)
        {
            this.txtLog.Text = "";
            SetTextMesssage("日志已清空");
        }

    }
}
