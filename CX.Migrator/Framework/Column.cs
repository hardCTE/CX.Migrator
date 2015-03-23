using System;
using System.Data;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 数据表列设置
    /// </summary>
    [Serializable]
    public class Column
    {
        //*
        private string _name;
        private DbType _type;
        private int _size;
        private ColumnProperty _property;
        private object _defaultValue;
        //*/
        /// <summary>
        /// 数据表的列设置
        /// </summary>
        public Column()
        {
        }
        //*
        /// <summary>
        /// 系统内定DbType.UInt32为数据库的自增列数据类型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="property"></param>
        /// <param name="defaultValue"></param>
        public Column(string name, DbType type, int size, ColumnProperty property, object defaultValue=null)
        {
            Name = name;
            Type = type;
            Size = size;
            ColumnProperty = property;
            DefaultValue = defaultValue;
        }
        /// <summary>
        /// 列名称
        /// </summary>
        public string Name { get { return _name; } set { _name = value; } }
        /// <summary>
        /// 数据类型
        /// </summary>
        public DbType Type { get { return _type; } set { _type = value; } }
        /// <summary>
        /// 列长度
        /// </summary>
        public int Size { get { return _size; } set { _size = value; } }
        /// <summary>
        /// 列属性
        /// </summary>
        public ColumnProperty ColumnProperty { get { return _property; } set { _property = value; } }
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get { return _defaultValue; } set { _defaultValue = value; } }
        /// <summary>
        /// 是否自增列
        /// </summary>
        public bool IsIdentity
        {
            get
            {
                return (ColumnProperty & ColumnProperty.Identity) == ColumnProperty.Identity;
            }
        }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey
        {
            get
            {
                return (ColumnProperty & ColumnProperty.PrimaryKey) == ColumnProperty.PrimaryKey;
            }
        }
        /// <summary>
        /// 是否不能为空
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsNotNull
        {
            get
            {
                return (ColumnProperty & ColumnProperty.NotNull) == ColumnProperty.NotNull;
            }
        }
        /// <summary>
        /// 是否建立唯一索引
        /// </summary>
        public bool IsUnique
        {
            get
            {
                return (ColumnProperty & ColumnProperty.Unique) == ColumnProperty.Unique;
            }
        }
        /// <summary>
        /// 是否建立普通索引
        /// </summary>
        public bool IsIndexed
        {
            get
            {
                return (ColumnProperty & ColumnProperty.Indexed) == ColumnProperty.Indexed;
            }
        }
    }
}