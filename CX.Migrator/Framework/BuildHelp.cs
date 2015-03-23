#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
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
