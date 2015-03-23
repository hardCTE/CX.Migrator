#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;
using System.Data;
using CX.Migrator.Framework;

namespace CX.Migrator.Configs
{
    /// <summary>
    /// SqlCe数据库类型匹配
    /// </summary>
    internal class SqlCeTypeMap : TypeMapBase
    {
        /// <summary>
        /// SqlCe数据库类型匹配
        /// </summary>
        internal SqlCeTypeMap()
        {
            MapDbType(DbType.AnsiStringFixedLength, "char(255)");
            MapDbType(DbType.AnsiStringFixedLength, 8000, "char({0})");
            MapDbType(DbType.AnsiString, "varchar(255)");
            MapDbType(DbType.AnsiString, 8000, "varchar({0})");
            MapDbType(DbType.AnsiString, 2147483647, "text");
            MapDbType(DbType.Binary, "varbinary(8000)");
            MapDbType(DbType.Binary, 8000, "varbinary({0})");
            MapDbType(DbType.Binary, 2147483647, "image");
            MapDbType(DbType.Boolean, "bit");
            MapDbType(DbType.Byte, "tinyint");
            MapDbType(DbType.Currency, "money");
            MapDbType(DbType.Date, "datetime");
            MapDbType(DbType.DateTime, "datetime");
            MapDbType(DbType.DateTime2, "datetime");
            MapDbType(DbType.Decimal, "decimal(19,5)");
            MapDbType(DbType.Decimal, 19, "decimal(19, {0})");
            MapDbType(DbType.Double, "double precision");
            MapDbType(DbType.Guid, "uniqueidentifier");
            MapDbType(DbType.Int16, "smallint");
            MapDbType(DbType.UInt16, "int");
            MapDbType(DbType.Int32, "int");
            MapDbType(DbType.UInt32, "int");
            MapDbType(DbType.Int64, "bigint");
            MapDbType(DbType.UInt64, "bigint");
            MapDbType(DbType.Single, "float"); 
            MapDbType(DbType.StringFixedLength, "nchar(255)");
            MapDbType(DbType.StringFixedLength, 4000, "nchar({0})");
            MapDbType(DbType.String, "nvarchar(255)");
            MapDbType(DbType.String, 4000, "nvarchar({0})");
            MapDbType(DbType.String, 1073741823, "ntext");
            MapDbType(DbType.Time, "datetime");

            MapDbProperty(ColumnProperty.Identity, "identity");
            MapDbProperty(ColumnProperty.PrimaryKey, "primary key not null");
            MapDbProperty(ColumnProperty.NotNull, "not null");
            MapDbProperty(ColumnProperty.PrimaryKey_Identity, "not null primary key identity");
            MapDbProperty(ColumnProperty.Identity_NotNull, "not null identity");
        }
    }
}
