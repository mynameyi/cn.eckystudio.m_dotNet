using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace EckyStudio.M.FunctionComponentModel
{
    public class Json
    {
        public static T parse<T>(string jsonString)
        {
            using (var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
            }
        }

        public static string stringify(object jsonObject)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                new DataContractJsonSerializer(jsonObject.GetType()).WriteObject(ms, jsonObject);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
    
}
