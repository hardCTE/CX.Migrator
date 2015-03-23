using System;
using System.Data;
using CX.Migrator.Framework;

namespace CXMigratorTest
{
    public class Test3 : MigrationBase
    {
        /// <summary>
        /// 系统数据表版本
        /// </summary>
        public override int Version
        {
            get { return 2; }
        }

        /// <summary>
        /// 升级
        /// </summary>
        protected override void UpAction()
        {
            AddTable("CCC", new Column("Id", DbType.Int32,1, ColumnProperty.PrimaryKey_Identity),
                new Column("Name", DbType.String, 80, ColumnProperty.Indexed, "测试默认值"),
                new Column("Birthday",DbType.DateTime,1,ColumnProperty.Indexed),
                new Column("Age",DbType.Int32,1,ColumnProperty.Indexed,30),
                new Column("Address", DbType.String, 1000, ColumnProperty.Indexed)
                );
        }

        /// <summary>
        /// 撤销升级
        /// </summary>
        protected override void DownAction()
        {
            DropTable("CCC");
        }
    }
}
