/***********************************************
 * 功能：基于NPOI库操作 Excel
 * 构建目标：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：2021-10-13
 * 修改信息：
 * 1、基于NPOI 2.5.1 版本开发
 * 
 * 备注：
 **********************************************/


using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.db
{
    public class NPOIExcelHelper : ExcelModel
    {
        IWorkbook mBook;
        FileStream mFs;
        bool mClosed = false;

        public NPOIExcelHelper(string fileName) : base(fileName)
        {
            MakeSureAbsPaht(ref fileName);

            if (!File.Exists(fileName))
            {
                mFs = File.Open(fileName, FileMode.Create);

                string ext = Path.GetExtension(fileName).ToLower();
                if (".xls".Equals(ext))
                {
                    mBook = new HSSFWorkbook();
                }
                else if (".xlsx".Equals(ext)) {
                    mBook = new XSSFWorkbook();
                }

                mBook.CreateSheet("Sheet1");
                mBook.Write(mFs);

                mFs.Flush();
                mFs.Close();

                mBook.Close();
            }

            mBook = NPOI.SS.UserModel.WorkbookFactory.Create(fileName);
            mFs = File.Open(fileName, FileMode.Open);
        }

        public override void Close()
        {
            //throw new NotImplementedException();
            if (mClosed)
                throw new Exception("NPOI Excel已经关闭！！");

            Flush();
            mFs.Close();
            mBook.Close();

            mClosed = true;
        }

        public override void Flush()
        {
            //throw new NotImplementedException();
            mBook.Write(mFs);//更新到文件
            mFs.Flush();
        }

        public override int FindCell(string text,int columnIndex,int sheetIndex = 1)
        {
            //throw new NotImplementedException();

            int ret = -1;

            --sheetIndex;
            --columnIndex;

            int rowCount = mBook.GetSheetAt(sheetIndex).PhysicalNumberOfRows;
            ISheet sheet = mBook.GetSheetAt(sheetIndex);
            ICell cell;
            for (int i = 0; i < rowCount; i++) {
                cell = sheet.GetRow(i).GetCell(columnIndex);
                if (text.Equals(cell.StringCellValue)){
                    ret = cell.RowIndex + 1;
                    break;
                }
            }
            return ret;
        }

        public override int GetSheetColumnCount(int sheetIndex = 1)
        {
            //throw new NotImplementedException();
            if (GetSheetRowCount(sheetIndex) == 0)
                return 0;

            sheetIndex--;
            return mBook.GetSheetAt(sheetIndex).GetRow(0).LastCellNum;
        }

        public override int GetSheetRowCount(int sheetIndex = 1)
        {
            //throw new NotImplementedException();

            --sheetIndex; //NPOI的下标从0开始，需要减1
            return mBook.GetSheetAt(sheetIndex).PhysicalNumberOfRows;
        }

        public override string GetValue(int row, int column, int sheetIndex = 1)
        {
            //throw new NotImplementedException();
            --sheetIndex;
            --row;
            --column;

            ISheet sheet = mBook.GetSheetAt(sheetIndex);
            if (sheet == null)
                return null;

            IRow r = sheet.GetRow(row);
            if (r == null)
                return null;

            ICell cell = r.GetCell(column);
            if (cell == null)
                return null;

            if (cell.CellType == CellType.Numeric) {
                return cell.NumericCellValue.ToString();
            }

            return cell.StringCellValue;
        }

        public override void SetValue(int row, int column, string value, int sheetIndex = 1)
        {
            --sheetIndex;// NPOI的下标从0开始，需要减1
            --row;
            --column;

            ISheet sheet = mBook.GetSheetAt(sheetIndex);
            IRow r = sheet.GetRow(row) ?? sheet.CreateRow(row);
            ICell cell = r.GetCell(column) ?? r.CreateCell(column);
            cell.SetCellType(CellType.String);
            cell.SetCellValue(value);
        }
    }
}
