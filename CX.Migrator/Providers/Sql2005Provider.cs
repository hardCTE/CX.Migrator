using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CX.Migrator.Configs;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// Sql2005数据库执行者
    /// </summary>
    internal class Sql2005Provider : Sql2000Provider
    {
        /// <summary>
        /// Sql2005数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public Sql2005Provider(CXConnection conection):base(conection)
        {
            this.TypeMap = new Sql2000TypeMap();
        }
        /// <summary>
        /// 删除外键约束
        /// </summary>
        /// <param name="tableName">表名</param>
        internal override void DropForeignKey(string tableName)
        {
            var list = GetAllForeignKey(tableName);
            if (list == null || list.Count < 1)
                return;
            foreach (var sql in list)
                Execute(String.Format("alter table {0} drop constraint {1}", Quote(tableName), Quote(sql)));
        }
        /// <summary>
        /// 获取表的所有外键
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        protected override List<string> GetAllForeignKey(string tableName)
        {
            string sql = string.Format(@"select name from sys.foreign_key_columns f,sys.objects o where f.constraint_object_id=o.object_id and f.parent_object_id=object_id('{0}')", tableName);
            List<string> list = null;
            using (IDataReader reader = ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    if (list == null)
                        list = new List<string>();
                    list.Add(reader.GetValue(0).ToString());
                }
            }
            return list;
        }
    }
}
/*删除外键
 --第一步:找出test2表上的外键约束名字
--2000
exec sp_helpconstraint 'test2'
--可以在constraint_name 属性中找到外键约束名字
--2005
select name  
from  sys.foreign_key_columns f join sys.objects o on f.constraint_object_id=o.object_id 
where f.parent_object_id=object_id('test2')

--第二步：删除外键约束
alter table test2 drop constraint FK__test2__id__08EA5793 
 */