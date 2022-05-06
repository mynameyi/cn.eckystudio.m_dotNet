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
 * 3.增加实例日志系统方法 Instance(object key)
 * 备注：
 * #日志包含的信息点：
 * 基本信息：时间、所属领域、逻辑层次、日志体、线程ID，扩展信息：所属文件（含所在行列）、方法（含参数）
 * 
 **********************************************/
using EckyStudio.M.BaseModel.log;
using EckyStudio.M.BaseModel.log.email;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class LogSystem
{
    //public delegate void NoReturnValueNoArgumentMethod();//无返回值，无参数代理,也可使用系统的Action代理
    //public delegate void NoReturnValueWithBoolArgMethod(bool trueOrFalse);//无返回值，带布尔参数代理,也可使用Action<bool>
    static List<LogTargetModel> sListLogTarget = new List<LogTargetModel>();

    //private static ILogTarget sDefaultLogTarget = new DefaultLogTarget();
    private static LogTargetModel sDefaultLogTarget = new TextLogTarget();

    static Control sControl = null;
    public static bool IsSimple = false;

    //日志缓冲区
    private static LogTail sLogTailCache = new LogTail(24*1024);//默认大小为24K

    private static ExceptionReporter sReporter = new ExceptionReporter();

    static LogSystem(){
        sListLogTarget.Add(sDefaultLogTarget);
        sReporter.Start();
    }

    #region ----------------------日志输出目标相关功能----------------------
    public static void AppendLogTarget(LogTargetModel logTarget) {
        sListLogTarget.Add(logTarget);

        if (logTarget is CtrlLogTarget) {
            sControl = (logTarget as CtrlLogTarget).GetControl();
        }
    }

    public static void AppendLogTarget(System.Windows.Forms.RichTextBox rtb) {
        RichTextBoxLogTarget logTarget = new RichTextBoxLogTarget(rtb);
        AppendLogTarget(logTarget);
    }

    public static void SetLogTarget(LogTargetModel logTarget) {
        sListLogTarget.Clear();
        //sListLogTarget.Add(logTarget);
        AppendLogTarget(logTarget);
    }

    public static void SetLogTarget(System.Windows.Forms.RichTextBox rtb) {
        sListLogTarget.Clear();
        AppendLogTarget(rtb);
    }

    [Obsolete("已过时，最新使用 OuputAllLogTarget(int logicLevel, string message)")]
    protected static void OuputAllLogTarget(string message) {
        int count = sListLogTarget.Count;
        for (int i = 0; i < count; i++) {
            sListLogTarget[i].OutputMessage(message);
        }
    }

    protected static void OuputAllLogTarget(int logicLevel, string message) {
        lock (sDefaultLogTarget)
        {
            string time = "", domain = "", threadId = Thread.CurrentThread.ManagedThreadId.ToString(),extInfo="";
            //时间,精确到毫秒
            time = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //sf.GetMethod().DeclaringType.Name;
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.StackFrame[] frames = st.GetFrames();
            int index;
            System.Diagnostics.StackFrame sf = FindValidEntryFrame(/*logicLevel == LogLevel.LEVEL_FCM ?(IsDebuggable?2:1):0,*/frames,out index);

            //附加堆栈信息
            if (logicLevel == LogLevel.LEVEL_UNKNOW_ERROR)
            {
                message += GenerateStackTrace(frames,index);
            }

            MethodBase mb = sf.GetMethod();

            //所属领域
            domain = mb.DeclaringType.Name + "." + mb.Name;

            //获取文件扩展信息
            extInfo = GetSimpleName(sf.GetFileName()) + "!" + mb.ReflectedType.Name + "." + mb.Name + GetParameters(mb) + "," + sf.GetFileLineNumber();

            //输出到缓冲区
            sLogTailCache.Append(time, domain, logicLevel.ToString(), message, threadId, extInfo);

            //发送异常报告
            if (logicLevel == LogLevel.LEVEL_UNKNOW_ERROR) {
                sReporter.AddReport(sLogTailCache.Tail);
            }

            int count = sListLogTarget.Count;
            for (int i = 0; i < count; i++)
            {
                sListLogTarget[i].OutputMessage(time, domain, logicLevel.ToString(), message, threadId , extInfo);
            }
        }
    }
    #endregion

    #region ----------------实例日志系统功能--------------------------------
    /// <summary>
    /// 实例日志系统
    /// </summary>
    static Hashtable sHashTable = new Hashtable();
    public static LogSystemInstance Instance(object key)
    {
        LogSystemInstance lsi;
        if (sHashTable.Contains(key))
        {
            lsi = (LogSystemInstance)sHashTable[key];
        }
        else {
            lsi = new LogSystemInstance();
            sHashTable.Add(key,lsi);
        }
        return lsi;
    }

    public static void RemoveLogSystemInstance(object key) {
        sHashTable.Remove(key);
    }
    #endregion


    /// <summary>
    /// 基本日志打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <returns>用于优化流程代码可读性，无意义</returns>
    public static bool Print(string message) {
        //string detailMsg = GetAttachInfo(null) +  message;
        OuputAllLogTarget(LogLevel.LEVEL_GENERAL, message);
        return false;
    }

    /// <summary>
    /// 带堆栈级别的的打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="level"></param>
    [Obsolete]
    public static void Print(string message, int level) {
        string detailMsg = GetAttachInfo(level) + message;
        OuputAllLogTarget(LogLevel.LEVEL_GENERAL,detailMsg);
    }

    /// <summary>
    /// 带逻辑级别的打印函数
    /// </summary>
    /// <param name="logicLevel">逻辑级别</param>
    /// <param name="message">打印的消息</param>
    public static void Print(int logicLevel, string message) {
        OuputAllLogTarget(logicLevel, message);
    }

    /// <summary>
    /// 带返回值的日志打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="returnValue"></param>
    /// <returns></returns>
    public static bool Print(string message, bool returnValue)
    {
        Print(message);
        return returnValue;
    }

    /// <summary>
    /// 带返回值，带回调函数的日志打印函数
    /// </summary>
    /// <param name="message"></param>
    /// <param name="returnValue"></param>
    /// <param name="callBack">执行回掉函数，用于优化代码可读性</param>
    /// <returns>用于优化代码可读性</returns>
    public static bool Print(string message,bool returnValue, /*NoReturnValueNoArgumentMethod*/Action  callBack) {
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
    public static bool Print(string message, /*NoReturnValueNoArgumentMethod*/Action callBack) {
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
    public static bool Print(string message, bool returnValue, /*NoReturnValueWithBoolArgMethod*/Action<bool> callback) {
        Print(message);
        if (sControl != null && sControl.InvokeRequired)
        {
            sControl.Invoke(callback, returnValue);
        }
        else
            callback.Invoke(returnValue);
        return returnValue;
    }

    /// <summary>
    /// 处理致命异常，致命异常表示该异常如果发生，程序无法继续运行
    /// </summary>
    /// <param name="ex"></param>
    public static void HandleFatalException(Exception ex) {
        StringBuilder sb = new StringBuilder();
        sb.Append("\r\n************** 异常堆栈信息 **************\r\n");
        sb.Append(ex.ToString() + "\r\n");

        //sb.Append(GenerateStackTrace(null,out index));
        //string msg = ex.ToString();

        //ex.StackTrace
        //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(ex);       
        //sb.Append(st.FrameCount);
        //sb.Append(st.ToString());

        OuputAllLogTarget(LogLevel.LEVEL_UNKNOW_ERROR, sb.ToString());
        if (sDefaultLogTarget != null) {
            sDefaultLogTarget.Notify(0);
        }
        MessageBox.Show("Application will exit!", "Occure fatal error",MessageBoxButtons.OK,MessageBoxIcon.Error);
        Application.Exit();
        return;
    }

    public static void HandleGenernalException(Exception ex) {
        ;
    }

    private static string GetAttachInfo(int? level) {
        if (IsSimple)
            return "";

        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
        System.Diagnostics.StackFrame sf = st.GetFrame((level == null) ? 0 :level.Value);

        //测试代码
        //System.Diagnostics.StackFrame[] frames = st.GetFrames();
        //System.Diagnostics.Trace.WriteLine("length = " + frames.Length);
        //for (int i = 0; i < frames.Length; i++)
        //{
        //    System.Diagnostics.Trace.WriteLine("name = " + frames[i].GetMethod().Name + ","+ frames[i].GetMethod().DeclaringType.IsEquivalentTo(typeof(LogSystem)));
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

    /// <summary>
    /// 查找有效的入口Frame,用于提取有用入口函数，需要过滤LogSystem本身函数，以及接口，在debug版本程序中，会有接口类
    /// </summary>
    /// <param name="frames"></param>
    /// <returns></returns>
    private static System.Diagnostics.StackFrame FindValidEntryFrame(/*int offsetLevel,*/System.Diagnostics.StackFrame[] frames,out int index) {
        index = 0;
        System.Diagnostics.StackFrame sf = null;
        for (int i = 0; i < frames.Length; i++) {
            sf = frames[i];
            //Type t = sf.GetMethod().DeclaringType;
            //if (!t.IsEquivalentTo(typeof(LogSystem)) &&/* !t.IsInterface*/!t.IsAbstract && t.Name != "DefaultTracer") {

            //    //if (offsetLevel > 0)
            //    //    sf = frames[i + offsetLevel];
            //    break;
            //}
            if (IsValidFrame(sf))
            {
                index = i;
                break;
            }
        }
        return sf;
    }

    private static bool IsValidFrame(System.Diagnostics.StackFrame sf) {
        Type t = sf.GetMethod().DeclaringType;
        if (!t.IsEquivalentTo(typeof(LogSystem)) &&/* !t.IsInterface*/!t.IsAbstract && t.Name != "DefaultTracer")
        {
            //if (offsetLevel > 0)
            //    sf = frames[i + offsetLevel];
            return true;
        }
        return false;
    }

    private static string GenerateStackTrace(System.Diagnostics.StackFrame[] frames,int index) {
        int iEntryPos = index;
        if (frames == null)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            frames = st.GetFrames();

            iEntryPos = -1;
            for (int i = 0; i < frames.Length; i++)
            {
                if (IsValidFrame(frames[i]))
                {
                    iEntryPos = i;
                    break;
                }
            }

            if (iEntryPos == -1)
                return "Stack Exception!!";
        }

        StringBuilder sb = new StringBuilder();
        for (int i = iEntryPos + 1; i < frames.Length && i < 13; i++) {
            //sb.Append("   "+string.Format("{0}.{1}: {2}", frames[i].GetMethod().DeclaringType.Name, frames[i].GetMethod().Name , "\r\n"));
            sb.Append(new System.Diagnostics.StackTrace(frames[i]).ToString());
        }
        return sb.ToString();
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


    public static class LogLevel {
        /// <summary>
        /// 未知错误
        /// </summary>
        public static int LEVEL_UNKNOW_ERROR = 100;
        /// <summary>
        /// 未确定错误
        /// </summary>
        public static int LEVEL_POSSIBLE_ERROR = 28;
        /// <summary>
        /// 普通级别
        /// </summary>
        public static int LEVEL_GENERAL = 21;
        /// <summary>
        /// 组件级别
        /// </summary>
        public static int LEVEL_FCM = 10;
    }
}

