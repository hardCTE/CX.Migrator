#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using System.Data;

namespace CX.Migrator.Helper
{
    /// <summary>
    /// 数据库访问对象保管类
    /// </summary>
    public class CXConnection : IDisposable
    {
        IDbConnection _connection;
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection Connection
        {
            get { return _connection; }
            set
            {
                if(value!=null)
                {
                    if (value.State != ConnectionState.Open)
                        value.Open();
                }
                _connection = value;
            }
        }
        /// <summary>
        /// 数据库事务对象
        /// </summary>
        public IDbTransaction Transaction { get; set; }
        /// <summary>
        /// 数据库提供者
        /// </summary>
        public SqlProvider Provider { get; set; }
        /// <summary>
        /// 创建IDbCommand
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IDbCommand CreateCommand(string sql = null)
        {
            IDbCommand command = Connection.CreateCommand();
            command.Transaction = Transaction;
            command.CommandText = sql;
            return command;
        }
        /// <summary>
        /// 开启事务
        /// </summary>
        public void BeginTransaction()
        {
            if (Connection == null || Transaction != null)
                return;
            Transaction = Connection.BeginTransaction();
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        /// <param name="startNew">开始新事务</param>
        public void RollbackTrsation(bool startNew = false)
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
            }
            isRollbacking = false;
            if (startNew && Connection != null)
                Transaction = Connection.BeginTransaction();
            else
                Transaction = null;
        }
        /// <summary>
        /// 保存事务
        /// </summary>
        /// <param name="startNew">开始新事务</param>
        public void SaveTrsation(bool startNew = true)
        {
            if (Transaction != null)
            {
                if (isRollbacking)
                    Transaction.Rollback();
                else
                    Transaction.Commit();
            }
            if (startNew && Connection != null)
                Transaction = Connection.BeginTransaction();
            else
                Transaction = null;
        }
        bool isRollbacking = false;
        /// <summary>
        /// 标记事务出错(稍后自动回滚)
        /// </summary>
        public void TransactionError()
        {
            isRollbacking = true;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Disposing(true);
            GC.SuppressFinalize(this);
        }
        bool isDisposed = false;
        protected void Disposing(bool isDisposing)
        {
            if (isDisposed)
                return;
            if (Transaction != null)
            {
                //双重检查
                if (isDisposed)
                    return;
                isDisposed = true;
                if (isRollbacking)
                    Transaction.Rollback();
                else
                    Transaction.Commit();
                Transaction.Dispose();
                if (Connection != null && Connection.State != ConnectionState.Closed)
                    Connection.Dispose();
            }
            else
            {
                //双重检查
                if (isDisposed)
                    return;
                isDisposed = true;
                if (Connection != null && Connection.State != ConnectionState.Closed)
                    Connection.Dispose();
            }
        }
        /// <summary>
        /// 析构函数
        /// </summary>
        ~CXConnection()
        {
            Disposing(false);
        }
        /// <summary>
        /// 创建定制的数据库访问对象
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static CXConnection Create(SqlProvider provider, IDbConnection connection, IDbTransaction transaction = null)
        {
            var cxConnection = new CXConnection();
            cxConnection.Connection = connection;
            cxConnection.Provider = provider;
            cxConnection.Transaction = transaction;
            return cxConnection;
        }
    }
}
