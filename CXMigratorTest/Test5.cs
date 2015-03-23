using System;
using System.Data;
using CX.Migrator.Framework;

namespace CXMigratorTest
{
    public class Test5 : MigrationBase
    {
        /// <summary>
        /// 系统数据表版本
        /// </summary>
        public override int Version
        {
            get { return 4; }
        }
        /// <summary>
        /// 升级
        /// </summary>
        protected override void UpAction()
        {
            AddTable("EEE", new Column("Id", DbType.Int32,1, ColumnProperty.PrimaryKey_Identity),
                new Column("Name", DbType.String, 80, ColumnProperty.Indexed),
                new Column("Birthday", DbType.DateTime, 1, ColumnProperty.Null),
                new Column("Age", DbType.Int32, 1, ColumnProperty.Indexed),
                new Column("Count", DbType.Int64, 1, ColumnProperty.Indexed),
                new Column("Price", DbType.Decimal, 19, ColumnProperty.Indexed),
                new Column("Zhongliang", DbType.Single, 19, ColumnProperty.Indexed),
                new Column("Address", DbType.String, 1000, ColumnProperty.Unique),
                new Column("TestA", DbType.String, 1000, ColumnProperty.Null)
                );
            //RenameTable("EEE","GGG");
            //*
            AddColumn("EEE",
                new Column("ShouRu", DbType.Currency, 19, ColumnProperty.Indexed));
            //ChangeColumn("EEE", "TestA", new Column("TestA", DbType.String, 10009, ColumnProperty.Indexed));
            //AddForeignKey("EEE", "Age", "DDD", "Age");
            //*/
        }
        /// <summary>
        /// 撤销升级
        /// </summary>
        protected override void DownAction()
        {
            DropTable("EEE");
        }
    }
}
