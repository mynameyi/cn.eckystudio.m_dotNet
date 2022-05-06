/***********************************************
 * 功能：Windows下有关数据库的相关操作
 * 构建目标：打造专业单线程数据操作类（2011-10-18）
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2010-3-1
 * 最后修改时间：2011-9-22
 * 修改信息：
 * 1:将数据类分拆为OLEDB数据库类以及SQL数据库两大类(2011-9-13)
 * 2.增加QueryConditionBuilder和QueryBodyBuilder类，用于快速构建SQL查询语句；(2011-9-22)
 * 3.增加CloneTable函数，用于表的克隆(2011-11-29)
 * 4.增加CreateTable函数，根据SQL语句创建表(2011-12-14)
 * 5.增加VeryInsert函数(2011-12-15)
 * 6.更改nonQueryEasyCall为ExcuteUnsafeNonQuery(2011-12-15)
 * 7.增加GetRecordList的泛型函数
 * 8. ExecSafeNonQuery 添加函数用法说明  2014-11-23
 * 9. 增加线程同步功能 2014-11-25
 * 备注：
 **********************************************/
using System;
using System.Data;
using System.Configuration;
using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.HtmlControls;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data.OleDb;

/// <summary>
///sqlDBHandler 的摘要说明
/// </summary>
public class OleDbHandler
{
    //SqlConnection sqlConn;
    //SqlCommand sqlCmd;
    OleDbConnection sqlConn;
    OleDbCommand sqlCmd;
    //public sqlDBHandler()
    //{
    //    //
    //    //TODO: 在此处添加构造函数逻辑
    //    //
    //    sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
    //    if (sqlConn.State.Equals(ConnectionState.Closed))
    //    {
    //        sqlConn.Open();
    //        sqlCmd = new SqlCommand();
    //        sqlCmd.Connection = sqlConn;
    //    }
    //}
    /// <summary>
    /// 创建一个数据库操作类
    /// </summary>
    /// <param name="strDbPath">输入数据库名称,相对路径或绝对路径</param>
    public OleDbHandler(string strDbPath)
    {
        string strConn;
        if (CheckIsAbsPath(strDbPath))
        {
            strConn = "Data Source='" + strDbPath + "';";
        }
        else
        {
            strConn = "Data Source ='" + GetCurrentDirectory() + strDbPath + "';";
        }

        // "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + fileName + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
        strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;" + strConn;
        sqlConn = new OleDbConnection(strConn);
        try
        {
            sqlConn.Open();
        }
        catch
        {
            //System.Windows.Forms.MessageBox.Show("打开数据库失败") ;
            throw new Exception("打开数据库失败");
        }
        sqlCmd = new OleDbCommand();
        sqlCmd.Connection = sqlConn;
    }
    //public DataTable getRecordSet(string sql, SqlParameter[] sqlParams)
    //{
    //    sqlCmd.CommandText = sql;
    //    SqlDataAdapter sqlAdapter = new SqlDataAdapter();
    //    foreach (SqlParameter sqlParam in sqlParams)
    //    {
    //        sqlCmd.Parameters.Add(sqlParam);
    //    }
    //    sqlAdapter.SelectCommand = sqlCmd;
    //    DataSet ds = new DataSet();
    //    sqlAdapter.Fill(ds);
    //    DataTable dt = new DataTable();
    //    dt = ds.Tables[0];
    //    return dt;
    //}

    //public DataTable getRecordSet(string sql)
    //{
    //    sqlCmd.CommandText = sql;
    //    SqlDataAdapter sqlAdapter = new SqlDataAdapter();
    //    sqlAdapter.SelectCommand = sqlCmd;
    //    DataSet ds = new DataSet();
    //    sqlAdapter.Fill(ds);
    //    DataTable dt = new DataTable();
    //    dt = ds.Tables[0];
    //    return dt;
    //}

    //public SqlDataReader getDataReader(string sql)
    //{
    //    sqlCmd.CommandText = sql;
    //    return sqlCmd.ExecuteReader();
    //}
    public OleDbDataReader getDataReader(string sql)
    {
        sqlCmd.CommandText = sql;
        return sqlCmd.ExecuteReader();
    }

    //public SqlDataReader getDataReader(string sql,SqlParameter[] sqlParams)
    //{
    //    sqlCmd.CommandText = sql;
    //    foreach (SqlParameter sqlParam in sqlParams)
    //    {
    //        sqlCmd.Parameters.Add(sqlParam);
    //    }
    //    return sqlCmd.ExecuteReader();
    //}

    public OleDbDataReader getDataReader(string sql, SqlParameter[] sqlParams)
    {
        sqlCmd.CommandText = sql;
        foreach (SqlParameter sqlParam in sqlParams)
        {
            sqlCmd.Parameters.Add(sqlParam);
        }
        return sqlCmd.ExecuteReader();

    }

    public object getDbObject(string sql)
    {
        sqlCmd.CommandText = sql;
        return sqlCmd.ExecuteScalar();
    }

    public object getDbObject(string sql, SqlParameter[] sqlParams)
    {
        sqlCmd.CommandText = sql;
        sqlCmd.CommandType = getCommandType(sql);
        foreach (SqlParameter sqlParam in sqlParams)
        {
            sqlCmd.Parameters.Add(sqlParam);
        }
        return sqlCmd.ExecuteScalar();
    }
	
