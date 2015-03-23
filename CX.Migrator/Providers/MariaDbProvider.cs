using System;
using System.Data;
using System.Text;
using CX.Migrator.Configs;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// MariaDb数据库执行者
    /// </summary>
    internal class MariaDbProvider : MysqlProvider
    {
        /// <summary>
        /// MariaDb数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public MariaDbProvider(CXConnection conection):base(conection)
        {
            this.TypeMap = new MySqlTypeMap();
        }
        /// <summary>
        /// 默认架构或引擎名称
        /// <para>mssql/mysql/MariaDb/postgresql</para>
        /// </summary>
        protected override string DefaultEngineOrSchama
        {
            get
            {
                return "XtraDB";
            }
        }
    }
}
