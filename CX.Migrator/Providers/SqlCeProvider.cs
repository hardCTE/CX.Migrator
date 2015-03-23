using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CX.Migrator.Configs;
using CX.Migrator.Framework;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// SqlCe数据库执行者
    /// </summary>
    internal class SqlCeProvider : ProviderBase
    {
        /// <summary>
        /// SqlCe数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public SqlCeProvider(CXConnection conection)
        {
            this.CXConnection = conection;
            this.TypeMap = new SqlCeTypeMap();
        }
        #region 辅助方法
        /// <summary>
        /// 获取查询最后一个成功执行的数据库的版本号
        /// </summary>
        protected override string GetLastVersionSql
        {
            get
            {
                return "select top 1 V from Migrator_Version order by Id desc";
            }
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="isProcedure">是否存储过程</param>
        /// <returns></returns>
        protected override int Execute(string sql, bool isProcedure = false)
        {
            /*
            try
            {
                IDbCommand command = CreateCommand(sql);
                return command.ExecuteNonQuery();
                LogToText(sql);
            }
            catch (Exception ex)
            {
                isRollbacking = true;
                LogToText(sql + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                return 0;
            }
            //*/
            var sqlCommands = sql.Split(';');
            int count = 0;
            foreach (var commandText in sqlCommands)
            {
                if (string.IsNullOrEmpty(commandText))
                    continue;
                try
                {
                    IDbCommand command = CreateCommand(commandText);
                    count += command.ExecuteNonQuery();
                    LogToText(commandText);
                }
                catch (Exception ex)
                {
                    isRollbacking = true;
                    LogToText(commandText + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
                    return 0;
                }
            }
            return count;
        }
        #endregion 辅助方法

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
                return;
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
            builder.Append(")");
            Execute(builder.ToString());
            Execute(indexBuild.ToString());
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
        protected override BuildHelp BuildColumn(string tableName, Column column,bool withDefault=true)
        {
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Length = 0;
            builder.Append(Quote(column.Name));
            builder.Append(" ");
            builder.Append(GetColumnTypeString(column, withDefault));
            BuildHelp returnModel = new BuildHelp();
            returnModel.BuildSql = builder.ToString();
            builder.Length = 0;
            if (IsCanIndexd(column))
            {
                builder.Append("create nonclustered index ");
                //builder.Append("create index ");
                builder.Append(tableName);
                builder.Append("_");
                builder.Append(column.Name);
                builder.Append("_asc on ");
                builder.Append(Quote(tableName));
                builder.Append(" (");
                builder.Append(Quote(column.Name));
                builder.Append(" desc);");
            }
            else if (IsCanUnique(column))
            {
                builder.Append("create unique nonclustered index ");
                //builder.Append("create unique index ");
                builder.Append(tableName);
                builder.Append("_");
                builder.Append(column.Name);
                builder.Append("_asc on ");
                builder.Append(Quote(tableName));
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
            //return tableOrColumnName;
            return string.Format("[{0}]", tableOrColumnName);
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
            if (column.IsIdentity || column.DefaultValue == null)
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

        #region 辅助判断方法
        /// <summary>
        /// 是否可以建立索引
        /// <para>mssql的text还有varchar>450不能建立索引</para>
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override bool IsCanIndexd(Column column)
        {
            if (!column.IsIndexed)
                return false;
            var typeText = TypeMap.GetDbString(column);
            if (typeText.IndexOf("text", StringComparison.Ordinal) > -1)
                return false;
            if (typeText.IndexOf("varchar", StringComparison.Ordinal) > -1 && column.Size > 450)
                return false;
            return true;
        }
        /// <summary>
        /// 是否可以建立唯一索引
        /// <para>mssql的text还有varchar>450不能建立唯一索引</para>
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override bool IsCanUnique(Column column)
        {
            if (!IsCanIndexd(column))
                return false;
            return column.IsUnique;
        }
        /// <summary>
        /// 查询表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="outTip">如果与结果同为真或同为假,输出报告</param>
        /// <returns></returns>
        protected override bool ExistsTable(string tableName, bool outTip = true)
        {
            bool result = false;
            using (IDataReader reader =
                ExecuteReader(string.Format("select * from information_schema.tables where table_name='{0}'", tableName)))
            {
                result = reader.Read();
            }
            if (result && outTip)
                ConsoleExists(tableName);
            else if (!result && !outTip)
                ConsoleNotExists(tableName);
            return result;
        }
        #endregion 辅助判断方法

        #region 约束管理
        /// <summary>
        /// 根据约束名称移除约束
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="constraintName">约束名称</param>
        internal override void DropConstraintByName(string tableName, string constraintName)
        {
            if (!ExistsConstraintName(tableName,constraintName))
            {
                return;
            }
            Execute(String.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", Quote(tableName), Quote(constraintName)));
        }
        /// <summary>
        /// 移除列上所有约束
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列的名称</param>
        internal override void DropConstraint(string tableName, string columnName)
        {
            var list = GetAllConstraint(tableName,columnName);
            if (list != null)
            {
                foreach (var constraint in list)
                    Execute(string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", Quote(tableName), Quote(constraint)));
            }
            else
                ConsoleOut(string.Concat("表",tableName,"的列",columnName,"上不存在任何约束"));
        }
        /// <summary>
        /// 移除列上所有索引
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列的名称</param>
        internal void DropIndexs(string tableName, string columnName)
        {
            var sql = string.Concat("sp_helpindex ", Quote(tableName));
            var command = CreateCommand(sql);
            string tempStart = string.Concat(columnName,"(").ToLower();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string key = null;
                    string name = null;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        switch (reader.GetName(i).ToLower())
                        {
                            case "index_keys":
                                if (reader.GetValue(i).ToString().ToLower().IndexOf(tempStart, StringComparison.Ordinal) == 0)
                                    key = reader.GetValue(i).ToString();
                                break;
                            case "index_name":
                                name = reader.GetValue(i).ToString();
                                break;
                        }
                    }
                    if (key != null && name != null)
                    {
                        var tempsql = string.Concat("drop index ", Quote(tableName), ".", Quote(name));
                        dict[tempsql] = key;
                    }
                }
            }
            foreach(string key in dict.Keys)
            {
                Execute(key);
            }
        }
        /// <summary>
        /// 获取列的所有约束名称
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        protected List<string> GetAllConstraint(string tableName, string columnName)
        {
            string sql = string.Format(@"select object_name(constid) as name from syscolumns as a,sysconstraints as b where a.id=b.id and a.colid=b.colid and object_name(a.id)='{0}'and a.name='{1}'", tableName, columnName);
            List<string> list = null;
            using (IDataReader reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    if (list == null)
                        list = new List<string>();
                    list.Add(reader.GetValue(0).ToString());
                }
            }
            return list;
        }
        /// <summary>
        /// 查询列是否存在约束
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal override bool ExistsConstraints(string tableName, string columnName)
        {
            return GetAllConstraint(tableName, columnName) != null;
        }
        /// <summary>
        /// 查询约束是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="constraintName"></param>
        /// <returns></returns>
        internal override bool ExistsConstraintName(string tableName, string constraintName, bool outTip = true)
        {
            bool result = false;
            string sql = string.Format(@"select 1 from syscolumns as a,sysconstraints as b where a.id=b.id and a.colid=b.colid and object_name(a.id)= '{0}' and object_name(constid)='{1}'", tableName, constraintName);
            using (IDataReader reader = ExecuteReader(sql))
            {
                result = reader.Read();
            }
            if (!result && outTip)
                ConsoleOut(string.Concat("约束名",constraintName,"不存在"));
            return result;
        }
        /// <summary>
        /// 禁用表上所有约束
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal override void DisableAllConstraint(string tableName)
        {
            if (!ExistsTable(tableName, false))
            {
                return;
            }
            Execute(string.Format("alter table {0} nocheck constraint all", Quote(tableName)));
        }
        /// <summary>
        /// 启用表上所有约束
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal override void EnableAllConstraint(string tableName)
        {
            if (!ExistsTable(tableName, false))
            {
                return;
            }
            Execute(string.Format("alter table {0} check constraint all", Quote(tableName)));
        }
        #endregion 约束管理
        /// <summary>
        /// 添加新列
        /// <para>Sqlite添加列时如果指定非空同时还必须指定默认值</para>
        /// <para>对数据没影响</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="newColumn"></param>
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
        /// 添加外键约束
        /// <para>表和列必须都存在</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">要添加外键的列</param>
        /// <param name="refTable">外键目标表名</param>
        /// <param name="refColumn">外键目标表列</param>
        internal override void AddForeignKey(string tableName, string column, string refTable, string refColumn)
        {
            LogToText("SqlCe数据库不支持外键");
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
            var tableInfo = versionHelp.GetTableByName(oldTableName);
            if (tableInfo == null)
            {
                ConsoleOut("表设定信息丢失,重命名失败");
                return;
            }
            //1.添加新表
            AddTable(newTableName, tableInfo.ColumnArray(), false);
            string columnFields = tableInfo.GetInsertColumns();
            //2.插入数据
            Execute(string.Format("insert into {0} ({1}) select {1} from {2}", Quote(newTableName), columnFields, Quote(oldTableName)));
            //3.删除旧表
            if (Execute(string.Concat("drop table ", Quote(oldTableName))) != -1 && isTracking)
                versionHelp.RenameTable(oldTableName, newTableName);
        }
        #region 特殊辅助方法
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
            string sql = string.Format("alter table {0} drop column {1}", Quote(tableName), Quote(columnName));
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
                string sql = string.Format("alter table {0} alter column {1} {2}", Quote(tableName), Quote(oldColumnName), TypeMap.GetDbString(column));
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
        /// <summary>
        /// 重命名列
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
            Column column = GetColumnByName(tableName, oldName);
            if (column == null)
            {
                ConsoleOut("表设定信息丢失无法执行重命名操作");
                return;
            }
            column.Name = newColumn;
            AddColumn(tableName, column, false);
            Execute(string.Format("update {0} set {1}={2}", Quote(tableName), Quote(newColumn), Quote(oldName)));
            DropColumn(tableName, oldName, false);
            if (isTracking)
                versionHelp.RenameColumn(tableName, oldName, newColumn);
        }
        //EXEC sp_rename '[EEE].[Age]', '[Age111]', 'COLUMN'
        /// <summary>
        /// 查询列是否存在
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="outTip"></param>
        /// <returns></returns>
        internal override bool ExistsColumn(string table, string column,bool outTip=true)
        {
            if (!ExistsTable(table, false))
                return false;
            bool result = false;
            using (var reader =ExecuteReader(string.Format("select * from information_schema.columns where table_name='{0}' and column_name='{1}'", table, column)))
            {
                result = reader.Read();
            }
            if (result && outTip)
                ConsoleExists(table,column);
            else if (!result && !outTip)
                ConsoleNotExists(table,column);
            return result;
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
            //alter table [ATest] add constraint PK_TableName primary key([newid])
            //1.添加主键
            string sql = string.Format("alter table {0} add constraint PK_{1}_{2} primary key({3})", Quote(tableName), tableName, columnName, Quote(columnName));
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
            string sql = string.Format("alter table {0} add {1} int identity(1,1) not null primary key", Quote(tableName), Quote(columnName));
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
            //if (!ExistsConstraintName(tableName, "PK_" + tableName,false))
            //    return;
            //DropConstraint(tableName, columnName);
            string pkName = null;
            string sql = string.Format("Select Name from sysobjects where Parent_Obj=OBJECT_ID('{0}') and xtype='PK'", tableName);
            using (var reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    pkName = reader.GetValue(0) as string;
                }
            }
            if (pkName == null)
                return;
            //Alter table tableName Drop
            DropConstraintByName(tableName, pkName);
            if (isTracking)
                versionHelp.DropPrimaryKey(tableName, columnName);
        }
        #endregion 特殊辅助方法
    }
}