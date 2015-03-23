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
    /// Sqlite数据库类型匹配
    /// </summary>
    internal class SqliteTypeMap : TypeMapBase
    {
        /// <summary>
        /// Sqlite数据库类型匹配
        /// </summary>
        internal SqliteTypeMap()
        {
            MapDbType(DbType.Binary, "blob");
            MapDbType(DbType.Byte, "tinyint");
            MapDbType(DbType.Int16, "short");
            MapDbType(DbType.Int32, "int");
            MapDbType(DbType.Int64, "long");
            MapDbType(DbType.SByte, "short");
            MapDbType(DbType.UInt16, "int");
            MapDbType(DbType.UInt32, "integer");
            MapDbType(DbType.UInt64, "long");
            MapDbType(DbType.Currency, "money");
            MapDbType(DbType.Decimal, "decimal");
            MapDbType(DbType.Double, "double");
            MapDbType(DbType.Single, "single");
            MapDbType(DbType.VarNumeric, "numeric");
            MapDbType(DbType.String, "nvarchar(255)");
            MapDbType(DbType.String, 8000, "nvarchar({0})");
            MapDbType(DbType.String, 1073741823, "ntext");
            MapDbType(DbType.AnsiStringFixedLength, "char(255)");
            MapDbType(DbType.AnsiStringFixedLength, 1000, "char({0})");
            MapDbType(DbType.AnsiString, "varchar(255)");
            MapDbType(DbType.AnsiString, 1000, "varchar({0})");
            MapDbType(DbType.DateTime, "datetime");
            MapDbType(DbType.Time, "time");
            MapDbType(DbType.DateTime2, "timestamp");
            MapDbType(DbType.Boolean, "bool");
            MapDbType(DbType.Guid, "uniqueidentifier");

            MapDbProperty(ColumnProperty.Identity, "integer autoincrement");
            MapDbProperty(ColumnProperty.PrimaryKey, "primary key not null");
            MapDbProperty(ColumnProperty.NotNull, "not null");
            MapDbProperty(ColumnProperty.PrimaryKey_Identity, "integer not null primary key autoincrement");
            MapDbProperty(ColumnProperty.Identity_NotNull, "integer autoincrement not null");
        }
    }
}
