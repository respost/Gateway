using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using 充值网关.utils;

namespace 充值网关
{
    public partial class Form1 : Form
    {
        /*--------------------软件更新配置 start------------------*/
        private double version = 1.3;
        private string updateUrl = "http://www.18pay.net/gateway/";
        /*--------------------软件更新配置 end--------------------*/
        //套接字
        private SocketHelper socket = new SocketHelper();
        //代理
        private delegate void SetPos(string info);
        //客户端连接对象
        private Session socketClient;
        //数据库操作对象
        private MysqlHelper mysqlHelper;
        private SqlDbHelper sqlDbHelper;
        //数据库类型
        private string dbtype = "mysql";
        //游戏列表
        private Dictionary<int, Game> gameList = new Dictionary<int, Game>();

        public Form1()
        {
            //加载嵌入资源
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
            LoadUpdateFile();
            ReadAppConfig();
            InitGame();
        }

        #region 内存回收
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        private static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Form1.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion
        #region 加载更新程序
        private static void LoadUpdateFile()
        {
            string appPath = Environment.CurrentDirectory + "\\updater.exe";
            if (!File.Exists(appPath))
            {
                FileStream str = new FileStream(appPath, FileMode.Create);
                str.Write(global::充值网关.Properties.Resources.updater, 0, global::充值网关.Properties.Resources.updater.Length);
            }
        }
        #endregion
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
            this.txtTable.Text = AppSettings.GetValue("table");
            this.txtColumnAccount.Text = AppSettings.GetValue("columnAccount");
            this.txtColumnGold.Text = AppSettings.GetValue("columnGold");
            this.txtSql.Text = AppSettings.GetValue("sql");
            dbtype = AppSettings.GetValue("DbType");
            if (!string.IsNullOrEmpty(dbtype) && dbtype.Equals("sqlserver"))
            {
                radioSqlServer.Checked = true;
            }
            else
            {
                radioMysql.Checked = true;
            }
            checkBoxCustomSQL.Checked = Convert.ToBoolean(AppSettings.GetValue("customSQL"));

        }
        private void InitGame()
        {
            Game game1 = new Game(1, "梦幻古龙", "Account", "loginName", "yuanbao", "sql server");
            Game game2 = new Game(2, "三国英雄传", "game_acc", "account", "point", "sql server");
            Game game3 = new Game(3, "热血江湖", "tbl_account", "fld_id", "fld_rxpiont", "sql server");
            Game game4 = new Game(4, "神泣", "users_master", "userid", "point", "sql server");
            Game game5 = new Game(5, "征途", "POINTBONUS0000", "ACCOUNT", "POINT", "mysql");
            Game game6 = new Game(6, "龙驹", "account", "login", "cash", "mysql");
            Game game7 = new Game(7, "蜀门", "t_account", "name", "gd", "mysql");
            Game game8 = new Game(8, "奇迹MU[积分]", "MEMB_INFO", "memb___id", "JF", "sql server");
            Game game9 = new Game(9, "奇迹MU[元宝]", "MEMB_INFO", "memb___id", "myCash", "sql server");

            Game game10 = new Game(10, "奇迹MU[元宝送VIP]", "MEMB_INFO", "memb___id", "myCash", "sql server");
            StringBuilder sb10 = new StringBuilder();
            sb10.AppendLine("update MEMB_INFO set myCash=myCash+{gold} where memb___id='{account}' ");
            sb10.AppendLine("DECLARE @Stat int ");
            sb10.AppendLine("SELECT @Stat=member FROM MEMB_INFO WHERE memb___id = '{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=1 ");
            sb10.AppendLine("Where {gold} >= 100 and @Stat=0 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=2 ");
            sb10.AppendLine("Where {gold} >= 200 and @Stat=1 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=3 ");
            sb10.AppendLine("Where {gold} >= 300 and @Stat=2 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=4 ");
            sb10.AppendLine("Where {gold} >= 400 and @Stat=3 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=5 ");
            sb10.AppendLine("Where {gold} >= 500 and @Stat=4 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=6 ");
            sb10.AppendLine("Where {gold} >= 600 and @Stat=5 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=7 ");
            sb10.AppendLine("Where {gold} >= 700 and @Stat=6 and memb___id='{account}' ");
            sb10.AppendLine("UPDATE MEMB_INFO SET member=8 ");
            sb10.AppendLine("Where {gold} >= 800 and @Stat=7 and memb___id='{account}' ");
            game10.Sql = sb10.ToString();
            Game game11 = new Game(11, "奇迹MU S12以上[元宝]", "MEMB_INFO", "memb___id", "myCash", "sql server");
            game11.Sql = "update [muonline].dbo.T_InGameShop_Point set GoblinPoint=GoblinPoint+{gold} where AccountID='{account}'";

            Game game12 = new Game(12, "墨香", "chr_log_info", "id_loginid", "GoldMoney", "sql server");

            Game game13 = new Game(13, "墨香[多库]", "chr_log_info", "id_loginid", "GoldMoney", "sql server");
            game13.Sql = "UPDATE [mhgame].[dbo].[TB_USERINFO] set UserMallMoney=UserMallMoney+{gold} where User_IDX in(select id_idx from chr_log_info where id_loginid='{account}')";

            Game game14 = new Game(14, "墨香[md5加密]", "chr_log_info", "id_loginid", "GoldMoney", "sql server");
            game14.Sql = "update chr_log_info set GoldMoney=GoldMoney+{gold} where id_idx=(select id_idx from chr_log_info_bak where id_loginid='{account}')";

            Game game15 = new Game(15, "天堂1", "", "", "", "mysql");
            game15.Sql = "insert into character_warehouse (account_name,item_id,count)values('{account}','40308',{gold})";

            Game game16 = new Game(16, "天堂2", "", "", "", "mysql");
            game16.Sql = "update userden set Cash=Cash+{gold} where UserSN=(select UserSN from userinfo where UserID='{account}')";

            Game game17 = new Game(17, "新天堂2", "", "", "", "mysql");
            game17.Sql = "insert into account_gsdata (account_name,var,value)values('{account}','PRIME_POINTS',{gold})";

            Game game18 = new Game(18, "问道", "", "", "", "mysql");
            StringBuilder sb18 = new StringBuilder();
            sb18.AppendLine("insert into yuanbao(zhanghao,jin,shifoulingqu,shijian,jine,order_id)values('{account}',{gold},0,NULL,{money},NOW());");
            sb18.AppendLine("insert into charge(account,coin,money,state,add_time,update_time,code)values('{account}','{money}',0,'{gold}',NOW(),NOW(),(select yaoqingma from accounts where account='{account}'));");
            game18.Sql = sb18.ToString();

            Game game19 = new Game(19, "极致S3龙之谷[点卷]", "", "", "", "sql server");
            game19.Sql = "exec dbo.P_AddCashIncome '{account}',1,1,1,{gold},NULL";

            Game game20 = new Game(20, "天下无双", "", "", "", "sql server");
            StringBuilder sb20 = new StringBuilder();
            sb20.AppendLine("insert into SerialNo(szAccount,ItemInfo,RecvTime,SerialNo,type,ExceedTime) values ('{account}','1;3346,'+CONVERT([varchar],{gold},(0)),getdate(),getdate(),1,'20211231');");
            sb20.AppendLine("UPDATE PlayerAccount SET Zjb=Zjb+{gold} WHERE szAccount='{account}';");
            sb20.AppendLine("insert into ZhanZhuLog (szAccount,money)values('{account}',CONVERT([int],{gold},(0)));");
            game20.Sql = sb20.ToString();

            Game game21 = new Game(21, "天龙八部[元宝]", "", "", "", "sql server");
            game20.Sql = "update account set point=point+{gold} where name='{account}@game.sohu.com'";

            Game game22 = new Game(22, "华夏免费版[额度]", "", "", "", "sql server");
            game22.Sql = "update [MudHx].[dbo].[tMoneyBySellGood] set iMoney = '{gold}' where iUserID=(select iuserid from [MudHxCenter].[dbo].[tUserInfo_Center]  where szusername = '{account}')";
            gameList.Add(1, game1);
            gameList.Add(2, game2);
            gameList.Add(3, game3);
            gameList.Add(4, game4);
            gameList.Add(5, game5);
            gameList.Add(6, game6);
            gameList.Add(7, game7);
            gameList.Add(8, game8);
            gameList.Add(9, game9);
            gameList.Add(10, game10);
            gameList.Add(11, game11);
            gameList.Add(12, game12);
            gameList.Add(13, game13);
            gameList.Add(14, game14);
            gameList.Add(15, game15);
            gameList.Add(16, game16);
            gameList.Add(17, game17);
            gameList.Add(18, game18);
            gameList.Add(19, game19);
            gameList.Add(20, game20);
            gameList.Add(21, game21);
            gameList.Add(22, game22);
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
                if (Convert.ToInt32(txtPort) > 65535)
                {
                    MessageBox.Show("端口范围1 ~ 65535");
                    return;
                }
                try
                {
                    AppSettings.SetValue("port", txtPort);
                }
                catch (Exception) { }
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
                catch (Exception) { }
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


        private void Socket_getSocketText(Session sesson, string text)
        {
            //算法密钥
            string key = this.txtKey.Text.Trim();
            //DES解密
            string decode = Des.decrypt(text, key);
            if (string.IsNullOrEmpty(decode))
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
            DbObject dbObject = new DbObject();
            dbObject.Host = this.txtDbHost.Text.Trim();
            dbObject.Port = this.txtDbPort.Text.Trim();
            dbObject.User = this.txtDbUser.Text.Trim();
            dbObject.Password = this.txtDbPassword.Text.Trim();
            dbObject.Name = data["database"];
            if (string.IsNullOrEmpty(dbObject.Name))
            {
                Error("分区数据库不能为空，请前往平台修改分区设置");
                return;
            }
            bool isMysql = false;
            if (dbtype.Equals("mysql"))
            {
                isMysql = true;
            }
            updateDatabase(isMysql,data, dbObject);
        }

        private void updateDatabase(bool isMysql,Dictionary<String, String> data, DbObject dbObject)
        {
            if (isMysql){
                string serverStr = String.Format("Data Source={0};Database={1};User Id={2};Password={3};port={4};CharSet=utf8;", dbObject.Host, dbObject.Name, dbObject.User, dbObject.Password, dbObject.Port);
                if (mysqlHelper == null){
                    if (isDebug.Checked)
                    {
                        SetTextMesssage("MySQL数据库连接语句：" + serverStr);
                    }
                    mysqlHelper = new MysqlHelper(serverStr);
                }
                //检测数据库连接
                if (!mysqlHelper.CheckConnectStatus()){
                    Error("数据库连接失败，请检查网关数据库配置是否正确");
                    return;
                }
            }
            else{
                string serverStr = String.Format("Data Source={0},{1};Initial Catalog={2};User Id={3};Password={4};", dbObject.Host, dbObject.Port, dbObject.Name, dbObject.User, dbObject.Password);
                if (sqlDbHelper == null)
                {
                    if (isDebug.Checked)
                    {
                        SetTextMesssage("SQL Server数据库连接语句：" + serverStr);
                    }
                    sqlDbHelper = new SqlDbHelper(serverStr);
                }
                //检测数据库连接
                if (!sqlDbHelper.CheckConnectStatus()){
                    Error("数据库连接失败，请检查网关数据库配置是否正确");
                    return;
                }
            }
            //账号
            string account = data["account"];
            //金额
            double money = Convert.ToDouble(data["money"]);
            //充值比例
            double scale = Convert.ToDouble(data["scale"]);
            //游戏币
            double gold = money * scale;
            //分区
            string quname = data["quname"];
            //游戏币别名
            string alias = data["alias"];
            //订单编号
            string ordernumber = data["ordernumber"];
            string txtSql = "";
            //是否使用自定义充值语句
            if (checkBoxCustomSQL.Checked)
            {
                txtSql = this.txtSql.Text.Trim();
                //替换SQL的字符
                txtSql = txtSql.Replace("{account}", account);
                txtSql = txtSql.Replace("{money}", money.ToString());
                txtSql = txtSql.Replace("{gold}", gold.ToString());
                txtSql = txtSql.Replace("{order_no}", ordernumber.ToString());
            }
            else
            {
                string txtTable = this.txtTable.Text.Trim();
                if (string.IsNullOrEmpty(txtTable))
                {
                    SetTextMesssage("游戏币表名不能为空");
                    return;
                }
                string txtColumnAccount = this.txtColumnAccount.Text.Trim();
                if (string.IsNullOrEmpty(txtColumnAccount))
                {
                    SetTextMesssage("账号字段名不能为空");
                    return;
                }
                string txtColumnGold = this.txtColumnGold.Text.Trim();
                if (string.IsNullOrEmpty(txtColumnGold))
                {
                    SetTextMesssage("游戏币字段名不能为空");
                    return;
                }
                txtSql = String.Format("update {0}  set {1}={1}+{2} where {3}='{4}'", txtTable, txtColumnGold, gold, txtColumnAccount, account);
            }

            if (isDebug.Checked)
            {
                SetTextMesssage("sql语句：" + txtSql);
            }
            //执行SQL语句
            bool res = isMysql ? MysqlResult(txtSql) : SQLServerResult(txtSql);
            if (res)
            {
                //充值成功返回
                string yxb = gold >= 10000 ? (gold / 10000).ToString() + '万' : gold.ToString();
                string msg = String.Format("{0} 玩家“{1}”成功充值{2}元，获得{3}{4}个。", quname, account, money, alias, yxb);
                SetTextMesssage(msg);
                sendMsgBySocket(1, msg);
            }
            else
            {
                //充值失败返回
                string msg = String.Format("{0} 玩家“{1}”充值失败，充值金额{2}元，订单编号：{3}", quname, account, money, ordernumber);
                SetTextMesssage(msg);
                sendMsgBySocket(0, msg);
            }
        }
        private bool SQLServerResult(string sql)
        {
            bool flag = false;
            int res = sqlDbHelper.ExecuteSql(sql);
            //如果是储存过程
            if (sql.ToUpper().Contains("EXEC"))
            {
                SetTextMesssage("储存过程执行结果：" + res);
                if (res != 0)
                {
                    flag = true;
                }
            }
            else
            {
                if (res > 0)
                {
                    flag = true;
                }
            }
            return flag;
        }
        private bool MysqlResult(string sql)
        {
            bool flag = false;
            int res = mysqlHelper.commonExecute(sql);
            //执行储存过程
            if (sql.ToUpper().Contains("CALL"))
            {
                SetTextMesssage("储存过程执行结果：" + res);
                if (res != 0)
                {
                    flag = true;
                }
            }
            else
            {
                if (res > 0)
                {
                    flag = true;
                }
            }
            return flag;
        }
        private void sendMsgBySocket(int code, string msg)
        {
            if (socketClient != null)
            {
                String paramString = Tool.toJson(code, msg);
                socketClient.Send(System.Text.Encoding.Default.GetBytes(paramString));
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
            if (string.IsNullOrEmpty(txtDbHost))
            {
                txtDbHost = "127.0.0.1";
            }
            //数据库端口
            string txtDbPort = this.txtDbPort.Text.Trim();
            if (string.IsNullOrEmpty(txtDbPort))
            {
                txtDbPort = "3306";
            }
            //数据库用户
            string txtDbUser = this.txtDbUser.Text.Trim();
            if (string.IsNullOrEmpty(txtDbUser))
            {
                txtDbUser = "root";
            }
            //数据库密码
            string txtDbPassword = this.txtDbPassword.Text.Trim();
            if (string.IsNullOrEmpty(txtDbPassword))
            {
                txtDbPassword = "123456";
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
            dbtype = radioSqlServer.Checked ? "sqlserver" : "mysql";
            string txtTable = this.txtTable.Text.Trim();          
            string txtColumnAccount = this.txtColumnAccount.Text.Trim();
            string txtColumnGold = this.txtColumnGold.Text.Trim();
            bool customSQL = checkBoxCustomSQL.Checked;
            string txtSql = this.txtSql.Text.Trim();
            try
            {
                AppSettings.SetValue("DbType", dbtype);
                AppSettings.SetValue("DbHost", txtDbHost);
                AppSettings.SetValue("DbPort", txtDbPort);
                AppSettings.SetValue("DbUser", txtDbUser);
                AppSettings.SetValue("DbPassword", txtDbPassword);
                AppSettings.SetValue("appid", txtAppid);
                AppSettings.SetValue("appkey", txtAppkey);
                AppSettings.SetValue("key", txtKey);         
                AppSettings.SetValue("table", txtTable);
                AppSettings.SetValue("columnAccount", txtColumnAccount);
                AppSettings.SetValue("columnGold", txtColumnGold);
                AppSettings.SetValue("customSQL", customSQL.ToString());
                AppSettings.SetValue("sql", txtSql);
                MessageBox.Show("保存成功");
            }
            catch (Exception)
            {
                MessageBox.Show("保存失败");
            }

        }


        private void labelClearLog_Click(object sender, EventArgs e)
        {
            this.txtLog.Text = "";
            SetTextMesssage("日志已清空");
        }

        private void radioSqlServer_CheckedChanged(object sender, EventArgs e)
        {
            if (radioSqlServer.Checked)
            {
                txtDbHost.Text = "localhost";
                txtDbPort.Text = "1433";
                txtDbUser.Text = "sa";
                txtDbPassword.Text = "123456";
            }
            else
            {
                txtDbHost.Text = "127.0.0.1";
                txtDbPort.Text = "3306";
                txtDbUser.Text = "root";
                txtDbPassword.Text = "123456";
            }
        }

        private void timerMemory_Tick(object sender, EventArgs e)
        {
            //内存优化
            ClearMemory();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 设置行高
            ImageList imgList = new ImageList();
            // 分别是宽和高
            imgList.ImageSize = new Size(1, 23);
            // 这里设置listView的SmallImageList ,用imgList将其撑大
            listView1.SmallImageList = imgList;
            //使用子线程检查更新
            new Thread(new ThreadStart(this.UpdateRun)) { IsBackground = true }.Start();
            //加载游戏列表
            loadGameList();
        }

        private void loadGameList()
        {
            //清空内容
            this.listView1.Items.Clear();
            foreach (Game game in gameList.Values)
            {
                ListViewItem item = new ListViewItem();
                item.Text = game.Id.ToString();
                item.SubItems.Add(game.Name);
                this.listView1.Items.Add(item);
            }
            //让listView自适应内容宽度
            foreach (ColumnHeader item in this.listView1.Columns)
            {
                item.Width = -2;
            }
        }
        //更新线程
        private void UpdateRun()
        {
            try
            {
                double signVersion = Convert.ToDouble(XmlHelper.GetNodeValue(this.updateUrl + "update.xml", "Version", "Num"));
                if (this.version < signVersion)
                {
                    //启动程序
                    string appPath = Environment.CurrentDirectory + "\\updater.exe";
                    System.Diagnostics.Process.Start(appPath);
                    this.Close();
                }
            }
            catch (Exception) { }

        }

        private void checkBoxCustomSQL_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxCustomSQL.Checked)
            {
                groupBoxCustomSQL.Visible = true;
            }
            else
            {
                groupBoxCustomSQL.Visible = false;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedIndexCollection indexes = this.listView1.SelectedIndices;
            if (indexes.Count > 0)
            {
                int index = indexes[0];
                int id = Convert.ToInt32(this.listView1.Items[index].SubItems[0].Text);//获取第一列的值
                Game game = gameList[id];
                txtTable.Text = game.Table;
                txtColumnAccount.Text = game.Account;
                txtColumnGold.Text = game.Gold;
                if (!(string.IsNullOrEmpty(game.Sql)))
                {
                    checkBoxCustomSQL.Checked = true;
                    txtSql.Text = game.Sql;
                }
                else
                {
                    checkBoxCustomSQL.Checked = false;
                }
                if (game.Dbtype.Equals("mysql"))
                {
                    radioMysql.Checked = true;
                }
                else
                {
                    radioSqlServer.Checked = true;
                }
            }
        }

        private void btnDbTest_Click(object sender, EventArgs e)
        {
            bool isMysql = false;
            if (radioMysql.Checked)
            {
                isMysql = true;
            }
            if (!Tool.checkStr(this.txtDbName, "数据库名称不能为空"))
                return;
            if (!Tool.checkStr(this.txtDbHost, "数据库IP不能为空"))
                return;
            if (!Tool.checkStr(this.txtDbPort, "数据库端口不能为空"))
                return;
            if (!Tool.checkStr(this.txtDbUser, "数据库用户不能为空"))
                return;
            if (!Tool.checkStr(this.txtDbUser, "数据库密码不能为空"))
                return;
            //创建数据库连接
            DbObject dbObject = new DbObject();
            dbObject.Host = this.txtDbHost.Text.Trim();
            dbObject.Port = this.txtDbPort.Text.Trim();
            dbObject.User = this.txtDbUser.Text.Trim();
            dbObject.Password = this.txtDbPassword.Text.Trim();
            dbObject.Name = this.txtDbName.Text.Trim();
            if (isMysql)
            {
                string serverStr = String.Format("Data Source={0};Database={1};User Id={2};Password={3};port={4};CharSet=utf8;", dbObject.Host, dbObject.Name, dbObject.User, dbObject.Password, dbObject.Port);
                SetTextMesssage("MySQL数据库连接语句：" + serverStr);
                mysqlHelper = new MysqlHelper(serverStr);
                //检测数据库连接
                if (!mysqlHelper.CheckConnectStatus())
                {
                    MessageBox.Show("数据库连接失败");
                }
                else
                {
                    MessageBox.Show("数据库连接成功");
                }
                mysqlHelper = null;
            }
            else
            {
                string serverStr = String.Format("Data Source={0},{1};Initial Catalog={2};User Id={3};Password={4};", dbObject.Host, dbObject.Port, dbObject.Name, dbObject.User, dbObject.Password);
                SetTextMesssage("SQL Server数据库连接语句：" + serverStr);
                sqlDbHelper = new SqlDbHelper(serverStr);
                //检测数据库连接
                if (!sqlDbHelper.CheckConnectStatus())
                {
                    MessageBox.Show("数据库连接失败");
                }
                else
                {
                    MessageBox.Show("数据库连接成功");
                }
                sqlDbHelper = null;
            }
        }
    }
}
