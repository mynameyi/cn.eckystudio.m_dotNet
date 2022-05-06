/****************************************************************
 * 
 * 
 */ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TextLogTarget : LogTargetModel
{
    //30,记录分隔符，29，分组符 28 文件分隔符， 31单元分隔符
    const char SYMBOL_RECORD_SEPERATOR = '\x1E';
    const char SYMBOL_RECORD_BORDER_BEGIN = '\x1F';
    const char SYMBOL_RECORD_BORDER_END = '\x1F';
    Log.file.FileSeperator mLog = new Log.file.FileSeperator(1024*1024);//默认分割大小为1M
    
    public TextLogTarget() {
        //mLog.RecordSeparator = '\x1E';//30,记录分隔符，29，分组符 28 文件分隔符， 31单元分隔符
    }

    public override void OutputMessage(params string[] message)
    {
        //throw new NotImplementedException();
        StringBuilder sbRecord = new StringBuilder(SYMBOL_RECORD_BORDER_BEGIN);
        foreach (string s in message)
        {
            sbRecord.Append(s + SYMBOL_RECORD_SEPERATOR);
        }

        sbRecord[sbRecord.Length - 1] = SYMBOL_RECORD_BORDER_END;//替换最后记录分隔符为记录终止符号
        sbRecord.Append("\r\n");

        string r = sbRecord.ToString();
        mLog.Write(r);
    }

    public override void Notify(int status)
    {
        base.Notify(status);
        if (status == 0) {
            mLog.Flush();
        }
    }
}

