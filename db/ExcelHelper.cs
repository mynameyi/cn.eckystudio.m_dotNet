using System.IO;
using EXCEL = Microsoft.Office.Interop.Excel;



public class ExcelHelper
{
    EXCEL.Application mApp;
    EXCEL.Workbooks mWbs;
    EXCEL.Workbook mWb;
    string mFileName = null;

    bool mClosed = false;

    public ExcelHelper(string fileName) {
        mApp = new EXCEL.Application();
        mApp.Visible = false;
        mWbs = mApp.Workbooks;

        if (!IsAbsPath(fileName))
            fileName = GetCurrentDirectory() + fileName;

        if (File.Exists(fileName))
        {
            mWb = mWbs.Add(fileName);//加载excel表
        }
        else {
            mFileName = fileName;
            mWb = mWbs.Add(true);//添加空白工作簿
                                 //Microsoft.Office.Interop.Excel.Worksheet sheet = (Microsoft.Office.Interop.Excel.Worksheet)mWb.Sheets.get_Item(1);
                                 //Microsoft.Office.Interop.Excel._Worksheet xSt1 = (Microsoft.Office.Interop.Excel._Worksheet)mWb.Sheets.Add(System.Reflection.Missing.Value, sheet, System.Reflection.Missing.Value, System.Reflection.Missing.Value);//添加空白表格
                                 ////xSt1.Name = "sheet1";
                                 //xSt1.Activate();
            //mWb.Sheets[1].Activate();
           
        }
    }

    public void Close()
    {
        if (mClosed)
            return;

        if (mFileName != null) {
            //mWb.SaveCopyAs(mFileName);
            mWb.SaveAs(mFileName);
        }

        //mWb.Save();
        mWb.Close();
        mWbs.Close();
        mApp.Quit();
        System.Runtime.InteropServices.Marshal.ReleaseComObject(mApp);

        mClosed = true;
    }

    /// <summary>
    /// 获取Excel单元格的值,Excel表格下标从1开始
    /// </summary>
    /// <param name="row">行号，从1开始</param>
    /// <param name="column">列号，从1开始</param>
    /// <param name="sheetIndex"></param>
    /// <returns></returns>
    public string GetValue(int row, int column,int sheetIndex = 1) {
        return mWb.Sheets[sheetIndex].Cells[row, column].Text;
    }

    public void SetValue(int row, int column, string value, int sheetIndex = 1)
    {
        mWb.Sheets[sheetIndex].Cells[row, column].Value2 = value;
    }

    /// <summary>
    /// 获取Excel 表的有效行数
    /// </summary>
    /// <param name="sheetIndex">工作表的位置，默认为第一张表</param>
    /// <returns></returns>
    public int GetSheetRowCount(int sheetIndex = 1) {
        return mWb.Sheets[sheetIndex].UsedRange.Rows.Count;
    }

    /// <summary>
    ///  获取Excel 表的有效列数
    /// </summary>
    /// <param name="sheetIndex">工作表的位置，默认为第一张表</param>
    /// <returns></returns>
    public int GetSheetColumnCount(int sheetIndex = 1) {
        return mWb.Sheets[sheetIndex].UsedRange.Columns.Count;
    }

    /// <summary>
    /// 判断用于输入路径是绝对路径还是相对路径
    /// </summary>
    /// <param name="strPath">传入用于输入的路径</param>
    /// <returns></returns>
    private static bool IsAbsPath(string path)
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
    private static string GetCurrentDirectory()
    {
        string strAbsPath = System.AppDomain.CurrentDomain.BaseDirectory;// System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
        //return strAbsPath.Substring(0, strAbsPath.LastIndexOf('\\') + 1);
        return strAbsPath;
    }
}

