using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace EckyStudio.M.BaseModel.dmm.web
{
    public abstract class XMLEntityRequestModel : XMLEntityCommonModel
    {
        protected string mDocumentTag = "xml";

        public String ToXML()
        {

            try {

                Preconditioning();//执行预处理方法

                Type t = this.GetType();
                FieldInfo[] fields =  t.GetFields();

                MemoryStream stream = new MemoryStream();
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);

                writer.WriteStartElement(mDocumentTag);

                //写入命名元素
                for (int i = 0; i < fields.Length; i++) {

                    string value = (string)fields[i].GetValue(this);
                    if (!string.IsNullOrEmpty(value))
                    {
                        writer.WriteStartElement(fields[i].Name);
                        writer.WriteValue(value);
                        writer.WriteEndElement();
                    }               
                }

                //写入未知元素
                foreach (var v in mExtendMember) {
                    if (!string.IsNullOrEmpty(v.Value))
                    {
                        writer.WriteStartElement(v.Key);
                        writer.WriteValue(v.Value);
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();

                writer.Flush();
                writer.Close();

                byte[] xmlData = stream.ToArray(); 
                return Encoding.UTF8.GetString(xmlData);
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return "";
        }

        /// <summary>
        /// toXML前的预置处理方法,例如签名和自动生成一些字段等特殊处理
        /// </summary>
        protected abstract void Preconditioning();
    }
}
