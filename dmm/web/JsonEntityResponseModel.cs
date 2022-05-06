using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;

namespace EckyStudio.M.BaseModel.dmm.web
{
    public abstract class JsonEntityResponseModel : JsonEntityCommonModel
    {

        public bool From(string json) {
            _json = json;

            if (!Deserialize(json))
                return false;
            return Postconditioning();
        }


        private bool Deserialize(string json)
        {
            mExtendMember.Clear();

            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            for (int i = 0; i < dic.Count; i++)
            {
                //Console.Write(dic.ElementAt(i).Value.GetType() + ",");

                KeyValuePair<string, object> e = dic.ElementAt(i);
                FieldInfo f = this.GetType().GetField(e.Key);
                if (f != null)
                {
                    Type t = f.FieldType;
                    if (t.IsSubclassOf(typeof(JsonEntityResponseModel)))
                    {
                        f.SetValue(this, Activator.CreateInstance(t));//实例化对象
                        ((JsonEntityResponseModel)f.GetValue(this)).From((string)e.Value);
                    }
                    else if (t == typeof(string))
                    {
                        f.SetValue(this, (string)e.Value);
                    }
                    else if (t == typeof(int) || (t == typeof(Int64)))
                    {
                        f.SetValue(this,Convert.ToInt32(e.Value));
                    }
                    else if (t == typeof(float))
                    {
                        f.SetValue(this,(float)Convert.ToDouble(e.Value));
                    }
                    else if (t == typeof(double))
                    {
                        f.SetValue(this, (double)e.Value);
                    }
                    else if (t == typeof(int[]))
                    {
                        f.SetValue(this, ((Newtonsoft.Json.Linq.JArray)e.Value).ToObject<int[]>());
                    }
                    else if (t == typeof(string[]))
                    {
                        f.SetValue(this, ((Newtonsoft.Json.Linq.JArray)e.Value).ToObject<string[]>());
                    }
                    else if (t == typeof(double[]))
                    {
                        f.SetValue(this, ((Newtonsoft.Json.Linq.JArray)e.Value).ToObject<double[]>());
                    }
                    else if (t == typeof(float[]))
                    {
                        f.SetValue(this, ((Newtonsoft.Json.Linq.JArray)e.Value).ToObject<float[]>());
                    }
                    else if (t.IsArray && t.GetElementType().IsSubclassOf(typeof(JsonEntityResponseModel))) {
                        string[] obj_array  = ((Newtonsoft.Json.Linq.JArray)e.Value).ToObject<string[]>();
                        //object[] objs = new object[obj_array.Length];
                        Array arr = Array.CreateInstance(t.GetElementType(), obj_array.Length);
                        for (int j = 0; j < obj_array.Length; j++) {

                            JsonEntityResponseModel resp = (JsonEntityResponseModel)Activator.CreateInstance(t.GetElementType());
                            if (!resp.From(obj_array[j]))
                                return false;

                            arr.SetValue(resp, j);// = resp;
                        }
                        f.SetValue(this, Convert.ChangeType(arr,t));//实例化对象
                    }
                    else
                    {
                        f.SetValue(this, e.Value);
                    }
                }
                else
                {
                    AddMember(e.Key, e.Value.ToString());//存放扩展成员
                }
            }
            return true;
        }

        private string _json;
        public override string ToString()
        {
            return System.Web.HttpUtility.HtmlDecode(_json);
        }

        protected virtual bool Postconditioning() {
            return true;
        }
    }
}
