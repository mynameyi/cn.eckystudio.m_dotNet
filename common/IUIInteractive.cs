/***********************************************
 * 功能：UI交互接口
 * 构建目标：定义一种更容易与UI交互的约定（可用于afm自动化任务）
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
using System.Windows.Forms;

namespace EckyStudio.M.BaseModel
{
    public interface IUIInteractive
    {
        void Bind(string taskName, Control c, string ctrlName);
    }
}

