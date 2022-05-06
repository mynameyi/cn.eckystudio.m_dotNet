using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


public class DefaultLogTarget : ILogTarget
{
    StreamWriter mSwLog = new StreamWriter("logsystem.txt");
    public void OutputMessage(params string[] messages)
    {
        //throw new NotImplementedException();

        if ((messages == null) || (messages.Length == 0))
            return;

        mSwLog.Write(messages[0]);
        mSwLog.Flush();
    }

    ~DefaultLogTarget()
    {
        //mSwLog.Flush();
        //mSwLog.Close();
    }
}

