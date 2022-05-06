/***********************************************
 * 功能：日志目标接口
 * 目标：定义日志目标的基本行为
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2017-8-19
 * 完成时间：
 * 最后修改时间：2017-8-19
 * 修改信息：
 * 备注：
 **********************************************/


using System.Windows.Forms;

public abstract class ControlLogTarget : ILogTarget
{
    public abstract void OutputMessage(string message);
    public abstract Control GetControl();
}

