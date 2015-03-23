using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using CX.Migrator.Providers;
using CX.Migrator.Helper;

namespace CX.Migrator
{
    /// <summary>
    /// 数据库迁移工作类
    /// <para>主要执行数据库迁移工作</para>
    /// </summary>
    public class MigrationUtil : IMigrationUtil,IDisposable
    {
        /// <summary>
        /// 数据库提供者
        /// </summary>
        ProviderBase DbProvider;
        /// <summary>
        /// 数据库迁移者工作类
        /// </summary>
        /// <param name="fromProvider">源数据库提供者(Sqlite/PostgreSql/MySql/MariaDb/Oracle/SqlCe/Mssql系列)</param>
        /// <param name="fromString">源数据库连接字符串</param>
        /// <param name="toProvider">目标数据库提供者(Sqlite/PostgreSql/MySql/MariaDb/Oracle/SqlCe/Mssql系列)</param>
        /// <param name="toString">目标数据库连接字符串</param>
        /// <param name="logFilePath">输出的日志存储路径</param>
        /// <param name="schemaOrEngine">Mysql和MariaDb是存储引擎,postgresql是架构</param>
        public MigrationUtil(string fromProvider, string fromString, string toProvider, string toString, string logFilePath = null, string schemaOrEngine = null)
            : this((SqlProvider)Enum.Parse(typeof(SqlProvider), fromProvider, true), fromString, (SqlProvider)Enum.Parse(typeof(SqlProvider), toProvider, true), toString, logFilePath, schemaOrEngine)
        {
        }
        /// <summary>
        /// 数据库迁移者工作类
        /// </summary>
        /// <param name="fromProvider">源数据库提供者</param>
        /// <param name="fromString">源数据库连接字符串</param>
        /// <param name="toProvider">目标数据库提供者</param>
        /// <param name="toString">目标数据库连接字符串</param>
        /// <param name="logFilePath">输出的日志存储路径</param>
        /// <param name="schemaOrEngine">Mysql和MariaDb是存储引擎,postgresql是架构</param>
        public MigrationUtil(SqlProvider fromProvider, string fromString, SqlProvider toProvider, string toString, string logFilePath = null, string schemaOrEngine = null)
        {
            var from = CreateConnection(fromProvider, fromString);
            var to = CreateConnection(toProvider, toString);
            this.fromProvider = fromProvider;
            this.toProvider = toProvider;
            try
            {
                if (from.State != ConnectionState.Open)
                {
                    from.Open();
                    fromTransaction = from.BeginTransaction();
                }
            }
            catch
            {
                throw new Exception("打开源数据库失败");
            }
            var toDbName = to.Database;
            try
            {
                if (to.State != ConnectionState.Open)
                {
                    to.Open();
                }
            }
            catch
            {
                //创建目标数据库
                CreateDb(toProvider, to, toDbName);
            }
            DbProvider = ProviderBase.Create(CXConnection.Create(toProvider,to), logFilePath, schemaOrEngine);
            DbProvider.TrackingOff();
            fromConnection = from;
            toConnection = to;
        }
        private void CreateDb(SqlProvider provider,IDbConnection connection,string dbName)
        {
            ConnectParser parser = new ConnectParser();
            switch(provider)
            {
                case SqlProvider.SqlCe:
                    var type = Type.GetType("System.Data.SqlServerCe.SqlCeEngine");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("System.Data.SqlServerCe");
                        type = assembly.GetType("System.Data.SqlServerCe.SqlCeEngine");
                    }
                    var sqlce=Activator.CreateInstance(type,new object[]{connection.ConnectionString});
                    //type.GetProperty("ConnectionString").SetValue(sqlce, connection.ConnectionString);
                    type.GetMethod("CreateDatabase", BindingFlags.Public | BindingFlags.Instance).Invoke(sqlce,null);
                    break;
                case SqlProvider.Sql2000:
                case SqlProvider.Sql2005:
                case SqlProvider.Sql2008:
                case SqlProvider.Sql2012:
                case SqlProvider.Sql2014:
                    connection.ConnectionString = parser.Defaultmssql(connection.ConnectionString);
                    connection.Open();
                    Execute(connection, string.Concat("Create database ", dbName));
                    connection.ChangeDatabase(dbName);
                    break;
                case SqlProvider.MariaDb:
                case SqlProvider.MySql:
                    connection.ConnectionString = parser.Defaultmysql(connection.ConnectionString);
                    connection.Open();
                    //Execute(connection, "Create database " + dbName);
                    Execute(connection, string.Concat("Create database ", dbName));
                    connection.ChangeDatabase(dbName);
                    break;
                case SqlProvider.PostgreSql:
                    connection.ConnectionString = parser.Defaultpostgres(connection.ConnectionString);
                    connection.Open();
                    Execute(connection, string.Concat("Create database ", dbName));
                    connection.ChangeDatabase(dbName);
                    break;
                case SqlProvider.Oracle:
                    connection.ChangeDatabase("ORCL");
                    //connection.Open();
                    Execute(connection, "Create database " + dbName);
                    break;
                case SqlProvider.Sqlite:
                    break;
            }
            //connection.ChangeDatabase(dbName);
        }
        /// <summary>
        /// 创建IDbCommand
        /// </summary>
        /// <param name="sql"></param>
        protected void Execute(IDbConnection connection, string sql = null)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        private IDbConnection CreateConnection(SqlProvider provider, string connectString)
        {
            Type type = null;
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
                    break;
                case SqlProvider.Sql2000:
                case SqlProvider.Sql2005:
                case SqlProvider.Sql2008:
                case SqlProvider.Sql2012:
                case SqlProvider.Sql2014:
                    connection = new SqlConnection(connectString);
                    break;
                case SqlProvider.MariaDb:
                case SqlProvider.MySql:
                    type = Type.GetType("MySql.Data.MySqlClient.MySqlConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("MySql.Data");
                        type = assembly.GetType("MySql.Data.MySqlClient.MySqlConnection");
                    }
                    break;
                case SqlProvider.Oracle:
                    type = Type.GetType("Oracle.DataAccess.Client.OracleConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("Oracle.DataAccess");
                        type = assembly.GetType("Oracle.DataAccess.Client.OracleConnection");
                    }
                    break;
                case SqlProvider.PostgreSql:
                    type = Type.GetType("Npgsql.NpgsqlConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("Npgsql");
                        type = assembly.GetType("Npgsql.NpgsqlConnection");
                    }
                    break;
                case SqlProvider.Sqlite:
                    type = Type.GetType("Mono.Data.Sqlite.SqliteConnection");
                    if (type == null)
                    {
                        var assembly = AppDomain.CurrentDomain.Load("Mono.Data.Sqlite");
                        type = assembly.GetType("Mono.Data.Sqlite.SqliteConnection");
                    }
                    break;
            }
            if (type != null)
            {
                connection = (IDbConnection)Activator.CreateInstance(type);
                connection.ConnectionString = connectString;
            }
            return connection;
        }
        SqlProvider fromProvider { get; set; }
        SqlProvider toProvider { get; set; }
        IDbConnection fromConnection { get; set; }
        IDbConnection toConnection { get; set; }
        IDbTransaction fromTransaction { get; set; }
        #region IMigrationUtil接口
        /// <summary>
        /// 数据库迁移
        /// </summary>
        /// <returns></returns>
        public bool DbMigration()
        {
            var version = new VersionHolder(fromProvider);
            //初始化表
            var listTables = version.GetAllTables();
            foreach (var table in listTables)
            {
                DbProvider.AddTable(table.TableName,table.ColumnArray());
                DbProvider.MigrationData(ExecuteReader(table.TableName), table, BatchSize);
            }
            return true;
        }
        /// <summary>
        /// 批量提交阀值(默认为1000)
        /// <para>批量插入数据时达到此数量即保存事务</para>
        /// <para>设置为0则关闭批量提交(所有操作完成后在Dispose提交)</para>
        /// </summary>
        public uint BatchSize
        {
            get { return _batchSize; }
            set  { _batchSize = value; }
        }
        uint _batchSize=1000;
        private IDataReader ExecuteReader(string tableName)
        {
            IDbCommand command = fromConnection.CreateCommand();
            command.CommandText = string.Concat("select * from ",tableName);
            command.Transaction = fromTransaction;
            return command.ExecuteReader();
        }
        #endregion IMigrationUtil接口
        /// <summary>
        /// 释放资源的接口
        /// </summary>
        public void Dispose()
        {
            DbProvider.Dispose();
            fromTransaction.Commit();
            fromConnection.Dispose();
        }
    }
}
