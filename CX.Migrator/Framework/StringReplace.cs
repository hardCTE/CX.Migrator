#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;
using System.Text;

namespace CX.Migrator.Framework
{
    /// <summary>
    /// 忽略大小写并保持原本大小写替换字符串
    /// <para>一个实例可反复使用不影响最终结果</para>
    /// </summary>
    internal class StringReplace
    {
        StringBuilder builder = null;
        /// <summary>
        /// 结果字符串
        /// </summary>
        internal string Result { get; private set; }
        /// <summary>
        /// 忽略大小写并保持原本大小写替换字符串
        /// <para>一个实例可反复使用不影响最终结果</para>
        /// </summary>
        /// <param name="compareStr"></param>
        /// <param name="replace"></param>
        /// <param name="newStr"></param>
        /// <returns>返回结果是否替换过</returns>
        internal bool Replace(string compareStr, string replace, string newStr)
        {
            string oldStr = compareStr.Clone().ToString();
            compareStr = compareStr.ToLower();
            replace = replace.ToLower();
            //不匹配
            if (compareStr.IndexOf(replace, StringComparison.Ordinal) == -1)
            {
                Result= oldStr;
                return false;
            }
            if (builder == null)
                builder = new StringBuilder();
            else
                builder.Length = 0;
            int replaceLength = replace.Length;
            int newIndex = 0;
            while ((newIndex = compareStr.IndexOf(replace, StringComparison.Ordinal)) > -1)
            {
                if (newIndex > 0)
                {
                    builder.Append(oldStr.Substring(0, newIndex));
                }
                builder.Append(newStr);
                oldStr = oldStr.Substring(newIndex + replaceLength);
                compareStr = compareStr.Substring(newIndex + replaceLength);
            }
            builder.Append(oldStr);
            Result = builder.ToString();
            return true;
        }
    }
}
