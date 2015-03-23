using System;
using System.Data;
using CX.Migrator.Framework;

namespace CX.Migrator.Configs
{
    /// <summary>
    /// PostgreSql数据库类型匹配
    /// </summary>
    internal class PostgreSqlTypeMap : TypeMapBase
    {
        /// <summary>
        /// PostgreSql数据库类型匹配
        /// </summary>
        internal PostgreSqlTypeMap()
        {
            MapDbType(DbType.AnsiStringFixedLength, "char(255)");
            MapDbType(DbType.AnsiStringFixedLength, 8000, "char({0})");
            MapDbType(DbType.AnsiString, "varchar(255)");
            MapDbType(DbType.AnsiString, 8000, "varchar({0})");
            MapDbType(DbType.AnsiString, 2147483647, "text");
            MapDbType(DbType.Binary, "bytea");
            MapDbType(DbType.Binary, 2147483647, "bytea");
            MapDbType(DbType.Boolean, "boolean");
            //MapDbType(DbType.Byte, "int2");
            MapDbType(DbType.Byte, "smallint");
            MapDbType(DbType.Currency, "decimal(16,4)");
            MapDbType(DbType.Date, "date");
            MapDbType(DbType.DateTime, "timestamp");
            MapDbType(DbType.Decimal, "decimal(18,5)");
            MapDbType(DbType.Decimal, 2, "decimal(18, {0})");
            MapDbType(DbType.Double, "float8");
            //MapDbType(DbType.Int16, "int2");
            MapDbType(DbType.Int16, "smallint");
            //MapDbType(DbType.Int32, "int4");
            MapDbType(DbType.Int32, "int");
            MapDbType(DbType.UInt32, "serial");
            //MapDbType(DbType.Int64, "int8");//int8和bigint均可
            MapDbType(DbType.Int64, "bigint");
            //MapDbType(DbType.Single, "float4");
            MapDbType(DbType.Single, "real");
            MapDbType(DbType.StringFixedLength, "char(255)");
            MapDbType(DbType.StringFixedLength, 4000, "char({0})");
            MapDbType(DbType.String, "varchar(255)");
            MapDbType(DbType.String, 4000, "varchar({0})");
            MapDbType(DbType.String, 1073741823, "text");
            MapDbType(DbType.Time, "time");
            MapDbType(DbType.Guid, "uuid");

            MapDbProperty(ColumnProperty.Identity, "serial");
            MapDbProperty(ColumnProperty.PrimaryKey, "primary key not null");
            MapDbProperty(ColumnProperty.NotNull, "not null");
            MapDbProperty(ColumnProperty.PrimaryKey_Identity, "serial primary key not null");
            MapDbProperty(ColumnProperty.Identity_NotNull, "serial not null");
             
            // not null
        }
    }
}
