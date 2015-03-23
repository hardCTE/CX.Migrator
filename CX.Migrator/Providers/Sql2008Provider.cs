#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
//
#endregion
using System;
using System.Linq;
using CX.Migrator.Configs;
using CX.Migrator.Helper;

namespace CX.Migrator.Providers
{
    /// <summary>
    /// Sql2008数据库执行者
    /// </summary>
    internal class Sql2008Provider : Sql2005Provider
    {
        /// <summary>
        /// Sql2008数据库执行者
        /// </summary>
        /// <param name="conection"></param>
        public Sql2008Provider(CXConnection conection)
            : base(conection)
        {
            this.TypeMap = new Sql2000TypeMap();
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