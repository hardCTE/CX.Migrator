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
    /// MySql数据库类型匹配
    /// </summary>
    internal class MySqlTypeMap : TypeMapBase
    {
        /// <summary>
        /// MySql数据库类型匹配
        /// </summary>
        internal MySqlTypeMap()
        {
            MapDbType(DbType.AnsiStringFixedLength, "char(255)");
            MapDbType(DbType.AnsiStringFixedLength, 255, "char({0})");
            MapDbType(DbType.AnsiStringFixedLength, 65535, "text");
            MapDbType(DbType.AnsiStringFixedLength, 16777215, "mediumtext");
            MapDbType(DbType.AnsiString, "varchar(255)");
            MapDbType(DbType.AnsiString, 21845, "varchar({0})");
            MapDbType(DbType.AnsiString, 65535, "text");
            MapDbType(DbType.AnsiString, 16777215, "mediumtext");
            MapDbType(DbType.Binary, "longblob");
            MapDbType(DbType.Binary, 127, "tinyblob");
            MapDbType(DbType.Binary, 65535, "blob");
            MapDbType(DbType.Binary, 16777215, "mediumblob");
            MapDbType(DbType.Boolean, "bit");
            MapDbType(DbType.Byte, "tinyint unsigned");
            MapDbType(DbType.Currency, "decimal(19,3)");
            MapDbType(DbType.Date, "date");
            MapDbType(DbType.DateTime, "datetime");
            MapDbType(DbType.DateTime2, "datetime");
            MapDbType(DbType.Decimal, "decimal");
            MapDbType(DbType.Decimal, 19, "decimal(19,{0})");
            MapDbType(DbType.Double, "double");
            MapDbType(DbType.Guid, "varchar(40)");
            MapDbType(DbType.Int16, "smallint");
            MapDbType(DbType.UInt16, "integer");
            MapDbType(DbType.Int32, "integer");
            MapDbType(DbType.UInt32, "int");
            MapDbType(DbType.Int64, "bigint");
            MapDbType(DbType.UInt64, "bigint unsigned");
            MapDbType(DbType.Single, "float");
            MapDbType(DbType.StringFixedLength, "char(255)");
            MapDbType(DbType.StringFixedLength, 255, "char({0})");
            MapDbType(DbType.StringFixedLength, 65535, "text");
            MapDbType(DbType.StringFixedLength, 16777215, "mediumtext");
            MapDbType(DbType.String, "varchar(255)");
            MapDbType(DbType.String, 21845, "varchar({0})");
            MapDbType(DbType.String, 65535, "text");
            MapDbType(DbType.String, 16777215, "mediumtext");
            MapDbType(DbType.Time, "time");

            MapDbProperty(ColumnProperty.Identity, "auto_increment");
            MapDbProperty(ColumnProperty.PrimaryKey_Identity, "not null auto_increment");
            MapDbProperty(ColumnProperty.Identity_NotNull, "not null auto_increment");
        }
    }
}
