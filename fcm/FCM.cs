/***********************************************
 * 名称：功能组件模型（Function/Feature Component Model）
 * 功能/用途：
 * 构建目标：用于约定功能组件的基本模型，其主要目标用于跟踪组件的内部运行情况，便于异常的处理与跟踪。
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2014-12-14
 * 修改记录：
 * 1:设计了基本功能组件的架构（2014-12-14）
 * 2.添加SetTracer函数（2016-9-15）
 * 3.增加Ecky日志系统的默认跟踪器 DefaultTracer 2017-9-6
 * 4.增加Print函数重载功能，丰富Print方法种类 2017-9-6
 * 5.增加  LoadErrorCodeMappingTable(out Dictionary<int, string> table) 函数 2017-10-24
 * 备注：
 **********************************************/
using EckyStudio.M.BaseModel;
using System;
using System.Collections.Generic;
using static LogSystem;

namespace EckyStudio.M.FunctionComponentModel
{
    public abstract class FCM
    {
        /// <summary>
        /// 错误码，用于指示执行功能组件相关方法过程中发生的错误
        /// </summary>
        private int mErrorCode;
        /// <summary>
        /// 错误对应的相关参数
        /// </summary>
        private object[] mErrorArguments;
        /// <summary>
        /// 错误码的映射表，使用Dictionary的目的的是加速映射表的搜索速度
        /// </summary>
        protected readonly Dictionary<int, string> ERROR_CODE_MAPPING_TABLE = new Dictionary<int, string>()
        {
            {111,""}
        };
        /// <summary>
        /// 日志跟踪器
        /// </summary>
        private ITracer mTracer;

        public FCM():this(new DefaultTracer()) {
        }

        public FCM(ITracer tracer)
        {
            mTracer = tracer;
            InitErrorCodeMappingTable(out ERROR_CODE_MAPPING_TABLE);//加载默认的错误码映射表
        }

        protected void LoadErrorCodeMappingTable(Tuple<int, string>[] table) {
            if (table == null)
                return;

            for (int i = 0; i < table.Length; i++) {
                ERROR_CODE_MAPPING_TABLE.Add(table[i].Item1, table[i].Item2);
            }
        }

        /// <summary>
        /// 初始化错误映射表，主要用于派生类初始化错误映射表
        /// </summary>
        /// <param name="table">输出已经初始化的错误映射表</param>
        protected virtual void InitErrorCodeMappingTable(out Dictionary<int, string> table) {
            table = new Dictionary<int, string>();
        }

        protected void SetErrorCode(int errorCode, params object[] errorArgus)
        {
            mErrorCode = errorCode;
            mErrorArguments = errorArgus;
        }

        protected void SetErrorCodeAndPrint(int errorCode, params object[] errorArgus)
        {
            SetErrorCode(errorCode, errorArgus);
            Print();
        }

        public virtual int GetErrorCode()
        {
            return mErrorCode;
        }

        public virtual string GetErrorMessage()
        {
            string ret;
            if (ERROR_CODE_MAPPING_TABLE != null)
            {
                if (ERROR_CODE_MAPPING_TABLE.TryGetValue(mErrorCode, out ret))
                {
                    if (mErrorArguments != null)
                    {
                        ret = String.Format(ret, mErrorArguments);
                    }
                }
                ret = "错误" + mErrorCode + "：" + ret;
            }
            else
            {
                ret = "";
            }
            return ret;
        }

        //日志打印类函数
        protected void Print()
        {
            Print(GetErrorMessage());
        }

        protected void Print(string text)
        {
            if (mTracer != null)
            {
                mTracer.Print(text);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(text);
            }
        }

        protected void Print(string format, params object[] args)
        {
            Print(String.Format(format, args));
        }

        protected bool Print(string text, bool retValue) {
            Print(text);
            return retValue;
        }

        protected void Print(string text , Action callback) {
            Print(text);
            callback.Invoke();
        }

        protected bool Print(string text, bool retValue, Action callback) {
            Print(text);
            callback.Invoke();
            return retValue;
        }

        protected bool Print(string text, bool retValue, Action<bool> callback) {
            Print(text);
            callback.Invoke(retValue);
            return retValue;
        }

        public void SetTracer(ITracer tracer)
        {
            mTracer = tracer;
        }
    }
}

