using System;
using System.Data;
using CX.Migrator.Framework;
using CX.Migrator.Providers;
using CX.Migrator.Helper;

namespace CX.Migrator
{
    /// <summary>
    /// 数据库操作便捷工具类
    /// </summary>
    public class UtilityTools:MigratorBase,IDisposable
    {
        /// <summary>
        /// 数据库操作便捷工具类
        /// </summary>
        /// <param name="connection">定制的数据库访问管理类</param>
        /// <param name="schemaOrEngine">Mysql和MariaDb是存储引擎,postgresql是架构</param>
        /// <param name="logFilePath">日志文件路径</param>
        public UtilityTools(CXConnection connection, string schemaOrEngine = null,string logFilePath=null)
        {
            base.provider = ProviderBase.Create(connection, logFilePath, schemaOrEngine);
        }
        /// <summary>
        /// 释放资源的接口
        /// </summary>
        public void Dispose()
        {
            base.provider.Dispose();
        }
    }
}