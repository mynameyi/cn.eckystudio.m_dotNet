using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EckyStudio.M.BaseModel
{
    public class DefaultTracer : ITracer
    {
        void ITracer.OnException(Exception ex)
        {
            //throw new NotImplementedException();
        }

        void ITracer.Print(string info)
        {
            //throw new NotImplementedException();

            //if (LogSystem.IsDebuggable)
            //{
            //    LogSystem.Print(info, 4);
            //}
            //else {
            //    LogSystem.Print(info, 3);
            //}
            LogSystem.Print(LogSystem.LogLevel.LEVEL_FCM, info);
        }
    }
}
