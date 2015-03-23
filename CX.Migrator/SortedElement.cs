#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using System.Collections.Generic;
using CX.Migrator.Framework;

namespace CX.Migrator
{
    /// <summary>
    /// 排好序的待操作元素实体类
    /// </summary>
    internal class SortedElement
    {
        bool isSorded = false;
        bool isAscing = false;
        List<MigrationBase> _runElements = new List<MigrationBase>();
        /// <summary>
        /// 从低到高排序的待操作元素
        /// </summary>
        public List<MigrationBase> AscingElements
        {
            get
            {
                if (!isSorded)
                {
                    _runElements.Sort();
                    isSorded = true;
                    isAscing = true;
                }
                if (!isAscing)
                {
                    _runElements.Sort();
                    isAscing = true;
                }
                return _runElements;
            }
        }
        /// <summary>
        /// 从高到低排序的待操作元素
        /// </summary>
        public List<MigrationBase> DescingElements
        {
            get
            {
                if (!isSorded)
                {
                    _runElements.Sort();
                    isSorded = true;
                    isAscing = true;
                }
                if (isAscing)
                {
                    _runElements.Reverse();
                    isAscing = false;
                }
                return _runElements;
            }
        }
        /// <summary>
        /// 添加待操作元素
        /// </summary>
        /// <param name="el"></param>
        public void AddMigration(MigrationBase el)
        {
            _runElements.Add(el);
        }
    }
}
