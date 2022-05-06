/***********************************************
 * 名称：多语言管理模型
 * 功能/用途：定义一种多语言管理模型，方便语言的切换、维护
 * 目标：建立一套最简洁、最快速的多语言管理机制
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间/首次完成时间：2017-10-21
 * 修改记录：
 * 
 * 备注：
 * 需要优化问题：
 * ??默认值如何存储，计划通过特性实例化。
 * 遵循约定：
 * 1.模型通过成员变量与ini格式文件的键值对相关联
 * 2.成员变量名需要与键名保持一致，推荐 名称使用全大写，下划线分隔形式
 * 3.
 **************************************************/
using EckyLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;

namespace EckyStudio.M.BaseModel.DataManagementModel
{
    public abstract class MultiLanguageManagementModel
    {
        const string SETCTION = "LANGUAGE";

        public delegate void LanguageSwitchedHandler();
        /// <summary>
        /// 语言切换事件，通知界面刷新
        /// </summary>
        public event LanguageSwitchedHandler LanguageSwitched;

        public void SwitchTo(string fileName) {
            Config3_0 c = new Config3_0(fileName);

            var fields = GetType().GetFields();
            //foreach (var f in fields) {
                foreach (var a in fields) {
                    //Attribute.GetCustomAttribute()
                    if (Attribute.IsDefined(a, typeof(DataItemAttribute)))
                    {
                         object v = a.GetValue(this);//默认值为原来使用的值
                         a.SetValue(this, c.ReadString(SETCTION, a.Name, v.ToString()));
                    }
                    //4.5 code
                    //foreach (var ca in a.CustomAttributes) {
                    //    if (ca.AttributeType == typeof(DataItemAttribute)) {

                    //    }
                    //}
                }
            //}
            c.Dispose();
            if (LanguageSwitched != null) {
                LanguageSwitched.Invoke();
            }      
        }
    }
}
