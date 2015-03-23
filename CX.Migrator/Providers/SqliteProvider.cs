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
    /// Sqlite数据库执行者
    /// </summary>
    internal class SqliteProvider : ProviderBase
    {
        /// <summary>
        /// Mysql数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public SqliteProvider(CXConnection conection)
        {
            this.CXConnection = conection;
            this.TypeMap = new SqliteTypeMap();
        }

        #region Sql组建方法
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName"></param>
        internal override void CreateDB(string dbName)
        {
            ConsoleOut("Sqlite不存在Db概念");
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
            //builder.Append("create table if not exists ");
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
                builder.Append("create index [");
                builder.Append(tableName);
                builder.Append("_");
                builder.Append(column.Name);
                builder.Append("_asc]  on ");
                builder.Append(Quote(tableName));
                builder.Append(" (");
                builder.Append(Quote(column.Name));
                builder.Append(" asc);");
            }
            else if (IsCanUnique(column))
            {
                builder.Append("create unique index [");
                builder.Append(tableName);
                builder.Append("_");
                builder.Append(column.Name);
                builder.Append("_asc]  on ");
                builder.Append(Quote(tableName));
                builder.Append(" (");
                builder.Append(Quote(column.Name));
                builder.Append(" asc);");
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
        /// 查询数据库是否存在
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        internal override bool ExistsDb(string dbName, bool outTip = true)
        {
            if (outTip)
                Console.Out.WriteLine("Sqlite不存在Db概念");
            return false;
        }
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
        /// <param name="outTip">同为真或同为假,输出信息</param>
        /// <returns></returns>
        protected override bool ExistsTable(string tableName, bool outTip = true)
        {
            bool result = false;
            using (IDataReader reader = ExecuteReader(string.Format("select name from sqlite_master where type='table' and name='{0}'", tableName)))
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
        /// 添加外键约束
        /// <para>表和列必须都存在</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">要添加外键的列</param>
        /// <param name="refTable">外键目标表名</param>
        /// <param name="refColumn">外键目标表列</param>
        internal override void AddForeignKey(string tableName, string column, string refTable, string refColumn)
        {
            string tip = "Sqlite数据库不支持外键";
            LogToText(tip);
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
            string sql = string.Format("alter table {0} rename to {1}", Quote(oldTableName), Quote(newTableName));
            if (Execute(sql) != -1 && isTracking)
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
            var dict = GetTableAllBuildingInfo(tableName);
            if (dict == null || dict.Count < 1)
                return;
            var newTableName = string.Concat(tableName, "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            //1、修改原表的名称
            var command1 = CreateCommand(string.Format("alter table {0} rename to {1}", Quote(tableName), Quote(newTableName)));
            command1.ExecuteNonQuery();
            //2、新建修改字段后的表(替换建表函数和索引)
            string newSql = null;
            foreach (var key in dict.Keys)
            {
                if (dict[key].IsTableSql)
                {
                    newSql = dict[key].Sql;
                    break;
                }
            }
            StringReplace replace = new StringReplace();
            var tempStart = newSql.IndexOf('(') + 1;
            var tempEnd = newSql.LastIndexOf(')');
            var fileds = newSql.Substring(tempStart, tempEnd - tempStart).Split(',');
            var tempOldColumnName = Quote(columnName);
            //找出旧字段
            for (int i = 0; i < fileds.Length; i++)
            {
                if (replace.Replace(fileds[i], tempOldColumnName, string.Empty))
                {
                    ///置空
                    fileds[i] = string.Empty;
                }
            }
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Length = 0;
            for (int i = 0; i < fileds.Length; i++)
            {
                if (string.IsNullOrEmpty(fileds[i]))
                    continue;
                builder.Append(fileds[i]);
                if (i < fileds.Length - 1)
                    builder.Append(",");
            }
            if (builder.ToString().EndsWith(","))
                builder.Length -= 1;
            newSql = string.Concat(newSql.Substring(0, tempStart), builder.ToString(), newSql.Substring(tempEnd));
            Execute(newSql);
            //3、从旧表中查询出数据 并插入新表
            //"pragma table_info(CX_Log)"
            var command2 = CreateCommand(string.Format("pragma table_info({0})", newTableName));
            List<string> list = new List<string>();
            using (var reader2 = command2.ExecuteReader())
            {
                while (reader2.Read())
                {
                    for (int i = 0; i < reader2.FieldCount; i++)
                    {
                        if (reader2.GetName(i).ToLower() == "name")
                            list.Add(reader2.GetValue(i).ToString());
                    }
                    //break;
                }
            }
            builder.Length = 0;
            var tempOldColumn = columnName.ToLower();
            for (int i = 0; i < list.Count; i++)
            {
                //忽略旧列
                if (list[i].ToLower() == tempOldColumn)
                    continue;
                else
                    builder.Append(list[i]);
                if (i < list.Count - 1)
                    builder.Append(",");
            }
            Execute(string.Format("insert into {0} ({1}) select {1} from {2}", Quote(tableName), builder.ToString(), Quote(newTableName)));

            //4、删除旧表
            Execute(string.Concat("drop table ", newTableName));
            //创建索引
            var tempKeyPart = string.Concat("_", columnName, "_asc");
            foreach (var key in dict.Keys)
            {
                if (replace.Replace(key, tempKeyPart, string.Empty))
                {
                    continue;
                }
                if (!dict[key].IsTableSql)
                {
                    Execute(dict[key].Sql);
                }
            }
            if (isTracking)
                versionHelp.DropColumn(tableName, columnName);
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
            var dict = GetTableAllBuildingInfo(tableName);
            if (dict == null || dict.Count < 1)
                return;
            var newTableName = string.Concat(tableName, "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            //1、修改原表的名称
            var command1 = CreateCommand(string.Format("alter table {0} rename to {1}", Quote(tableName), Quote(newTableName)));
            command1.ExecuteNonQuery();
            //2、新建修改字段后的表(替换建表函数和索引)
            string newSql = null;
            foreach (var key in dict.Keys)
            {
                if (dict[key].IsTableSql)
                {
                    newSql = dict[key].Sql;
                    break;
                }
            }
            StringReplace replace = new StringReplace();
            var tempStart = newSql.IndexOf('(') + 1;
            var tempEnd = newSql.LastIndexOf(')');
            var fileds = newSql.Substring(tempStart, tempEnd - tempStart).Split(',');
            var tempOldColumnName = Quote(oldName);
            //找出旧字段
            for (int i = 0; i < fileds.Length; i++)
            {
                if (replace.Replace(fileds[i], tempOldColumnName, Quote(newColumn)))
                    fileds[i] = replace.Result;
            }
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Length = 0;
            for (int i = 0; i < fileds.Length; i++)
            {
                builder.Append(fileds[i]);
                if (i < fileds.Length - 1)
                    builder.Append(",");
            }
            newSql = string.Concat(newSql.Substring(0, tempStart), builder.ToString(), newSql.Substring(tempEnd));
            Execute(newSql);
            //3、从旧表中查询出数据 并插入新表
            //"pragma table_info(CX_Log)"
            IDbCommand command2 = CreateCommand(string.Format("pragma table_info({0})", newTableName));
            List<string> list = new List<string>();
            using (var reader2 = command2.ExecuteReader())
            {
                while (reader2.Read())
                {
                    for (int i = 0; i < reader2.FieldCount; i++)
                    {
                        if (reader2.GetName(i).ToLower() == "name")
                            list.Add(reader2.GetValue(i).ToString());
                    }
                    //break;
                }
            }
            builder.Length = 0;
            var tempOldColumn = oldName.ToLower();
            for (int i = 0; i < list.Count; i++)
            {
                //替换旧列
                if (list[i].ToLower() == tempOldColumn)
                    builder.Append(newColumn);
                else
                    builder.Append(list[i]);
                if (i < list.Count - 1)
                    builder.Append(",");
            }
            Execute(string.Format("insert into {0} ({1}) select * from {2}", Quote(tableName), builder.ToString(), Quote(newTableName)));

            //4、删除旧表
            Execute(string.Concat("drop table ", newTableName));
            //创建索引
            var tempKeyPart = string.Concat("_", oldName, "_asc");
            foreach (var key in dict.Keys)
            {
                if (replace.Replace(key, tempKeyPart, string.Empty))
                {
                    continue;
                }
                if (!dict[key].IsTableSql)
                {
                    Execute(dict[key].Sql);
                }
            }
            var tempCompare = string.Concat("_", oldName, "_");
            foreach (var key in dict.Keys)
            {
                if (!dict[key].IsTableSql)
                {
                    if (replace.Replace(dict[key].Sql, Quote(oldName), Quote(newColumn)))
                    {
                        var indexSql = replace.Result;
                        if (replace.Replace(indexSql, tempCompare, string.Concat("_", newColumn, "_")))
                        {
                            Execute(replace.Result);
                        }
                    }
                }
            }
            if (isTracking)
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
            _ChangeColumn(tableName, oldColumnName, column);
            if (isTracking)
                versionHelp.ChangeColumn(tableName, oldColumnName, column);
        }
        /// <summary>
        /// 获取表的所有创建Sql和索引命令
        /// <para>存储在系统表中</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected Dictionary<string, SqliteIndexEntity> GetTableAllBuildingInfo(string tableName)
        {
            Dictionary<string, SqliteIndexEntity> dict;
            IDbCommand command = CreateCommand(string.Format("select * from sqlite_master where lower(tbl_name)=lower('{0}') order by name", tableName));
            using (var reader = command.ExecuteReader())
            {
                dict = new Dictionary<string, SqliteIndexEntity>();
                while (reader.Read())
                {
                    SqliteIndexEntity setting = new SqliteIndexEntity();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        switch (reader.GetName(i).ToLower())
                        {
                            case "name":
                                setting.Name = reader.GetValue(i) as string;
                                if (setting.Name != null)
                                    setting.Name = setting.Name.ToLower();
                                break;
                            case "sql":
                                setting.Sql = reader.GetValue(i) as string;
                                break;
                            case "type":
                                var tempType = (reader.GetValue(i) as string);
                                if (tempType != null)
                                {
                                    setting.IsTableSql = "table" == tempType.ToLower();
                                }
                                break;
                        }
                    }
                    dict[setting.Name] = setting;
                }
            }
            return dict;
        }
        protected void _ChangeColumn(string tableName, string oldColumnName, Column column)
        {
            var dict = GetTableAllBuildingInfo(tableName);
            if (dict == null || dict.Count < 1)
                return;
            //var oldTableName = tableName.ToLower();
            var newTableName = string.Concat(tableName, "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            //1、修改原表的名称
            var command1 = CreateCommand(string.Format("alter table {0} rename to {1}", Quote(tableName), Quote(newTableName)));
            command1.ExecuteNonQuery();
            //2、新建修改字段后的表(替换建表函数和索引)
            string newSql = null;
            foreach(var key in dict.Keys)
            {
                if(dict[key].IsTableSql)
                {
                    newSql = dict[key].Sql;
                    break;
                }
            }
            StringReplace replace = new StringReplace();
            var tempStart = newSql.IndexOf('(')+1;
            var tempEnd = newSql.LastIndexOf(')');
            var fileds = newSql.Substring(tempStart, tempEnd-tempStart).Split(',');
            var buildHelp = BuildColumn(tableName, column);
            //var tempOldColumnName= Quote(oldColumnName);
            //找出旧字段
            for (int i = 0; i < fileds.Length;i++ )
            {
                if(replace.Replace(fileds[i],Quote(oldColumnName),""))
                {
                    fileds[i] = buildHelp.BuildSql;
                    break;
                }
                /*
                var tempOld=fileds[i].Split(' ')[0].ToLower();
                if (tempOld == oldColumnName || tempOld == tempOldColumnName)
                {
                    fileds[i] = buildHelp.BuildSql;
                    break;
                }//*/
            }
            builder.Length = 0;
            for (int i = 0; i < fileds.Length; i++)
            {
                builder.Append(fileds[i]);
                if (i < fileds.Length - 1)
                    builder.Append(",");
            }
            newSql = string.Concat(newSql.Substring(0, tempStart), builder.ToString(), newSql.Substring(tempEnd));
            Execute(newSql);
            //3、从旧表中查询出数据 并插入新表
            //"pragma table_info(CX_Log)"
            IDbCommand command2 = CreateCommand(string.Format("pragma table_info({0})", newTableName));
            List<string> list = new List<string>();
            using (var reader2 = command2.ExecuteReader())
            {
                while (reader2.Read())
                {
                    for (int i = 0; i < reader2.FieldCount; i++)
                    {
                        if (reader2.GetName(i).ToLower() == "name")
                            list.Add(reader2.GetValue(i).ToString());
                    }
                    //break;
                }
            }
            builder.Length = 0;            
            for (int i = 0; i < list.Count;i++ )
            {
                //替换旧列
                if (list[i].ToLower() == oldColumnName.ToLower())
                    builder.Append(column.Name);
                else
                    builder.Append(list[i]);
                if (i < list.Count - 1)
                    builder.Append(",");
            }
            Execute(string.Format("insert into {0} ({1}) select * from {2}", Quote(tableName), builder.ToString(), Quote(newTableName)));

            //4、删除旧表
            Execute(string.Concat("drop table ", newTableName));
            //创建索引
            var tempKeyPart = string.Concat("_", oldColumnName, "_asc");
            foreach (var key in dict.Keys)
            {
                if (replace.Replace(key, tempKeyPart,string.Empty))
                {
                    continue;
                }
                if (!dict[key].IsTableSql)
                {
                    Execute(dict[key].Sql);
                }
            }
            Execute(buildHelp.IndexSql);
        }
        #endregion 特殊辅助方法
    }
    /// <summary>
    /// Sqlite索引
    /// </summary>
    internal class SqliteIndexEntity
    {
        /// <summary>
        /// 索引名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 索引
        /// </summary>
        public string Sql { get; set; }
        /// <summary>
        /// 是否创建表的Sql
        /// </summary>
        public bool IsTableSql { get; set; }
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