    /// <summary>
    /// 轻松执行SQL语句并且有防注入的安全功能。
    /// </summary>
    /// <param name="strSql"></param>
    /// <param name="domainValues"></param>
   public void ExecuteSafeNonQuery(string strSql,params object[] domainValues)
   {
        sqlCmd.CommandText = strSql;
        sqlCmd.CommandType = getCommandType(strSql);

        string[] varNames = new string[domainValues.Length];
        Regex reg = new Regex(@"@\S+?[,)\s]");
        //Regex reg = new Regex(@"@(\S+?)[,)\s]");
        MatchCollection matColl = reg.Matches(strSql);

        for (int i = 0; i < matColl.Count; i++)
        {
            varNames[i] = matColl[i].Value.Substring(0, matColl[i].Value.Length - 1).Trim();
            //varNames[i] = matColl[i].Groups[1].Value;
        }

        int j = 0;
        foreach (object domainValue in domainValues)
        {
            SqlParameter sqlParam = new SqlParameter(varNames[j], domainValue);
            sqlCmd.Parameters.Add(sqlParam);
            j++;
        }
        sqlCmd.ExecuteNonQuery();
    }
    public void ExecSafeNonQuery(string sql, SqlParameter[] sqlParams)
    {
        sqlCmd.CommandText = sql;
        sqlCmd.CommandType = getCommandType(sql);
        foreach (SqlParameter sqlParam in sqlParams)
        {
            sqlCmd.Parameters.Add(sqlParam);
        }
        //set nocount on; insert into tbUser(a, b) values(“a”,”b”); select @@identity;  获取插入的自增列
        sqlCmd.ExecuteNonQuery();
    }
    /// <summary>
    /// 已弃用。更改为ExecSafeNonQuery
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="sqlParams"></param>
    public void execNonQuery(string sql, SqlParameter[] sqlParams)
    {
        ExecSafeNonQuery(sql, sqlParams);
    }
    /// <summary>
    /// 执行非安全的非查询操作。直接执行传进来的SQL语句，有被注入的隐患，不过对于一些安全不高的语句，可以使用该函数，因为其调用轻松，适合执行 内定的SQL语句
    /// </summary>
    /// <param name="sql"></param>
    public void ExcuteUnsafeNonQuery(string sql)
    {
        sqlCmd.CommandText = sql;
        sqlCmd.ExecuteNonQuery();
    }

    /// <summary>
    /// 已弃用，功能同ExcuteUnsafeNonQuery
    /// </summary>
    /// <param name="sql"></param>
    public void nonQueryEasyCall(string sql)
    {
        ExcuteUnsafeNonQuery(sql);
    }

    /// <summary>
    /// 非常插入，通过非常的手段返回标识值。
    /// </summary>
    /// <returns>返回插入的标志值</returns>
    public int VeryInsert(string sql, SqlParameter[] sqlParams)
    {
        //通过调用即时创建的存储过程来执行插入语句，同时将插入语句的标识值返回给用户
        return 0;
    }
    ///// <summary>
    ///// 获取一个验证码
    ///// </summary>
    ///// <returns></returns>
    //public SqlDataReader getValidateNum()
    //{
    //    sqlCmd.CommandText = "select count(*) from dhcVdNum";
    //    int RC = (int)sqlCmd.ExecuteScalar();
    //    Random ranCountMaker = new Random();
    //    int ranNum = ranCountMaker.Next(1, RC) - 1;
    //    //sqlCmd.CommandText = "select vdNumID,vdNumber from ( select dhcVdNum.*,Row_Number() over(order by vdNumID) num from dhcVdNum ) where num ="+ranNum;
    //    //sqlCmd.CommandText = "select vdNumID,vdNumber,RowNum from dhcVdNum where RowNum=" + ranNum;
    //    //sqlCmd.CommandText = "select Row_Number() OVER(order by vdNumID) AS 'rowNo',vdNumID,vdNumber from dhcVdNum where rowNo =" + ranNum;
    //    sqlCmd.CommandText = "select top 1 vdNumID,vdNumber from dhcVdNum where vdNumID not in (select top " + ranNum + " vdNumID from dhcVdNum)";
    //    //sqlCmd.CommandText = "select * from (select RowNum as N, t.* from dhcVdNum t) where N='"+ranNum+"'";
    //    //sqlCmd.CommandText = "select top 1 vdNumID,vdNumber from(select *,Row_Number() OVER(order by vdNumID) rowNo from dhcVdNum where 1=1)" ;
    //    SqlDataReader sqlDr = sqlCmd.ExecuteReader();
    //    return sqlDr;
    //}

    ///// <summary>
    ///// 分页检索函数，用于返回用户单击的某个页码的时候返回指定的页的内容
    ///// </summary>
    ///// <param name="PageNo">用户提交上来的页码</param>
    ///// <param name="PageCount">用于设定的每页显示的条数</param>
    ///// <param name="tableName">指定需要查询的表</param>
    ///// <param name="domainName">指定按什么字段来排序取值</param>
    ///// <param name="wholePages">返回总的页数</param>
    //public DataTable splitPageSelect(int PageNo, int PageCount, string tableName, string domainName,ref int wholePages)
    //{
    //    string sql0 = string.Format("select count(*) from {0}", tableName);
    //    wholePages=(int)(((int)getDbObject(sql0))/PageCount)+1;
    //    int beginIndex, endIndex;
    //    beginIndex = (PageNo - 1) * PageCount + 1;
    //    endIndex = PageNo * PageCount;
    //    string sql = "select * from(select *,Row_Number() OVER(ORDER BY " + domainName + ") rowNo from " + tableName + ") AS t where rowNo between " + beginIndex + " and " + endIndex;
    //    sqlCmd.CommandText = sql;
    //    SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd);
    //    DataTable dt = new DataTable();
    //    sqlAdapter.Fill(dt);
    //    return dt;
    //}

