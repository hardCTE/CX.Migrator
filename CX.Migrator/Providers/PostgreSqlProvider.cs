using System;
using System.Data;
using System.Text;
using CX.Migrator.Configs;
using CX.Migrator.Framework;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// PostgreSql数据库执行者
    /// </summary>
    internal class PostgreSqlProvider : ProviderBase
    {
        /// <summary>
        /// PostgreSql数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public PostgreSqlProvider(CXConnection conection)
        {
            this.CXConnection = conection;
            this.TypeMap = new PostgreSqlTypeMap();            
        }
        #region Sql组建方法
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="isTracking">是否追踪</param>
        /// <param name="columns"></param>
        internal override void AddTable(string tableName, bool isTracking = true, params Column[] columns)
        {
            if (ExistsTable(tableName))
            {
                return;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("create table ");
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
            builder.Append(");");
            Execute(string.Concat(builder.ToString(), indexBuild.ToString()));
            if (isTracking)
                versionHelp.AddTable(tableName, columns);
        }
        /// <summary>
        /// 获取列名后面的类型和自增列主键默认值等信息
        /// </summary>
        /// <param name="column"></param>
        /// <param name="withDefault"></param>
        /// <returns></returns>
        protected override string GetColumnTypeString(Column column, bool withDefault = true)
        {
            switch (column.ColumnProperty)
            {
                case ColumnProperty.PrimaryKey_Identity:
                case ColumnProperty.Identity:
                case ColumnProperty.Identity_NotNull:
                    return string.Concat(TypeMap.MapTypeString(column.ColumnProperty), withDefault?Default(column):string.Empty);
                default:
                    return string.Concat(TypeMap.GetDbString(column), " ", TypeMap.MapTypeString(column.ColumnProperty), withDefault ? Default(column) : string.Empty);
            }
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
            builder.Append(GetColumnTypeString(column,withDefault));
            BuildHelp returnModel = new BuildHelp();
            returnModel.BuildSql = builder.ToString();
            builder.Length = 0;
            if (IsCanIndexd(column))
            {
                builder.Append("create index ");
                builder.Append(FindRealTableName(tableName, true));
                builder.Append("_");
                builder.Append(FindRealColumnName(tableName, column.Name, true));
                builder.Append("_desc  on ");
                builder.Append(Quote(tableName));
                builder.Append(" (");
                builder.Append(Quote(column.Name));
                builder.Append(" desc);");
            }
            else if (IsCanUnique(column))
            {
                builder.Append("create unique index ");
                builder.Append(FindRealTableName(tableName, true));
                builder.Append("_");
                builder.Append(FindRealColumnName(tableName, column.Name, true));
                builder.Append("_desc  on ");
                builder.Append(tableName);
                builder.Append(" (");
                builder.Append(Quote(column.Name));
                builder.Append(" desc);");
            }
            returnModel.IndexSql = builder.ToString();
            return returnModel;
        }
        /// <summary>
        /// 获取数据库表和列名对应的完整写法
        /// </summary>
        /// <param name="tableOrColumnName"></param>
        /// <returns></returns>
        protected override string Quote(string tableOrColumnName)
        {
            return tableOrColumnName;
            //return string.Format("\"{0}\"", tableOrColumnName);
        }
        /*
        /// <summary>
        /// 获取数据库表名称完整写法
        /// <para>postgresql专用</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected override string QuoteTableName(string tableName)
        {
            return string.Format("\"{0}\"", tableName.ToLower());
        }
        //*/
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
            /*
            var typeText = typeMap.GetDbString(column);
            if (typeText.IndexOf("blob", StringComparison.Ordinal) > -1 || typeText.IndexOf("text", StringComparison.Ordinal) > -1)
                return string.Empty;
            //*/
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

        #region 列级修改
        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void DropColumn(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            string sql = string.Format("alter table {0} drop {1}", Quote(FindRealTableName(tableName, true)), Quote(FindRealColumnName(tableName, columnName, true)));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.DropColumn(tableName, columnName);
        }
        /// <summary>
        /// 添加主键
        /// <para>主键必须为不能为空的列</para>
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
            string sql = string.Format("alter table {0} add primary key ({1})", Quote(tableName), Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddPrimaryKey(tableName, columnName);
        }
        /// <summary>
        /// 添加自增主键
        /// <para>主键必须不存在</para>
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void AddPrimaryKeyIdentity(string tableName, string columnName, bool isTracking = true)
        {
            if (ExistsColumn(tableName, columnName))
            {
                return;
            }
            //1.添加主键
            string sql = string.Format("alter table {0} add {1} serial primary key not null", Quote(tableName), Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddColumn(tableName, new Column { ColumnProperty = ColumnProperty.PrimaryKey_Identity, Type = DbType.Int32, Name = columnName, Size = 1 });
            //versionHelp.AddPrimaryKeyIdentity(tableName, columnName);
        }
        /// <summary>
        /// 删除主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void DropPrimaryKey(string tableName, string columnName, bool isTracking = true)
        {
            //ALTER TABLE "TestA" DROP CONSTRAINT "TestA_pkey"; 
            string sql = string.Format("alter table {0} drop constraint {1}", Quote(tableName), Quote(tableName + "_pkey"));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.DropPrimaryKey(tableName, columnName);
        }
        #endregion 列级修改

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
            //var typeText = typeMap.GetDbString(column);
            //if (typeText.IndexOf("blob", StringComparison.Ordinal) > -1 || typeText.IndexOf("text", StringComparison.Ordinal) > -1)
            //    return false;
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
            //var typeText = typeMap.GetDbString(column);
            //if (typeText.IndexOf("blob", StringComparison.Ordinal) > -1 || typeText.IndexOf("text", StringComparison.Ordinal) > -1)
            //    return false;
            //if (typeText.IndexOf("varchar", StringComparison.Ordinal) > -1 && column.Size > 330)
            //    return false;
            return true;
        }
        /// <summary>
        /// 查询表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        protected override bool ExistsTable(string tableName, bool outTip = true)
        {
            var result = FindRealTableName(tableName) != null;
            if (result && outTip)
                ConsoleExists(tableName);
            else if (!result && !outTip)
                ConsoleNotExists(tableName);
            return result;
        }
        /// <summary>
        /// 查找真实的TableName，用来应对区分大小写的问题
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="returnDefault">返回默认值</param>
        /// <returns></returns>
        protected string FindRealTableName(string tableName, bool returnDefault = false)
        {
            using (IDataReader reader = ExecuteReader(string.Format("select table_name from information_schema.tables where table_schema = '{0}' and lower(table_name) = lower('{1}')", EngineOrSchama, tableName)))
            {
                while(reader.Read())
                {
                    return reader.GetValue(0).ToString();
                }
            }
            return returnDefault ? tableName : null;
        }
        /// <summary>
        /// 查找真实的columnName，用来应对区分大小写的问题
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="returnDefault">返回默认值</param>
        /// <returns></returns>
        protected string FindRealColumnName(string tableName,string columnName,bool returnDefault=false)
        {
            if (!ExistsTable(tableName, false))
            {
                return returnDefault ? columnName : null;
            }
            using (var reader = ExecuteReader(string.Format("select column_name from information_schema.columns where table_schema = '{0}' and lower(table_name) = lower('{1}') and lower(column_name) = lower('{2}')", EngineOrSchama, tableName, columnName)))
            {
                while (reader.Read())
                {
                    return reader.GetValue(0).ToString();
                }
            }
            return returnDefault?columnName:null;
        }
        /// <summary>
        /// 查询列是否存在
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        internal override bool ExistsColumn(string table, string column,bool outTip=true)
        {
            var result = FindRealColumnName(table, column) != null;
            if (result && outTip)
                ConsoleExists(table,column);
            else if (!result && !outTip)
                ConsoleNotExists(table,column);
            return result;
        }
        #endregion 辅助判断方法
        /// <summary>
        /// 添加新列
        /// <para>Sqlite添加列时如果指定非空同时还必须指定默认值</para>
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="newColumn"></param>
        /// <param name="isTracking"></param>
        internal override void AddColumn(string tableName, Column newColumn, bool isTracking = true)
        {
            if (ExistsColumn(tableName, newColumn.Name))
            {
                return;
            }
            if (newColumn.IsNotNull)
            {
                string defaultValue = Default(newColumn);
                if (string.IsNullOrEmpty(defaultValue))
                {
                    string tip = string.Format("列{0}指定为非空的同时必须指定默认值", newColumn.Name);
                    LogToText(tip);
                    return;
                }
            }
            var builded = BuildColumn(tableName, newColumn);
            string sql = string.Format("alter table {0} add {1};{2}", Quote(tableName), builded.BuildSql, builded.IndexSql);
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddColumn(tableName, newColumn);
        }
        /// <summary>
        /// 重命名表名称
        /// </summary>
        /// <param name="oldTableName"></param>
        /// <param name="newTableName"></param>
        /// <param name="isTracking"></param>
        internal override void RenameTable(string oldTableName, string newTableName, bool isTracking = true)
        {
            if (!ExistsTable(oldTableName, false) || ExistsTable(newTableName))
            {
                return;
            }
            string sql = string.Format("alter table {0} rename to {1}", Quote(FindRealTableName(oldTableName, true)), Quote(newTableName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.RenameTable(oldTableName, newTableName);
        }
        #region 特殊辅助方法
        /// <summary>
        /// 重命名列名
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldName"></param>
        /// <param name="newColumn"></param>
        /// <param name="isTracking"></param>
        internal override void RenameColumn(string tableName, string oldName, string newColumn, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, oldName, false) || ExistsColumn(tableName, newColumn))
            {
                return;
            }
            string sql = string.Format("alter table {0} rename column {1} to {2}", Quote(tableName), Quote(oldName), Quote(newColumn));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.RenameColumn(tableName, oldName, newColumn);
        }
        /// <summary>
        /// 修改列
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="column"></param>
        /// <param name="isTracking"></param>
        internal override void ChangeColumn(string tableName, string oldColumnName, Column column, bool isTracking = true)
        {
            string oldlower = oldColumnName.ToLower();
            string newlower = column.Name.ToLower();
            if (!ExistsColumn(tableName, oldColumnName, false) || (oldlower != newlower && ExistsColumn(tableName, column.Name)))
            {
                return;
            }
            //修改数据类型
            if (oldlower == newlower)
            {
                string sql = string.Format("alter table {0} alter column {1} type {2}", Quote(tableName), Quote(oldColumnName), TypeMap.GetDbString(column));
                Execute(sql);
            }
            else
            {
                AddColumn(tableName, column, false);
                string sql1 = string.Format("update {0} set {1}={2}", Quote(tableName), Quote(column.Name), Quote(oldColumnName));
                Execute(sql1);
                DropColumn(tableName, oldColumnName, false);
            }
            if (isTracking)
                versionHelp.ChangeColumn(tableName, oldColumnName, column);
        }
        #endregion 特殊辅助方法

        #region 约束管理
        /// <summary>
        /// 移除约束
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="constraintName">约束名称</param>
        internal override void DropConstraintByName(string tableName, string constraintName)
        {
            /*
            if(!ExistsConstraintName(tableName,constraintName))
            {
                return;
            }//*/
            //alter table products drop constraint some_name;
            string sql = string.Format("alter table {0} drop constraint {1}", Quote(FindRealTableName(tableName, true)), constraintName);
            Execute(sql);
        }
        /// <summary>
        /// 在列上建立普通索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void AddIndex(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            string sql = string.Format("create index {0}_{1}_desc on {2} ({3} desc);", tableName, columnName, Quote(tableName), Quote(columnName));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddIndex(tableName, columnName);
        }
        /// <summary>
        /// 在列上建立唯一索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="isTracking"></param>
        internal override void AddUnique(string tableName, string columnName, bool isTracking = true)
        {
            if (!ExistsColumn(tableName, columnName, false))
            {
                return;
            }
            string sql = string.Format("create unique index {0}_{1}_desc on {2} ({3} desc);", tableName, columnName, Quote(FindRealTableName(tableName, true)), Quote(FindRealColumnName(tableName, columnName, true)));
            if (Execute(sql) != -1 && isTracking)
                versionHelp.AddUnique(tableName,columnName);
        }
        /// <summary>
        /// 添加外键约束
        /// <para>表和列必须都存在</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">要添加外键的列</param>
        /// <param name="refTable">外键目标表名</param>
        /// <param name="refColumn">外键目标表列</param>
        internal override void AddForeignKey(string tableName, string column, string refTable, string refColumn)
        {
            if (!ExistsColumn(tableName, column, false) || !ExistsColumn(refTable, refColumn, false))
            {
                return;
            }
            string sql = string.Format("alter table {0} add constraint FK_{1}_{2} foreign key ({3}) references {4} ({5})", Quote(FindRealTableName(tableName, true)), tableName, column, Quote(FindRealColumnName(tableName, column, true)), Quote(FindRealTableName(refTable, true)), Quote(FindRealColumnName(refTable, refColumn, true)));
            Execute(sql);
        }
        /*FOREIGN KEY 限制必須參考 PRIMARY KEY 或 UNIQUE 限制。
        兩個鍵欄位必須有相容的資料類型。
        必須在參考和被參考的資料表有 REFERENCES 權限。//*/
        #endregion 约束管理

        #region DB级管理
        /// <summary>
        /// 查询数据库是否存在
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        internal override bool ExistsDb(string dbName,bool outTip=true)
        {
            var dbOld = CXConnection.Connection.Database;
            try
            {
                CXConnection.Connection.ChangeDatabase(dbName);
                CXConnection.Connection.ChangeDatabase(dbOld);
                //var command = CreateCommand(string.Concat("use ",Quote(dbName)));
                //command.ExecuteNonQuery();
                return true;
            }
            catch(Exception ex)
            {
                CXConnection.Connection.ChangeDatabase(dbOld);
                if (!outTip)
                    ConsoleNotExists(string.Concat("数据库",dbName,"不存在"));
                return false;
            }
        }
        /// <summary>
        /// 更换数据库连接的目标数据库
        /// </summary>
        /// <param name="dbName"></param>
        internal override void UseDb(string dbName)
        {
            try
            {
                CXConnection.SaveTrsation(false);
                CXConnection.Connection.ChangeDatabase(dbName);
                CXConnection.BeginTransaction();
            }
            catch
            {
                CXConnection.BeginTransaction();
            }
        }
        /// <summary>
        /// 默认架构或引擎名称
        /// <para>mssql/mysql/MariaDb/postgresql</para>
        /// </summary>
        protected override string DefaultEngineOrSchama
        {
            get
            {
                return "public";
            }
        }
        #endregion DB级管理
    }
}
/*Sqlite修改列做法
1、修改原表的名称
ALTER TABLE table RENAME TO tableOld;
2、新建修改字段后的表
CREATE TABLE table(ID INTEGER PRIMARY KEY AUTOINCREMENT, Modify_Username text not null);
3、从旧表中查询出数据 并插入新表
INSERT INTO table SELECT ID,Username FROM tableOld;
4、删除旧表
DROP TABLE tableOld;
//*/