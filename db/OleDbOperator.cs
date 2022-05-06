/***********************************************
 * 功能： OLE数据库操作类
 * 构建目标：打造专业单线程OLE数据库操作类（2019-12-30）
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2019-12-30
 * 最后修改时间：2019-12-30
 * 修改信息：
 * 备注：
 **********************************************/

using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;


public class OleDbOperator
{

    const string EXCEL_EXTEND_PROP_XLS = ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=4\"";
    const string EXCEL_EXTEND_PROP_XLSX = ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=4\"";

    const string PROVIDER_4_0 = "Provider=Microsoft.Jet.OLEDB.4.0;";
    const string PROVIDER_12_0 = "Provider=Microsoft.ACE.OLEDB.12.0;";

    OleDbConnection mOleDbConn;
    OleDbCommand mOleDbCmd;

    /// <summary>
    /// 创建一个数据库操作类
    /// </summary>
    /// <param name="strDbPath">输入数据库、Excel表名称,相对路径或绝对路径</param>
    public OleDbOperator(string strDbPath)
    {
        string strConn;
        if (IsAbsPath(strDbPath))
        {
            strConn = "Data Source='" + strDbPath + "';";
        }
        else
        {
            strConn = "Data Source ='" + GetCurrentDirectory() + strDbPath + "';";
        }

        string fileType = System.IO.Path.GetExtension(strDbPath).ToLower();
        if (".xls".Equals(fileType))
        {
            strConn = PROVIDER_4_0 + strConn + EXCEL_EXTEND_PROP_XLS;
        }
        else if (".xlsx".Equals(fileType))
        {
            strConn = PROVIDER_12_0 + strConn + EXCEL_EXTEND_PROP_XLSX;
        }
        else {
            strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;" + strConn;
        }

        mOleDbConn = new OleDbConnection(strConn);
        try
        {
            mOleDbConn.Open();
        }
        catch(Exception ex)
        {
            //System.Windows.Forms.MessageBox.Show("打开数据库失败") ;
            throw new Exception("打开数据库失败");
        }
        mOleDbCmd = new OleDbCommand();
        mOleDbCmd.Connection = mOleDbConn;
    }

    public bool EXCEL_InsertItem(string tableName,params string[] items) {

        string sql = "insert into [" + tableName + "$] values({0})";

        string strLink = "";
        for (int i = 0; i < items.Length; i++) {
            strLink += '\'' + items[i] + '\'' + ',';
        }
        strLink = strLink.Remove(strLink.Length - 1);

        sql = string.Format(sql, strLink);
        try
        {
            mOleDbCmd.CommandText = sql;
            mOleDbCmd.ExecuteNonQuery();           
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.Message);
            return false;
        }
        return true;
    }

    /// <summary>
    /// 立即保存数据到硬盘
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool EXCEL_InsertItemImediately(string tableName, params string[] items)
    {

        if(mOleDbConn.State == System.Data.ConnectionState.Closed)
            mOleDbConn.Open();
        
        OleDbCommand oleCmd = new OleDbCommand();
        oleCmd.Connection = mOleDbConn;

        string sql = "insert into [" + tableName + "$] values({0})";

        string strLink = "";
        for (int i = 0; i < items.Length; i++)
        {
            strLink += '\'' + items[i] + '\'' + ',';
        }
        strLink = strLink.Remove(strLink.Length - 1);

        sql = string.Format(sql, strLink);
        try
        {
            oleCmd.CommandText = sql;
            oleCmd.ExecuteNonQuery();
            mOleDbConn.Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.Message);
            return false;
        }
        return true;
    }

    public bool EXCEL_InsertItemBySql(string sql)
    {
        try
        {
            mOleDbCmd.CommandText = sql;
            mOleDbCmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex.Message);
            return false;
        }
        return true;
    }

    public bool EXCEL_IsExsitValue(string tableName,string header,string value) {
        string sql = "select * from [" + tableName + "$] " + "where " + header + "='" + value + "'";
        try
        {
            mOleDbCmd.CommandText = sql;
            object obj = mOleDbCmd.ExecuteScalar();

            if (obj == null)
                return false;
        }
        catch (Exception ex) {
            return false;
        }

        return true;
    }


    public object[] EXCEL_GetRow(string tableName, string header, string value)
    {
        object[] ret = null;
        string sql = "select * from [" + tableName + "$] " + "where " + header + "='" + value + "'";
        try
        {
            mOleDbCmd.CommandText = sql;
            OleDbDataReader reader = mOleDbCmd.ExecuteReader();

            if (reader.Read())
            {
                ret = new object[reader.FieldCount];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = reader[i];
                }
                reader.Close();
                return ret;
            }
            else
            {
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            return null;
        }

        return ret;
    }

    /// <summary>
    /// 判断用于输入路径是绝对路径还是相对路径
    /// </summary>
    /// <param name="strPath">传入用于输入的路径</param>
    /// <returns></returns>
    public static bool IsAbsPath(string path)
    {
        if (path.IndexOf('\\') == 0 || path.IndexOf(':') == 1)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取当前程序所在的目录
    /// </summary>
    /// <returns></returns>
    public static string GetCurrentDirectory()
    {
        string strAbsPath = System.AppDomain.CurrentDomain.BaseDirectory;// System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
        //return strAbsPath.Substring(0, strAbsPath.LastIndexOf('\\') + 1);
        return strAbsPath;
    }
}

