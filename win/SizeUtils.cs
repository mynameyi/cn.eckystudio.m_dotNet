/***********************************************
 * 名称：尺寸调节助手
 * 功能/用途：用于快速实现子控件跟随窗口尺寸调整变化而变化（按比例调节窗口界面布局）
 * 目标：用最简单的方式实现窗口调整
 * 应用情景：
 * 1.所有可调窗口。。。
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间/首次完成时间：2017-12-1
 * 修改记录：
 * 1.修改为支持多个窗体缩放，原来功能仅支持一个 2019-8-29
 * 备注：
 **************************************************/
using System;
using System.Collections.Generic;
using System.Windows.Forms;

public static class SizeUtils
{
    static Dictionary<Control, List<int>> sContainerTable = new Dictionary<Control, List<int>>();

    //static List<int> sStatus;
    /*方法已过时，已采用新方法,新方法采用Dictionary,支持多个窗体  2019-8-29 */
    ///// <summary>
    ///// 保存控件的尺寸、位置状态
    ///// </summary>
    ///// <param name="status">可以为空</param>
    //public static void RegisterSizeChanged(Control container) {

    //    if (sStatus == null) {
    //        sStatus = new List<int>();
    //    }

    //    //保存原尺寸
    //    sStatus.Add(container.Width);
    //    sStatus.Add(container.Height);

    //    RetriveSizeStatus(container, ref sStatus);

    //    container.SizeChanged += Container_SizeChanged;
    //}

    //public static void UpdateSizeStatus(Control container)
    //{
    //    if (sStatus == null)
    //        return;

    //    sStatus.Clear();

    //    //保存原尺寸
    //    sStatus.Add(container.Width);
    //    sStatus.Add(container.Height);

    //    RetriveSizeStatus(container, ref sStatus);
    //}

    /// <summary>
    /// 保存控件的尺寸、位置状态
    /// </summary>
    /// <param name="status">可以为空</param>
    public static void RegisterSizeChanged(Control container)
    {

        if (sContainerTable.ContainsKey(container)) {
            return;
        }

        List<int> status = new List<int>();

        //保存原尺寸
        status.Add(container.Width);
        status.Add(container.Height);

        RetriveSizeStatus(container, ref status);

        sContainerTable.Add(container, status);

        container.SizeChanged += Container_SizeChanged;
    }

    private static void Container_SizeChanged(object sender, EventArgs e)
    {
        //throw new NotImplementedException();
        Control c = (Control)sender;
        int w = c.ClientRectangle.Width;
        int h = c.ClientRectangle.Height;

        //ReSize(c, ref sStatus, w, h);
        ReSize(c, sContainerTable[c], w, h);
    }



    /// <summary>
    /// 调节窗口大小
    /// </summary>
    /// <param name="container"></param>
    /// <param name="status"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static void ReSize(Control container,List<int> status,int width,int height) {
        if (status == null)
            return;

        int index = 2;
        Reset(container,status,ref index);//首先恢复控件原来大小，防止多次变化由于小数与整数之间转换导致的移位

        int w = status[0];
        int h = status[1];

        double wRate = 1.0f * width / w;
        double hRate = 1.0f * height / h;

        ResizeControl(container, wRate, hRate);
    }

    private static void ResizeControl(Control container,double wRate,double hRate) {
        foreach (Control c in container.Controls) {
            c.Width = (int)(c.Width * wRate);
            c.Height = (int)(c.Height * hRate);

            System.Drawing.Point p = new System.Drawing.Point();
            p.X = (int)(c.Location.X * wRate);
            p.Y = (int)(c.Location.Y * hRate);
            c.Location = p;

            if (IsContainer(c)) {
                ResizeControl(c,wRate,hRate);
            }
        }
    }

    private static bool IsContainer(Control c) {
        if (c is IContainerControl || c is Panel || c is TabControl || c is GroupBox)//容器目前没有共同类
            return true;

        return false;
    }

    /// <summary>
    /// 递归
    /// </summary>
    /// <param name="container"></param>
    /// <param name="status"></param>
    /// <param name="index"></param>
    private static void Reset(Control container,List<int> status,ref int index) {
        foreach (Control c in container.Controls) {
            //恢复尺寸
            c.Width = status[index++];
            c.Height = status[index++];

            //恢复位置
            System.Drawing.Point p = new System.Drawing.Point();
            p.X = status[index++];
            p.Y = status[index++];
            c.Location = p;

            if (IsContainer(c))
            {
                Reset(c, status, ref index);//获取子控件的位置、尺寸状态
            }
        }
    }

    private static void RetriveSizeStatus(Control container ,ref List<int> status) {
        foreach (Control c in container.Controls) {
            status.Add(c.Width);
            status.Add(c.Height);

            status.Add(c.Location.X);
            status.Add(c.Location.Y);

            if (IsContainer(c)) {
                RetriveSizeStatus(c, ref status);//获取子控件的位置、尺寸状态
            }
        }

    }
}

