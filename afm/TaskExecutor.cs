/***********************************************
 * 功能：自动化流程执行器
 * 构建目标：用于约定自动化流程的执行
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2014-12-14
 * 最后修改时间：2016-9-15
 * 修改信息：
 * 1.增加异常处理功能 2016-9-17
 * 2.增加初始化和结束任务 SetInitTask()、SetDestroyTask() 2019-9-12
 * 备注：
 **********************************************/
using EckyStudio.M.BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EckyStudio.M.AutomationFlowModel
{
    public class TaskExecutor
    {
        public OnExceptionHandleMethods OnExceptionHandleMethod = OnExceptionHandleMethods.RETRY;

        private bool mIsExecuting = false;
        ITracer mTracer;
        AutoResetEvent mAutoRestEvent = new AutoResetEvent(false);

        //TaskContext mContext;
        SortedList<string,object> mTaskPipe;

        static TaskExecutor()
        { 
            //测试静态构造函数
        }

        public TaskExecutor(ITracer tracer)
        {
            SetTracer(tracer);

            //mContext = new TaskContext();//创建任务上下文
            mTaskPipe = new SortedList<string, object>();//创建任务管道
        }

        private Queue<Task> mExecutedTasks = new Queue<Task>();//已执行的task列表
        private Queue<Task> mTasks = new Queue<Task>();//未执行的task列表
        private Task mTask = null;//正在执行的task

        private Action<SortedList<string, object>> mInitTask;
        private Control mInitCtrl;

        private Action<SortedList<string, object>> mEndTask;
        private Control mEndCtrl;


        /// <summary>
        /// 添加任务到队列
        /// </summary>
        /// <param name="task"></param>
        public void Enter(Task task)
        {
            if (mTracer != null)
            {
                task.SetTracer(mTracer);
            }
            //task.mContext = mContext;//添加上下文到Task
            task.mTaskPipe = mTaskPipe;
            mTasks.Enqueue(task);
        }

        /// <summary>
        /// 添加多个任务到队列
        /// </summary>
        /// <param name="tasks"></param>
        public void Enter(params Task[] tasks) {
            if (tasks == null)
                return;

            foreach (Task t in tasks) {
                Enter(t);//添加到队列
            }
        }

        /// <summary>
        /// 设置初始化任务
        /// </summary>
        /// <param name="a"></param>
        /// <param name="c"></param>
        public void SetInitTask(Action<SortedList<string,object>> a, Control c = null)
        {
            mInitCtrl = c;
            mInitTask = a;
        }

        public void SetEndTask(Action<SortedList<string, object>> a, Control c = null)
        {
            mEndCtrl = c;
            mEndTask = a;
        }

        private void ExeSpecialTask(Action<SortedList<string, object>> a, Control c)
        {
            if (c != null && c.InvokeRequired)
            {
                c.Invoke(/*new Action(() => { a.Invoke(mTaskPipe); })*/ a,mTaskPipe  /*new Action(()=> { ExeSpecialTask(a,c); }))*/);
                return;
            }
            a.Invoke(mTaskPipe);
        }

        /// <summary>
        /// 异步执行任务
        /// </summary>
        public void ExecuteAsyn()
        {
            lock (this)
            {
                if (mIsExecuting)
                {
                    mTracer.Print("任务正在执行中，请稍后再试！！");
                    return;
                }

                mIsExecuting = true;
            }

            Thread thread = new Thread(() => {
                if (mTasks.Count == 0)
                {
                    lock (this)
                    {
                        mIsExecuting = false;
                    }
                    return;
                }
            RetryTaskChain:

                ExeSpecialTask(mInitTask, mInitCtrl);//执行初始化任务

                Task task = null;
                int status;
                try
                {
                    do
                    {
                        task = mTasks.Dequeue();               
                    Retry:
                        task.prepare();
                        task.SetExecutedStatus(TaskExecutedStatus.EXECUTING);
                        status = task.execute();
                        switch (status)
                        {
                            case Task.TASK_RESULT_FATAL_ERROR:
                                task.SetExecutedStatus(TaskExecutedStatus.EXECUTE_FAIL);
                                task.report();
                                mTask = task;
                                goto Exit;
                            case Task.TASK_RESULT_GENERAL_ERROR_NEED_MANUAL_INTERVETION:
                                task.SetExecutedStatus(TaskExecutedStatus.EXECUTE_FAIL);
                                task.wait();
                                break;
                            case Task.TASK_RESULT_GENERAL_ERROR_IGNORABLE:
                                task.SetExecutedStatus(TaskExecutedStatus.EXECUTE_FAIL);
                                mExecutedTasks.Enqueue(task);
                                break;
                            case Task.TASK_RESULT_SUCCESS:
                                task.SetExecutedStatus(TaskExecutedStatus.EXECUTE_SUCCEED);
                                mExecutedTasks.Enqueue(task);
                                break;
                            default:
                                Thread.Sleep(status);
                                goto Retry;
                        }

                    } while (mTasks.Count > 0);


                }
                catch (Exception ex)
                {
                    mTracer.OnException(ex);
                    mTask = task;
                }

            Exit:
                ExeSpecialTask(mEndTask, mEndCtrl);//执行结束任务

                mTaskPipe.Clear();//清空任务管道数据

                mTracer.Print("Exit.....");
                if (RebuildTaskSequeue())
                {
                    switch (OnExceptionHandleMethod)
                    {
                        case OnExceptionHandleMethods.RETRY:
                            mTracer.Print("Retry.....");
                            goto RetryTaskChain;
                        case OnExceptionHandleMethods.STOP:
                            break;
                        case OnExceptionHandleMethods.WAIT:
                            mAutoRestEvent.WaitOne();
                            break;
                        default:
                            break;
                    }
                }

                lock (this)
                { 
                    mIsExecuting = false;
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private bool RebuildTaskSequeue()
        {
            bool ret = false;

            if (mTask != null)
            {
                mExecutedTasks.Enqueue(mTask);
                ret = true;
            }

            while(mTasks.Count > 0)
            {
                mExecutedTasks.Enqueue(mTasks.Dequeue());
            }

            Queue<Task> temp = mTasks;
            mTasks = mExecutedTasks;
            mExecutedTasks = temp;
            mExecutedTasks.Clear();         
            mTask = null;

            return ret;
        }

        public void SetTracer(ITracer tracer)
        {
            mTracer = tracer;
        }

        //public const Dictionary<int, string> MAPPING_ERROR_CODE = new Dictionary<int, string> { }
        public static readonly Dictionary<int, string> MAPPING_ERROR_CODE = new Dictionary<int, string>() { 
            {1000,"网络连接失败"},
            {1001,"获取网页数据失败"},
            {1002,"匹配期数错误"},
            {1003,"匹配号码错误"}
        };

        public static string MapErrorString(int ErrorCode, params string[] args)
        { 
            string ret = "";
            if(MAPPING_ERROR_CODE.TryGetValue(ErrorCode,out ret))
            {
                if (args != null)
                {
                    ret = String.Format(ret, args);
                }
            }
            return ret;
        }
    }    
}
