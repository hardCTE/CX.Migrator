using System;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 数据库版本更新提交基础类
    /// <para>用于继承</para>
    /// </summary>
    public abstract class MigrationBase : MigratorBase, IComparable<MigrationBase>
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public abstract int Version { get; }

        #region 升级降级的接口
        /// <summary>
        /// 数据库升级接口
        /// </summary>
        internal void Up()
        {
            UpAction();
            AfterUp();
        }
        /// <summary>
        /// 数据库降级接口
        /// </summary>
        internal void Down()
        {
            DownAction();
            AfterDown();
        }
        /// <summary>
        /// 升级数据库实际操作方法
        /// </summary>
        protected abstract void UpAction();
        /// <summary>
        /// 降级数据库实际操作方法
        /// </summary>
        protected abstract void DownAction();
        /// <summary>
        /// 数据库升级后执行
        /// </summary>
        protected virtual void AfterUp()
        {

        }
        /// <summary>
        /// 数据库降级后执行
        /// </summary>
        protected virtual void AfterDown()
        {

        }
        #endregion 升级降级的接口
        /// <summary>
        /// 排序接口
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(MigrationBase other)
        {
            return this.Version.CompareTo(other.Version);
        }
    }
}
