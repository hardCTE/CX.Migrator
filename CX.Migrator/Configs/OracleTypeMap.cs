#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using System.Data;
using CX.Migrator.Framework;

namespace CX.Migrator.Configs
{
    /// <summary>
    /// Oracle数据库类型匹配
    /// </summary>
    internal class OracleTypeMap : TypeMapBase
    {
        /// <summary>
        /// Oracle数据库类型匹配
        /// </summary>
        internal OracleTypeMap()
        {
            MapDbType(DbType.AnsiStringFixedLength, "char(255)");
            MapDbType(DbType.AnsiStringFixedLength, 2000, "char({0})");
            MapDbType(DbType.AnsiString, "varchar2(255)");
            MapDbType(DbType.AnsiString, 2000, "varchar2({0})");
            MapDbType(DbType.AnsiString, 2147483647, "clob"); 
            MapDbType(DbType.Binary, "raw(2000)");
            MapDbType(DbType.Binary, 2000, "raw({0})");
            MapDbType(DbType.Binary, 2147483647, "blob");
            MapDbType(DbType.Boolean, "number(1,0)");
            MapDbType(DbType.Byte, "number(3,0)");
            MapDbType(DbType.Currency, "number(19,1)");
            MapDbType(DbType.Date, "date");
            MapDbType(DbType.DateTime, "timestamp(4)");
            MapDbType(DbType.Decimal, "number(19,5)");
            MapDbType(DbType.Decimal, 19, "number(19, {0})");
            MapDbType(DbType.Double, "double precision"); 
            //MapDbType(DbType.Guid, "char(38)");
            MapDbType(DbType.Int16, "number(5,0)");
            MapDbType(DbType.Int32, "number(10,0)");
            MapDbType(DbType.Int64, "number(20,0)");
            MapDbType(DbType.Single, "float(24)");
            MapDbType(DbType.StringFixedLength, "nchar(255)");
            MapDbType(DbType.StringFixedLength, 2000, "nchar({0})");
            MapDbType(DbType.String, "nvarchar2(255)");
            MapDbType(DbType.String, 2000, "nvarchar2({0})");
            MapDbType(DbType.String, 1073741823, "nclob");
            MapDbType(DbType.Time, "date");
            MapDbProperty(ColumnProperty.Null, String.Empty);
        }
    }
}
