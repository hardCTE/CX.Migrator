#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using CX.Migrator.Providers;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 数据库更新功能访问类
    /// </summary>
    public class MigratorBase
    {
        /// <summary>
        /// 数据库提供者基类
        /// </summary>
        internal ProviderBase provider { get; set; }

        #region 数据列级别
        /// <summary>
        /// 添加新列
        /// <para>对数据没影响</para>
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="newColumn"></param>
        public MigratorBase AddColumn(string tableName, Column newColumn)
        {
            provider.AddColumn(tableName, newColumn);
            return this;
        }
        /// <summary>
        /// 删除列
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public MigratorBase DropColumn(string tableName, string columnName)
        {
            provider.DropColumn(tableName, columnName);
            return this;
        }
        /// <summary>
        /// 修改列
        /// <para>对mssql执行此操作会移除列上所有约束</para>
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="column"></param>
        public MigratorBase ChangeColumn(string tableName, string oldColumnName, Column column)
        {
            provider.ChangeColumn(tableName, oldColumnName, column);
            return this;
        }
        /// <summary>
        /// 重命名列
        /// <para>对mssql执行此操作会移除列上所有约束</para>
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="oldName">旧列名</param>
        /// <param name="newColumn">新列名</param>
        /// <returns></returns>
        public MigratorBase RenameColumn(string tableName, string oldName, string newColumn)
        {
            provider.RenameColumn(tableName, oldName, newColumn);
            return this;
        }
        /// <summary>
        /// 在列上建立普通索引
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public MigratorBase AddIndex(string tableName, string columnName)
        {
            provider.AddIndex(tableName, columnName);
            return this;
        }
        /// <summary>
        /// 在列上建立唯一索引
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public MigratorBase AddUnique(string tableName, string columnName)
        {
            provider.AddUnique(tableName, columnName);
            return this;
        }
        #endregion 数据列级别

        #region 约束管理
        /// <summary>
        /// 根据约束名称移除约束
        /// <para>支持Mssql和Postgresql系列</para>
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="constraintName">约束名称</param>
        public void DropConstraintByName(string tableName, string constraintName)
        {
            provider.DropConstraintByName(tableName, constraintName);
        }
        /// <summary>
        /// 移除列上所有约束
        /// <para>支持Mssql和Postgresql系列</para>
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnName">列的名称</param>
        public void DropConstraint(string tableName, string columnName)
        {
            provider.DropConstraint(tableName, columnName);
        }
        /// <summary>
        /// 查询列是否存在约束
        /// <para>仅支持Mssql系列</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool ExistsConstraints(string tableName, string columnName)
        {
            return provider.ExistsConstraints(tableName, columnName);
        }
        /// <summary>
        /// 查询约束名称是否存在
        /// <para>仅支持Mssql系列</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="constraintName"></param>
        /// <returns></returns>
        public bool ExistsConstraintName(string tableName, string constraintName)
        {
            return provider.ExistsConstraintName(tableName, constraintName);
        }
        /// <summary>
        /// 禁用表上所有约束
        /// <para>仅支持Mssql系列</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void DisableAllConstraint(string tableName)
        {
            provider.DisableAllConstraint(tableName);
        }
        /// <summary>
        /// 启用表上所有约束
        /// <para>仅支持Mssql系列</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void EnableAllConstraint(string tableName)
        {
            provider.EnableAllConstraint(tableName);
        }
        #endregion 约束管理

        #region 数据表级别
        /// <summary>
        /// 添加表
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        public void AddTable(string tableName, params Column[] columns)
        {
            provider.AddTable(tableName, columns);
        }
        /// <summary>
        /// 重命名表名称
        /// <para>对数据没影响</para>
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="oldTableName"></param>
        /// <param name="newTableName"></param>
        public void RenameTable(string oldTableName, string newTableName)
        {
            provider.RenameTable(oldTableName, newTableName);
        }
        /// <summary>
        /// 添加外键约束
        /// <para>表和列必须都存在</para>
        /// <para>不支持嵌入式数据库(Sqlite和Sqlce)</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="column">要添加外键的列</param>
        /// <param name="refTable">外键目标表名</param>
        /// <param name="refColumn">外键目标表列</param>
        public void AddForeignKey(string tableName, string column, string refTable, string refColumn)
        {
            provider.AddForeignKey(tableName, column, refTable, refColumn);
        }
        /// <summary>
        /// 删除外键约束
        /// <para>不支持嵌入式数据库(Sqlite和Sqlce)</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        public void DropForeignKey(string tableName)
        {
            provider.DropForeignKey(tableName);
        }
        /// <summary>
        /// 删除表
        /// <para>支持所有数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName)
        {
            provider.DropTable(tableName);
        }
        /// <summary>
        /// 修改数据表的默认字符集
        /// <para>仅支持Mysql数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public MigratorBase ChangeTableCharset(string tableName, string charset)
        {
            provider.ChangeTableCharset(tableName, charset);
            return this;
        }
        /// <summary>
        /// 删除主键
        /// <para>支持Mysql/Mssql/SqlCe/Postgresql数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public MigratorBase DropPrimaryKey(string tableName, string columnName)
        {
            provider.DropPrimaryKey(tableName, columnName);
            return this;
        }
        /// <summary>
        /// 添加主键
        /// <para>对数据没影响</para>
        /// <para>支持Mysql/Mssql/SqlCe/Postgresql数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public MigratorBase AddPrimaryKey(string tableName, string columnName)
        {
            provider.AddPrimaryKey(tableName, columnName);
            return this;
        }
        /// <summary>
        /// 添加自增主键
        /// <para>对数据没影响</para>
        /// <para>支持Mysql/Mssql/SqlCe/Postgresql数据库</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        public MigratorBase AddPrimaryKeyIdentity(string tableName, string columnName)
        {
            provider.AddPrimaryKeyIdentity(tableName, columnName);
            return this;
        }
        #endregion 数据表级别

        #region Db数据库级别
        /// <summary>
        /// 修改数据库的默认字符集
        /// <para>仅支持Mysql数据库</para>
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public MigratorBase ChangeDbCharset(string dbName, string charset)
        {
            provider.ChangeDbCharset(dbName, charset);
            return this;
        }
        /// <summary>
        /// 更换数据库连接的目标数据库
        /// <para>不支持嵌入式数据库(Sqlite和Sqlce)</para>
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public MigratorBase UseDb(string dbName)
        {
            provider.UseDb(dbName);
            return this;
        }
        /// <summary>
        /// 判断数据库类型
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public bool IfDataBase(SqlProvider dbType)
        {
            return provider.IfDataBase(dbType);
        }
        /// <summary>
        /// 创建数据库
        /// <para>此方法不支持Sqlite</para>
        /// </summary>
        /// <param name="dbName"></param>
        public MigratorBase CreateDB(string dbName)
        {
            provider.CreateDB(dbName);
            return this;
        }
        #endregion Db数据库级别
    }
}
