#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 设定表的列属性
    /// </summary>
    [Flags]
    public enum ColumnProperty:int
    {
        /// <summary>
        /// 不指定
        /// </summary>
        None = 1,
        /// <summary>
        /// 允许空
        /// </summary>
        Null = 2,
        /// <summary>
        /// 不允许为空
        /// </summary>
        NotNull = 4,
        /// <summary>
        /// 自增列
        /// </summary>
        Identity = 8,
        /// <summary>
        /// 唯一约束
        /// </summary>
        Unique = 16,
        /// <summary>
        /// 建立非唯一索引
        /// </summary>
        Indexed = 32,
        /// <summary>
        /// 指定为非负类型
        /// </summary>
        Unsigned = 64,
        /// <summary>
        /// 外键
        /// </summary>
        ForeignKey = Unsigned | Null,
        /// <summary>
        /// 主键(主键包含NotNull/Unsigned)
        /// </summary>
        PrimaryKey = 128 | Unsigned | NotNull,
        /// <summary>
        /// 自增和非空
        /// </summary>
        Identity_NotNull = Identity | NotNull,
        /// <summary>
        /// 主键和自增(主键包含NotNull/Unsigned)
        /// </summary>
        PrimaryKey_Identity = PrimaryKey | Identity
    }
}
