using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 充值网关
{
    /// <summary>
    /// 数据库对象
    /// </summary>
    class DbObject
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string host;

        public string Host
        {
            get { return host; }
            set { host = value; }
        }
        private string port;

        public string Port
        {
            get { return port; }
            set { port = value; }
        }
        private string user;

        public string User
        {
            get { return user; }
            set { user = value; }
        }
        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
    }
}
