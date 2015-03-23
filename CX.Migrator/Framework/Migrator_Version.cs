using System;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 数据库版本维护者实体类
    /// </summary>
    public class Migrator_Version : IComparable<Migrator_Version>
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime R { get; set; }
        /// <summary>
        /// 所属版本
        /// </summary>
        public int V { get; set; }
        /// <summary>
        /// 排序接口
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Migrator_Version other)
        {
            return this.R.CompareTo(other.R);
        }
    }
}
