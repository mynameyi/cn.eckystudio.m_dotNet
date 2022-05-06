/***********************************************
 * 功能： Oracle数据库操作类
 * 构建目标：打造专业单线程Oracle数据库操作类（2019-12-31）
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2019-12-31
 * 最后修改时间：2019-12-31
 * 修改信息：
 * 备注：
 **********************************************/
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class OracleDbOperator
{

    OracleConnection mConn;
    public OracleDbOperator(string connString = null)
    {
        if (connString == null)
            connString = ConnectionStringUtils.ToConnString();

        mConn = new OracleConnection(connString);
    }

    public bool Open() {
        try
        {
            mConn.Open();
        }
        catch (Exception ex) {
            return false;
        }
        return true;
    }

    public void Close() {
        mConn?.Close();
    }

    public bool IsExist(string table,string domain,string value) {

        Open();
        string sql = "select "+ domain + " from " + table + " where " + domain + " = '" + value + "'";
        OracleCommand cmd = new OracleCommand();
        cmd.CommandText = sql;
        cmd.Connection = mConn; 
        object obj = cmd.ExecuteScalar();
        Close();

        if (obj == null)
            return true;
        return false;
    }

    //OracleTransaction updateProcess = conn.BeginTransaction();
    //try {   
    //   OracleCommand comm1 = conn.CreateCommand();
    //    comm1.CommandText = strUpdateSql1;  
    //   comm1.Transaction = updateProcess;  
    //   comm1.ExecuteNonQuery();
    //   OracleCommand comm2 = conn.CreateCommand();
    //    comm2.CommandText = strUpdateSql2;
    //   comm2.Transaction = updateProcess;
    //   comm2.ExecuteNonQuery();
    //   updateProcess.Commit(); 
    //} 
    //catch() 
    //{   
    //  updateProcess.Rollback();
    //}
    //finally {    conn.Close(); } } 


    //private const String connString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.210)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));User Id=TEST;Password=TEST123";

    public class ConnectionStringUtils
    {
        private const string CONNECT_TEMPLATE = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL={0})(HOST={1})(PORT={2}))(CONNECT_DATA=(SERVICE_NAME={3})));User Id={4};Password={5};";
        public static string PROTOCOL = "TCP";
        public static string HOST = "127.0.0.1";
        public static string PORT = "1521";
        public static string SID = "orcl";
        public static string USER_ID = "sys";
        public static string PASSWORD = "sys";
        public static string ToConnString() {       
            return string.Format(CONNECT_TEMPLATE,PROTOCOL,HOST,PORT,SID,USER_ID,PASSWORD);
        }
    }
}

