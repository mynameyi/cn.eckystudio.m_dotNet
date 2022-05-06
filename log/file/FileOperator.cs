/***********************************************
 * 功能：文件操作类组件：封装文件操作的基本功能
 * 构建目标：封装文件操作的基本功能，简化操作步骤，自动识别BOM头
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2017-9-20
 * 最后修改时间：2017-9-20
 * 修改信息：
 * 1:完成文件操作基本功能（2017-9-20）
 * 2.增加文件移动功能 MoveTo 2017-10-18
 * 3.增加Cover、Clear函数 2018-5-2
 * 备注：
 **********************************************/
using EckyStudio.M.FunctionComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Log.file
{
    public class FileOperator : FCM
    {
        FileStream mFileStream;
        Encoding mFileEncoding;
        public FileOperator(string fn)
        {
            if (!OpenFile(fn, out mFileStream, out mFileEncoding))
                Print("打开文件失败！！");
        }

        ~FileOperator()
        {
            Dispose();
        }

        public bool Write(string text)
        {
            return Write(text, SeekOrigin.End);
        }

        public bool WriteLine(string text)
        {
            return Write(text + "\r\n");
        }

        public bool Write(string text, SeekOrigin origin)
        {
            if (mFileStream == null)
                return Print("文件未打开！！", false);

            byte[] b = mFileEncoding.GetBytes(text);
            lock (this)
            {
                mFileStream.Seek(0, origin);
                mFileStream.Write(b, 0, b.Length);
                mFileStream.Flush();//更新内容到文件
            }

            return true;
        }

        /// <summary>
        /// 覆盖原内容
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool Cover(string text) {
            if (mFileStream == null)
                return Print("文件未打开！！", false);

            byte[] b = mFileEncoding.GetBytes(text);
            lock (this)
            {
                mFileStream.Seek(0, System.IO.SeekOrigin.Begin);
                mFileStream.Write(b, 0, b.Length);
                mFileStream.SetLength(b.Length);//截取之后的字符
                mFileStream.Flush();//更新内容到文件
            }

            return true;
        }

        public string GetFileContent()
        {
            int size = (int)mFileStream.Length;
            byte[] FileBuffer = new byte[size];
            int count = mFileStream.Read(FileBuffer, 0, size);
            return mFileEncoding.GetString(FileBuffer);
        }

        public long FileLength {
            get {
                return mFileStream.Length;
            }
        }

        public void Flush() {
            mFileStream.Flush();
        }

        public void Dispose()
        {
            try
            {
                mFileStream.Flush();
                mFileStream.Close();
                mFileStream.Dispose();
            }
            catch
            {
                ;
            }
        }

        /// <summary>
        /// 执行MoveTo会关闭文件流
        /// </summary>
        /// <param name="dstDirName">目标目录名字</param>
        public void MoveTo(string dstDirName)
        {
            Dispose();//关闭当前流

            string fn = Path.GetFileName(mFileStream.Name);
            if (!Path.IsPathRooted(dstDirName))
            {
                dstDirName = System.AppDomain.CurrentDomain.BaseDirectory + dstDirName;
            }

            fn = dstDirName + "\\" + fn;
            MakeSurePathOK(fn);

            File.Move(mFileStream.Name, fn);
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return mFileStream.Seek(offset, origin);
        }

        public void Clear() {
            mFileStream.Seek(0, System.IO.SeekOrigin.Begin);
            mFileStream.SetLength(0);
        }

        private static bool OpenFile(string fn, out FileStream fs, out Encoding encoding)
        {
            //--------------------------------------------------------------------------------
            try
            {
                if (!Path.IsPathRooted(fn))
                {
                    fn = System.AppDomain.CurrentDomain.BaseDirectory + fn;
                }

                MakeSurePathOK(fn);

                fs = new System.IO.FileStream(fn, System.IO.FileMode.OpenOrCreate);
            }
            catch (Exception ex)
            {
                throw new Exception("创建文件失败！！:" + ex.Message + "," + fn);
            }

            //br = new System.IO.BinaryReader(fs);
            //BOM头，Byte Order Mark,识别文件的编码格式，其选择合适编码打开文件
            byte[] BOM = new byte[3];
            fs.Read(BOM, 0, 3);
            if (BOM[0] == 255 && BOM[1] == 254)//BOM:FF FE
            {
                encoding = Encoding.Unicode;
            }
            else if (BOM[0] == 254 && BOM[1] == 255)//FE FF
            {
                encoding = Encoding.BigEndianUnicode;
            }
            else if (BOM[0] == 239 && BOM[1] == 187 && BOM[2] == 191)//EF BB BF
            {
                encoding = Encoding.UTF8;
            }
            else
            {
                encoding = Encoding.Default;//ANSI编码
            }
            fs.Seek(0, System.IO.SeekOrigin.Begin);//移到文件开始处
            return true;
        }

        public static void MakeSurePathOK(string absPath)
        {
            absPath = absPath.TrimEnd('\\');
            string dirPath = absPath.Substring(0, absPath.LastIndexOf('\\'));
            if (Directory.Exists(dirPath))
                return;

            MakeSurePathOK(dirPath);
            Directory.CreateDirectory(dirPath);
        }
    }
}

