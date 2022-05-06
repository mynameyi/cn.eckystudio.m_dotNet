/***********************************************
 * 功能：初始化启动器
 * 目标：显示初始化语句，建立统一的初始化模型
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2015-4-5
 * 完成时间：2015-4-5
 * 修改信息：
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;


public static class Initializer
{
    public static void initialize(Form form, Action action)
    {
        Panel p = new Panel();
        Label l = new Label();
        l.Text = "正在启动......";
        l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        l.Dock = DockStyle.Fill;
        l.Font = new Font("宋体",18);
        p.Controls.Add(l);

        form.Controls.Add(p);
        p.Width = form.ClientRectangle.Width;
        p.Height = form.ClientRectangle.Height;
        p.BringToFront();
        
        Thread t = new Thread(new ThreadStart(new Action(() => {
            action.Invoke();

            if (form == null || form.IsDisposed)
                return;

            System.Diagnostics.Trace.WriteLine("System.Diagnostics.Trace can prevent the exception occure!!");//this line can prevent the exception of calling a disposed Object

            form.Invoke(new Action(() => {
                p.Visible = false;
            }));
        })));
        t.IsBackground = true;
        t.Start();
    }
}

