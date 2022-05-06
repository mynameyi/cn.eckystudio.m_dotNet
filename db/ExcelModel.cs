/***********************************************
 * 功能：Excel操作的通用模型
 * 构建目标：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：2021-10-13
 * 修改信息：
 * 
 * 备注：
 **********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.db
{
    public abstract class ExcelModel
    {
        public ExcelModel(string fileName) {

        }

        public abstract void Close();//关闭文件
        public abstract void Flush();//立即更新到文件

        public abstract string GetValue(int row, int column, int sheetIndex = 1);
        public abstract void SetValue(int row, int column, string value, int sheetIndex = 1);
        public abstract int GetSheetRowCount(int sheetIndex = 1);
        public abstract int GetSheetColumnCount(int sheetIndex = 1);
        public abstract int FindCell(string text ,int columnIndex,int sheetIndex = 1);

        protected void MakeSureAbsPaht(ref string fileName) {
            if (!IsAbsPath(fileName))
                fileName = GetCurrentDirectory() + fileName;
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
}
