using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 充值网关
{
    class Game
    {
        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string table;//游戏币表名

        public string Table
        {
            get { return table; }
            set { table = value; }
        }
        private string account;//账号字段名

        public string Account
        {
            get { return account; }
            set { account = value; }
        }
        private string gold;//游戏币字段名

        public string Gold
        {
            get { return gold; }
            set { gold = value; }
        }
        private string dbtype;//数据库类型

        public string Dbtype
        {
            get { return dbtype; }
            set { dbtype = value; }
        }
        private string sql;//自定义sql语句

        public string Sql
        {
            get { return sql; }
            set { sql = value; }
        }

        public Game()
        {
        }
        public Game(int _id, string _name, string _table, string _account, string _gold, string _dbtype)
        {
            this.id = _id;
            this.name = _name;
            this.table = _table;
            this.account = _account;
            this.gold = _gold;
            this.dbtype = _dbtype;
        }

    }
}
