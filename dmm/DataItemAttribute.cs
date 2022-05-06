/***********************************************
 * 名称：数据项属性模型
 * 功能/用途：用于标识一项数据，可携带最多三个附加值，用于描述数据项的相关特性
 * 构建目标：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2017-10-21
 * 最后修改时间：
 * 修改信息：
 * 1.将KeyName成员修改为Z，变为万能值，可根据实际情况赋予不同用途 2017-11-23
 * 2.增加多个附件值，分别为 Z、Y、X 2017-11-23
 * 备注：
 **********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.DataManagementModel
{ 
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property,AllowMultiple =false,Inherited = false)]
    public class DataItemAttribute :System.Attribute
    {
        /// <summary>
        /// 给特性带一个附加值，该附加值根据实际需要，赋予不同的功能，可以充当默认值、键名、节名等等
        /// 对于ConfigItem，Z为默认值，Y为节名，X为键名
        /// </summary>
        public DataItemAttribute() {
            //EckyLanguage.Config c = new EckyLanguage.Config(@"d:\aaa.ini");
            //c.WriteString("Test", "DataItem", "VAlue");
        }

        /// <summary>
        /// 给特性带一个附加值，该附加值根据实际需要，赋予不同的功能，可以充当默认值、键名、节名等等
        /// 对于ConfigItem，Z为默认值，Y为节名，X为键名
        /// </summary>
        /// <param name="z">需要携带的附加值，对于ConfigItem该处表示默认值</param>
        public DataItemAttribute(string z) {
            //EckyLanguage.Config c = new EckyLanguage.Config(@"E:\EckyStudio\CloudSync\和彩云\EckyStudio\EckyStudio.M\JQLHelper\bin\Debug\aaa.ini");
            //c.WriteString("Test", "DataItem", "VAlue");
            Z = z;
        }

        /// <summary>
        /// 给特性带二个附加值，该附加值根据实际需要，赋予不同的功能，可以充当默认值、键名、节名等等
        /// </summary>
        /// <param name="z">需要携带的附加值，对于ConfigItem该处表示默认值</param>
        /// <param name="y">需要携带的第二附加值，对于ConfigItem该处表示节名</param>
        public DataItemAttribute(string z,string y)
        {
            //EckyLanguage.Config c = new EckyLanguage.Config(@"E:\EckyStudio\CloudSync\和彩云\EckyStudio\EckyStudio.M\JQLHelper\bin\Debug\aaa.ini");
            //c.WriteString("Test", "DataItem", "VAlue");
            Z = z;
            Y = y;
        }

        public DataItemAttribute(object method){
            Func<string> f = (Func<string>)method;
            Z = f.Invoke();
        }

        /// <summary>
        /// 给特性带三个附加值，该附加值根据实际需要，赋予不同的功能，可以充当默认值、节名 、键名等等
        /// </summary>
        /// <param name="z">需要携带的附加值</param>
        /// <param name="y">需要携带的第二个附加值</param>
        /// <param name="x">需要携带的第三个附加值</param>
        public DataItemAttribute(string z,string y,string x)
        {
            //EckyLanguage.Config c = new EckyLanguage.Config(@"E:\EckyStudio\CloudSync\和彩云\EckyStudio\EckyStudio.M\JQLHelper\bin\Debug\aaa.ini");
            //c.WriteString("Test", "DataItem", "VAlue");
            Z = z;
            Y = y;
            X = x;
        }

        /// <summary>
        /// 特性的附加值，原来成员名称为KeyName
        /// </summary>
        public string Z = null;
        public string Y = null;
        public string X = null;

        public int AttachInfoCount {
            get {
                int count = 0;
                if (Z != null)
                    ++count;

                if (Y != null)
                    ++count;

                if (X != null)
                    ++count;

                return count;
            }
        }
    }
}
