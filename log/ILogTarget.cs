/***********************************************
 * 功能：日志目标处理器接口
 * 目标：定义日志目标处理器的基本行为
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2017-8-19
 * 完成时间：
 * 最后修改时间：2017-8-19
 * 修改信息：
 * 1.已丢弃
 * 备注：
 **********************************************/

using System;

[Obsolete("ILogTarget已丢弃，已更改为抽象类 LogTargetModel")]
public interface ILogTarget
{
    /// <summary>
    /// 用于输出日志信息
    /// </summary>
    /// <param name="messages"></param>
    void OutputMessage(params string[] messages);
}

