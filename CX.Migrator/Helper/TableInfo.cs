#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using CX.Migrator.Framework;

namespace CX.Migrator.Helper
{
    /// <summary>
    /// 数据库表完整信息
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// 数据库表完整信息
        /// </summary>
        public TableInfo() { AllColumns = new Dictionary<string, Column>(); }
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 表的全部列属性设置
        /// </summary>
        public Dictionary<string, Column> AllColumns { get; set; }
        /// <summary>
        /// 将所有列转换成List
        /// </summary>
        /// <returns></returns>
        internal List<Column> ColumnArray()
        {
            List<Column> columns = null;
            foreach (var value in AllColumns.Values)
            {
                if (columns == null)
                    columns = new List<Column>();
                columns.Add(value);
            }
            return columns;
        }
        /// <summary>
        /// 获取所有可以插入数据的列,用,分隔(可用于Insert语句和Select语句)
        /// </summary>
        /// <returns></returns>
        internal string GetInsertColumns()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var value in AllColumns.Values)
            {
                if (value.IsIdentity)
                    continue;
                builder.Append(value.Name);
                builder.Append(",");
            }
            if (builder.Length > 0)
                builder.Length -= 1;
            return builder.ToString();
        }
    }
}
