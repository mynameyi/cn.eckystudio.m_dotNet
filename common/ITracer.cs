/***********************************************
 * 功能：外部跟踪器
 * 构建目标：用于调用者跟踪 功能组件或者自动化任务的内容运行情况
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2016-9-17
 * 最后修改时间：
 * 修改信息：
 * 备注：
 **********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EckyStudio.M.BaseModel
{
    public interface ITracer
    {
        /// <summary>
        /// 打印跟踪信息
        /// </summary>
        /// <param name="info">需要打印的信息</param>
        void Print(string info);

        void OnException(Exception ex);
    }
}

