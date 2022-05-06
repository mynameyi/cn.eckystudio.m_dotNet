/***********************************************
 * 名称:FileSeperator
 * 功能/用途：用于分割文件输出，分割为小文件，方便操作
 * 目标：从原有EckyLanguage独立出来，专门针对最简单的记录语言应用
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2011-8-1
 * 修改信息： 
 * 1.增加编辑函数 UpdateRecord(int row,int column,string text) 2017-11-1
 * 2.增加Flush函数 2018-6-14
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Log.file
{
    public class FileSeperator
    {
        internal FileOperator mFileOperator;

        int mSize;
        public FileSeperator(int size)
        {
            mSize = size;
        }

        object objLockAdd = 1;
        public bool Write(string r)
        {
            lock (objLockAdd)
            {
                if (mFileOperator == null) {
                    mFileOperator = new FileOperator(GenerateFileName());
                }


                mFileOperator.Write(r);

                if (mFileOperator.FileLength > mSize)
                {
                    Dispose();
                }
            }

            return true;
        }

        public void Flush() {
            mFileOperator.Flush();
        }

        private void Dispose()
        {
            mFileOperator.Dispose();
            mFileOperator = null;
        }

        private string GenerateFileName() {
            return "log\\files\\log-" + System.DateTime.Now.ToString("yyyyMMddHHmmss.fff")+ ".txt";
        }
    }
}
