using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace EckyStudio.M.BaseModel.dmm.web
{
    public abstract class XMLEntityResponseModel : XMLEntityCommonModel
    {
        protected string STR_DOCUMENT_TAG = "xml";

        public bool From(string xmlText)
        {
            if (!ParseXML(xmlText))
                return false;
            return Postconditioning();
        }

        private bool ParseXML(string xmlText)
        {
            try {
                mExtendMember.Clear();//清空列表
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlText));
                XmlTextReader reader = new XmlTextReader(ms);

                //reader.ReadStartElement();
                while (reader.ReadState != ReadState.EndOfFile)
                {
                    reader.Read();
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            string name = reader.Name;

                            if (reader.Depth == 0) //跳过根节点
                                break;

                            FieldInfo f = this.GetType().GetField(name);
                            if (f != null)
                            {
                                reader.Read();
                                f.SetValue(this, reader.Value);
                            }else {
                                reader.Read();
                                AddMember(name, reader.Value);//存放扩展成员
                            }                           
                            break;
                    }
                }
            } catch (Exception ex) {
                System.Diagnostics.Trace.Write(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 后处理方法，用于From方法后置处理工作，From方法调用ParseXML之后需要校验签名，需要使用者自行处理
        /// </summary>
        protected abstract bool Postconditioning();
    }

    //public abstract class XMLEntityResponseModel
    //{
    //    public boolean parseXML(String xmlText)
    //    {
    //        try
    //        {
    //            ByteArrayInputStream bais = new ByteArrayInputStream(xmlText.getBytes("UTF-8"));
    //            Class cls = this.getClass();
    //            Field[] fields = cls.getFields();//cls.getDeclaredFields();

    //            XmlPullParser parser = Xml.newPullParser();

    //            parser.setInput(bais, "UTF-8");
    //            int eventType = parser.getEventType();
    //            while (eventType != XmlPullParser.END_DOCUMENT)
    //            {
    //                switch (eventType)
    //                {
    //                    case XmlPullParser.START_TAG:
    //                        String name = parser.getName();
    //                        for (int i = 0; i < fields.length; i++)
    //                        {
    //                            if (fields[i].getName().equals(name))
    //                            {
    //                                parser.next();
    //                                fields[i].set(this, parser.getText());
    //                                break;
    //                            }
    //                        }
    //                }
    //                eventType = parser.next();
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            ex.printStackTrace();
    //            return false;
    //        }
    //        return true;
    //    }
    //}
}
