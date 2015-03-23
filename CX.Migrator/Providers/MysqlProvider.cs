#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;
using System.Text;
using CX.Migrator.Configs;
using CX.Migrator.Framework;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// Mysql数据库执行者
    /// </summary>
    internal class MysqlProvider : ProviderBase
    {
        /// <summary>
        /// Mysql数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public MysqlProvider(CXConnection conection)
        {
            this.CXConnection = conection;
            this.TypeMap = new MySqlTypeMap();
        }
        /// <summary>
        /// 修改数据库的默认字符集
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        internal override void ChangeDbCharset(string dbName, string charset)
        {
            /*
            alter database maildb default character set utf8;//修改数据库的字符集
            alter table mailtable default character set utf8;//修改表的字符集
             */
            string sql = string.Format("alter database {0} default character set {1}", Quote(dbName), charset);
            Execute(sql);
        }
        /// <summary>
        /// 修改数据表的默认字符集
        /// <para>仅mysql需要</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        internal override void ChangeTableCharset(string tableName, string charset)
        {
            string sql = string.Format("alter table {0} default character set {1}", Quote(tableName), charset);
            Execute(sql);
        }

        #region Sql组建方法
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName"></param>
        internal override void CreateDB(string dbName)
        {
            string sql = string.Format("create database if not exists {0} default character set utf8 collate utf8_general_ci", Quote(dbName));
            Execute(sql);
        }
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="isTracking">是否追踪</param>
        /// <param name="columns"></param>
        internal override void AddTable(string tableName, bool isTracking = true, params Column[] columns)
        {
            if (ExistsTable(tableName))
                return;
            StringBuilder builder = new StringBuilder();
            builder.Append("create table ");
            //builder.Append("create table if not exists ");
            builder.Append(Quote(tableName));
            builder.Append(" (");
            StringBuilder indexBuild = new StringBuilder();
            foreach (var column in columns)
            {
                var tempBuild = BuildColumn(tableName, column);
                builder.Append(tempBuild.BuildSql);
                builder.Append(",");
                indexBuild.Append(tempBuild.IndexSql);
            }
            builder.Length = builder.Length - 1;
            builder.Append(") engine=");
            builder.Append(EngineOrSchama);
            builder.Append(" collate=utf8_general_ci;");
            Execute(string.Concat(builder.ToString(), indexBuild.ToString()));
            if (isTracking)
                versionHelp.AddTable(tableName, columns);
        }
        /// <summary>
        /// 组建创建列和索引的Sql语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <param name="withDefault"></param>
        /// <returns></returns>
        protected override BuildHelp BuildColumn(string tableName, Column column, bool withDefault = true)
        {
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Length = 0;
            builder.Append(Quote(column.Name));
            builder.Append(" ");
            builder.Append(GetColumnTypeString(column, withDefault));
            if (column.IsPrimaryKey)
            {
                builder.Append(",primary key (");
                builder.Append(Quote(column.Name));
                builder.Append(")");
            }
            BuildHelp returnModel = new BuildHelp();
            returnModel.BuildSql = builder.ToString();
            builder.Length = 0;
            if (IsCanIndexd(column))
            {
                builder.Append("alter table ");
                builder.Append(Quote(tableName));
                builder.Append(" add index ");
                builder.Append(tableName);
                builder.Append("_");
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
                builder.Append(tableName);
                builder.Append("_");
                builder.Append(column.Name);
                builder.Append("_asc (");
                builder.Append(Quote(column.Name));
                builder.Append(");");
            }
            returnModel.IndexSql = builder.ToString();
            return returnModel;
        }
        /// <summary>
        /// 默认架构或引擎名称
        /// <para>mssql/mysql/MariaDb/postgresql</para>
        /// </summary>
        protected override string DefaultEngineOrSchama
        {
            get
            {
                return "InnoDB";
            }
        }
        /// <summary>
        /// 转换成bool
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool DbValueToBool(object value)
        {
            int i = 0;
            if (int.TryParse(value.ToString(), out i))
                return i == 1;
            return false;
        }
        /// <summary>
        /// 获取数据库表和列名对应的完整写法
        /// </summary>
        /// <param name="tableOrColumnName"></param>
        /// <returns></returns>
        protected override string Quote(string tableOrColumnName)
        {
            return string.Format("`{0}`", tableOrColumnName);
        }
        /// <summary>
        /// 返回默认值字符串
        /// <para>mysql的blob和text不能有默认值</para>
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override string Default(Column column)
        {
            //主键不支持默认值
            if (column.IsIdentity||column.IsPrimaryKey || column.DefaultValue == null)
                return string.Empty;
            var typeText = TypeMap.GetDbString(column);
            if (typeText.IndexOf("blob", StringComparison.Ordinal) > -1 || typeText.IndexOf("text", StringComparison.Ordinal) > -1)
                return string.Empty;
            if (TypeMap.IsBoolType(column))
            {
                return String.Format(" default {0} ", (((bool)column.DefaultValue) ? 1 : 0));
            }
            if (TypeMap.IsStringType(column))
            {
                return String.Format(" default '{0}' ", column.DefaultValue);
            }
            return String.Format(" default {0} ", column.DefaultValue);
        }
        #endregion Sql组建方法

        #region 辅助判断方法
        /// <summary>
        /// 是否可以建立索引
        /// <para>mysql的blob和text还有varchar不能建立索引</para>
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override bool IsCanIndexd(Column column)
        {
            if (!column.IsIndexed)
                return false;
            var typeText = TypeMap.GetDbString(column);
            if (typeText.IndexOf("blob", StringComparison.Ordinal) > -1 || typeText.IndexOf("text", StringComparison.Ordinal) > -1)
                return false;
            return true;
        }
        /// <summary>
        /// 是否可以建立唯一索引
        /// <para>mysql的blob和text还有varchar>330不能建立唯一索引</para>
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override bool IsCanUnique(Column column)
        {
            if (!column.IsUnique)
                return false;
            var typeText = TypeMap.GetDbString(column);
            if (typeText.IndexOf("blob", StringComparison.Ordinal) > -1 || typeText.IndexOf("text", StringComparison.Ordinal) > -1)
                return false;
            if (typeText.IndexOf("varchar", StringComparison.Ordinal) > -1 && column.Size > 330)
                return false;
            return true;
        }
        #endregion 辅助判断方法
        /// <summary>
        /// 重命名列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldName"></param>
        /// <param name="newColumn"></param>
        /// <param name="isTracking"></param>
        internal override void RenameColumn(string tableName, string oldName, string newColumn, bool isTracking = true)
        {
            if (ExistsColumn(tableName, oldName) && !ExistsColumn(tableName, newColumn, false))
            {
                var column = GetColumnByName(tableName,oldName);
                column.Name = newColumn;
                AddColumn(tableName,column,false);
                //迁移数据
                Execute(string.Format("update {0} set {1}={2}", Quote(tableName), Quote(newColumn), Quote(oldName)));
                //删除旧列
                DropColumn(tableName,oldName,false);
                if (isTracking)
                    versionHelp.RenameColumn(tableName,oldName,newColumn);
            }
        }
        /// <summary>
        /// 添加主键
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void AddPrimaryKey(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            //Alter table testp add primary key(newid);
            //alter table testp MODIFY newid INT UNSIGNED AUTO_INCREMENT;

            //1.添加主键
            string sql = string.Format("alter table {0} add primary key({1})", Quote(tableName), Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddPrimaryKey(tableName, columnName);
        }
        /// <summary>
        /// 添加自增主键
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void AddPrimaryKeyIdentity(string tableName, string columnName, bool isTracking = true)
        {
            if (ExistsColumn(tableName, columnName))
            {
                //1.添加主键
                string sql = string.Format("alter table {0} add primary key({1})", Quote(tableName), Quote(columnName));
                Execute(sql);
                //2.添加自增
                sql = string.Format("alter table {0} modify {1} int auto_increment", Quote(tableName), Quote(columnName));
                //sql = string.Format("alter table {0} modify {1} int unsigned auto_increment", Quote(tableName), Quote(columnName));
                if (Execute(sql) != -1 && isTracking)
                    versionHelp.AddPrimaryKeyIdentity(tableName, columnName);
            }
        }
        /// <summary>
        /// 删除主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void DropPrimaryKey(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            //1.删除自增长
            string sql = string.Format("alter table {0} modify {1} int", Quote(tableName), Quote(columnName));
            Execute(sql);
            //2.删除主建
            sql = string.Format("alter table {0} drop primary key", Quote(tableName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.DropPrimaryKey(tableName, columnName);
        }
    }
}
