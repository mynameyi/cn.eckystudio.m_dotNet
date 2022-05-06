/***********************************************
 * 功能：自动化流程的任务模型
 * 构建目标：用于约定自动化流程的执行
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2014-12-14
 * 最后修改时间：2016-9-15
 * 修改信息：
 * 备注：
 * 1.增加任务衔接功能 2016-9-24
 * 2.利用泛型思想，实现外部对象交互：包括控件交互、其他对象交互 2017-10-18
 * 3.增加UI安全执行函数 2017-10-18
 * 4.整理代码，将Task功能归纳为 ：2017-10-18
 *     基本流程功能、
 *     日志功能、
 *     任务间关联衔接功能、
 *     外部对象交互功能（包括UI交互）、
 *     Config加载功能、
 *     计划任务功能 
 *     
 * 5. 增加 依赖性声明功能 2018-7-24
 * 6. 将sTaskPipe变量统一移到TaskContext中管理 2019-9-12
 **********************************************/
using EckyStudio.M.BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EckyStudio.M.AutomationFlowModel
{
    public abstract class Task
    {
        #region task result
        /// <summary>
        /// 发生一般性错误，默认忽略，继续执行下一条任务
        /// </summary>
        public const int TASK_RESULT_GENERAL_ERROR_IGNORABLE = -2;
        /// <summary>
        /// 发生一般性错误，需要人为干预，决定是否需要继续往下执行
        /// </summary>
        public const int TASK_RESULT_GENERAL_ERROR_NEED_MANUAL_INTERVETION = -3;
        /// <summary>
        /// 发生致命错误，不可恢复
        /// </summary>
        public const int TASK_RESULT_FATAL_ERROR = -1;
        /// <summary>
        /// 成功执行任务
        /// </summary>
        public const int TASK_RESULT_SUCCESS = 0;
        #endregion

        protected RetryCounter mCounter = new RetryCounter();
        /// <summary>
        /// 任务最近一次的执行状态
        /// </summary>
        protected TaskExecutedStatus mLastExecutedStatus = TaskExecutedStatus.NEVER_EXECUTED;

        /// <summary>
        /// 执行当前任务
        /// </summary>
        /// <returns>
        /// 返回执行状态值：
        /// TASK_RESULT_GENERAL_ERROR_IGNORABLE(-2):表示执行任务过程中发生可忽略的一般错误但可以继续执行下一任务
        /// TASK_RESULT_GENERAL_ERROR_NEED_MANUAL_INTERVETION(-3) ：需要人为干预决定是否执行下一个任务
        /// TASK_RESULT_FATAL_ERROR(-1)：表示执行任务过程中发生不可恢复错误，发生此种情况时，调用report()函数报告当前错误
        /// TASK_RESULT_SUCCESS(0) ：表示执行任务成功，可以进入下一个任务
        /// 大于0 ：表示重复执行当前任务，返回值的大小表示当前重试任务的等待时间
        /// </returns>
        public abstract int execute();
        /// <summary>
        /// 报告当前当前发生的错误
        /// </summary>
        public abstract void report();
        /// <summary>
        /// 等待进一步的指示
        /// </summary>
        public abstract void wait();
        /// <summary>
        /// 任务准备函数，用于复位或者初始化操作，实际情况可根据task的 mLastExecutedStatus值情况进行处理
        /// </summary>
        public virtual void prepare()
        {
            ;
        }

        public void SetExecutedStatus(TaskExecutedStatus status)
        {
            mLastExecutedStatus = status;
        }

        #region -----日志相关功能，跟踪器相关接口---------
        protected ITracer mTracer;
        protected int mErrorCode = 0;

        public void SetTracer(ITracer tracer)
        {
            mTracer = tracer;
        }

        public void Print(string info)
        {
            string taskName = this.GetType().Name;
            if (mTracer != null)
            {
                mTracer.Print(taskName + ":" + info);
            }
        }
        #endregion

        #region -------- 任务衔接功能 ---------
        /* 任务衔接功能主要是通过在Task创建公共的变量列表用于任务之间的数据交互，
         * 相当于在任务之间建立一条管道。
         */

        /// <summary>
        /// 变量列表，用于任务之间变量传送
        /// </summary>
        ///private static SortedList<string, object> sTaskPipe = new SortedList<string, object>();

        //sTaskPipe统一放置TaskContext中进行管理
        //internal TaskContext mContext; //停用
        internal SortedList<string,object> mTaskPipe; //用于存放任务管道，由任务执行者赋值


        /// <summary>
        /// 添加或者更改一个变量
        /// </summary>
        /// <param name="key">变量名</param>
        /// <param name="value">变量值</param>
        protected void PipePush(string key, object value)
        {
            //throw new NotImplementedException();

            /*
            if (sTaskPipe.ContainsKey(key))
            {
                sTaskPipe[key] = value;
            } else
            {
                sTaskPipe.Add(key, value);
            }*/

            if (mTaskPipe.ContainsKey(key))
            {
                mTaskPipe[key] = value;
            }
            else
            {
                mTaskPipe.Add(key, value);
            }

        }

        protected T PipeGet<T>(string key)
        {
            //throw new NotImplementedException();
            /*
            return (T)sTaskPipe[key];*/

            return (T)mTaskPipe[key];
        }

       protected void PipeClear()
        {
            //throw new NotImplementedException();

            //sTaskPipe.Clear();
            mTaskPipe.Clear();
        }

        protected void PipeRemove(string key)
        {
            //throw new NotImplementedException();
            //sTaskPipe.Remove(key);
            mTaskPipe.Remove(key);
        }

        #endregion

        #region --------UI交互功能、外部对象交互功能 -------------
        //以泛型的思想来实现外部交互功能，泛型类的继承，支持外部对象,参考派生泛型类 Task<T> Task<T1,T2> ....
        protected Control mControl;

        //UI安全执行函数
        /// <summary>
        /// 带一个参数的UI线程执行函数
        /// </summary>
        /// <typeparam name="F"></typeparam>
        /// <param name="m"></param>
        /// <param name="f"></param>
        protected void SafeExecute<F>(Action<F> m, F f)
        {        
            if (mControl != null && mControl.InvokeRequired)
            {
                mControl.Invoke(m, f);
            }
            else
                m.Invoke(f);
        }

        /// <summary>
        /// 带两个参数的UI线程执行函数
        /// </summary>
        /// <typeparam name="F1"></typeparam>
        /// <typeparam name="F2"></typeparam>
        /// <param name="m"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        protected void SafeExecute<F1, F2>(Action<F1, F2> m, F1 f1, F2 f2) {
            if (mControl != null && mControl.InvokeRequired)
            {
                mControl.Invoke(m, f1,f2);
            }
            else
                m.Invoke(f1,f2);
        }

        #endregion
        //配置加载功能，从 Config配置中加载节名为任务名的配置

        //邮件指令伺服系统
        #region ------计划任务功能-------
        #endregion

        #region ------依赖性声明功能---------------
        //为了更贴合实际情况，Task与Task之间通常存在着调用先后或者依赖的问题，为了让Task之间执行有较好的体验，通过增加依赖性声明来约束Task之间的调用顺序
        //如果出现调用顺序或者缺少某个Task,则给出明确提示，便于Task之间的使用
        #endregion
    }


    #region ---------带外部交互对象的Task-----------------
    /// <summary>
    /// 支持外部对象交互，仅一个外部对象
    /// </summary>
    /// <typeparam name="T">外部对象类型</typeparam>
    public abstract class Task<T> :Task
    {
        protected T mExternalObject;
        public Task(T t) {
            mExternalObject = t;

            //如果外部对象为控件，则转换为Control对象供使用
            if (mExternalObject != null && mExternalObject is Control)
            {
                object obj = mExternalObject;//泛型不支持强制转换，需要先转为object,再进行强转
                mControl = (Control)obj;
            }
        }
    }

    /// <summary>
    /// 如果外部对象有控件，必须将控件放在第一个参数
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public abstract class Task<T1, T2> : Task<T1> {
        protected T2 mExternalObject2;

        public Task(T1 t1, T2 t2) : base(t1){
            mExternalObject2 = t2;
        }
    }

    #endregion

    public enum TaskExecutedStatus
    {
        /// <summary>
        /// 任务从未执行
        /// </summary>
        NEVER_EXECUTED,
        /// <summary>
        /// 任务正在执行中（尚未完成执行），如果执行过程钟发生异常，则会停留在该状态
        /// </summary>
        EXECUTING,
        /// <summary>
        /// 任务执行成功
        /// </summary>
        EXECUTE_SUCCEED,//执行成功
        /// <summary>
        /// 任务执行失败
        /// </summary>
        EXECUTE_FAIL,//执行失败
    }
}
