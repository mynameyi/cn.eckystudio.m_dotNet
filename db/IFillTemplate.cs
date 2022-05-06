/***********************************************
 * 功能：数据库记录的填充模板
 * 构建目标：提取oracle、mysql、mssql数据库之间的共性（2021-1-7）
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：2021-3-19
 * 修改信息：
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.db
{
    public interface IFillTemplate
    {
        void Fill(DataRow dr);
    }
}
