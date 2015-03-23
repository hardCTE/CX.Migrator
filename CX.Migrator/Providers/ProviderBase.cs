using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CX.Migrator.Configs;
using CX.Migrator.Framework;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// 数据库提供者基类
    /// </summary>
    internal abstract class ProviderBase:IDisposable
    {
        #region 数据库操作局部变量
        /// <summary>
        /// 定制的数据库连接对象
        /// </summary>
        internal CXConnection CXConnection { get; set; }
        //*/
        /// <summary>
        /// 类型映射
        /// </summary>
        protected TypeMapBase TypeMap { get; set; }
        #region 架构或引擎名称
        /// <summary>
        /// 默认架构或引擎名称
        /// <para>mssql/mysql/MariaDb/postgresql</para>
        /// </summary>
        protected virtual string DefaultEngineOrSchama
        {
            get
            {                
                return _engineOrSchama;
            }
        }
        /// <summary>
        /// 默认存储引擎
        /// </summary>
        string _engineOrSchama=null;
        /// <summary>
        /// 架构或引擎名称
        /// <para>mssql/mysql/MariaDb/postgresql</para>
        /// </summary>
        protected string EngineOrSchama
        {
            get
            {
                return _engineOrSchama??DefaultEngineOrSchama;
            }
            set
            {
                _engineOrSchama = value;
            }
        }
        #endregion 架构或引擎名称
        /// <summary>
        /// 数据库更改的版本
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 是否可以输出日志到文件
        /// </summary>
        protected bool isCanLogFile { get { return !string.IsNullOrEmpty(_outLogFilePath); } }
        string _outLogFilePath;
        /// <summary>
        /// 输出结果文本路径
        /// </summary>
        protected string OutLogFilePath
        {
            get
            {
                return _outLogFilePath;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    string dir = Path.GetDirectoryName(value);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                }
                _outLogFilePath = value;
            }
        }
        VersionHolder _versionHelp;
        /// <summary>
        /// 数据库版本维护员
        /// </summary>
        protected VersionHolder versionHelp
        {
            get
            {
                if (_versionHelp == null)
                {
                    _versionHelp = new VersionHolder(CXConnection.Provider);
                }
                return _versionHelp;
            }
        }
        /// <summary>
        /// 禁用版本跟踪
        /// </summary>
        internal void TrackingOff()
        {
            versionHelp.Tracking = false;
        }
        /// <summary>
        /// 启用版本跟踪
        /// </summary>
        internal void TrackingOn()
        {
            versionHelp.Tracking = true;
        }
        #endregion 数据库操作局部变量
        
        #region 数据库相关
        /// <summary>
        /// 创建IDbCommand
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected IDbCommand CreateCommand(string sql = null)
        {
            return CXConnection.CreateCommand(sql);
        }
        #endregion 数据库相关

        #region 创建数据库提供者
        /// <summary>
        /// 创建数据库执行者
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="conection"></param>
        /// <param name="logFilePath">日志文件路径</param>
        /// <param name="engineOrSchama"></param>
        /// <returns></returns>
        internal static ProviderBase Create(CXConnection conection, string logFilePath, string engineOrSchama)
        {
            ProviderBase tempProvider = null;
            switch(conection.Provider)
            {
                case SqlProvider.MariaDb:
                    tempProvider = new MariaDbProvider(conection);
                    break;
                case SqlProvider.MySql:
                    tempProvider = new MysqlProvider(conection);
                    break;
                case SqlProvider.SqlCe:
                    tempProvider = new SqlCeProvider(conection);
                    break;
                case SqlProvider.Sql2000:
                    tempProvider = new Sql2000Provider(conection);
                    break;
                case SqlProvider.Sql2005:
                    tempProvider = new Sql2005Provider(conection);
                    break;
                case SqlProvider.Sql2008:
                case SqlProvider.Sql2012:
                case SqlProvider.Sql2014:
                    tempProvider = new Sql2008Provider(conection);
                    break;
                case SqlProvider.PostgreSql:
                    tempProvider = new PostgreSqlProvider(conection);
                    break;
                case SqlProvider.Sqlite:
                    tempProvider = new SqliteProvider(conection);
                    break;
            }
            tempProvider.OutLogFilePath = logFilePath;
            tempProvider.EngineOrSchama = engineOrSchama;
            //tempProvider.InitLogTable();
            tempProvider.CXConnection.BeginTransaction();
            return tempProvider;
        }
        #endregion 创建数据库提供者

        #region 数据库和表判断
        /// <summary>
        /// 是否事务回滚中
        /// </summary>
        internal bool isRollbacking { get; set; }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="isProcedure">是否存储过程</param>
        /// <returns></returns>
        protected virtual int Execute(string sql, bool isProcedure = false)
        {
            try
            {
                IDbCommand command = CreateCommand(sql);
                command.CommandType = isProcedure ? CommandType.StoredProcedure : CommandType.Text;
                int count = command.ExecuteNonQuery();
                LogToText(sql);
                return count;
            }
            catch (Exception ex)
            {
                isRollbacking = true;
                LogToText(sql + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                return -1;
            }
        }        
        /// <summary>
        /// 返回IDataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected IDataReader ExecuteReader(string sql)
        {
            IDbCommand command = CreateCommand(sql);
            ConsoleOut(sql);
            return command.ExecuteReader();
        }
        /// <summary>
        /// 判断是否数据类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        internal bool IfDataBase(SqlProvider provider)
        {
            return CXConnection.Provider == provider;
        }
        /// <summary>
        /// 判断数据库是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        protected virtual bool ExistsTable(string tableName,bool outTip=true)
        {
            try
            {
                IDbCommand command = CreateCommand(string.Concat("select 1 from ", tableName));
                ConsoleOut(command.CommandText);
                command.ExecuteNonQuery();
                if (outTip)
                    ConsoleExists(tableName);
                return true;
            }
            catch
            {
                if (!outTip)
                    ConsoleNotExists(tableName);
                return false;
            }
        }
        #endregion 数据库和表判断

        #region 版本管理
        /// <summary>
        /// 获取查询最后一个成功执行的数据库的版本号
        /// </summary>
        protected virtual string GetLastVersionSql
        {
            get
            {
                return string.Format("select {0} from {1} order by {2} desc limit 1", Quote("V"), Quote(LogTableName), Quote("Id"));
            }
        }
        /// <summary>
        /// 获取删除已经降级的版本号
        /// </summary>
        protected virtual string GetDeleteVersionSql
        {
            get
            {
                return string.Format("delete from {0} where {1}={2}V", Quote(LogTableName), Quote("V"), SqlParamPre);
            }
        }
        /// <summary>
        /// 获取最后更新的数据库版本号
        /// </summary>
        /// <returns></returns>
        internal int GetLastVersion()
        {
            InitLogTable();
            IDbCommand command = CreateCommand(GetLastVersionSql);
            ConsoleOut(command.CommandText);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        /// <summary>
        /// 获取数据库表和列名对应的完整写法
        /// </summary>
        /// <param name="tableOrColumnName"></param>
        /// <returns></returns>
        protected virtual string Quote(string tableOrColumnName)
        {
            return string.Format("[{0}]",tableOrColumnName);
        }
        /// <summary>
        /// 获取数据库表名称完整写法
        /// <para>postgresql专用</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected virtual string QuoteTableName(string tableName)
        {
            return string.Format("\"{0}\"", tableName.ToLower());
        }
        /// <summary>
        /// 添加更新成功的版本号
        /// </summary>
        /// <param name="version"></param>
        internal void AddVersion(int version)
        {
            InitLogTable();
            IDbCommand command = CreateCommand(InsertLogSql);
            var param = command.CreateParameter();
            param.DbType = DbType.DateTime;
            param.Direction = ParameterDirection.Input;
            param.ParameterName = "R";
            param.Value = DateTime.Now;
            command.Parameters.Add(param);
            param = command.CreateParameter();
            param.DbType = DbType.Int32;
            param.Direction = ParameterDirection.Input;
            param.ParameterName = "V";
            param.Value = version;
            command.Parameters.Add(param);
            ConsoleOut(command.CommandText);
            command.ExecuteNonQuery();
        }
        /// <summary>
        /// 删除已经降级的版本号
        /// </summary>
        /// <param name="version"></param>
        internal void DeleteVersion(int version)
        {
            InitLogTable();
            IDbCommand command = CreateCommand(GetDeleteVersionSql);
            var param = command.CreateParameter();
            param.DbType = DbType.Int32;
            param.Direction = ParameterDirection.Input;
            param.ParameterName = "V";
            param.Value = version;
            command.Parameters.Add(param);
            ConsoleOut(command.CommandText);
            command.ExecuteNonQuery();
        }
        #endregion 版本管理

        #region 辅助方法
        /// <summary>
        /// 根据表和列名获取列设定
        /// <para>内置支持,从开始建数据库使用本类库可获得完整支持(支持所有数据库)</para>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        protected Column GetColumnByName(string table, string columnName)
        {
            return versionHelp.GetColumnByName(table,columnName);
                /*
            return Array.Find(GetColumns(table),
                delegate(Column column)
                {
                    return column.Name == columnName;
                });//*/
        }
        /*
        /// <summary>
        /// 获取表的所有列
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected virtual Column[] GetColumns(string table)
        {
            List<Column> columns = new List<Column>();
            using (
                IDataReader reader =
                    ExecuteReader(
                        string.Format("select COLUMN_NAME, IS_NULLABLE from information_schema.columns where table_name = '{0}'", table)))
            {
                while (reader.Read())
                {
                    Column column = new Column(reader.GetString(0), DbType.String);
                    string nullableStr = reader.GetString(1);
                    bool isNullable = nullableStr == "YES";
                    column.ColumnProperty |= isNullable ? ColumnProperty.Null : ColumnProperty.NotNull;

                    columns.Add(column);
                }
            }
            return columns.ToArray();
        }
        //*/
        /// <summary>
        /// 转换成bool
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool DbValueToBool(object value)
        {
            return (bool)value;
        }
        /// <summary>
        /// Console输出表的约束不存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="constraintName"></param>
        protected void ConsoleNotExistsConstraint(string tableName, string constraintName)
        {
            ConsoleOut(string.Concat("表", tableName, "的约束", constraintName, "不存在"));
        }
        /// <summary>
        /// Console输出表不存在
        /// </summary>
        /// <param name="tableName"></param>
        protected void ConsoleNotExists(string tableName)
        {
            ConsoleOut(string.Concat("表", tableName, "不存在"));
        }
        /// <summary>
        /// Console输出表上的列不存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        protected void ConsoleNotExists(string tableName, string column)
        {
            ConsoleOut(string.Concat("表", tableName, "的列", column, "不存在"));
        }
        /// <summary>
        /// Console输出表已经存在
        /// </summary>
        /// <param name="tableName"></param>
        protected void ConsoleExists(string tableName)
        {
            ConsoleOut(string.Concat("表", tableName, "已经存在"));
        }
        /// <summary>
        /// Console输出表上的列已经存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        protected void ConsoleExists(string tableName, string column)
        {
            ConsoleOut(string.Concat("表", tableName, "的列", column, "已经存在"));
        }
        /// <summary>
        /// Console输出信息
        /// </summary>
        /// <param name="msg"></param>
        protected void ConsoleOut(string msg)
        {
            Console.Out.WriteLine(msg);
        }
        #endregion 辅助方法
        /// <summary>
        /// 更换数据库连接的目标数据库
        /// </summary>
        /// <param name="dbName"></param>
        internal virtual void UseDb(string dbName)
        {
            var db = CXConnection.Connection.Database;
            try
            {
                CXConnection.Connection.ChangeDatabase(dbName);
            }
            catch
            {
                CXConnection.Connection.ChangeDatabase(db);
            }
        }
        /// <summary>
        /// 修改数据库的默认字符集
        /// <para>仅mysql需要</para>
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        internal virtual void ChangeDbCharset(string dbName, string charset)
        {
            ConsoleOut("仅Mysql需要修改字符集");
        }
        /// <summary>
        /// 修改数据表的默认字符集
        /// <para>仅mysql需要</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        internal virtual void ChangeTableCharset(string tableName, string charset)
        {
            ConsoleOut("仅Mysql需要修改字符集");
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName"></param>
        internal virtual void CreateDB(string dbName)
        {
            if (ExistsDb(dbName))
            {
                ConsoleOut(string.Format("数据库{0}已经存在", dbName));
            }
            else
            {
                string sql = string.Format("create database {0}", Quote(dbName));
                Execute(sql);
            }
        }
        /// <summary>
        /// 重命名表名称
        /// </summary>
        /// <param name="oldTableName"></param>
        /// <param name="newTableName"></param>
        /// <param name="isTracking"></param>
        internal virtual void RenameTable(string oldTableName, string newTableName, bool isTracking = true)
        {
            if (!ExistsTable(oldTableName, false) || ExistsTable(newTableName))
            {
                return;
            }
            string sql = string.Format("alter table {0} rename {1}", Quote(oldTableName), Quote(newTableName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.RenameTable(oldTableName, newTableName);
        }
        /// <summary>
        /// 添加外键约束
        /// <para>表和列必须都存在</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">要添加外键的列</param>
        /// <param name="refTable">外键目标表名</param>
        /// <param name="refColumn">外键目标表列</param>
        internal virtual void AddForeignKey(string tableName, string column, string refTable, string refColumn)
        {
            if (!ExistsTable(tableName) || !ExistsTable(refTable, false) || !ExistsColumn(tableName, column, false) || !ExistsColumn(refTable, refColumn, false))
            {
                return;
            }
            string sql = string.Format("alter table {0} add constraint fk_{1} foreign key({2}) references {3} ({4})"
                , Quote(tableName), column, Quote(column), Quote(refTable), Quote(refColumn));
            Execute(sql);
        }
        /// <summary>
        /// 删除外键约束
        /// </summary>
        /// <param name="tableName">表名</param>
        internal virtual void DropForeignKey(string tableName)
        {
            LogToText("删除外键功能尚未实现");
        }
        /// <summary>
        /// 添加主键
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal virtual void AddPrimaryKey(string tableName, string columnName, bool isTracking = true)
        {
            ConsoleOut("尚未实现添加主键功能");
        }
        /// <summary>
        /// 添加自增主键
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal virtual void AddPrimaryKeyIdentity(string tableName, string columnName, bool isTracking = true)
        {
            ConsoleOut("尚未实现添加自增主键功能");
        }
        /// <summary>
        /// 添加新列
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="newColumn"></param>
        /// <param name="isTracking"></param>
        internal virtual void AddColumn(string tableName, Column newColumn,bool isTracking=true)
        {
            if (ExistsColumn(tableName, newColumn.Name))
            {
                return;
            }
            var builded = BuildColumn(tableName, newColumn);
            string sql = string.Format("alter table {0} add column {1};{2}", Quote(tableName), builded.BuildSql, builded.IndexSql);
            if (Execute(sql)!=-1&&isTracking)
                versionHelp.AddColumn(tableName, newColumn);
        }
        protected StringBuilder builder = null;
        /// <summary>
        /// 获取列名后面的类型和自增列主键默认值等信息
        /// </summary>
        /// <param name="column"></param>
        /// <param name="withDefault">带默认值</param>
        /// <returns></returns>
        protected virtual string GetColumnTypeString(Column column,bool withDefault=true)
        {
            return string.Concat(TypeMap.GetDbString(column), " ", TypeMap.MapTypeString(column.ColumnProperty),withDefault? Default(column):string.Empty);
        }
        /// <summary>
        /// 组建创建列和索引的Sql语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <param name="withDefault"></param>
        /// <returns></returns>
        protected virtual BuildHelp BuildColumn(string tableName, Column column,bool withDefault=true)
        {
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Length = 0;
            builder.Append(Quote(column.Name));
            builder.Append(" ");
            builder.Append(GetColumnTypeString(column,withDefault));
            BuildHelp returnModel = new BuildHelp();
            returnModel.BuildSql = builder.ToString();
            builder.Length = 0;
            if (IsCanIndexd(column))
            {
                builder.Append("alter table ");
                builder.Append(Quote(tableName));
                builder.Append(" add index ");
                builder.Append(column.Name);
                builder.Append("_asc (");
                builder.Append(Quote(column.Name));
                builder.Append(");");
            }
            else if (IsCanUnique(column))
            {
                builder.Append("alter table ");
                builder.Append(Quote(tableName));
                builder.Append(" add unique ");
                builder.Append(column.Name);
                builder.Append("_asc (");
                builder.Append(Quote(column.Name));
                builder.Append(");");
            }
            returnModel.IndexSql = builder.ToString();
            return returnModel;
        }
        /// <summary>
        /// 删除主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal virtual void DropPrimaryKey(string tableName, string columnName, bool isTracking = true)
        {
            ConsoleOut("尚未实现删除主键功能");
        }
        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal virtual void DropColumn(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            string sql = string.Format("alter table {0} drop  column {1}", Quote(tableName), Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.DropColumn(tableName, columnName);
        }
        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="column"></param>
        /// <param name="isTracking"></param>
        internal virtual void ChangeColumn(string tableName, string oldColumnName, Column column, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, oldColumnName, false) || ExistsColumn(tableName, column.Name))
            {
                return;
            }
            var builded = BuildColumn(tableName, column);
            string sql = string.Format("alter table {0} change column {1} {2};{3}", Quote(tableName), Quote(oldColumnName ?? column.Name), builded.BuildSql, builded.IndexSql);
            if (Execute(sql) != -1 && isTracking)
                versionHelp.ChangeColumn(tableName, oldColumnName, column);
        }
        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldName"></param>
        /// <param name="newColumn"></param>
        /// <param name="isTracking"></param>
        internal virtual void RenameColumn(string tableName, string oldName, string newColumn, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, oldName, false) || ExistsColumn(tableName, newColumn))
            {
            }
            string sql = string.Format("exec sp_rename '{0}.{1}', '{2}', 'column'", Quote(tableName), Quote(oldName), Quote(newColumn));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.RenameColumn(tableName, oldName, newColumn);
        }
        /// <summary>
        /// 在列上建立普通索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal virtual void AddIndex(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            string sql = string.Format("alter table {0}  add index {1}_asc ({2});", Quote(tableName), columnName, Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddIndex(tableName, columnName);
        }
        /// <summary>
        /// 在列上建立唯一索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal virtual void AddUnique(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            string sql = string.Format("alter table {0}  add unique {1}_asc ({2});", Quote(tableName), columnName, Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
            versionHelp.AddUnique(tableName, columnName);
        }
        /// <summary>
        /// 查询列是否存在
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="outTip">输出提示</param>
        /// <returns></returns>
        internal virtual bool ExistsColumn(string table, string column,bool outTip=true)
        {
            try
            {
                string sql = String.Format("select {0} from {1}", Quote(column), Quote(table));
                IDbCommand command = CreateCommand(sql);
                ConsoleOut(sql);
                command.ExecuteNonQuery();
                if (outTip)
                    ConsoleExists(table,column);
                return true;
            }
            catch (Exception)
            {
                if (!outTip)
                    ConsoleNotExists(table,column);
                return false;
            }
        }
        /// <summary>
        /// 查询数据库是否存在
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        internal virtual bool ExistsDb(string dbName, bool outTip = true)
        {
            try
            {
                string sql = String.Format("use {0}", Quote(dbName));
                IDbCommand command = CreateCommand(sql);
                ConsoleOut(sql);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                if (outTip)
                    ConsoleOut(string.Concat("数据库",dbName,"不存在"));
                return false;
            }
        }
        #region 添加表
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        internal void AddTable(string tableName, params Column[] columns)
        {
            AddTable(tableName,true,columns);
        }
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="isTracking">是否追踪</param>
        /// <param name="columns"></param>
        internal abstract void AddTable(string tableName, bool isTracking = true, params Column[] columns);
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <param name="isTracking">是否追踪</param>
        internal void AddTable(string tableName, List<Column> columns, bool isTracking = true)
        {
            if (columns == null)
                return;
            AddTable(tableName, isTracking, columns.ToArray());
        }
        #endregion 添加表

        #region 迁移数据
        /// <summary>
        /// 迁移数据
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="table"></param>
        /// <param name="batchSize"></param>
        internal void MigrationData(IDataReader reader, TableInfo table, uint batchSize)
        {
            string insertSql = null;
            using (reader)
            {
                uint insertedCount = 0;
                while (reader.Read())
                {
                    if (insertSql == null)
                        insertSql = BuildInsertSql(table);
                    IDbCommand command = CreateCommand(insertSql);
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var tempLower = reader.GetName(i).ToLower();
                        if (!table.AllColumns.ContainsKey(tempLower) || table.AllColumns[tempLower].IsIdentity)
                            continue;
                        AddParamter(command, table.AllColumns[tempLower].Name, reader.GetValue(i));
                    }
                    command.ExecuteNonQuery();
                    insertedCount += 1;
                    if (batchSize>0&&insertedCount >= batchSize)
                    {
                        CXConnection.SaveTrsation();
                        insertedCount = 0;
                    }
                }
            }
        }
        /// <summary>
        /// 添加插入数据的参数
        /// </summary>
        /// <param name="command"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        private void AddParamter(IDbCommand command, string columnName, object value)
        {
            var parameter = command.CreateParameter();
            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = columnName;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
        /// <summary>
        /// 创建插入数据的命令
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private string BuildInsertSql(TableInfo table)
        {
            StringBuilder builder = new StringBuilder("insert into ");
            StringBuilder values = new StringBuilder();
            builder.Append(table.TableName);
            builder.Append(" (");
            foreach (var column in table.AllColumns.Values)
            {
                if (column.IsIdentity)
                    continue;
                builder.Append(column.Name);
                builder.Append(",");
                values.Append(SqlParamPre);
                values.Append(column.Name);
                values.Append(",");
            }
            builder.Length -= 1;
            values.Length -= 1;
            builder.Append(") values (");
            builder.Append(values.ToString());
            builder.Append(")");
            return builder.ToString();
        }
        #endregion 迁移数据

        #region 约束管理
        /// <summary>
        /// 移除列上所有约束
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列的名称</param>
        internal virtual void DropConstraint(string tableName, string columnName)
        {
            ConsoleOut("尚未实现此功能:DropConstraint");
        }
        /// <summary>
        /// 查询列是否存在约束
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal virtual bool ExistsConstraints(string tableName, string columnName)
        {
            ConsoleOut("尚未实现此功能:ExistsConstraints");
            return false;
        }
        /// <summary>
        /// 查询约束名称是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="constraintName"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        internal virtual bool ExistsConstraintName(string tableName, string constraintName,bool outTip=true)
        {
            ConsoleOut("尚未实现此功能:ExistsConstraintName");
            return false;
        }
        /// <summary>
        /// 移除约束
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="constraintName">约束名称</param>
        internal virtual void DropConstraintByName(string tableName, string constraintName)
        {
            ConsoleOut("尚未实现此功能:DropConstraintByName");
        }
        /// <summary>
        /// 禁用表上所有约束
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal virtual void DisableAllConstraint(string tableName)
        {
            ConsoleOut("尚未实现此功能:DisableAllConstraint");
        }
        /// <summary>
        /// 启用表上所有约束
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal virtual void EnableAllConstraint(string tableName)
        {
            ConsoleOut("尚未实现此功能:EnableAllConstraint");
        }
        #endregion 约束管理

        #region 公共属性
        /// <summary>
        /// 返回默认值字符串
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected abstract string Default(Column column);
        /// <summary>
        /// 是否可以建立普通索引
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected virtual bool IsCanIndexd(Column column)
        {
            return column.IsIndexed;
        }
        /// <summary>
        /// 是否可以建立唯一索引
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected virtual bool IsCanUnique(Column column)
        {
            return column.IsUnique;
        }
        #endregion 公共属性

        #region 删除表
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="isTracking"></param>
        internal void DropTable(string tableName, bool isTracking = true)
        {
            if (!ExistsTable(tableName, false))
            {
                return;
            }
            if (Execute(string.Concat("drop table ", Quote(tableName))) != -1 && isTracking)
                versionHelp.DropTable(tableName);
        }
        #endregion 删除表

        #region 数据库参数前缀
        /// <summary>
        /// 数据库参数前缀
        /// </summary>
        protected string SqlParamPre
        {
            get
            {
                return SqlInfoHelper.SqlParamPre(this.CXConnection.Provider);
            }
        }
        #endregion 数据库参数前缀

        #region 日志
        string insertLogSql;
        string InsertLogSql
        {
            get
            {
                if(insertLogSql==null)
                {
                    insertLogSql = string.Format(@"Insert into {0} ({1},{2}) values ({3}V,{3}R)", Quote(LogTableName), Quote("V"), Quote("R"), SqlParamPre);
                }
                return insertLogSql;
            }
        }
        /// <summary>
        /// 初始化日志表
        /// </summary>
        protected virtual void InitLogTable()
        {
            if (ExistsTable(LogTableName))
                return;
            AddTable(LogTableName,
                new Column{Name= "Id",Type= DbType.UInt32,Size= 1,ColumnProperty= ColumnProperty.PrimaryKey_Identity},
                new Column { Name = "R", Type = DbType.DateTime, Size = 20, ColumnProperty = ColumnProperty.Null },
                new Column { Name = "V", Type = DbType.Int32, Size = 20, ColumnProperty = ColumnProperty.Null }
                );
        }
        /*
        /// <summary>
        /// 初始化日志表
        /// </summary>
        protected virtual void InitLogTable()
        {
            try
            {
                IDbCommand command = CreateCommand("select 1 from " + Quote(LogTableName));
                ConsoleOut(command.CommandText);
                command.ExecuteNonQuery();
            }
            catch
            {
                AddTable(LogTableName,
                    new Column("Id", DbType.UInt32, 1, ColumnProperty.PrimaryKey_Identity),
                    new Column("R", DbType.DateTime, 20, ColumnProperty.Null),
                    new Column("V", DbType.Int32, 20, ColumnProperty.Null)
                    );
            }
        }//*/
        /// <summary>
        /// 记录日志到文件
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="newLogPath">新日志路径(用于调试)</param>
        protected void LogToText(string msg,string newLogPath=null)
        {
            ConsoleOut(msg);
            if (OutLogFilePath == null && newLogPath == null)
                return;
            if (isCanLogFile)
                File.AppendAllText(newLogPath??OutLogFilePath, string.Concat(msg, Environment.NewLine, Environment.NewLine));
        }
        protected readonly string LogTableName = typeof(Migrator_Version).Name;
        #endregion 日志
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (CXConnection != null)
            {
                CXConnection.Dispose();
            }
        }
    }
}
