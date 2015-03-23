#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;

namespace CX.Migrator
{
    /// <summary>
    /// 数据库提供者类型
    /// </summary>
    public enum SqlProvider : int
    {
        /// <summary>
        /// Sqlite数据库提供者
        /// </summary>
        Sqlite=1,
        /// <summary>
        /// PostgreSql数据库提供者
        /// </summary>
        PostgreSql=2,
        /// <summary>
        /// MySQL和MariaDb数据库提供者
        /// </summary>
        MySql = 4,
        /// <summary>
        /// Mysql分支
        /// </summary>
        MariaDb=8,
        /// <summary>
        /// Oracle数据库提供者
        /// </summary>
        Oracle = 16,
        /// <summary>
        /// 微软嵌入式数据库
        /// </summary>
        SqlCe = 32,
        /// <summary>
        /// 微软SqlServer2000以及以前的版本
        /// </summary>
        Sql2000 = 64,
        /// <summary>
        /// 微软SqlServer2005以及以前的版本
        /// </summary>
        Sql2005 = 128,
        /// <summary>
        /// 微软SqlServer2008以及以前的版本
        /// </summary>
        Sql2008 = 256,
        /// <summary>
        /// 微软SqlServer2012以及以前的版本
        /// </summary>
        Sql2012 = 512,
        /// <summary>
        /// 微软SqlServer2014以及以前的版本
        /// </summary>
        Sql2014 = 1024,
    }
}