    public void delRecords()
    {
    }

    private CommandType getCommandType(string sql)
    {
        sql = sql.Trim();
        //select update insert,delete
        sql = sql.Remove(7);
        if (sql == "select " || sql == "update " || sql=="insert " || sql == "delete ")
        {
            return CommandType.Text;
        }
        else
        {
            return CommandType.StoredProcedure;
        }

    }
    /// <summary>
    /// 点击率计数器
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="changeDomain"></param>
    /// <param name="domainName"></param>
    /// <param name="ID"></param>
    public void ClickCountIncreaser(string tableName,string changeDomain,string domainName,string ID)
    {
        string sql = string.Format("select {0} from {1} where {2}=@ID", domainName, tableName,changeDomain);
        SqlParameter[] sqlParams =
        {
            new SqlParameter("@ID",SqlDbType.Int)
        };
        sqlParams[0].Value = int.Parse(ID);

        int clickCount=(int)getDbObject(sql, sqlParams);
        clickCount++;
        sql = string.Format("update {0} set {1}={2} where {3}={4}", tableName, domainName, clickCount, changeDomain,ID);
        nonQueryEasyCall(sql);
    }

    #region[--辅助函数--]
    /// <summary>
    /// 获取当前程序所在的目录
    /// </summary>
    /// <returns></returns>
    public string GetCurrentDirectory()
    {
        string strAbsPath = System.AppDomain.CurrentDomain.BaseDirectory;// System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
        //return strAbsPath.Substring(0, strAbsPath.LastIndexOf('\\') + 1);
        return strAbsPath;
    }
    /// <summary>
    /// 判断用于输入路径是绝对路径还是相对路径
    /// </summary>
    /// <param name="strPath">传入用于输入的路径</param>
    /// <returns></returns>
    public bool CheckIsAbsPath(string strPath)
    {
        if (strPath.IndexOf('\\') == 0 || strPath.IndexOf(':') == 1)
        {
            return true;
        }
        return false;
    }
    public string MatchProString(string strRegEx, string strMath)
    {
        return null;
    }
    #endregion

}

public class SqlDbHandler
{

    private const string CONNECT_TEMPLATE = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\{0};Integrated Security=True;Connect Timeout=30";


    SqlConnection sqlConn;
    SqlCommand sqlCmd;

    //public SqlDbHandler()
    //{
    //    //
    //    //TODO: 在此处添加构造函数逻辑
    //    //
    //    sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
    //    if (sqlConn.State.Equals(ConnectionState.Closed))
    //    {
    //        sqlConn.Open();
    //        sqlCmd = new SqlCommand();
    //        sqlCmd.Connection = sqlConn;
    //    }
    //}

    public SqlDbHandler(string strDbPath):this(true,strDbPath) {
    }

    /// <summary>
    /// 创建一个数据库操作类
    /// </summary>
    /// <param name="strDbPath">输入数据库名称,相对路径或绝对路径</param>
    public SqlDbHandler(bool isDefaultConnectionString ,string strDbPath)
    {
        string strConn;
        //if (CheckIsAbsPath(strDbPath))
        //{ strConn = "Data Source='" + strDbPath + "';"; }
        //else
        //{
        //    strConn = "Data Source ='" + GetCurrentDirectory() + strDbPath + "';";
        //}
        //strConn = "Data Source=192.168.8.11;Initial Catalog=GDEMC_Eimp;Persist Security Info=True;User ID=maxzc;Password=adoenjoy";

        if (isDefaultConnectionString)
            strConn = string.Format(CONNECT_TEMPLATE, strDbPath);
        else
            strConn = strDbPath;

        sqlConn = new SqlConnection(strConn);
        try
        {
            sqlConn.Open();
        }
        catch(Exception ex)
        {
            System.Windows.Forms.MessageBox.Show("打开数据库失败") ;
            throw ex;
        }
        sqlCmd = new SqlCommand();
        sqlCmd.Connection = sqlConn;
    }

