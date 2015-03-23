using System;

namespace CX.Migrator.Helper
{
    /// <summary>
    /// 数据库前缀帮助类
    /// </summary>
    public class SqlInfoHelper
    {
        /// <summary>
        /// 根据不同的数据库提供者信息获取对应的数据操作参数前缀
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static string SqlParamPre(SqlProvider provider)
        {
            switch (provider)
            {
                case SqlProvider.MariaDb:
                case SqlProvider.MySql:
                    return "@";
                case SqlProvider.PostgreSql:
                    return ":";
                case SqlProvider.SqlCe:
                case SqlProvider.Sql2000:
                case SqlProvider.Sql2005:
                case SqlProvider.Sql2008:
                case SqlProvider.Sql2012:
                case SqlProvider.Sql2014:
                    return "@";
                case SqlProvider.Sqlite:
                    return "@";
                //Oracle版本参数未确定
                case SqlProvider.Oracle:
                    return "?";
                default:
                    return null;
            }
        }
    }
}