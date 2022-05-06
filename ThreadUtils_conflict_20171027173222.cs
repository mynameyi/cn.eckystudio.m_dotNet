/***********************************************
 * 功能：线程辅助函数
 * 构建目标：封装跨线程操作的相关功能
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2017-10-18
 * 最后修改时间：
 * 修改信息：
 * 1:完成异步执行以及UI线程执行的函数  2017-10-18
 * 2.增加 public static Thread AsynExecute(Func<bool> func) 2017-10-20
 * 备注：
 **********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public static class ThreadUtils
{
    /// <summary>
    /// 异步执行任务
    /// </summary>
    /// <param name="ts"></param>
    /// <returns></returns>
    public static Thread AsynExecute(ThreadStart ts) {
        Thread t = new Thread(ts);
        t.IsBackground = true;
        t.Start();
        return t;
    }

    /// <summary>
    /// 带布尔返回值的异步执行函数，主要�主要用于优化流程代码的可读性，返回值无意义
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Thread AsynExecuteWithResult(Func<bool> func) {
        Thread t = new Thread(()=> {
            func.Invoke();
        });

        t.IsBackground = true;
        t.Start();
        return t;
    }

    /// <summary>
    /// 带一个参数异步执行函数
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <param name="a"></param>
    /// <param name="f"></param>
    /// <returns></returns>
    public static Thread AsynExecute<F>(Action<F> a, F f) {
        Thread t = new Thread(()=> {
            a.Invoke(f);
        });
        t.IsBackground = true;
        t.Start();
        return t;
    }

    /// <summary>
    /// 带两个函数的异步执行函数
    /// </summary>
    /// <typeparam name="F1"></typeparam>
    /// <typeparam name="F2"></typeparam>
    /// <param name="a"></param>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    /// <returns></returns>
    public static Thread AsynExecute<F1, F2>(Action<F1,F2> a, F1 f1,F2 f2) {
        Thread t = new Thread(() => {
            a.Invoke(f1,f2);
        });
        t.IsBackground = true;
        t.Start();
        return t;
    }

    /// <summary>
    /// UI安全执行函数
    /// </summary>
    /// <param name="c"></param>
    /// <param name="a"></param>
    public static void SafeExecute(Control c,Action a) {
        if (c.InvokeRequired)
        {
            c.Invoke(a);
        }
        else {
            a.Invoke();
        }
    }
    /// <summary>
    /// UI安全执行函数,带一参数
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <param name="c"></param>
    /// <param name="a"></param>
    /// <param name="f"></param>
    public static void SafeExecute<F>(Control c, Action<F> a,F f) {
        if (c.InvokeRequired)
        {
            c.Invoke(a,f);
        }
        else
        {
            a.Invoke(f);
        }
    }

    /// <summary>
    /// UI安全执行函数，带两个参数
    /// </summary>
    /// <typeparam name="F1"></typeparam>
    /// <typeparam name="F2"></typeparam>
    /// <param name="c"></param>
    /// <param name="a"></param>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    public static void SafeExecute<F1,F2>(Control c, Action<F1,F2> a, F1 f1,F2 f2)
    {
        if (c.InvokeRequired)
        {
            c.Invoke(a, f1,f2);
        }
        else
        {
            a.Invoke(f1,f2);
        }
    }

    /// <summary>
    /// UI安全执行函数，异步执行
    /// </summary>
    /// <param name="c"></param>
    /// <param name="a"></param>
    public static void SafeAsyncExecute(Control c, Action a) {
        c.BeginInvoke(a);
    }

    /// <summary>
    /// UI安全执行函数，异步执行，带一个函数
    /// </summary>
    /// <typeparam name="F"></typeparam>
    /// <param name="c"></param>
    /// <param name="a"></param>
    /// <param name="f"></param>
    public static void SafeAsyncExecute<F>(Control c, Action<F> a, F f) {
        c.BeginInvoke(a, f);
    }

    /// <summary>
    /// UI安全执行函数，异步执行，带两个函数
    /// </summary>
    /// <typeparam name="F1"></typeparam>
    /// <typeparam name="F2"></typeparam>
    /// <param name="c"></param>
    /// <param name="a"></param>
    /// <param name="f1"></param>
    /// <param name="f2"></param>
    public static void SafeAsyncExecute<F1, F2>(Control c, Action<F1, F2> a, F1 f1, F2 f2) {
        c.BeginInvoke(a, f1, f2);
    }