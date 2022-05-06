/***********************************************
 * 名称：日志系统模型
 * 功能/用途：构建一个强大的日志跟踪的系统模型
 * 目标：构建一个强大的日志跟踪的系统模型
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2017-8-19
 * 修改记录：
 * 1.增加基本日志功能 2017-8-19
 * 2.丰富Print方法，修改代理为系统 Action类型 2017-9-7
 * 
 **********************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class LogSystemInstance
{
    List<ILogTarget> mListLogTarget = new List<ILogTarget>();

    static Control sControl = null;

    public static bool IsSimple = false;

    public void AppendLogTarget(ILogTarget logTarget) {
        mListLogTarget.Add(logTarget);

        if (logTarget is CtrlLogTarget) {
            sControl = (logTarget as CtrlLogTarget).GetControl();
        }
    }

    //public void AppendLogTarget(System.Windows.Forms.RichTextBox rtb) {
    //    RichTextBoxLogTarget logTarget = new RichTextBoxLogTarget(rtb);
    //    AppendLogTarget(logTarget);
    //}

    public void SetLogTarget(ILogTarget logTarget) {
        mListLogTarget.Clear();
        //sListLogTarget.Add(logTarget);
        AppendLogTarget(logTarget);
    }

    //public void SetLogTarget(System.Windows.Forms.RichTextBox rtb) {
    //    mListLogTarget.Clear();
    //    AppendLogTarget(rtb);
    //}

    protected void OuputAllLogTarget(string message) {
        int count = mListLogTarget.Count;
        for (int i = 0; i < count; i++) {
            mListLogTarget[i].OutputMessage(message);
        }
    }

    /// <summary>
    /// 实例日志系统
    /// </summary>
    static Hashtable sHashTable = new Hashtable();
    public static LogSystemInstance Instance(object instance)
    {
        return new LogSystemInstance();
    }

    /// <summary>
    /// 基本日志打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <returns>用于优化流程代码可读性，无意义</returns>
    public bool Print(string message) {
        string detailMsg = GetAttachInfo(null) +  message + "\r\n";
        OuputAllLogTarget(detailMsg);
        return false;
    }

    /// <summary>
    /// 带堆栈级别的的打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="level"></param>
    public void Print(string message, int level) {
        string detailMsg = GetAttachInfo(level) + message + "\r\n";
        OuputAllLogTarget(detailMsg);
    }

    /// <summary>
    /// 带返回值的日志打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="returnValue"></param>
    /// <returns></returns>
    public bool Print(string message, bool returnValue)
    {
        Print(message);
        return returnValue;
    }

    /// <summary>
    /// 带返回值，带回调函数的日志打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="returnValue"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public bool Print(string message,bool returnValue, /*NoReturnValueNoArgumentMethod*/Action  callBack) {
        Print(message);

        if (sControl != null)
        {
            sControl.Invoke(callBack);
        }
        else
            callBack.Invoke();
        return returnValue;
    }

    /// <summary>
    /// 带Action日志打印，用于优化流程代码的可读性
    /// </summary>
    /// <param name="message"></param>
    /// <param name="callBack"></param>
    /// <returns>用于优化流程代码的可读性，无意义</returns>
    public bool Print(string message, /*NoReturnValueNoArgumentMethod*/Action callBack) {
        Print(message);

        if (sControl != null && sControl.InvokeRequired) {
            sControl.Invoke(callBack);
        }else
            callBack.Invoke();
        return false;
    }

    /// <summary>
    /// 带Action，带参数日志打印,用于优化流程代码的可读性
    /// </summary>
    /// <param name="message"></param>
    /// <param name="returnValue"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public bool Print(string message, bool returnValue, /*NoReturnValueWithBoolArgMethod*/Action<bool> callback) {
        Print(message);
        if (sControl != null && sControl.InvokeRequired)
        {
            sControl.Invoke(callback, returnValue);
        }
        else
            callback.Invoke(returnValue);
        return returnValue;
    }

    private static string GetAttachInfo(int? level) {
        if (IsSimple)
            return "";

        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        System.Diagnostics.StackFrame sf = st.GetFrame((level == null) ? 2 :level.Value);

        //测试代码
        //System.Diagnostics.StackFrame[] frames = st.GetFrames();
        //System.Diagnostics.Trace.WriteLine("length = " + frames.Length);
        //for (int i = 0; i < frames.Length; i++)
        //{
        //    System.Diagnostics.Trace.WriteLine("name = " + frames[i].GetMethod().Name);
        //}

        StringBuilder sb = new StringBuilder();
        sb.Append(GetSimpleName(sf.GetFileName())+"!");//增加文件名
        MethodBase mb = sf.GetMethod();
        //System.Diagnostics.Trace.WriteLine("Assembly = " + mb.ReflectedType.Assembly.GetName().Name);

        sb.Append(mb.ReflectedType.Name+"." + mb.Name + GetParameters(mb));//增加函数名字
        sb.Append("," + sf.GetFileLineNumber());//增加文件行和列
        sb.Append(",tid=" + Thread.CurrentThread.ManagedThreadId +":");//增加线程ID
        return sb.ToString();
    }

    private static string GetSimpleName(string fn)
    {
        if (fn == null)
            return "";
        string ret = fn.Substring(fn.LastIndexOf('\\') + 1);
        return ret;
    }

    private static string GetParameters(MethodBase method) {
        StringBuilder strParams = new StringBuilder("(");
        ParameterInfo[] pis = method.GetParameters();
        for (int i = 0; i < pis.Length; i++)
        {
            strParams.Append(pis[i].Name + ",");
        }

        if (pis.Length == 0)
        {
            strParams.Append(")");

        }
        else {
            strParams[strParams.Length - 1] = ')';
        }

        return strParams.ToString();
    }

    static bool? _IsDebuggale = null;
    /// <summary>
    ///  判断当前执行程序是Debug版本还是Release版本
    /// </summary>
    public static bool IsDebuggable {
        get {
            if (_IsDebuggale != null)
                return _IsDebuggale.Value;

            //var assembly = System.Reflection.Assembly.LoadFile(FileName);
            var assembly = Assembly.GetEntryAssembly();
            //Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes(typeof(System.Diagnostics.DebuggableAttribute), false);
            if (attributes.Length > 0)
            {
                var debuggable = attributes[0] as System.Diagnostics.DebuggableAttribute;
                if (debuggable != null)
                    _IsDebuggale = (debuggable.DebuggingFlags & System.Diagnostics.DebuggableAttribute.DebuggingModes.Default) == System.Diagnostics.DebuggableAttribute.DebuggingModes.Default;
                else
                    _IsDebuggale =  false;
            }
            else
                _IsDebuggale = false;

            return _IsDebuggale.Value;
        }
    }
}