    public DataTable getRecordSet(string sql, SqlParameter[] sqlParams)
    {
        lock (this)
        {
            sqlCmd.Parameters.Clear();//当参数不清空，上一次的错误会再次引发SqlCommand()的内部异常
            sqlCmd.CommandText = sql;
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                {
                    sqlCmd.Parameters.Add(sqlParam);
                }
            }

            sqlAdapter.SelectCommand = sqlCmd; 
             DataSet ds = new DataSet();
            sqlAdapter.Fill(ds);
            DataTable dt = new DataTable();
            dt = ds.Tables[0];
            return dt;
        }
    }
    public DataTable getRecordSet(string sql)
    {
        return getRecordSet(sql,null);
    }
    public System.Collections.Generic.List<object[]> GetRecordList(string sql)
    {
        System.Collections.Generic.List<object[]> RecordList = new System.Collections.Generic.List<object[]>();
        DataTable dt = getRecordSet(sql);
        int iRows = dt.Rows.Count;
        int iCols = dt.Columns.Count;

        for (int i = 0; i < iRows; i++)
        {
            object[] objRecordElement = new object[iCols];
            for (int j = 0; j < iCols; j++)
            {
                objRecordElement[j] = dt.Rows[i][j];
            }
            RecordList.Add(objRecordElement);
        }
        return RecordList;
    }

    /// <summary>
    /// 使用模板格式化输出的列表
    /// </summary>
    /// <typeparam name="T">用于格式化的类模板，必须继承IFillTemplate接口</typeparam>
    /// <param name="sql">查询语句</param>
    /// <returns></returns>
    public System.Collections.Generic.List<T> GetRecordList<T>(string sql, params object[] domainValues) where T : IFillTemplate
    {
        System.Collections.Generic.List<T> RecordList = new System.Collections.Generic.List<T>();
            
        DataTable dt = getRecordSet(sql,makeSqlParameter(sql,domainValues));
        int iRows = dt.Rows.Count;

        for (int i = 0; i < iRows; i++)
        {
            //通过反射调用类构造函数来构造泛型类
            //T tRecord = default(T);
            //Type t = typeof(T);
            //Type[] tArgs = new Type[0];
            //System.Reflection.ConstructorInfo ci = t.GetConstructor(tArgs);
            //ci.Invoke(new object[] { });
            
            T tRecord =(T) Activator.CreateInstance(typeof(T));//调用泛型类的构造函数
            tRecord.Fill(dt.Rows[i]);

            RecordList.Add(tRecord);
        }
        return RecordList;
    }

    public object[] getOneLine(string sql, params object[] domainValues)
    { 
        SqlDataReader reader = getDataReader(sql,domainValues);
        if (reader.Read())
        {
            object[] objs = new object[reader.FieldCount];
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i] = reader[i];
            }
            reader.Close();
            return objs;
        }
        else {
            reader.Close();
        }
        return null;
    }

    public SqlDataReader getDataReader(string sql,params object[] domainValues)
    {
        return getDataReader(sql,makeSqlParameter(sql,domainValues));
    }


    public SqlDataReader getDataReader(string sql, SqlParameter[] sqlParams)
    {
        sqlCmd.Parameters.Clear();
        sqlCmd.CommandText = sql;

        if (sqlParams != null)
        {
            foreach (SqlParameter sqlParam in sqlParams)
            {
                sqlCmd.Parameters.Add(sqlParam);
            }
        }
        return sqlCmd.ExecuteReader();
    }

    /// <summary>
    /// 泛型函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public T GetEntity<T>(string sql) where  T : IFillTemplate
    {
        DataTable dt = getRecordSet(sql);
        if (dt.Rows.Count > 0)
        {
            T tRecord = (T)Activator.CreateInstance(typeof(T));//调用泛型类的构造函数
            tRecord.Fill(dt.Rows[0]);

            return tRecord;
        }
        return default(T);
    }

    /// <summary>
    /// 获取一个
    /// </summary>
    /// <param name="sql">变量名定义遵循C语言变量名定义规则</param>
    /// <param name="values"></param>
    /// <returns></returns>
    public object getDbObject(string sql, params object[] values)
    {

        if (values == null || values.Length == 0)
        {
            return getDbObject(sql, null);
        }

        //Regex reg = new Regex(@"(@\S+)[,)\s]?");
        //Regex reg = new Regex(@"(@\S+?)[,)\s]|@(\S+)$");
        Regex reg = new Regex(@"(@[a-zA-Z_][^,)\s]*)");
        MatchCollection coll = reg.Matches(sql);
        SqlParameter[] parammeters = new SqlParameter[values.Length];

        int j=0;
        foreach (Match c in coll)
        {
            //for(int i=0;i<c.Groups.Count;i++)
            //{
            //    System.Diagnostics.Trace.WriteLine("count ="+c.Groups[i].Value);
            //}
            parammeters[j] = new SqlParameter(c.Groups[0].Value, values[j]);
            j++;
        }
        return getDbObject(sql, parammeters);
    }

    public object getDbObject(string sql, SqlParameter[] sqlParams)
    {
        lock (this)
        {
            sqlCmd.CommandText = sql;
            sqlCmd.CommandType = getCommandType(sql);
            sqlCmd.Parameters.Clear();

            if (sqlParams != null)
            {
                foreach (SqlParameter sqlParam in sqlParams)
                {
                    sqlCmd.Parameters.Add(sqlParam);
                }
            }
            return sqlCmd.ExecuteScalar();
        }
    }
	
   /// <summary>
   /// 安全执行插入与更新功能，防止字符串连接出现注入等问题
   /// </summary>
   /// <param name="strSql">带变量名称的SQL语句，如insert into t1('f1','f2') values(@f1,@filed2)</param>
   /// <param name="domainValues">变量的对应的值</param>
   public int ExecuteSafeNonQuery(string strSql,params object[] domainValues)
   {
       lock (this) {
            sqlCmd.CommandText = strSql;
            sqlCmd.CommandType = getCommandType(strSql);
            sqlCmd.Parameters.Clear();

            if (domainValues != null && domainValues.Length != 0)
            { 
                string[] varNames = new string[domainValues.Length];
                //Regex reg = new Regex(@"@\S+?[,)\s]");
                Regex reg = new Regex(@"(@[a-zA-Z_][^,)\s]*)");
                //Regex reg = new Regex(@"(@\S+?)[,)\s]|@(\S+)$");
                MatchCollection matColl = reg.Matches(strSql);

                for (int i = 0; i < matColl.Count;i++ )
                {
                    //varNames[i] = matColl[i].Value.Substring(0,matColl[i].Value.Length - 1).Trim();
                    varNames[i] = matColl[i].Groups[0].Value;
                }
                int j = 0;
                foreach (object domainValue in domainValues)
                {
                    SqlParameter sqlParam = new SqlParameter(varNames[j], domainValue);
                    sqlCmd.Parameters.Add(sqlParam);
                    j++;
                }
            }
            return sqlCmd.ExecuteNonQuery();
       }
    }

    //public void execNonQuery(string sql, SqlParameter[] sqlParams)
    //{
    //    sqlCmd.CommandText = sql;
    //    sqlCmd.CommandType = getCommandType(sql);
    //    foreach (SqlParameter sqlParam in sqlParams)
    //    {
    //        sqlCmd.Parameters.Add(sqlParam);
    //    }
    //    sqlCmd.ExecuteNonQuery();
    //}

    public void nonQueryEasyCall(string sql)
    {
        sqlCmd.CommandText = sql;
        sqlCmd.ExecuteNonQuery();
    }
    /// <summary>
    /// 判断是否存在某个对象
    /// </summary>
    /// <param name="objName">可以填入表名</param>
    /// <returns></returns>
    public bool Exist(string objName)
    {
        string[] restrictions = new string[4];
        restrictions[0] = null;//表示要获取的架构信息所在的数据库，使用null表示默认当前数据库
        restrictions[1] = null;//表示要获取信息的所属数据库角色，默认为dbo
        restrictions[2] = objName;//表示要获取对象的名称
        restrictions[3] = null;//表示表的类型
        DataTable tableInfo = sqlConn.GetSchema("Tables", restrictions);
        if (tableInfo.Rows.Count > 0)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 克隆表，在SQL2005上测试通过
    /// </summary>
    /// <param name="TableName"></param>
    /// <param name="newName"></param>
    public void CloneTable(string TableName,string newName)
    {
        //string[] restrictions = new string[4];
        //restrictions[0] = null;//表示要获取的架构信息所在的数据库，使用null表示默认当前数据库
        //restrictions[1] = null;//表示要获取信息的所属数据库角色，默认为dbo
        //restrictions[2] = TableName;//表示要获取对象的名称
        //restrictions[3] = null;//表示表的类型
        //DataTable tableInfo = sqlConn.GetSchema("Tables",restrictions);

        //for(int i = 0;i<tableInfo.Rows.Count;i++)
        //{
        //    for (int j = 0; j < tableInfo.Columns.Count; j++)
        //    {
        //        object obj = tableInfo.Rows[i][j];
        //    }
        //}


        //-----------------------MSSQL2005表复制方法----------------------

        //获取有关字段信息
        string sql = string.Format("select syscolumns.name,syscolumns.domain,syscolumns.status,syscolumns.isnullable,systypes.name,systypes.length from sysobjects,syscolumns,systypes where sysobjects.name = '{0}' and sysobjects.id = syscolumns.id and syscolumns.xtype = systypes.xtype and systypes.name not in('_default_','sysname') ", TableName);
        System.Collections.Generic.List<object[]> ColumnsInfoList = GetRecordList(sql);

        System.Collections.Generic.List<ColumnInfo> ColumnsDetailedInfo = new System.Collections.Generic.List<ColumnInfo>();
        foreach (object[] obj in ColumnsInfoList)
        {
            int iTemp = 0;
            ColumnInfo ci = new ColumnInfo();
            ci.ColumnName =(string)obj[0];//列名

            iTemp = (int)obj[1];
            if (iTemp != 0)
            {
                sql = string.Format("select xtype from sysobjects where Id = {0} ", iTemp);
                string strType = (string)getDbObject(sql);
            }

            iTemp = Convert.ToInt32((byte)obj[2]);
            iTemp = 0x80 & iTemp;
            ci.IsIdentifiedColumn = iTemp == 0x80 ? true : false;//是否是标识列
            if (ci.IsIdentifiedColumn)
            { 
                ci.Seed = ((decimal)getDbObject(string.Format("select IDENT_SEED('{0}')",TableName))).ToString();
                ci.Increaser = ((decimal)getDbObject(string.Format("select IDENT_INCR('{0}')", TableName))).ToString();
            }
            ci.NullType = ((int)obj[3]) == 1 ? "NULL" : "NOT NULL";//是否允许为空
            ci.ColumnTypeName = (string)obj[4];
            ci.ColunmnTypeLength = ((Int16)obj[5]).ToString();
            //ci.Seed = ((decimal)obj[6]).ToString();
            //ci.Increaser = ((decimal)obj[7]).ToString();

            ColumnsDetailedInfo.Add(ci);
        }
        //获取有关约束信息，包括主键、唯一约束
        //int iTableId = (int)getDbObject(string.Format("select Id from sysobjects where name = '{0}'", TableName));//获取表ID
        //sql = string.Format("select sysobjects.parent_obj,sysobjects.id,sysconstraints.id,sysconstraints.constid,sysconstraints.colid,* from sysobjects,sysconstraints,sysindexes where sysobjects.parent_obj = '{0}'", iTableId);
        //sql = string.Format("select sysconstraints.constid,sysconstraints.status,sysconstraints.colid,syscolumns.colid,* from sysconstraints,syscolumns where sysconstraints.id = '{0}'", iTableId);

        //提取主键约束(表级约束)
        //sql = string.Format("select sysobjects.name,sysobjects.xtype,syscolumns.name,sysindexes.keys from sysindexes,sysindexkeys,syscolumns,sysobjects where sysindexes.indid = 1 and sysindexes.id = sysindexkeys.id and sysindexes.indid = sysindexkeys.indid and sysindexes.id = syscolumns.id and sysindexkeys.colid = syscolumns.colid and sysobjects.parent_obj in(select id from sysobjects where name = '{0}') and  sysindexes.id in(select Id from sysobjects where name = '{0}')",TableName);
        sql = string.Format("select sysobjects.name,sysobjects.xtype,syscolumns.name from sysindexkeys,syscolumns,sysobjects where sysindexkeys.indid = 1 and sysindexkeys.id = syscolumns.id and sysindexkeys.colid = syscolumns.colid and sysobjects.parent_obj in(select id from sysobjects where name = '{0}') and  sysindexkeys.id in(select Id from sysobjects where name = '{0}')", TableName);
        System.Collections.Generic.List<ConstrainInfo> PrimaryKeyConstrain = GetRecordList<ConstrainInfo>(sql);
        //提取唯一约束(索引)
        sql = string.Format("select sysobjects.name,sysobjects.xtype,syscolumns.name from sysindexkeys,syscolumns,sysobjects where sysindexkeys.indid = 1 and sysindexkeys.id = syscolumns.id and sysindexkeys.colid = syscolumns.colid and sysobjects.parent_obj in(select id from sysobjects where name = '{0}') and  sysindexkeys.id in(select Id from sysobjects where name = '{0}')", TableName);
        System.Collections.Generic.List<ConstrainInfo> UniqueConstrain = GetRecordList<ConstrainInfo>(sql);
        
        System.Collections.Generic.List<object[]> ConstrainList = GetRecordList(sql);
        System.Collections.Generic.List<ConstrainInfo> ConstrainDetailedList = new System.Collections.Generic.List<ConstrainInfo>();

        //sql =string.Format("select name,xtype from sysobjects where parent_obj in(select id from sysobjects where name = '{0}')",TableName);
        //System.Collections.Generic.List<ConstrainInfo> cl = GetRecordList<ConstrainInfo>(sql);

        System.Text.StringBuilder sbSql = new System.Text.StringBuilder();
        sbSql.Append("CREATE TABLE "+ newName + "(");

    }
    class ColumnInfo:IFillTemplate
    {
        public string Owner;//用于标识表所属的用户，保留。
        public string ColumnName;
        public string ColumnTypeName;
        public string ColunmnTypeLength;
        //public string IsPrimaryKey;//是否主键
        public string NullType;//可空与不为空的关键字
        public int CheckOrRuleId;//当前列所包含CHECK或者RULE约束的ID，保留。
        public bool IsIdentifiedColumn;//是否为标识列
        public string Seed;//如果是标识的种子
        public string Increaser;//标识列的增量
        public string ColumnConstrainType;//列约束关键字，保留。
        public string ColumnConstrainName;//列约束的列名，保留。

        #region IFillTemplate 成员

        public void Fill(DataRow dr)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
    class ConstrainInfo:IFillTemplate
    {
        public string ConstrainName;
        public string ConstrainType;
        public System.Collections.Generic.List<string> ColumnNameList = new System.Collections.Generic.List<string>();//相关列 列表

        #region IFillTemplate 成员
        /// <summary>
        /// 填充类函数，用于格式化指定的数据库表
        /// </summary>
        /// <param name="dr">数据表行</param>
        public void Fill(DataRow dr)
        {
            ConstrainName =(string) dr[0];
            ConstrainType = (string)dr[1];
            ColumnNameList.Add((string)dr[2]);
            //throw new NotImplementedException();
        }

        #endregion
    }

    public bool CreateTable(string sql)
    {
        sqlCmd.CommandType = CommandType.StoredProcedure;
        sqlCmd.CommandText = sql;
//            @"CREATE TABLE [dbo].[FXZP1](
//	        [id] [int] IDENTITY(1,1) NOT NULL,
//	        [Time] [datetime] NULL,
//	        [Material] [nvarchar](20) COLLATE Chinese_PRC_CI_AS NULL,
//	        [Lightlevel] [text] COLLATE Chinese_PRC_CI_AS NULL,
//	        [Conc] [text] COLLATE Chinese_PRC_CI_AS NOT NULL,
//             CONSTRAINT [PK_FXZP1] PRIMARY KEY CLUSTERED 
//            (
//	            [id] ASC
//            )WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
//            ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]"
//            ;
        try
        {
            sqlCmd.ExecuteNonQuery();
        }
        catch 
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// 获取一个验证码
    /// </summary>
    /// <returns></returns>
    public SqlDataReader getValidateNum()
    {
        sqlCmd.CommandText = "select count(*) from dhcVdNum";
        int RC = (int)sqlCmd.ExecuteScalar();
        Random ranCountMaker = new Random();
        int ranNum = ranCountMaker.Next(1, RC) - 1;
        //sqlCmd.CommandText = "select vdNumID,vdNumber from ( select dhcVdNum.*,Row_Number() over(order by vdNumID) num from dhcVdNum ) where num ="+ranNum;
        //sqlCmd.CommandText = "select vdNumID,vdNumber,RowNum from dhcVdNum where RowNum=" + ranNum;
        //sqlCmd.CommandText = "select Row_Number() OVER(order by vdNumID) AS 'rowNo',vdNumID,vdNumber from dhcVdNum where rowNo =" + ranNum;
        sqlCmd.CommandText = "select top 1 vdNumID,vdNumber from dhcVdNum where vdNumID not in (select top " + ranNum + " vdNumID from dhcVdNum)";
        //sqlCmd.CommandText = "select * from (select RowNum as N, t.* from dhcVdNum t) where N='"+ranNum+"'";
        //sqlCmd.CommandText = "select top 1 vdNumID,vdNumber from(select *,Row_Number() OVER(order by vdNumID) rowNo from dhcVdNum where 1=1)" ;
        SqlDataReader sqlDr = sqlCmd.ExecuteReader();
        return sqlDr;
    }

    /// <summary>
    /// 分页检索函数，用于返回用户单击的某个页码的时候返回指定的页的内容
    /// </summary>
    /// <param name="PageNo">用户提交上来的页码</param>
    /// <param name="PageCount">用于设定的每页显示的条数</param>
    /// <param name="tableName">指定需要查询的表</param>
    /// <param name="domainName">指定按什么字段来排序取值</param>
    /// <param name="wholePages">返回总的页数</param>
    public DataTable splitPageSelect(int PageNo, int PageCount, string tableName, string domainName, ref int wholePages)
    {
        string sql0 = string.Format("select count(*) from {0}", tableName);
        wholePages = (int)(((int)getDbObject(sql0)) / PageCount) + 1;
        int beginIndex, endIndex;
        beginIndex = (PageNo - 1) * PageCount + 1;
        endIndex = PageNo * PageCount;
        string sql = "select * from(select *,Row_Number() OVER(ORDER BY " + domainName + ") rowNo from " + tableName + ") AS t where rowNo between " + beginIndex + " and " + endIndex;
        sqlCmd.CommandText = sql;
        SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCmd);
        DataTable dt = new DataTable();
        sqlAdapter.Fill(dt);
        return dt;
    }

    public void delRecords()
    {
    }

    private static CommandType getCommandType(string sql)
    {
        sql = sql.Trim();
        //select update insert,delete
        sql = sql.Remove(7);
        if (sql == "select " || sql == "update " || sql=="insert " || sql == "delete ")
        {
            return CommandType.Text;
        }
        else
        {
            return CommandType.StoredProcedure;
        }

    }

    public static SqlParameter[] makeSqlParameter(string sql, object[] values)
    {
        if (values == null||values.Length == 0)
            return null;

        Regex reg = new Regex(@"(@[a-zA-Z_][^,)\s]*)");
        MatchCollection matColl = reg.Matches(sql);

        string[] varNames = new string[matColl.Count];

        for (int i = 0; i < matColl.Count; i++)
        {
            varNames[i] = matColl[i].Groups[0].Value;
        }

        SqlParameter[] sqlParams =   new SqlParameter[values.Length];
        int j = 0;
        foreach (object value in values)
        {
            sqlParams[j] = new SqlParameter(varNames[j], value);
            j++;
        }

        return sqlParams;
    }
    public void ClickCountIncreaser(string tableName,string changeDomain,string domainName,string ID)
    {
        string sql = string.Format("select {0} from {1} where {2}=@ID", domainName, tableName,changeDomain);
        SqlParameter[] sqlParams =
        {
            new SqlParameter("@ID",SqlDbType.Int)
        };
        sqlParams[0].Value = int.Parse(ID);

        int clickCount=(int)getDbObject(sql, sqlParams);
        clickCount++;
        sql = string.Format("update {0} set {1}={2} where {3}={4}", tableName, domainName, clickCount, changeDomain,ID);
        nonQueryEasyCall(sql);
    }

    #region[--辅助函数--]
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
    /// <summary>
    /// 判断用于输入路径是绝对路径还是相对路径
    /// </summary>
    /// <param name="strPath">传入用于输入的路径</param>
    /// <returns></returns>
    public static bool CheckIsAbsPath(string strPath)
    {
        if (strPath.IndexOf('\\') == 0 || strPath.IndexOf(':') == 1)
        {
            return true;
        }
        return false;
    }
    public string MatchProString(string strRegEx, string strMath)
    {
        return null;
    }
    #endregion
    /// <summary>
    /// 构建查询实体
    /// </summary>
    /// <param name="Prefix"></param>
    /// <param name="DomainNameArray"></param>
    /// <param name="Suffix"></param>
    /// <returns></returns>
    public static string BuildSelectStatement(string Prefix, string[] DomainNameArray, string Suffix)
    {
        System.Text.StringBuilder sql = new System.Text.StringBuilder();
        foreach (string s in DomainNameArray)
        {
            if (s == "" || s == null)
            {
                continue;
            }

            sql.Append("[" + s + "],");
        }

        sql[sql.Length - 1] = ' ';

        if (Prefix.Length < 7)
        {
            Prefix += ' ';
        }
        if (Suffix.IndexOf("from") < 0)
        {
            Suffix = "from " + Suffix;
        }

        return Prefix + sql.ToString() + Suffix;
    }

    public interface IFillTemplate
    {
        void Fill(DataRow dr);
    }
}
/// <summary>
/// Sql条件子句构造类
/// </summary>
public class ConditionBuilder
{
    System.Collections.Generic.List<DomainElement> ConditionList = new System.Collections.Generic.List<DomainElement>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="DomainName"></param>
    /// <param name="Value"></param>
    /// <param name="ConType"></param>
    /// <param name="IsIngoreSingleQuotationMark">对于string类型，是否忽略增加单引号，该字段可留空，默认，string类型数据会增加单引号</param>
    public void Add(string DomainName, object Value,ConditionType ConType,bool? IsIgnoreSingleQuotationMark)
    {
        DomainElementCondition de = new DomainElementCondition();
        de.Domain = DomainName;
        de.Value = Value;
        de.ConType = ConType;
        de.IsIgnoreSingleQuotationMark = IsIgnoreSingleQuotationMark;
        ConditionList.Add(de);
    }
    public void Clear()
    {
        ConditionList.Clear();
    }
    public string Build()
    {
        string sql = "";
        foreach (DomainElementCondition de in ConditionList)
        {
            if (de.Domain == "" || de.Domain == null)
            {
                continue;
            }

            if (sql != "")
            {
                sql += " and ";//增加条件连接关键字
            }
            switch (de.ConType)
            {
                case ConditionType.Equal:
                    if (de.IsIgnoreSingleQuotationMark == true ||de.Value is int || de.Value is bool || de.Value is float || de.Value is double || de.Value is decimal)
                    {
                        sql += de.Domain + '=' + de.Value;//值无单引号
                        break;
                    }
                    sql += de.Domain + '=' + '\'' + de.Value + '\'';
                    break;
                case ConditionType.In:
                    sql += de.Domain + " in(" + de.Value + ")";
                    break;
                case ConditionType.Between:
                    break;
                case ConditionType.More:
                    break;
                case ConditionType.Less:
                    break;
                case ConditionType.NotIn:
                    break;
                default:
                    break;
            }
        }

        if (sql != "")
        {
            sql = "where " + sql;
        }
        return sql;
    }
    public enum ConditionType
    {
        Equal = 0,
        Between = 1,
        In = 2,
        More =3,
        Less = 4,
        NotIn = 5
    }
    class DomainElementCondition:DomainElement
    {
        public ConditionType ConType;
    }
}
class DomainElement
{
    public string Domain;
    public object Value;
    public bool? IsIgnoreSingleQuotationMark;//可空布尔类型
}

/// <summary>
/// SQL语句构造类
/// </summary>
public class SqlBuilder
{
    System.Collections.Generic.List<DomainElement> DomainList = new System.Collections.Generic.List<DomainElement>();
    public SqlBuilder()
    {
        ;
    }
    public SqlBuilder(string[] DomainNameArray, string[] DomainValueArray)
    {
        Add(DomainNameArray, DomainValueArray);
    }
    /// <summary>
    /// 添加SQL语句的字段
    /// </summary>
    /// <param name="DomainName">字段名</param>
    /// <param name="DomainValue">字段值</param>
    /// <param name="IsIgnoreSingleQuotationMark">是否忽略单引号</param>
    public void Add(string DomainName, string DomainValue, bool? IsIgnoreSingleQuotationMark)
    {
        ;
    }
    /// <summary>
    /// 添加SQL语句的字段名、字段值
    /// </summary>
    /// <param name="DomainNameArray">字段名数组</param>
    /// <param name="DomainValueArray">字段值数组</param>
    public void Add(string[] DomainNameArray, string[] DomainValueArray)
    {
        if (DomainValueArray == null)
        {
            foreach (string s in DomainNameArray)
            {
                DomainElement de = new DomainElement();
                de.Domain = s;
                de.Value = '@' + s;
                de.IsIgnoreSingleQuotationMark = true;
                DomainList.Add(de);
            }
            return;
        }

        int iLength = DomainNameArray.Length;
        for (int i = 0; i < iLength; i++)
        {
            DomainElement de = new DomainElement();
            de.Domain = DomainNameArray[i];
            de.Value = DomainValueArray[i];
            de.IsIgnoreSingleQuotationMark = true;
            DomainList.Add(de);
        }
    }
    /// <summary>
    /// 清空语句
    /// </summary>
    public void Clear()
    {
        DomainList.Clear();
    }
    /// <summary>
    /// 构造查询语句
    /// </summary>
    /// <param name="Prefix">加到语句的前缀</param>
    /// <param name="Suffix">加到语句的后缀</param>
    /// <returns></returns>
    public string Build(string Prefix,string Suffix)
    {
        string SqlType = null;
        System.Text.StringBuilder Sql = new System.Text.StringBuilder();

        if (Prefix.Length > 7)
        {
            Prefix = Prefix.Trim();
            SqlType = Prefix.Remove(7);
        }
        else
        {
            SqlType = Prefix;
        }
        SqlType = SqlType.ToLower();

        Sql.Append(Prefix);
        switch(SqlType)
        {
            case "select ":
                Sql.Append(' ');
                foreach (DomainElement de in DomainList)
                {
                    Sql.Append("[" + de.Domain + "],");
                }

                Sql[Sql.Length - 1] = ' ';
                break;
            case "update ":
                Sql.Append(" set ");
                foreach (DomainElement de in DomainList)
                {
                    if (de.IsIgnoreSingleQuotationMark == true || de.Value is int || de.Value is bool || de.Value is float || de.Value is double || de.Value is decimal)
                    {
                        Sql.Append(de.Domain + '=' + de.Value + ',');
                        continue;
                    }
                    Sql.Append(de.Domain + "='" + de.Value + "',");
                }
                Sql[Sql.Length - 1] = ' ';
                break;
            case "insert ":
                string DomainValueString = " values(";
                Sql.Append('(');
                foreach (DomainElement de in DomainList)
                {
                    if (de.Domain == "" || de.Domain == null)
                    {
                        continue;
                    }

                    Sql.Append(de.Domain + ',');

                    if (de.IsIgnoreSingleQuotationMark == true || de.Value is int || de.Value is bool || de.Value is float || de.Value is double || de.Value is decimal)
                    {
                        DomainValueString += de.Value + ",";//值无单引号
                        continue;
                    }
                    DomainValueString += "'" + de.Value + "'";
                }
                Sql[Sql.Length - 1] = ')';
                Sql.Append(DomainValueString);
                Sql[Sql.Length - 1] = ')';
                break;
            case "delete ":
                break;
            default:
                break;
        }
        Sql.Append(Suffix);
        return Sql.ToString();
    }
}

