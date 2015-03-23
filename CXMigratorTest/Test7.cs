using System;
using System.Data;
using CX.Migrator.Framework;

namespace CXMigratorTest
{
    public class Test7 : MigrationBase
    {
        /// <summary>
        /// 系统数据表版本
        /// </summary>
        public override int Version
        {
            get { return 6; }
        }
        /// <summary>
        /// 升级
        /// </summary>
        protected override void UpAction()
        {
            //ChangeColumn("EEE", "Age", new Column("Age123", DbType.Int64, 1, ColumnProperty.Indexed, 30));
            //RenameColumn("EEE", "Age", "Age111");
            //AddForeignKey("EEE","Age111","DDD","Age");
            //AddUnique("DDD", "Age");
            //AddIndex("DDD", "Birthday");
            //AddColumn("DDD", new Column("Test123",DbType.String,300,ColumnProperty.NotNull,"而是"));
            //RenameColumn("DDD", "Age", "Test0");
            //RenameTable("CCC", "TestC");
            //RenameTable("TestD", "DDD");
            DropColumn("EEE","TestA");
            //RenameColumn("DDD", "Test0", "Age");
            //ChangeColumn("DDD","Age",new Column("Test111",DbType.String,600,ColumnProperty.Indexed,"300"));
            //DropColumn("EEE", "Age");
            /*
            CreateDB("Test1").UseDb("Test1")
                .AddTable("QQQ", new Column("Id", DbType.UInt32, ColumnProperty.PrimaryKey_Identity),
                new Column("Name", DbType.String, 80, ColumnProperty.Indexed, "测试默认值"),
                new Column("Birthday", DbType.DateTime, 1, ColumnProperty.Indexed),
                new Column("Age", DbType.Int32, 1, ColumnProperty.Indexed, 30),
                new Column("Address", DbType.String, 1000, ColumnProperty.Unique)
                );//*/
        }
        /// <summary>
        /// 撤销升级
        /// </summary>
        protected override void DownAction()
        {
            RenameTable("TestD", "DDD");
            //RenameColumn("EEE", "Age111", "Age");
            //ChangeColumn("EEE", "Age123", new Column("Age", DbType.Int32, 1, ColumnProperty.Indexed, 30));
            // ("EEE", "Age111", "DDD", "Age");
        }
    }
}
