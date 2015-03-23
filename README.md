CX.Migrator
畅想数据库版本管理类库
类库目标
1.目标是为数据表和列索引的增删改提供代码级完美支持
2.方便插件和其他业务所需随时对数据库提供改动
3.完美迁移数据库

支持Sqlite/Mysql/MariaDb/Oracle/Sqlce/Mssql全系列/Postgresql

目前Sqlite/Mysql/Sqlce/Mssql2014/Postgresql测试已无大问题
Oracle暂未完成
Mssql2000/2005/2008/2012/MariaDb尚未测试

使用方法

    static void 代码中自定义更改()
    {
        var connect = new System.Data.SqlClient.SqlConnection("server=localhost;database=test;uid=sa;pwd=000000;");
        var cxconnection = new CX.Migrator.Helper.CXConnection();
        cxconnection.Connection = connect;
        cxconnection.Provider = SqlProvider.Sql2014;

        CX.Migrator.UtilityTools utility = new CX.Migrator.UtilityTools(cxconnection);
        utility.AddTable("Huiyuan",
            new Column { ColumnProperty = ColumnProperty.PrimaryKey_Identity, Name = "Id", Type = DbType.Int32 },
            new Column { ColumnProperty = ColumnProperty.NotNull, Name = "Name", Type = DbType.String, Size = 80 },
            new Column { ColumnProperty = ColumnProperty.Indexed, Name = "Sex", Type = DbType.Boolean, Size = 1 },
            new Column { ColumnProperty = ColumnProperty.Indexed, Name = "Birthday", Type = DbType.DateTime, Size = 1 },
            new Column { ColumnProperty = ColumnProperty.Indexed, Name = "Age", Type = DbType.Int32, Size = 1 },
            new Column { ColumnProperty = ColumnProperty.Null, Name = "Address", Type = DbType.String, Size = 300 }
            );
        utility.Dispose();
    }
    static void 数据库A迁移到B()
    {
        string postgres = "Server=localhost;Port=5432;User Id=postgres;Password=000000;Database=test3;";
        string sql2014 = "Integrated Security=SSPI;Persist Security Info =false;Initial Catalog = test3;Data Source = (local);";
        string mysql = "server=localhost;database=test33;uid=root;pwd=000000;";
        string sqlce = "Data Source = C:\\Test.sdf;encryption mode=platform default;Password=000000;";
        var 数据库迁移 = new MigrationUtil(SqlProvider.MySql, mysql, SqlProvider.PostgreSql, postgres, "C:\\move.log", "public");
        数据库迁移.DbMigration();
        数据库迁移.Dispose();
    }
    static void 从指定程序集升级数据库到最新版本()
    {
        AssemblyWorker runner = new AssemblyWorker(SqlProvider.Sql2014, "server=localhost;database=test;uid=sa;pwd=000000;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log");
        //Runner runner = new Runner(SqlProvider.MariaDb, "server=localhost;database=test;uid=root;pwd=000000;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log");
        //Runner runner = new Runner(SqlProvider.PostgreSql, "Server=localhost;Port=5432;User Id=postgres;Password=000000;Database=test;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log","public");
        //*
        runner.Up();
        runner.Dispose();
    }
    static void 从指定程序集降级数据库到最低版本()
    {
        AssemblyWorker runner = new AssemblyWorker(SqlProvider.Sql2014, "server=localhost;database=test;uid=sa;pwd=000000;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log");
        //Runner runner = new Runner(SqlProvider.MariaDb, "server=localhost;database=test;uid=root;pwd=000000;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log");
        //Runner runner = new Runner(SqlProvider.PostgreSql, "Server=localhost;Port=5432;User Id=postgres;Password=000000;Database=test;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log","public");
        //*
        runner.Down();
        runner.Dispose();
    }
    /// <summary>
    /// 必须是已经应用过这个版本并且数据库有记录才生效
    /// </summary>
    static void 从指定程序集应用指定数据库版本()
    {
        AssemblyWorker runner = new AssemblyWorker(SqlProvider.Sql2014, "server=localhost;database=test;uid=sa;pwd=000000;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log");

        runner.Apply(6);
        //*/
        runner.Dispose();
    }
    /// <summary>
    /// 必须是已经应用过这个版本并且数据库有记录才生效
    /// </summary>
    static void 从指定程序集撤销指定数据库版本()
    {
        AssemblyWorker runner = new AssemblyWorker(SqlProvider.Sql2014, "server=localhost;database=test;uid=sa;pwd=000000;", typeof(CXMigratorTest.Test1).Assembly, "C:\\3.log");

        runner.UnApply(6);
        runner.Dispose();
    }
