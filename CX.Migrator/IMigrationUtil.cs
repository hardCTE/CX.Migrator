#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;

namespace CX.Migrator
{
    /// <summary>
    /// 数据库迁移接口
    /// <para>主要执行数据库迁移工作</para>
    /// </summary>
    public interface IMigrationUtil
    {
        /// <summary>
        /// 数据库迁移
        /// </summary>
        /// <returns></returns>
        bool DbMigration();
        /// <summary>
        /// 批量提交阀值(默认为1000)
        /// <para>批量插入数据时达到此数量即保存事务</para>
        /// <para>设置为0则关闭批量提交(所有操作完成后在Dispose提交)</para>
        /// </summary>
        uint BatchSize { get; set; }
    }
}
