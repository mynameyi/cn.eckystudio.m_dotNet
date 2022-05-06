/***********************************************
 * 名称：任务上下文数据存放器
 * 功能：
 * 构建目标：用于存放任何执行过程中，需要共享的数据，目前包括管道链Pipe
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2019-9-12
 * 最后修改时间：
 * 修改信息：
 * 备注：
 * 1.添加TaskPipe成员 2019-9-12
 * 
 * 2.停用（2019-9-12），用回TaskPipe
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.AutomationFlowModel
{
    public class TaskContext
    {
        /// <summary>
        /// 任务管道，存放变量列表，用于任务之间变量传送，数据交互
        /// </summary>
        internal SortedList<string, object> TaskPipe = new SortedList<string, object>();

        /// <summary>
        /// 清空上下文，
        /// </summary>
        internal void Clear()
        {
            TaskPipe.Clear();//
        }
    }
}
