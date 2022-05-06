/***********************************************
 * 功能：XML实体模型公共类：用于构建XML实体的公共行为模型
 * 构建目标：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：
 * 最后修改时间：
 * 修改信息：
 * 1.完成基本功能 2019-6-2
 * 2.增加mExtendMember表用于存放未定义的扩展成员 2019-9-27
 * 备注：
 **********************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EckyStudio.M.BaseModel.dmm.web
{
    public class XMLEntityCommonModel
    {
        protected const string TAG_XML = "xml";

        /// <summary>
        /// 扩展成员表，用于存放未定义的成员
        /// </summary>
        protected Dictionary<string, string> mExtendMember = new Dictionary<string, string>();
        
        protected enum SignType
        {
            MD5,
            HMAC_SHA256
        }

        /// <summary>
        /// 获取DeepLink格式字符串签名
        /// </summary>
        /// <param name="type">签名算法，支持MD5和HMAC_SHA256</param>
        /// <param name="vars">身份关联key，为了增加签名的身份可靠性，一般会加入与身份关联的字符串，如密钥key等</param>
        protected string GetDeepLinkSign(SignType type,string key_str)
        {
            /*
            try
            {
                Type t = this.GetType();
                FieldInfo[] fields = t.GetFields();

                string[] field_names = new string[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    field_names[i] = fields[i].Name;
                }

                Array.Sort(field_names);

                StringBuilder sb = new StringBuilder();
                foreach (string name in field_names)
                {
                    string value = (string)t.GetField(name).GetValue(this);
                    if (!string.IsNullOrEmpty(value))
                    {
                        sb.Append(name + '=' + value + '&');
                    }
                }

                sb.Append("key=" + key_str);

                byte[] hash;
                switch (type)
                {
                    case SignType.HMAC_SHA256:
                        HMACSHA256 hmac = new HMACSHA256();
                        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                        break;
                    case SignType.MD5:
                    default:
                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                        hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                        break;
                }

                //转十六进制
                StringBuilder md5_str = new StringBuilder();
                foreach (byte h in hash)
                {
                    md5_str.Append(h.ToString("x2"));
                }
                return md5_str.ToString().ToUpper();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return "";
            */

            try
            {
                Type t = this.GetType();
                FieldInfo[] fields = t.GetFields();           

                KeyValue[] members = new KeyValue[mExtendMember.Count + fields.Length];
                for (int i = 0; i < members.Length; i++) {
                    members[i] = new KeyValue();
                }

                //添加命名成员
                for (int i = 0; i < fields.Length; i++)
                {
                    members[i].Key = fields[i].Name;
                    members[i].Value = 0;
                }

                //添加扩展成员
                int index = fields.Length;
                int count = mExtendMember.Count;
                for(int i= 0;i<count;i++) {
                    members[i + index].Key = mExtendMember.ElementAt(i).Key;
                    members[i + index].Value = 1;
                }

                Array.Sort(members);

                StringBuilder sb = new StringBuilder();
                foreach (var m in members)
                {
                    string value;
                    if (m.Value == 0)
                    {
                        value = (string)t.GetField(m.Key).GetValue(this);
                    }
                    else {
                        value = (string)mExtendMember[m.Key];
                    }

                    if (!string.IsNullOrEmpty(value))
                    {
                        sb.Append(m.Key + '=' + value + '&');
                    }

                }

                sb.Append("key=" + key_str);

                byte[] hash;
                switch (type)
                {
                    case SignType.HMAC_SHA256:
                        HMACSHA256 hmac = new HMACSHA256();
                        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                        break;
                    case SignType.MD5:
                    default:
                        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                        hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                        break;
                }

                //转十六进制
                StringBuilder md5_str = new StringBuilder();
                foreach (byte h in hash)
                {
                    md5_str.Append(h.ToString("x2"));
                }
                return md5_str.ToString().ToUpper();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return "";
        }


        /// <summary>
        ///  生成随机字符串
        /// </summary>
        /// <param name="is_fixed_len">如果固定长度为真，则shortest_len可指定长度，默认为不固定长度</param>
        /// <param name="shortest_len">最短长度，如果不固定长度，返回长度为2倍最短长度值，默认为16 </param>
        /// <returns>返回随机字符串</returns>
        protected string GenerateNonceStr(bool is_fixed_len = false, int shortest_len = 16 /*int? shortest_len*/)
        {
            Random random = new Random();
            int length = is_fixed_len ? shortest_len: random.Next(shortest_len) + shortest_len /*(shortest_len == null? 16:(int)shortest_len)*/;//生成随机字符串长度，16位起，不超过32位
            String str = "zxcvbnmlkjhgfdsaqwertyuiopQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
            //由Random生成随机数
            StringBuilder sb = new StringBuilder();
            //长度为几就循环几次
            for (int i = 0; i < length; ++i)
            {
                //产生0-61的数字
                int number = random.Next(str.Length);
                //将产生的数字通过length次承载到sb中
                sb.Append(str[number]);
            }
            //将承载的字符转换成字符串
            return sb.ToString();
        }

        /// <summary>
        /// 根据时间生成随机数，时间随机数后带三位随机数
        /// </summary>
        /// <param name="format">时间格式</param>
        /// <param name="prefix">随机数前缀</param>
        /// <param name="postfix">随机数后缀</param>
        /// <returns></returns>
        public string GenerateRandomStrByTime( string prefix = "",string postfix = "", string format = "yyyyMMddHHmmssfff") {
            String no = System.DateTime.Now.ToString(prefix + "yyyyMMddHHmmssfff");
            Random r = new Random();
            return no + (r.Next(900) + 100) + postfix;
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddMember(string name,string value) {
            mExtendMember.Add(name, value);//
        }

        public Dictionary<string, string> GetExtendMembers()
        {
            return mExtendMember;
        }

        private class KeyValue :IComparable{
            public string Key;
            public int Value;

            public int CompareTo(object obj)
            {
                KeyValue kv = obj as KeyValue;
                return Key.CompareTo(kv.Key);
                //throw new NotImplementedException();
            }
        }
    }
}   
    
