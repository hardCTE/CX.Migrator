using System;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 创建数据库命令帮助类
    /// </summary>
    internal class BuildHelp
    {
        /// <summary>
        /// 创建的列的Sql命令
        /// </summary>
        public string BuildSql { get; set; }
        /// <summary>
        /// 创建的索引的Sql命令
        /// </summary>
        public string IndexSql { get; set; }
    }
}
