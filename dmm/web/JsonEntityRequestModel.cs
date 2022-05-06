using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;
using System.Data;
using System.Reflection;
using System.Collections.Generic;

namespace EckyStudio.M.BaseModel.dmm.web
{
    public abstract class JsonEntityRequestModel : JsonEntityCommonModel
    {
        public string toJSON() {

            Preconditioning();//执行预处理

            Dictionary<string, object> dic = new Dictionary<string, object>();
            //添加命名元素
            Type t = this.GetType();
            FieldInfo[] fields = t.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                //Type t1 = fields[i].FieldType;

                object value = fields[i].GetValue(this);
                if (value is JsonEntityRequestModel)//嵌套JSON
                {
                    value = ((JsonEntityRequestModel)value).toJSON();
                }else if (value is JsonEntityRequestModel[]) {//嵌套对象数组
                    System.Collections.IEnumerator enumerator = ((JsonEntityRequestModel[])value).GetEnumerator();
                    LinkedList<string> list = new LinkedList<string>();
                    while (enumerator.MoveNext()) {
                        list.AddLast(((JsonEntityRequestModel)enumerator.Current).toJSON());
                    }
                    value = list.ToArray();
                }
                dic.Add(fields[i].Name,value == null ? "": value);
            }

            //添加未名元素
            foreach (var v in mExtendMember)
            {
                if (!string.IsNullOrEmpty(v.Value))
                {
                    dic.Add(v.Key, v.Value);
                }
            }
            return JsonConvert.SerializeObject(dic);
        }

        public string toDeepLink() {
            Preconditioning();
            return GetDeepLink();
        }

        /// <summary>
        /// toJSON前的预置处理方法,例如签名和自动生成一些字段等特殊处理
        /// </summary>
        protected abstract void Preconditioning();
    }
}
