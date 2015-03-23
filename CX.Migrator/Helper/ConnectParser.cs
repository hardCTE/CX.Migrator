#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;

namespace CX.Migrator.Helper
{
    /// <summary>
    /// 连接字符串转换器
    /// <para>用于数据库版本迁移时新数据库连接尚未创建对应数据库的补救措施</para>
    /// </summary>
    internal class ConnectParser
    {
        /// <summary>
        /// 获取Mssql默认数据库连接字符串
        /// </summary>
        /// <param name="connectString"></param>
        /// <returns></returns>
        public string Defaultmssql(string connectString)
        {
            connectString = connectString.ToLower();
            int index = connectString.IndexOf("initial catalog",StringComparison.Ordinal);
            int matchLength = 15;
            if(index==-1)
            {
                index = connectString.IndexOf("database", StringComparison.Ordinal);
                matchLength = 8;
            }
            //替换数据库
            if (index > -1)
            {
                string tempPart1 = connectString.Substring(0, index + matchLength);
                string tempPart2 = connectString.Substring(tempPart1.Length);
                index = tempPart2.IndexOf(';');
                //在最后
                if (index < 0||index==tempPart2.Length-1)
                {
                    return string.Concat(tempPart1, "= tempdb;");
                }
                string tempPart3 = tempPart2.Substring(index);
                return string.Concat(tempPart1, "= tempdb", tempPart3);
            }
            return connectString;
        }
        /// <summary>
        /// 获取Mysql默认数据库连接字符串
        /// </summary>
        /// <param name="connectString"></param>
        /// <returns></returns>
        public string Defaultmysql(string connectString)
        {
            connectString = connectString.ToLower();
            int index = connectString.IndexOf("database", StringComparison.Ordinal);
            int matchLength = 8;
            //替换数据库
            if (index > -1)
            {
                string tempPart1 = connectString.Substring(0, index + matchLength);
                string tempPart2 = connectString.Substring(tempPart1.Length);
                index = tempPart2.IndexOf(';');
                //在最后
                if (index < 0 || index == tempPart2.Length - 1)
                {
                    return string.Concat(tempPart1, "= test;");
                }
                string tempPart3 = tempPart2.Substring(index);
                return string.Concat(tempPart1, "= test", tempPart3);
            }
            return connectString;
        }
        /// <summary>
        /// 获取Postgresql默认数据库连接字符串
        /// </summary>
        /// <param name="connectString"></param>
        /// <returns></returns>
        public string Defaultpostgres(string connectString)
        {
            connectString = connectString.ToLower();
            int index = connectString.IndexOf("database", StringComparison.Ordinal);
            int matchLength = 8;
            //替换数据库
            if (index > -1)
            {
                string tempPart1 = connectString.Substring(0, index + matchLength);
                string tempPart2 = connectString.Substring(tempPart1.Length);
                index = tempPart2.IndexOf(';');
                //在最后
                if (index < 0 || index == tempPart2.Length - 1)
                {
                    return string.Concat(tempPart1, "= postgres;");
                }
                string tempPart3 = tempPart2.Substring(index);
                return string.Concat(tempPart1, "= postgres", tempPart3);
            }
            return connectString;
        }
    }
}
