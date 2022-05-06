/***********************************************
 * 功能：Mysql数据库操作类
 * 构建目标：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：2021-1-8
 * 修改信息：
 * 备注：
 * 1、基于Oracle官方Mysql.Data v6.9.12 版本开发
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace EckyStudio.M.BaseModel.db
{  
    public class MySqlOperator : DbModel
    {
        MySqlConnection mConn = null;
        MySqlCommand mCmd = null;
        /// <summary>
        /// 构造MySqlOperator类，传入数据库连接信息，如果留空，则使用ConnectionStringUtils类的默认值
        /// </summary>
        /// <param name="connecitonString">连接字符串</param>
        public MySqlOperator(string connecitonString = null) {
            if (connecitonString == null) {
                connecitonString = ConnectionStringUtils.ToConnectionString();
            }
            mConn = new MySqlConnection(connecitonString);
            mCmd = new MySqlCommand();
            mCmd.Connection = mConn;
        }

        public override void Close()
        {
            try
            {
                mConn?.Close();
                mConn?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        public override bool Open()
        {
            try
            {
                mConn.Open();
                return true;
            }
            catch(Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                return false;
            }
        }


        protected override void CheckOpen()
        {
            //throw new NotImplementedException();
            if (mConn == null)
                throw new Exception("打开数据库失败");

            if (mConn.State != System.Data.ConnectionState.Open)
                mConn.Open();
        }

        public override DbAdapterWrapper GetAdapterWrapper(string sql)
        {
            //throw new NotImplementedException();
            MySqlDataAdapter adapter = GetMySqlDataAdapter(sql);
            MySqlCommandBuilder buidler = new MySqlCommandBuilder(adapter);//根据SelectCommand,填充insert、delete、udpate command
            //adapter.InsertCommand = buidler.GetInsertCommand();
            //adapter.UpdateCommand = buidler.GetUpdateCommand();
            //adapter.DeleteCommand = buidler.GetDeleteCommand();

            DbAdapterWrapper wrapper = new DbAdapterWrapper(adapter);
            return wrapper;
        }

        public override DataSet GetDataSet(string sql)
        {
            MySqlDataAdapter adapter = GetMySqlDataAdapter(sql);
            DataSet ds = new DataSet(); 
            adapter.Fill(ds);
            return ds;
        }

        public override DataTable GetDataTable(string sql)
        {
            return GetDataTable(sql, null);
        }

        public override DataTable GetDataTable(string sql, DbParameter[] sqlParams)
        {
            MySqlDataAdapter adapter = GetMySqlDataAdapter(sql);
            if (sqlParams != null)
            {
                foreach (MySqlParameter sqlParam in sqlParams)
                {
                    adapter.SelectCommand.Parameters.Add(sqlParam);
                }
            }

            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
            //throw new NotImplementedException();
        }


        public override T GetEntity<T>(string sql)
        {
            //throw new NotImplementedException();
            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                T tRecord = (T)Activator.CreateInstance(typeof(T));//调用泛型类的构造函数
                tRecord.Fill(dt.Rows[0]);

                return tRecord;
            }
            return default(T);
        }

        public override T GetScalar<T>(string sql)
        {
            CheckOpen();

            MySqlCommand cmd = new MySqlCommand(sql, mConn);
            cmd.CommandType = GetCommandType(sql);
            object obj = cmd.ExecuteScalar();
            //cmd.Dispose();


            if (obj == null)
                throw new Exception("无法查询到数据(GetScalar)：" + sql);

            Type desT = typeof(T);
            Type srcT = obj.GetType();

            object ret = null;
            //如果源类型和目标类型一样，直接返回
            if (desT == srcT)
            {
                ret = obj;
            }
            else if (desT == typeof(int))
            {
                ret = Convert.ToInt32(obj);
            }
            else if (desT == typeof(bool))
            {
                //if (srcT == typeof(UInt64))
                //{
                //return (T)(object)
                ret = Convert.ToBoolean(obj);
                //}
            }
            else if(desT == typeof(string)){
                ret = Convert.ToString(obj);
            }

    
            return ret == null?default(T):(T)ret;
            //throw new NotImplementedException();
        }

        private MySqlDataAdapter GetMySqlDataAdapter(string sql) {
            CheckOpen();

            //adapter不能共用SqlCommand
            //mCmd.CommandText = sql;
            //mCmd.CommandType = GetCommandType(sql);      
            MySqlCommand cmd = new MySqlCommand(sql, mConn);
            cmd.CommandType = GetCommandType(sql);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            return adapter;
        }

        public override int ExecNonQuerySafety(string sql, params object[] domainValues)
        {
            long insertId;
            return ExecNonQuerySafety(out insertId, sql, domainValues);
        }

        public override int ExecNonQuerySafety(string sql, DbParameter[] sqlParams)
        {
            //throw new NotImplementedException();
            long insertId;
            return ExecNonQuerySafety(out insertId, sql, sqlParams);
        }

        public override int ExecNonQuerySafety(out long insertId, string sql, params object[] domainValues)
        {
            //throw new NotImplementedException();
            if (domainValues == null)
            {
                return ExecNonQuerySafety(out insertId, sql, null);
            }

            //string[] varNames = new string[domainValues.Length];
            Regex reg = new Regex(@"@(?<domainName>\S+?)[,)\s;]");//2021-1-8
            //Regex reg = new Regex(@"@\S+?[,)\s]");
            //Regex reg = new Regex(@"@(\S+?)[,)\s]");

            MatchCollection matColl = reg.Matches(sql);
            int count = matColl.Count;
            MySqlParameter[] sqlParams = new MySqlParameter[count];

            for (int i = 0; i < count; i++)
            {
                string domainName = matColl[i].Groups["domainName"].Value;
                //varNames[i] = matColl[i].Value.Substring(0, matColl[i].Value.Length - 1).Trim();
                //varNames[i] = matColl[i].Groups[1].Value;
                sqlParams[i] = new MySqlParameter(domainName, domainValues[i]);
            }
            return ExecNonQuerySafety(out insertId, sql, sqlParams);
        }

        public override int ExecNonQuerySafety(out long insertId, string sql, DbParameter[] sqlParams)
        {
            //throw new NotImplementedException();
            CheckOpen();

            mCmd.CommandText = sql;
            mCmd.CommandType = GetCommandType(sql);
            mCmd.Parameters.Clear();//清除上一次的参数

            foreach (MySqlParameter sqlParam in sqlParams)
            {
                mCmd.Parameters.Add(sqlParam);
            }
            int ret = mCmd.ExecuteNonQuery();
            insertId= mCmd.LastInsertedId;//获取InsertId
            return ret;
        }

        public override DbParameter[] MakeSqlParams(params string[] keyValuePairs)
        {
            //throw new NotImplementedException();
            if (keyValuePairs == null || keyValuePairs.Length == 0 || (keyValuePairs.Length % 2 != 0)) {
                return null;
            }

            int count = keyValuePairs.Length / 2;

            MySqlParameter[] parameters = new MySqlParameter[count];
            for (int i = 0; i < count; i++) {
                parameters[i] = new MySqlParameter(keyValuePairs[2 * i], keyValuePairs[2 * i + 1]);
            }
            return parameters;
        }

        public override DbDataReader GetDataReader(string sql)
        {
            return GetDataReader(sql, null);
        }

        public override DbDataReader GetDataReader(string sql, DbParameter[] sqlParams)
        {
            CheckOpen();
            return GetDataReader(mCmd, sql, sqlParams);
        }

        public override T[] GetColumnVector<T>(string sql)
        {
            // throw new NotImplementedException();
            return GetColumnVector<T>(sql, null);
        }

        public override T[] GetRowVector<T>(string sql)
        {
            //throw new NotImplementedException();
            return GetRowVector<T>(sql, null);
        }

        public override T[] GetColumnVector<T>(string sql, DbParameter[] sqlParams)
        {
            //throw new NotImplementedException();
            DbDataReader reader = GetDataReader(sql, sqlParams);

            Type desType = typeof(T);
            LinkedList<T> ret = new LinkedList<T>();
            while (reader.Read()) {
                object obj = reader[0];
                if (desType == typeof(string))
                {
                    obj = obj.ToString();
                }
                ret.AddLast((T)obj);
            }
            reader.Close();
            int count = ret.Count;
            return count==0 ? null:ret.ToArray();//default(T[]);

        }


        public override T[] GetRowVector<T>(string sql, DbParameter[] sqlParams)
        {
            //throw new NotImplementedException();
            DbDataReader reader = GetDataReader(sql, sqlParams);

            Type desType = typeof(T);

            if (reader.Read())
            {
                object[] objs = new object[reader.FieldCount];
                for (int i = 0; i < objs.Length; i++)
                {
                    object obj = reader[i];
                    if (desType == typeof(string)) {
                        obj = obj.ToString();
                    }
                    objs[i] = obj;
                }
                reader.Close();

                T[] ret = (T[])objs.Cast<T>();//强制转成泛型数组
                return ret;
            }

            reader.Close();           
            return null;//default(T[]);
        }

        public override string[] GetTableFileds( string tableName, string dbName= "")
        {
            string sql = string.Format("select column_name from information_schema.COLUMNS where table_name = '{0}'", tableName);
            if (!string.IsNullOrEmpty(dbName)) {
                sql += string.Format(" and table_schema = '{0}'",dbName);
            }
            return GetColumnVector<string>(sql);
        }

        public override bool IsExistTable(string name)
        {
            string sql = string.Format("SELECT count(*) FROM information_schema.TABLES WHERE table_name ='{0}'",name);
            return GetScalar<bool>(sql);
        }

        public override Dictionary<string, object> GetRowKeyValuePair(string sql)
        {
            //throw new NotImplementedException();
            return GetRowKeyValuePair(sql, null);
        }


        public override Dictionary<string, object> GetRowKeyValuePair(string sql, DbParameter[] sqlParams)
        {
            //throw new NotImplementedException();
            Dictionary<string, object> ret = null;

            DataTable dt = GetDataTable(sql, sqlParams);
            if (dt.Rows.Count > 0)
            {
                ret = new Dictionary<string, object>();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    ret.Add(dt.Rows[0].Table.Columns[i].ColumnName, dt.Rows[0][i]);
                }
            }

            return ret;
        }

        public class ConnectionStringUtils{
            private const string CONNECT_STRING_TEMPLATE = "server={0};user id={1};password={2};database={3};port={4};Charset={5};Connection Timeout={6}";
            public static string SERVER = "127.0.0.1";
            public static string PORT = "3306";
            public static string USER_ID = "root";
            public static string PASSWORD = "";
            public static string DATABASE = "";
            public static string CHARSET = "";
            public static string CONNECTION_TIMEOUT = "30";

            public static string ToConnectionString() {
                return string.Format(CONNECT_STRING_TEMPLATE,SERVER,USER_ID,PASSWORD,DATABASE,PORT,CHARSET,CONNECTION_TIMEOUT);
            }
        }
    }
}
