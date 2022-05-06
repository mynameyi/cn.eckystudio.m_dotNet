/***********************************************
 * 功能：构建数据库操作的通用模型
 * 构建目标：提取oracle、mysql、mssql数据库之间的共性（2021-1-7）
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：2021-1-7
 * 修改信息：
 * 1、添加数据库操作的Open、Close、GetDataSet、GetDataTable 接口（2021-1-7）
 * 2、添加ExecNonQuerySafety（2021-1-8）
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.db
{
    public abstract class DbModel
    {
        public abstract bool Open();
        public abstract void Close();
        protected abstract void CheckOpen();

        /// <summary>
        /// 获取数据库封装器，返回类封装了Adapter、DataTable、BindingSource三个类，方便快捷操作数据库
        /// </summary>
        /// <returns>返回DbAdapterWrapper</returns>
        public abstract DbAdapterWrapper GetAdapterWrapper(string sql);
        public abstract DataSet GetDataSet(string sql);
        public abstract DataTable GetDataTable(string sql);
        public abstract DataTable GetDataTable(string sql, DbParameter[] sqlParams);

        public abstract DbParameter[] MakeSqlParams(params string[] keyValuePairs);

        /// <summary>
        /// 获取数据库游标
        /// </summary>
        /// <returns></returns>
        public abstract DbDataReader GetDataReader(string sql);
        public abstract DbDataReader GetDataReader(string sql, DbParameter[] sqlParams);

        /// <summary>
        /// 执行查询语句，返回对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract T GetEntity<T>(string sql) where T : IFillTemplate;
        public abstract T GetScalar<T>(string sql);

        /// <summary>
        /// 获取结果集的列向量，也就是结果集的最左侧一列，垂直方向的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract T[] GetColumnVector<T>(string sql);
        public abstract T[] GetColumnVector<T>(string sql, DbParameter[] sqlParams);


        /// <summary>
        /// 获取结果集的行向量，也就是结果集的第一行，水平方向的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract T[] GetRowVector<T>(string sql);
        public abstract T[] GetRowVector<T>(string sql, DbParameter[] sqlParams);

        /// <summary>
        /// 获取行的键值对，将列名和值同时返回
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract Dictionary<string, object> GetRowKeyValuePair(string sql);
        public abstract Dictionary<string, object> GetRowKeyValuePair(string sql, DbParameter[] sqlParams);

        /// <summary>
        /// 执行非查询语句,增、删、改
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="domainValues"></param>
        public abstract int ExecNonQuerySafety(string sql, params object[] domainValues);
        public abstract int ExecNonQuerySafety(string sql, DbParameter[] sqlParams);
        public abstract int ExecNonQuerySafety(out long insertId, string sql, params object[] domainValues);
        public abstract int ExecNonQuerySafety(out long insertId, string sql, DbParameter[] sqlParams);


        /// <summary>
        /// 表相关操作，获取指定表的所有字段
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual string[] GetTableFileds(string dbName, string tableName) { throw new NotImplementedException(); }
        public virtual bool IsExistTable(string name) { throw new NotImplementedException(); }


        /// <summary>
        /// 识别sql语句的类型
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected virtual CommandType GetCommandType(string sql) {
            sql = sql.Trim();
            //select update insert,delete,call,create trigger
            sql = sql.ToLower();//转为小写

            //sql = sql.Remove(7);
            //if (sql == "select " || sql == "update " || sql == "insert " || sql == "delete ")
            //{
            //    return CommandType.Text;
            //}
            //else
            //{
            //    return CommandType.StoredProcedure;
            //}

            if (sql.StartsWith("select ") || 
                sql.StartsWith("update ") || 
                sql.StartsWith("insert ") || 
                sql.StartsWith("delete ") || 
                sql.StartsWith("call ") ||
                sql.StartsWith("create trigger"))
            {
                return CommandType.Text;
            }
            else
            {
                return CommandType.StoredProcedure;
            }
        }

        internal protected virtual DbDataReader GetDataReader(DbCommand cmd, string sql, DbParameter[] sqlParams) {
            cmd.Parameters.Clear();
            cmd.CommandText = sql;

            if (sqlParams != null)
            {
                foreach (DbParameter sqlParam in sqlParams)
                {
                    cmd.Parameters.Add(sqlParam);
                }
            }
            return cmd.ExecuteReader();
        }
    }
}
