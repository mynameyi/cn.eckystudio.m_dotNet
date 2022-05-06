/***********************************************
 * 名称：配置管理模型
 * 功能/用途：定义一种ini文件格式关联的的配置管理模型
 * 目标：建立一套最简洁、最快速的配置管理机制
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间/首次完成时间：2017-11-23
 * 修改记录：
 * 1.将ValueChangingHandler修改为 ValueChangedHandler，并增加两个参数sectionName 和 keyName   2019-09-02
 * 2.将 WriteItem方法由静态方法改为实例方法，使该类支持多实例应用 2019-9-2
 * 3.增加Flush方法，实现主动更新模式功能  2019-9-2
 * 
 * 备注：
 * 需要优化的问题：
 * ??配置模型由于底层使用了配置项与关联文件的关联上使用了静态思想，因此目前仅支持单例模式，只能实例化单个类使用，后期可考虑在关联文件对象直接传入ConfigItem中 2019-9-2
 * ??配置模型考虑引入  自动更新模式 和 主动更新模式，自动更新模式是指调用配置项目对象ConfigItem.Set函数时,自动将配置刷新到文件，主动更新模式是指直接对配置项赋初字符串初值，然后调用方法统一进行更新 2019-9-2
 * 
 * 遵循约定：
 * 1.模型使用ConfigItem成员与配置项一一对应
 * 2.DataItem附加信息说明：Z表示默认值,Y表示节名，X表示键名
 * 3.配置项不支持同名Key,即使节名不一样
 **************************************************/
using EckyLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.DataManagementModel
{
    /// <summary>
    /// 配置管理模型引入 自动更新模式 和 主动更新模式两种工作模式，一般来说自动更新模式更安全、及时，主动更新模式减少频繁写入文件，效率更高,默认工作于AutoFlushMode模式下
    /// 自动更新模式(WorkMode.AutoFlushMode)：是指调用配置项目对象ConfigItem.Set方法来改变配置值，自动刷新文件；
    /// 主动更新模式( WorkMode.PositiveFlushMode)：是指直接对配置项赋初字符串初值，然后主动调用Flush方法主动更新到文件，如不调用，则配置不会保存到文件。
    /// </summary>
    public class ConfigManagementModel
    {
        const string FN_DEFAULT = "default.ini";
        //static Config3_0 sConfig;  //2019-9-2修改
        Config3_0 mConfig;

        Dictionary<string, string> mSectionKeyMap = new Dictionary<string, string>();
        internal WorkMode mMode;

        /// <summary>
        /// 初始化配置文件，默认刷新模式为AutoFlushMode
        /// </summary>
        public ConfigManagementModel(WorkMode mode = WorkMode.AutoFlushMode) :this(null,mode) {
        }

        /// <summary>
        /// 初始化配置文件与类成员配置项的关联关系
        /// </summary>
        /// <param name="fileName">配置文件文件名或绝对路径</param>
        /// <param name="mode">工作模式</param>
        public ConfigManagementModel(string fileName, WorkMode mode = WorkMode.AutoFlushMode) {
            mMode = mode;

            if (fileName == null)
                fileName = FN_DEFAULT;

            //sConfig = new Config3_0(fileName);
            mConfig = new Config3_0(fileName);

            var fields = GetType().GetFields();
            foreach (var f in fields)
            {
                //var attr =(DataItemAttribute) Attribute.GetCustomAttribute(a, typeof(DataItemAttribute));
                //if (attr != null)
                if (f.FieldType == typeof(ConfigItem))
                //if (Attribute.IsDefined(a, typeof(DataItemAttribute)))
                {
                    //object v = f.GetValue(this);//默认值为原来使用的值

                    //赋初值，通过反射赋初值不会触发操作符重载
                    ConfigItem ci = new ConfigItem();
                    f.SetValue(this, ci);

                    //var fi = f.FieldType.GetField("Value"/*, System.Reflection.BindingFlags.NonPublic*/);
                    string sec = Config3_0.DEFAULT_SETCTION, key = f.Name, def = "";
                    //初始化特性
                    var attr = (DataItemAttribute)Attribute.GetCustomAttribute(f, typeof(DataItemAttribute));
                    if (attr != null)
                    {
                        //初始化特性声明的默认值
                        /*
                        switch (attr.AttachInfoCount)
                        {
                            case 3:
                                //f.SetValue(this, sConfig.ReadString(attr.Y, attr.X, attr.Z));
                                ci.Value = mConfig.ReadString(attr.Y, attr.X, attr.Z);

                                //设置键名
                                //fi.SetValue(f.GetValue(this), attr.X);
                                ci.Key = attr.X;

                                //设置节名
                                //fi = f.GetType().GetField("Section",System.Reflection.BindingFlags.NonPublic);
                                //fi.SetValue(f.GetValue(this), attr.Y);
                                ci.Section = attr.Y;
                                break;
                            case 2:
                                //f.SetValue(this, sConfig.ReadString(attr.Y, f.Name, attr.Z));
                                ci.Value = mConfig.ReadString(attr.Y, ci.Key, attr.Z);

                                //设置节名
                                //fi = f.GetType().GetField("Section", System.Reflection.BindingFlags.NonPublic);
                                //fi.SetValue(f.GetValue(this), attr.Y);
                                ci.Section = attr.Y;
                                break;
                            case 1://默认值
                                //f.SetValue(this, sConfig.ReadString(Config3_0.DEFAULT_SETCTION, f.Name, attr.Z));
                                ci.Value = mConfig.ReadString(Config3_0.DEFAULT_SETCTION, f.Name, attr.Z);
                                break;
                            default:
                                ci.Value = mConfig.ReadString(Config3_0.DEFAULT_SETCTION, f.Name, "");
                                break;
                        }

                        if (attr.AttachInfoCount > 0) {
                            ci.DefaultValue = attr.Z;
                        }
                        */
            
                        sec = string.IsNullOrEmpty(attr.Y) ? Config3_0.DEFAULT_SETCTION : attr.Y;
                        key = string.IsNullOrEmpty(attr.X) ? f.Name : attr.X;
                        def = string.IsNullOrEmpty(attr.Z) ? "" : attr.Z;              
                    }

                    ci.Section = sec;

                    //提取键名
                    //fi = f.FieldType.GetField("Key", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);//获取internal成员需要 System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic 两个标记
                    //fi.SetValue(f.GetValue(this), f.Name);
                    ci.Key = key;
                    ci.Value = mConfig.ReadString(sec, key, def);
                    ci.DefaultValue = def;           

                    if (mode == WorkMode.PositiveFlushMode) {
                        mSectionKeyMap.Add(ci.Key, ci.Section);
                    }

                    ci.mConfig = this;//将当前对象赋给配置项
                    
                    //4.5 code
                    //foreach (var ca in f.CustomAttributes) {
                    //    if (ca.AttributeType == typeof(DataItemAttribute)) {

                    //    }
                    //}
                }
            }

            LoadDynamicDefaultValue(fileName,mode);
        }

        /// <summary>
        /// 恢复默认设置
        /// </summary>
        public void RestoreDefaultConfig()
        {
            if (mConfig == null)
                return;

            var fields = GetType().GetFields();
            foreach (var f in fields)
            {
                if (f.FieldType == typeof(ConfigItem))
                {
                    ConfigItem ci = (ConfigItem)f.GetValue(this);
                    //ci.Value = ci.DefaultValue;
                    ci.Set(ci.DefaultValue);
                }
            }
        }

        /// <summary>
        /// 加载非常量的默认值,该动作在初始化配置文件之后执行
        /// </summary>
        protected virtual void LoadDynamicDefaultValue(string fileName = "",WorkMode mode = WorkMode.AutoFlushMode) {

        }

        /// <summary>
        /// 获取配置项
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="def">默认值</param>
        /// <param name="section">节名，默认为General</param>
        /// <returns></returns>
        public string GetConfigItem(string key,string def = "",string section = Config3_0.DEFAULT_SETCTION) {
            return mConfig.ReadString(section, key, def);
        }

        /// <summary>
        /// 获取指定配置项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetConfigItem<T>(string key,string section = Config3_0.DEFAULT_SETCTION) {
            string value = mConfig.ReadString(section, key, null);

            if (value == null)
                return default(T);

            object ret = value;
            if (typeof(T) == typeof(int))
            {
                ret = int.Parse(value);
            }
            else if (typeof(T) == typeof(double))
            {
                ret = double.Parse(value);
            }
            else if (typeof(T) == typeof(float))
            {
                ret = float.Parse(value);
            }
            else if (typeof(T) == typeof(bool))
            {
                ret = bool.Parse(value);
            }

            return (T)ret;
        }

        /// <summary>
        /// 写入配置项
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="section">节名，默认为General</param>
        public void WriteConfigItem(string key, object value,string section = Config3_0.DEFAULT_SETCTION)
        {
            WriteItem(section, key, value.ToString());
        }

        /// <summary>
        /// 更新配置到文件，采用PositiveFlushMode模式时，必须调用该函数才能刷新
        /// </summary>
        public void Flush()
        {
            if (mMode == WorkMode.AutoFlushMode)
                throw new Exception("当前工作于AutoFlushMode下，不支持主动调用Flush函数");

            var fields = GetType().GetFields();
            foreach (var f in fields)
            {
                if (f.FieldType == typeof(ConfigItem))
                {
                    WriteItem(mSectionKeyMap[f.Name], f.Name, ((ConfigItem)f.GetValue(this)).Value);
                }
            }
        }


        //static internal void WriteItem(string section,string key,string value)
        internal void WriteItem(string section, string key, string value)
        {
            if (mConfig == null)
                return;

            mConfig.WriteString(section, key, value);
        }

        public void Dispose()
        {
            mConfig.Dispose();
        }
    }


    /// <summary>
    /// 值改变代理,值改变之前触发该函数
    /// </summary>
    /// <param name="oldValue">原来的值</param>
    /// <param name="newValue">新值</param>
    //public delegate void ValueChangingHandler(string oldValue, string newValue);//一丢弃，已修改为 ValueChangedHandler

    /// <summary>
    /// 值改变代理,值改变之后触发该方法
    /// </summary>
    /// <param name="sectionName">节名</param>
    /// <param name="keyName">键名</param>
    /// <param name="oldValue">原来值</param>
    /// <param name="newValue">新值</param>
    public delegate void ValueChangedHandler(string sectionName,string keyName,string oldValue,string newValue);

    public enum WorkMode
    {
        AutoFlushMode,
        PositiveFlushMode
    }
}
