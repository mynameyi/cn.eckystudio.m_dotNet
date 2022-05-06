using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.AutomationFlowModel
{
    public enum OnExceptionHandleMethods
    {
        RETRY,//重新执行任务链
        WAIT,//等待干预
        STOP,//停止执行任务
    }
}
