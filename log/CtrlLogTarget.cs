/***********************************************
 * 名称：控件日志目标模型
 * 功能/用途：定义一个基于控件的日志目标模型，便于跨线程的调用
 * 目标：
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2017-9-19
 * 修改记录：
 * 
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


public abstract class CtrlLogTarget : LogTargetModel
{
    public abstract Control GetControl();
}

