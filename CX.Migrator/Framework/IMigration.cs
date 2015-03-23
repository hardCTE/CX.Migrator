using System;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 数据库版本提交接口
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// 版本号
        /// </summary>
        int Version { get; }
        /// <summary>
        /// 数据库升级接口
        /// </summary>
        void Up();
        /// <summary>
        /// 数据库降级接口
        /// </summary>
        void Down();
    }
}
