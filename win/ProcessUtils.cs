using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


public class ProcessUtils
{
    [DllImport("User32.dll")]
    static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    [DllImport("User32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

    static System.Threading.Mutex mx;
    static bool PreventReenterProcess(string ProcessMark)
    {
        bool IsSucess;
        mx = new System.Threading.Mutex(true, @"Global\" + ProcessMark, out IsSucess);
        if (IsSucess)
        {
            return false;
        }

        string CurrentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        System.Diagnostics.Process[] AllProcess = System.Diagnostics.Process.GetProcesses();

        System.Diagnostics.Process HasExistedProcess = null;
        int iProcessCount = AllProcess.Length;
        for (int i = 0; i < iProcessCount; i++)
        {
            if (AllProcess[i].ProcessName == CurrentProcessName && AllProcess[i].Id != System.Diagnostics.Process.GetCurrentProcess().Id)
            {
                HasExistedProcess = AllProcess[i];
                break;
            }
        }
        //MessageBox.Show("程序已经运行");

        IntPtr hWnd = HasExistedProcess.MainWindowHandle;
        //SendMessage(hWnd, 0x1C, 0x1, 0x0);
        //SendMessage(hWnd, 0x86, 0x200001, 0x0);
        //SendMessage(hWnd, 0x6, 0x200001, 0x0);
        //SendMessage(hWnd, 0x112, 0xf120, 0x0);
        PostMessage(hWnd, 0x112, 0xf120, 0x0);

        //Form.FromHandle(HasExistedProcess.MainWindowHandle).Show();
        //Application.Exit();
        return true;
    }

    static string[] sPaths;
    public static void AppendSearchPath(params string[] paths)
    {
        sPaths = paths;

        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
    }

    public static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        try
        {
            AssemblyName name = new AssemblyName(args.Name);

            string expectedFileName = name.Name + ".dll";
            string rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return LoadAssembly(rootDir, expectedFileName, sPaths);
        }
        catch
        {
            return null;
        }
    }

    private static Assembly LoadAssembly(string rootDir, string fileName, params string[] directories)
    {
        if (directories == null)
            return null;

        foreach (string directory in directories)
        {
            string path = Path.Combine(rootDir, directory, fileName);
            if (File.Exists(path))
                return Assembly.LoadFrom(path);
        }
        return null;
    }


    /// <summary>
    /// 运行cmd命令
    /// 不显示命令窗口
    /// </summary>
    /// <param name="cmdExe">指定应用程序的完整路径</param>
    /// <param name="cmdStr">执行命令行参数</param>
    public static string RunCmd(string cmdExe, string cmdStr)
    {
        //cmdExe = Environment.CurrentDirectory + "\\adbfile\\" + cmdExe;
        string result = null;
        try
        {
            using (Process myPro = new Process())
            {
                myPro.StartInfo.FileName = "cmd.exe";
                myPro.StartInfo.UseShellExecute = false;
                myPro.StartInfo.RedirectStandardInput = true;
                myPro.StartInfo.RedirectStandardOutput = true;
                myPro.StartInfo.RedirectStandardError = true;
                myPro.StartInfo.CreateNoWindow = true;
                myPro.Start();

                myPro.StandardOutput.DiscardBufferedData();

                //string str2 = string.Format(@"""{0}"" {1}", cmdExe, cmdStr);
                //myPro.StandardInput.WriteLine(str2);
                //myPro.StandardInput.AutoFlush = true;
                //result = myPro.StandardOutput.ReadToEnd();

                //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                string str = string.Format(@"""{0}"" {1} {2}", cmdExe, cmdStr, "&exit");
                myPro.StandardInput.WriteLine(str);
                myPro.StandardInput.AutoFlush = true;
                result = myPro.StandardOutput.ReadToEnd();
                result = result.Substring(result.IndexOf(str) + str.Length);

                //myPro.StandardOutput.BaseStream.Seek(0, SeekOrigin.Begin);


                myPro.WaitForExit();
                myPro.Close();
            }
        }
        catch
        {

        }
        return result;
    }
}

