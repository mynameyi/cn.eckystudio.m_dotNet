using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class ProgressBox
{
    Form mFrm = new Form();
    Button mBtnCancel = new Button();
    Label mLabTips = new Label();

    Thread mThread = null;

    public ProgressBox() {

        mFrm.SuspendLayout();
        // 
        // mLabTips
        // 
        mLabTips.AutoSize = true;
        mLabTips.Location = new System.Drawing.Point(64, 42);
        //mLabTips.Name = "label1";
        mLabTips.Size = new System.Drawing.Size(107, 12);
        mLabTips.TabIndex = 0;
        mLabTips.Text = "正在处理，请稍等...";

        // 
        // mBtnCancel
        // 
        mBtnCancel.Location = new System.Drawing.Point(77, 83);
        //mBtnCancel.Name = "button1";
        mBtnCancel.Size = new System.Drawing.Size(75, 23);
        mBtnCancel.TabIndex = 1;
        mBtnCancel.Text = "取消";
        mBtnCancel.UseVisualStyleBackColor = true;
        mBtnCancel.Click += MBtnCancel_Click;


        mFrm.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
        mFrm.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        mFrm.ClientSize = new System.Drawing.Size(238, 138);
        mFrm.Controls.Add(this.mBtnCancel);
        mFrm.Controls.Add(this.mLabTips);
        mFrm.MaximizeBox = false;
        mFrm.MinimizeBox = false;
        //mFrm.Name = "Template";
        mFrm.Text = "正在处理，请稍等";
        mFrm.StartPosition = FormStartPosition.CenterParent;
        mFrm.FormClosing += MFrm_FormClosing;

        mFrm.ResumeLayout(false);
        mFrm.PerformLayout();

        mFrm.Show();
        mFrm.Hide();
    }

    private void MFrm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing) {
            Stop();
        }
        //throw new NotImplementedException();
    }

    private void MBtnCancel_Click(object sender, EventArgs e)
    {
        //throw new NotImplementedException();
        Stop();
    }

    /// <summary>
    /// 强制停止线程任何
    /// </summary>
    private void Stop() {
        if (mThread != null)
        {
            try
            {
                mThread.Abort();
            }
            catch (Exception ex)
            {
            }
            Hide();//关闭窗口
        }
    }

    public void ShowAndRun(RunTemplate runnable) {
        mThread = ThreadUtils.AsynExecute(() => {
            ShowDialog();
            runnable.Invoke(this);
            mThread = null;
        });
    }

    public void Hide() {
        if (mFrm.InvokeRequired) {
            mFrm.Invoke(new Action(() => {
                Hide();
            }));
            return;
        }
        mFrm.Hide();
    }

    public void UpdateText(string text) {
        if (mLabTips.InvokeRequired) {
            mLabTips.BeginInvoke(new Action(() => {
                UpdateText(text);
            }));
            return;
        }
        mLabTips.Text = text;
    }

    private void ShowDialog() {
        if (mFrm.InvokeRequired)
        {
            mFrm.BeginInvoke(new Action(() =>
            {
                ShowDialog();
            }));
            return;
        }
        mFrm.ShowDialog();
    }

    public delegate void RunTemplate(ProgressBox box);
}

