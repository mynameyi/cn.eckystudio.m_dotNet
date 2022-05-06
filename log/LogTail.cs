/****************************************************************************
 * 名称:LogTail
 * 功能/用途：用于缓存日志的尾部，快速用于邮件发送
 * 目标：缓存最新日志，用于日志发送
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2018-5-2
 * 修改信息： 
 * 1.完成基本功能
 * 2.日志单元分隔增加水平制表符\t 和 结束增加回车符增加邮件日志的可读性 2018-6-14
 * 备注：
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.log
{
    public class LogTail
    {
        private StringBuilder mTail;
        private int mSize = 24 * 1024;

        public LogTail(int size) {
            mSize = size;
            mTail = new StringBuilder(mSize);
        }

        public void Append(params string[] message) {
            string text = MakeRecord(message);
            mTail.Append(text);
            if (mTail.Length > mSize) {
                mTail.Remove(0, mTail.Length - mSize);
            }
        }

        public string Tail {
            get {
                return mTail.ToString();
            }
        }

        private string MakeRecord(params string[] message) {
            StringBuilder sbRecord = new StringBuilder('\x1F');
            foreach (string s in message)
            {
                sbRecord.Append(s + '\t' + '\x1E');
            }

            sbRecord[sbRecord.Length - 1] = '\x1F';//替换最后的逗号为记录终止符号
            sbRecord.Append("\r\n");

            return sbRecord.ToString();
        }
    }
}
