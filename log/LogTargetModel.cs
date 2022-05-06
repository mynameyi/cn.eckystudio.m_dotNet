/***********************************************
 * 功能：日志目标处理器模型
 * 目标：定义日志目标处理器的基本处理模型
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2017-8-19
 * 完成时间：
 * 最后修改时间：2017-8-19
 * 修改信息：
 * 1.增加状态通知函数 Notify 2018-6-14
 * 备注：
 **********************************************/

public abstract class LogTargetModel
{
    /// <summary>
    /// 用于输出日志信息
    /// </summary>
    /// <param name="messages"></param>
    public abstract void OutputMessage(params string[] messages);
    /// <summary>
    /// 报告目标当前LogSystem的状态
    /// </summary>
    /// <param name="status">status 为0 表示程序发生严重错误，日志系统即将退出 </param>
    public virtual void Notify(int status) { }

}

