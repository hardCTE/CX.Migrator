using System;

namespace CX.Migrator
{
    /// <summary>
    /// 数据库更新执行者
    /// <para>以程序集为单位进行操作</para>
    /// </summary>
    public interface IAssemblyWorker:IDisposable
    {
        /// <summary>
        /// 直接升级到版本号
        /// </summary>
        /// <param name="version">版本号</param>
        void Up(int version);
        /// <summary>
        /// 直接降级到版本号
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="include">包含此版本号</param>
        void Down(int version, bool include = false);
        /// <summary>
        /// 直接升级到最新版本
        /// </summary>
        void Up();
        /// <summary>
        /// 直接降级到最低版本
        /// </summary>
        void Down();
        /// <summary>
        /// 单独应用某版本的更新Up()
        /// </summary>
        /// <param name="version"></param>
        void Apply(int version);
        /// <summary>
        /// 单独应用某版本的Down()
        /// </summary>
        /// <param name="version"></param>
        void UnApply(int version);
    }
}
