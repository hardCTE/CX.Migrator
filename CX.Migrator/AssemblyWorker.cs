#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using CX.Migrator.Framework;
using CX.Migrator.Helper;
using CX.Migrator.Providers;

namespace CX.Migrator
{
    /// <summary>
    /// 数据库更新执行者
    /// <para>以程序集为单位进行操作</para>
    /// </summary>
    public class AssemblyWorker : IAssemblyWorker
    {
        #region 构造函数及初始化
        /// <summary>
        /// 数据库提供者
        /// </summary>
        readonly ProviderBase DbProvider;
        /// <summary>
        /// 数据库更新执行者
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="migratorTypes">要升级的类型集合(继承自MigrationBase的类型集合)</param>
        /// <param name="logFilePath"></param>
        /// <param name="schema"></param>
        /// <param name="transaction">数据库事务</param>
        public AssemblyWorker(CXConnection connection, List<Type> migratorTypes, string logFilePath = null, string schema = null)
        {
            DbProvider = ProviderBase.Create(connection, logFilePath, schema);
            foreach (var t in migratorTypes)
            {
                MigrationBase mig = (MigrationBase)Activator.CreateInstance(t);
                mig.provider = DbProvider;
                runElements.AddMigration(mig);
            }
        }
        /// <summary>
        /// 数据库更新执行者
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="asm">要升级的类型所在程序集(继承自MigrationBase的程序集)</param>
        /// <param name="logFilePath"></param>
        /// <param name="schema"></param>
        public AssemblyWorker(CXConnection connection, Assembly asm, string logFilePath = null, string schema = null)
            : this(connection, asm.GetExportedTypes().Where(t => typeof(MigrationBase).IsAssignableFrom(t)).ToList(), logFilePath, schema)
        {
        }
        private IDbConnection CreateConnection(SqlProvider provider, string connectString)
        {
            Type type;
            IDbConnection connection = null;
            switch (provider)
            {
                case SqlProvider.SqlCe:
                    type = Type.GetType("System.Data.SqlServerCe.SqlCeConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("System.Data.SqlServerCe");
                        type = assembly.GetType("System.Data.SqlServerCe.SqlCeConnection");
                    }
                    connection = (IDbConnection)Activator.CreateInstance(type);
                    break;
                case SqlProvider.Sql2000:
                case SqlProvider.Sql2005:
                case SqlProvider.Sql2008:
                case SqlProvider.Sql2012:
                case SqlProvider.Sql2014:
                    connection = new SqlConnection();
                    break;
                case SqlProvider.MariaDb:
                case SqlProvider.MySql:
                    type = Type.GetType("MySql.Data.MySqlClient.MySqlConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("MySql.Data");
                        type = assembly.GetType("MySql.Data.MySqlClient.MySqlConnection");
                    }
                    connection = (IDbConnection)Activator.CreateInstance(type);
                    break;
                case SqlProvider.Oracle:
                    type = Type.GetType("Oracle.DataAccess.Client.OracleConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("Oracle.DataAccess");
                        type = assembly.GetType("Oracle.DataAccess.Client.OracleConnection");
                    }
                    connection = (IDbConnection)Activator.CreateInstance(type);
                    break;
                case SqlProvider.PostgreSql:
                    type = Type.GetType("Npgsql.NpgsqlConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("Npgsql");
                        type = assembly.GetType("Npgsql.NpgsqlConnection");
                    }
                    connection = (IDbConnection)Activator.CreateInstance(type);
                    break;
                case SqlProvider.Sqlite:
                    type = Type.GetType("Mono.Data.Sqlite.SqliteConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("Mono.Data.Sqlite");
                        type = assembly.GetType("Mono.Data.Sqlite.SqliteConnection");
                    }
                    connection = (IDbConnection)Activator.CreateInstance(type);
                    break;
            }
            connection.ConnectionString = connectString;
            return connection;
        }
        /// <summary>
        /// 数据库更新执行者
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connection"></param>
        /// <param name="migratorTypes">要升级的类型集合(继承自MigrationBase的类型集合)</param>
        /// <param name="logFilePath">日志文件路径</param>
        /// <param name="schema"></param>
        public AssemblyWorker(SqlProvider provider, string connectionString, List<Type> migratorTypes, string logFilePath = null, string schema = null)
        {
            var connection = CreateConnection(provider, connectionString);
            DbProvider = ProviderBase.Create(CXConnection.Create(provider, connection), logFilePath, schema);
            foreach (var t in migratorTypes)
            {
                MigrationBase mig = (MigrationBase)Activator.CreateInstance(t);
                mig.provider = DbProvider;
                runElements.AddMigration(mig);
            }
        }
        /// <summary>
        /// 数据库更新执行者
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connection"></param>
        /// <param name="assembly"></param>
        /// <param name="logFilePath">日志文件路径</param>
        /// <param name="schema"></param>
        public AssemblyWorker(SqlProvider provider, string connectionString, Assembly asm, string logFilePath = null, string schema = null)
            : this(provider, connectionString, asm.GetExportedTypes().Where(t => typeof(MigrationBase).IsAssignableFrom(t)).ToList(), logFilePath, schema)
        {
        }
        SortedElement runElements = new SortedElement();
        #endregion 构造函数及初始化
        /// <summary>
        /// 直接升级到最新版本
        /// </summary>
        public void Up()
        {
            DbProvider.CXConnection.SaveTrsation();
            var lastVersion = DbProvider.GetLastVersion();
            currentVersion = 0;
            foreach (var v in runElements.AscingElements)
            {
                if (v.Version > lastVersion)
                {
                    v.provider.Version = v.Version;
                    v.Up();
                    SetVersionLog(v.Version, true);
                }
            }
            Report("已经升级到版本" + currentVersion);
            SetVersionLog(0, true);
        }
        private void Report(string msg)
        {
            if (!DbProvider.isRollbacking)
                Console.Out.WriteLine(msg);
            else
                Console.Out.WriteLine("数据库操作错误,事务回滚到开始状态...");
        }
        int currentVersion;
        private void SetVersionLog(int version, bool isUp)
        {
            if (currentVersion != 0 && currentVersion != version)
            {
                if (isUp)
                    DbProvider.AddVersion(currentVersion);
                else
                    DbProvider.DeleteVersion(currentVersion);
            }
            currentVersion = version;
        }
        /// <summary>
        /// 直接降级到最低版本
        /// </summary>
        public void Down()
        {
            DbProvider.CXConnection.SaveTrsation();
            var lastVersion = DbProvider.GetLastVersion();
            currentVersion = 0;
            foreach (var v in runElements.DescingElements)
            {
                if (v.Version <= lastVersion)
                {
                    v.provider.Version = v.Version;
                    v.Down();
                    SetVersionLog(v.Version, false);
                }
            }
            Report("已经降级到版本" + currentVersion);
            SetVersionLog(0, false);
        }
        /// <summary>
        /// 直接升级到版本号
        /// </summary>
        /// <param name="version">版本号</param>
        public void Up(int version)
        {
            DbProvider.CXConnection.SaveTrsation();
            var lastVersion = DbProvider.GetLastVersion();
            currentVersion = 0;
            foreach (var v in runElements.AscingElements)
            {
                if (v.Version > lastVersion && v.Version <= version)
                {
                    v.provider.Version = v.Version;
                    v.Up();
                    SetVersionLog(v.Version, true);
                }
            }
            Report("已经升级到版本" + currentVersion);
            SetVersionLog(0, true);
        }
        /// <summary>
        /// 直接降级到版本号
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="include">包含此版本号</param>
        public void Down(int version, bool include = false)
        {
            DbProvider.CXConnection.SaveTrsation();
            var lastVersion = DbProvider.GetLastVersion();
            currentVersion = 0;
            foreach (var v in runElements.DescingElements)
            {
                if (v.Version <= lastVersion && v.Version >= version)
                {
                    v.provider.Version = v.Version;
                    if (v.Version == version)
                    {
                        if (include)
                        {
                            v.Down();
                            SetVersionLog(v.Version, false);
                        }
                        continue;
                    }
                    v.Down();
                    SetVersionLog(v.Version, false);
                }
            }
            Report("已经降级到版本" + currentVersion);
            SetVersionLog(0, false);
        }
        /// <summary>
        /// 单独应用某版本的更新Up()
        /// <para>可用于插件的安装时更改数据库</para>
        /// </summary>
        /// <param name="version"></param>
        public void Apply(int version)
        {
            DbProvider.CXConnection.SaveTrsation();
            currentVersion = 0;
            foreach (var v in runElements.AscingElements)
            {
                if (v.Version == version)
                {
                    v.provider.Version = v.Version;
                    v.Up();
                    SetVersionLog(v.Version, true);
                }
            }
            Report("已经应用版本" + currentVersion);
            SetVersionLog(0, true);
        }
        /// <summary>
        /// 单独应用某版本的Down()
        /// <para>可用于插件的卸载时撤销数据库更改</para>
        /// </summary>
        /// <param name="version"></param>
        public void UnApply(int version)
        {
            DbProvider.CXConnection.SaveTrsation();
            currentVersion = 0;
            foreach (var v in runElements.AscingElements)
            {
                if (v.Version == version)
                {
                    v.provider.Version = v.Version;
                    v.Down();
                    SetVersionLog(v.Version, true);
                }
            }
            Report("已经卸载版本" + currentVersion);
            SetVersionLog(0, true);
        }
        /// <summary>
        /// 单独应用某版本的更新Up(忽略版本)
        /// <para>可用于插件的安装时更改数据库</para>
        /// <para>要求应用所有类版本必须相同</para>
        /// </summary>
        public void ApplySingle()
        {
            DbProvider.CXConnection.SaveTrsation();
            currentVersion = 0;
            foreach (var v in runElements.AscingElements)
            {
                if (currentVersion == 0)
                    currentVersion = v.Version;
                if (v.Version == currentVersion)
                {
                    v.provider.Version = v.Version;
                    v.Up();
                    SetVersionLog(v.Version, true);
                }
                else
                {
                    DbProvider.CXConnection.TransactionError();
                }
            }
            if (DbProvider.isRollbacking)
                Report("操作出错，将回滚事务");
            else
            {
                Report("已按指定应用更改");
                SetVersionLog(0, true);
            }
        }
        /// <summary>
        /// 单独应用某版本的降级Down(忽略版本)
        /// <para>可用于插件的卸载时撤销数据库更改</para>
        /// <para>要求应用所有类版本必须相同</para>
        /// </summary>
        public void UnApplySingle()
        {
            DbProvider.CXConnection.SaveTrsation();
            currentVersion = 0;
            foreach (var v in runElements.AscingElements)
            {
                if (currentVersion == 0)
                    currentVersion = v.Version;
                if (v.Version == currentVersion)
                {
                    v.provider.Version = v.Version;
                    v.Down();
                    SetVersionLog(v.Version, true);
                }
                else
                {
                    DbProvider.CXConnection.TransactionError();
                }
            }
            if (DbProvider.isRollbacking)
                Report("操作出错，将回滚事务");
            else
            {
                Report("已按指定撤销更改");
                SetVersionLog(0, true);
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            DbProvider.Dispose();
        }
    }
}
