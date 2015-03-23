#region License
//   龙之舞2015  欢迎大家多提意见 6483478@qq.com
//   http://www.apache.org/licenses/LICENSE-2.0
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using CX.Migrator.Framework;

namespace CX.Migrator.Helper
{
    /// <summary>
    /// 版本管理员
    /// <para>从接手项目就开始维护一个项目的数据库信息</para>
    /// </summary>
    internal class VersionHolder
    {
        /// <summary>
        /// 重命名数据表设定信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="newTableName"></param>
        /// <returns></returns>
        public bool RenameTable(string tableName, string newTableName)
        {
            if (!Tracking)
                return false;
            var oldPath = GetSavePath(tableName);
            if (!File.Exists(oldPath))
                return false;
            var newPath = GetSavePath(newTableName);
            if (File.Exists(newPath))
                File.Delete(newPath);
            File.Move(oldPath,newPath);
            return true;
        }
        /// <summary>
        /// 添加数据表设定信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public bool AddTable(string tableName, params Column[] columns)
        {
            if (!Tracking)
                return false;
            TableInfo info = new TableInfo();
            info.TableName = tableName;
            foreach(var column in columns)
            {
                info.AllColumns[column.Name.ToLower()] = column;
            }
            return SerializeXml(info,GetSavePath(tableName));
        }
        #region 列修改
        /// <summary>
        /// 添加列设定信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool AddColumn(string tableName, Column column)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if(info==null)
            {
                info = new TableInfo();
            }
            info.AllColumns[column.Name.ToLower()] = column;
            return SerializeXml(info, savePath);
        }
        /// <summary>
        /// 更新数据列设定信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldColumnName"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool ChangeColumn(string tableName, string oldColumnName, Column column)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if(info==null)
            {
                info = new TableInfo();
            }
            var tempLower=oldColumnName.ToLower();
            if (info.AllColumns.ContainsKey(tempLower))
                info.AllColumns.Remove(tempLower);
            info.AllColumns[column.Name.ToLower()] = column;
            return SerializeXml(info, savePath);
        }
        /// <summary>
        /// 重命名数据列设定信息
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="oldName"></param>
        /// <param name="newColumn"></param>
        /// <returns></returns>
        public bool RenameColumn(string tableName, string oldName, string newColumn)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
            {
                info = new TableInfo();
            }
            var tempLower = oldName.ToLower();
            if (!info.AllColumns.ContainsKey(tempLower))
                return false;
            var tempColumn=info.AllColumns[tempLower];
            info.AllColumns.Remove(tempLower);
            info.AllColumns[newColumn.ToLower()] = tempColumn;
            return SerializeXml(info, savePath);
        }
        /// <summary>
        /// 移除表设定
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool DropTable(string tableName)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            if (File.Exists(savePath))
                File.Delete(savePath);
            return true;
        }
        /// <summary>
        /// 移除列设定
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool DropColumn(string tableName, string columnName)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
                return false;
            var tempLowerColumn = columnName.ToLower();
            if (!info.AllColumns.ContainsKey(tempLowerColumn))
            {
                return true;
            }
            info.AllColumns.Remove(tempLowerColumn);
            return SerializeXml(info, savePath);
        }
        /// <summary>
        /// 移除主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool DropPrimaryKey(string tableName, string columnName)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
                return false;
            var tempLowerColumn = columnName.ToLower();
            if (!info.AllColumns.ContainsKey(tempLowerColumn))
            {
                return true;
            }
            var column = info.AllColumns[tempLowerColumn];
            column.ColumnProperty &= ~ColumnProperty.Indexed;
            column.ColumnProperty &= ~ColumnProperty.PrimaryKey;
            column.ColumnProperty &= ~ColumnProperty.PrimaryKey_Identity;
            info.AllColumns[tempLowerColumn] = column;
            return SerializeXml(info, savePath);
        }
        /// <summary>
        /// 添加主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool AddPrimaryKey(string tableName, string columnName)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
                return false;
            var tempLowerColumn = columnName.ToLower();
            if (!info.AllColumns.ContainsKey(tempLowerColumn))
            {
                return true;
            }
            var column = info.AllColumns[tempLowerColumn];
            column.ColumnProperty |= ColumnProperty.PrimaryKey;
            info.AllColumns[tempLowerColumn] = column;
            return SerializeXml(info, savePath);
        }
        /// <summary>
        /// 添加带自增的主键
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool AddPrimaryKeyIdentity(string tableName, string columnName)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
                return false;
            var tempLowerColumn = columnName.ToLower();
            if (!info.AllColumns.ContainsKey(tempLowerColumn))
            {
                return true;
            }
            var column = info.AllColumns[tempLowerColumn];
            column.ColumnProperty |= ColumnProperty.PrimaryKey;
            column.ColumnProperty |= ColumnProperty.Identity;
            info.AllColumns[tempLowerColumn] = column;
            return SerializeXml(info, savePath);
        }
        #endregion 列修改

        #region 获取列设定
        /// <summary>
        /// 根据表名和列名和版本号获取对应的列设置
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public Column GetColumnByName(string tableName,string columnName)
        {
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
                return null;
            var tempLowerColumn=columnName.ToLower();
            if (info.AllColumns.ContainsKey(tempLowerColumn))
                return info.AllColumns[tempLowerColumn];
            return null;
        }
        /// <summary>
        /// 根据表名获取表所有的列设置
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<Column> GetAllColumns(string tableName)
        {
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
                return null;
            var list = new List<Column>();
            foreach(var column in info.AllColumns.Values)
            {
                list.Add(column);
            }
            return list;
        }
        #endregion 获取列设定
        /// <summary>
        /// 获取所有表设置信息
        /// </summary>
        /// <returns></returns>
        public List<TableInfo> GetAllTables()
        {
            var dir = SaveDirPath;
            if (!Directory.Exists(dir))
                return null;
            var files = Directory.GetFiles(SaveDirPath, "*.xml", SearchOption.TopDirectoryOnly);
            if (files.Length < 1)
                return null;
            var list = new List<TableInfo>();
            foreach (var file in files)
            {
                list.Add(DeserializeXml<TableInfo>(file));
            }
            return list;
        }
        /// <summary>
        /// 根据表名称获取表设定
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public TableInfo GetTableByName(string tableName)
        {
            return DeserializeXml<TableInfo>(GetSavePath(tableName));
        }

        #region 设定列属性
        /// <summary>
        /// 为列添加索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool AddIndex(string tableName, string columnName)
        {
            if (!Tracking)
                return false;
            return AddProperty(tableName, columnName, ColumnProperty.Indexed);
        }
        /// <summary>
        /// 为列添加唯一索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool AddUnique(string tableName, string columnName)
        {
            if (!Tracking)
                return false;
            return AddProperty(tableName, columnName, ColumnProperty.Unique);
        }
        /// <summary>
        /// 为列添加属性
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private bool AddProperty(string tableName, string columnName,ColumnProperty property)
        {
            if (!Tracking)
                return false;
            var savePath = GetSavePath(tableName);
            var info = DeserializeXml<TableInfo>(savePath);
            if (info == null)
            {
                return false;
            }
            var tempLower = columnName.ToLower();
            if (!info.AllColumns.ContainsKey(tempLower))
                return false;
            var tempColumn = info.AllColumns[tempLower];
            //已经有这个属性
            if ((tempColumn.ColumnProperty & property) == property)
                return true;
            tempColumn.ColumnProperty |= property;
            return SerializeXml(info, savePath);
        }
        #endregion 设定列属性
        /// <summary>
        /// 获取数据表属性设置保存完整文件名
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="version">对应的版本号</param>
        /// <returns></returns>
        private string GetSavePath(string tableName)
        {
            return string.Concat(SaveDirPath, tableName, ".xml");
        }
        /// <summary>
        /// 获取表设定的保存文件夹的路径
        /// </summary>
        private string SaveDirPath
        {
            get
            {
                var tempPath=string.Concat(basePath, "App_Data\\CXMigrator\\",CurrentDbType,"\\");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                return tempPath;
            }
        }
        /// <summary>
        /// 当前数据库类型
        /// </summary>
        private string CurrentDbType { get; set; }
        /// <summary>
        /// 版本管理员
        /// <para>从接手项目就开始维护一个项目的数据库信息</para>
        /// </summary>
        /// <param name="provider"></param>
        public VersionHolder(SqlProvider provider)
        {
            CurrentDbType = provider.ToString();
        }
        bool _tracking=true;
        /// <summary>
        /// 启用版本跟踪
        /// </summary>
        internal bool Tracking
        {
            get{ return _tracking; }
            set{_tracking = value;}
        }
        private string basePath = AppDomain.CurrentDomain.BaseDirectory;        
        /*
        /// <summary>
        /// 将对象序列化成json兵保存到指定的文件中
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="model">泛型对象</param>
        /// <param name="filePath">保存的完整路径</param>
        /// <returns></returns>
        private bool SerializeToPath<T>(T model, string filePath)
        {
            var serializer = CreateMsJsonSerializer(typeof(T));
            using (var stream = File.Open(filePath, FileMode.Create))
            {
                serializer.WriteObject(stream, model);
            }
            return true;
        }
        /// <summary>
        /// 从文件反序列化Json成类
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="filePath">文件完整路径</param>
        /// <returns></returns>
        private T DeserializeFromPath<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default(T);
            //*
            object returnObj = null;
            var serializer = CreateMsJsonSerializer(typeof(T));
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                returnObj = serializer.ReadObject(stream);
            }
            return (T)returnObj;
        }
        private DataContractJsonSerializer CreateMsJsonSerializer(Type type)
        {
            var setting = new DataContractJsonSerializerSettings();
            setting.DateTimeFormat = new DateTimeFormat(@"yyyy-MM-dd HH:mm:ss");
            setting.UseSimpleDictionaryFormat = true;//设置后全部使用字典键值对结果
            //setting.EmitTypeInformation = EmitTypeInformation.AsNeeded;
            return new DataContractJsonSerializer(type, setting);
        }
        //*/
        //*
        /// <summary>
        /// 从XML文件反序列化成类
        /// <para>使用微软类库</para>
        /// <para>所用的类必须全部都是带无参构造函数</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">文件完整路径</param>
        /// <returns></returns>
        private T DeserializeXml<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return default(T);
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            FileStream stream = null;
            XmlReader reader = null;
            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                reader = new XmlTextReader(stream);
                return (T)serializer.ReadObject(reader);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
                if (stream != null)
                    stream.Dispose();
            }
        }
        /// <summary>
        /// 将当前类序列化成XML保存
        /// <para>使用微软类库</para>
        /// <para>所用的类必须全部都是带无参构造函数</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="filePath">文件完整路径</param>
        /// <returns></returns>
        private bool SerializeXml<T>(T model, string filePath)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            FileStream stream = null;
            XmlWriter writer = null;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                settings.Indent = true;
                stream = File.Open(filePath, FileMode.Create);
                writer = XmlWriter.Create(stream, settings);
                serializer.WriteObject(writer, model);
                return true;
            }
            finally
            {
                if (writer != null)
                    writer.Dispose();
                if (stream != null)
                    stream.Dispose();
            }
        }
        //*/
    }
}
