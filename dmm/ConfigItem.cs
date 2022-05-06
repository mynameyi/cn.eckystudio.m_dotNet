/***********************************************
 * 名称：配置项
 * 功能/用途：关联一个实际配置项
 * 目标：配合ConfigManagementModel实现配置管理
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间/首次完成时间：2017-11-23
 * 修改记录：
 * 1.增加值改变事件 OnValueChanging 2017-11-28
 * 2.重写ToString()函数，方便+运算 2017-11-28
 * 备注：
 * 遵循约定：
 * 1.ConfigItem成员与配置项一一对应
 * 2.写：ConfigItem使用Set函数对配置项进行赋值
 * 3.读：StringValue,IntValue,DoubleValue,FloatValue
 * 4.增加隐式转换方法string(ConfigItem ci)，直接使用ConfigItem时，可以隐式转换为 StringValue类型值 2019-09-02
 * 
 * 开发计划：
 * 1.增加Change事件，一旦配置项改变，触发Changed事件（已完成）
 * 
 * 修改记录：
 * 1.增加ConfigManagementModel变量，改变原有的静态调用思想
 **************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.DataManagementModel
{
    public class ConfigItem
    {
        internal ConfigManagementModel mConfig;
        /// <summary>
        /// 字段所在节名
        /// </summary>
        internal string Section;
        /// <summary>
        /// 配置项键名
        /// </summary>
        internal string Key;
        /// <summary>
        /// 配置项的默认值
        /// </summary>
        internal string DefaultValue;
        /// <summary>
        /// 配置项键值
        /// </summary>
        internal string Value;

        /*
        /// <summary>
        /// 值改变事件
        /// </summary>
        public event ValueChangingHandler OnValueChanging;//已修改为 OnValueChanged */


        /// <summary>
        ///  值改变事件
        /// </summary>
        public event ValueChangedHandler OnValueChanged;


        /// <summary>
        /// 改变配置项的值
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value) {
            if (mConfig.mMode == WorkMode.PositiveFlushMode && string.IsNullOrEmpty(Section)) {
                throw new Exception("当前类工作在WorkMode.PositiveFlushMode 模式下，只能使用赋值符号赋值，并且使用Flush进行配置文件更新");
            }

            string newValue = value.ToString();

            //触发值改变事件
            /*
            if (newValue != Value) {
                if(OnValueChanging != null)
                    OnValueChanging(Value, newValue);
            }
            */

            Value = newValue;
            WriteItem(this);

            //触发值改变事件
            if (newValue != Value) {
                if(OnValueChanged != null)
                    OnValueChanged(Section , Key, Value, newValue);
            }

        }

        /// <summary>
        /// 设置默认值，用于设置非常量的默认值（不能通过特性指定的默认值）
        /// </summary>
        /// <param name="value">需要设置的默认值</param>
        public void SetDefaultValue(object value)
        {
            DefaultValue = value.ToString();

            if (string.IsNullOrEmpty(Value))
                Value = DefaultValue;
        }

        /// <summary>
        /// 获取属性，并且根据泛型指定的类型返回指定类型值，目前只支持float、double、bool、int
        /// </summary>
        /// <typeparam name="T">需要返回的值的类型</typeparam>
        /// <returns>返回值</returns>
        public T Get<T>()
        {
            object ret = Value;
            if (typeof(T) == typeof(int))
            {
                ret = IntValue;
            }
            else if (typeof(T) == typeof(double))
            {
                ret = DoubleValue;
            }
            else if (typeof(T) == typeof(float))
            {
                ret = FloatValue;
            }
            else if (typeof(T) == typeof(bool)) {
                ret = BoolValue;
            }

            return (T)ret;
        }

        /// <summary>
        /// 获取String类型值
        /// </summary>
        public string StringValue {
            get {
                return Value;
            }
        }
        
        /// <summary>
        /// 获取Int类型值
        /// </summary>
        public int IntValue {
            get {
                int iValue;
                return int.TryParse(Value,out iValue)?iValue:int.Parse(DefaultValue);
            }
        }

        public bool BoolValue {
            get {
                bool bValue;
                return bool.TryParse(Value, out bValue) ? bValue : bool.Parse(DefaultValue);
            }
        }

        public double DoubleValue {
            get {
                double dValue;
                return double.TryParse(Value, out dValue) ? dValue : double.Parse(DefaultValue);
            }
        }

        public float FloatValue {
            get {
                float fValue;
                return float.TryParse(Value, out fValue) ? fValue : float.Parse(DefaultValue);
            }
        }

        public bool IsNotSet {
            get {
                return String.IsNullOrEmpty(Value);
            }
        }

        public bool Equals(string value) {
            if (Value == value)
                return true;

            return false;
        }

        /// <summary>
        /// 用于+ 运算时，直接返回ConfigItem对应的值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //return base.ToString();
            return Value;
        }

        public static implicit operator ConfigItem(string value)
        {
            ConfigItem ci = new ConfigItem();
            ci.Value = value;
            return ci;
        }

        public static implicit operator ConfigItem(int value)
        {
            return new ConfigItem().Value = value.ToString();
        }

        public static implicit operator ConfigItem(double value)
        {
            return new ConfigItem().Value = value.ToString();
        }

        public static implicit operator ConfigItem(float value)
        {
            return new ConfigItem().Value = value.ToString();
        }

        /// <summary>
        /// 直接使用时，隐式转换为string类型值
        /// </summary>
        /// <param name="ci"></param>
        public static implicit operator string(ConfigItem ci)
        {
            return ci.Value;
        }


        /*
        private static ConfigItem WriteItem(ConfigItem ci) {
            //ConfigItem ci = new ConfigItem() { Value = value };
            ConfigManagementModel.WriteItem(ci.Section, ci.Key, ci.Value);
            return ci;
        }
        */

        private void WriteItem(ConfigItem ci)
        {
            if (mConfig != null)
                mConfig.WriteItem(ci.Section, ci.Key, ci.Value);
        }
    }
}
