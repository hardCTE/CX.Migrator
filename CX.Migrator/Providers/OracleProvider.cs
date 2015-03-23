#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;
using CX.Migrator.Configs;
using CX.Migrator.Framework;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// Oracle数据提供者
    /// </summary>
    internal class OracleProvider:ProviderBase
    {
        /// <summary>
        /// Oracle数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        /// <param name="engineOrSchama"></param>
        public OracleProvider(CXConnection conection, string engineOrSchama)
        {
            this.CXConnection = conection;
            this.EngineOrSchama = engineOrSchama;
            this.TypeMap = new OracleTypeMap();
            //Oracle部分尚未完成
            throw new NotImplementedException();
        }
        /// <summary>
        /// 添加表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="isTracking">是否追踪</param>
        /// <param name="columns"></param>
        internal override void AddTable(string tableName, bool isTracking = true, params Column[] columns)
        {
            throw new NotImplementedException();
        }

        protected override string Default(Column column)
        {
            throw new NotImplementedException();
        }
    }
}
