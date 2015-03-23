#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using CX.Migrator.Framework;

namespace CX.Migrator.Configs
{
    /// <summary>
    /// 数据库类型和CSharp类型对应表
    /// </summary>
    internal abstract class TypeMapBase
    {
        private readonly Dictionary<DbType, SortedList<int, string>> weighted = new Dictionary<DbType, SortedList<int, string>>();

        private readonly Dictionary<ColumnProperty, string> propertyMap = new Dictionary<ColumnProperty, string>();
        
        /// <summary>
        /// 匹配数据库类型和对应字符串
        /// </summary>
        /// <param name="typecode"></param>
        /// <param name="value"></param>
        public void MapDbType(DbType typecode, string value)
        {
            MapDbType(typecode, 0, value);
        }
        /// <summary>
        /// 匹配数据库类型和对应字符串
        /// </summary>
        /// <param name="typecode">DB数据类型</param>
        /// <param name="capacity">容量</param>
        /// <param name="value">对应字符串</param>
        public void MapDbType(DbType typecode, int capacity, string value)
        {
            SortedList<int, string> map;
            if (!weighted.TryGetValue(typecode, out map))
            {
                weighted[typecode] = map = new SortedList<int, string>();
            }
            map[capacity] = value;            
        }
        /// <summary>
        /// 匹配数据库属性和对应字符串
        /// </summary>
        /// <param name="property"></param>
        /// <param name="sql"></param>
        public void MapDbProperty(ColumnProperty property, string sql)
        {
            propertyMap[property] = sql;
        }
        /// <summary>
        /// 返回映射的特定字符串
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal string MapTypeString(ColumnProperty property)
        {
            if (propertyMap.ContainsKey(property))
                return propertyMap[property];
            return string.Empty;
        }
        /// <summary>
        /// 返回特定映射是否包含属性
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal bool ContainsProperty(ColumnProperty property)
        {
            return propertyMap.ContainsKey(property);
        }
        /// <summary>
        /// 获取数据类型对应的字符串
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal string GetDbString(Column column)
        {
            if (!weighted.ContainsKey(column.Type))
                return string.Empty;
            var list = weighted[column.Type];
            string returnStr=null;            
            foreach(var key in list.Keys)
            {
                if(returnStr==null)
                    returnStr=list[key];
                if (key >= column.Size)
                {
                    returnStr = list[key];
                    break;
                }
            }
            if (string.IsNullOrEmpty(returnStr))
                return "类型尚未定义";
            if (returnStr.IndexOf("{0}", StringComparison.Ordinal) > -1)
                return string.Format(returnStr,column.Size);
            return returnStr;
        }
        /// <summary>
        /// 是否bool类型
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal bool IsBoolType(Column column)
        {
            return column.Type == DbType.Boolean;
        }
        /// <summary>
        /// 是否字符串类型
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        internal bool IsStringType(Column column)
        {
            switch(column.Type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Guid:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Time:
                    return true;
            }
            return false;
        }
    }
}
