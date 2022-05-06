using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class RichTextBoxLogTarget : CtrlLogTarget
{
    RichTextBox mRichTextBox;
    public RichTextBoxLogTarget(RichTextBox rtb) {
        mRichTextBox = rtb;
    }

    public override void OutputMessage(string[] message)
    {
        // new NotImplementedException();
        if (mRichTextBox.InvokeRequired)
        {
            mRichTextBox.Invoke(new Action(() => { OutputMessage(message); }));
            return;
        }

        mRichTextBox.AppendText(message[0] + ":"+ message[3] + "\r\n"/* + "\r\n"*/);
        mRichTextBox.Select(mRichTextBox.TextLength - 2, 1);
        mRichTextBox.Focus();
    }

    public override Control GetControl()
    {
        return mRichTextBox;
    }
}

