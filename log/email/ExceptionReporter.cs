using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EckyStudio.M.BaseModel.log.email
{
    public class ExceptionReporter
    {
        Thread mTaskThread;
        BlockingQueue<TaskInfo> mTaskQueue = new BlockingQueue<TaskInfo>();

        EmailHelper mEmail;

        public ExceptionReporter() {
            mTaskThread = new Thread(HandleTask);
            mTaskThread.IsBackground = true;

            mEmail = new EmailHelper();
            mEmail.SetSmtpServer("smtp.163.com", "25", "cdstest21@163.com", "abc123456", false, false);
        }
        
        public void Start() {
            mTaskThread.Start();
        }

        public void AddReport(string msg) {
            AddReport("Unknown Error!!",msg);
        }

        public void AddReport(string title, string msg) {
            TaskInfo ti = new TaskInfo();
            ti.Title = title;
            ti.Message = msg;
            mTaskQueue.Put(ti);
        }

        private void HandleTask() {
            while (true) {
                TaskInfo info = mTaskQueue.Take();
                if (WindowsUtils.IsNetworkAvailable())
                {
                    try
                    {
                        mEmail.Send(info.Title, info.Message, "mynameyi@qq.com");

                        string[] files = Directory.GetFiles("log\\unsends\\");
                        for (int i = 0; i < files.Length; i++) {
                            FileOperator fo = new FileOperator(files[i]);
                            string strContent = fo.GetFileContent();
                            string title = strContent.Remove(0, strContent.LastIndexOf("\r\n"));
                            mEmail.Send(title, strContent, "mynameyi@qq.com");
                        }
                    }
                    catch (Exception ex) {
                        SaveToFile(info);
                    }
                }
                else {
                    SaveToFile(info);
                }  
            }
        }

        private void SaveToFile(TaskInfo info) {
            FileOperator file = new FileOperator(GenerateFileName());
            file.WriteLine(info.Title);
            file.Write(info.Message);
            file.Dispose();
        }


        private string GenerateFileName()
        {
            return "log\\unsends\\email-" + System.DateTime.Now.ToString("yyyyMMddHHmmss.fff") + ".txt";
        }
    }
}
