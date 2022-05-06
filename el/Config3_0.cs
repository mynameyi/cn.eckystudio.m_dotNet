/***********************************************
 * 功能：基于原有类构建Config 3.0操作类
 * 目标：
 * 1.增加 后置双斜线注释、单行双斜线注释、#注释、分号注释 等功能
 * 2.增加双引号字符串定域功能
 * 3.优化代码结构,规范，参数命名使用小驼峰
 * 版本号：V3.0;
 * 作者：Ecky Leung;
 * 立项时间：2017-10-21
 * 完成时间：
 * 修改信息： 
 * 2.
 * 备注：
 **********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyLanguage
{
    public class Config3_0
    {
        public const string DEFAULT_SETCTION = "General";

        string FileBody;
        string CurrentFileName;
        FileOperator mFileOperator;

        //List<KeywordAttributes> KeywordList = new List<KeywordAttributes>();
        //int ForeBorderCharacterPos;//前边界符位置
        //int BackBorderCharacterPos;//后边界符位置

        public Config3_0(string fileName)
        {
            mFileOperator = new FileOperator(fileName);
            //mFileOperator.Seek(0, System.IO.SeekOrigin.Begin);//移到文件开始处
            FileBody = mFileOperator.GetFileContent();

            //fs.Seek(0, System.IO.SeekOrigin.Begin);//移到文件开始处
            //br = new System.IO.BinaryReader(fs, FileEncode);
            //while (true)
            //{
            //    char c = br.ReadChar();
            //    fs.Position = 3;
            //    char[] ca = br.ReadChars(5);

            //    //string s = br.ReadString();
            //}
            CurrentFileName = fileName;
        }
        public void Dispose()
        {
            mFileOperator.Dispose();
        }
        public string ReadVariant(string SectionName, string VariantName, string DefaultValue)
        {
            return DefaultValue;
        }
        public string ReadString(string SectionName, string VariantName, string DefaultValue)
        {
            int fileLen = FileBody.Length;

            int SectionBeginPos = -1;
            int SectionEndPos = -1;
            int ForeBorderCharacterPos = -1;//前边界符位置
            int BackBorderCharacterPos = -1;//后边界符位置

            string _SectionName;
            string _VariantName;
            string _Value;

            if (SectionName == null)
            {
                ;
            }
            for (int i = 0; i < fileLen; i++)
            {
                if (FileBody[i] == '[')
                {
                    SectionBeginPos = i;
                }
                if (FileBody[i] == ']')
                {
                    SectionEndPos = i;
                    if (SectionBeginPos != -1 && SectionEndPos != -1)
                    {
                        _SectionName = FileBody.Substring(SectionBeginPos + 1, SectionEndPos - SectionBeginPos - 1);
                        if (_SectionName.Trim().Equals(SectionName))//当找到节点之后
                        {
                            for (int j = i; j < fileLen; j++)
                            {
                                switch (FileBody[j])
                                {
                                    case '\n':
                                        if (ForeBorderCharacterPos == -1)
                                        {
                                            ForeBorderCharacterPos = j;
                                        }
                                        else
                                        {
                                            BackBorderCharacterPos = j;
                                        }
                                        break;
                                    case ' ':
                                        if (ForeBorderCharacterPos == -1)
                                        {
                                            ForeBorderCharacterPos = j;
                                        }
                                        else
                                        {
                                            BackBorderCharacterPos = j;
                                        }
                                        break;
                                    case '=':
                                        if (ForeBorderCharacterPos == -1)
                                        {
                                            ForeBorderCharacterPos = j;
                                        }
                                        else
                                        {
                                            BackBorderCharacterPos = j;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                if (ForeBorderCharacterPos != -1 && BackBorderCharacterPos != -1)
                                {
                                    _VariantName = FileBody.Substring(ForeBorderCharacterPos + 1, BackBorderCharacterPos - ForeBorderCharacterPos - 1);

                                    int ValueTagPos = -1;
                                    if (_VariantName.Trim().Equals(VariantName))//找到属性名之后
                                    {
                                        for (int k = j; k < fileLen; k++)
                                        {
                                            if (FileBody[k] == '=')
                                            {
                                                ValueTagPos = k;
                                            }
                                            else if (FileBody[k] == '\n')
                                            {
                                                _Value = FileBody.Substring(ValueTagPos + 1, k - ValueTagPos - 1).Trim();
                                                return _Value;
                                            }
                                            else if (k == (fileLen - 1))
                                            {
                                                _Value = FileBody.Substring(ValueTagPos + 1, k - ValueTagPos).Trim();
                                                return _Value;
                                            }
                                        }
                                    }
                                    ForeBorderCharacterPos = BackBorderCharacterPos;
                                    BackBorderCharacterPos = -1;
                                }
                            }
                        }
                    }
                }
            }
        Exit:
            return DefaultValue;
        }
        public bool WriteString(string SectionName, string KeyName, string KeyValue)
        {
            //SectionName = SectionName.TrimStart('[');
            //SectionName = SectionName.TrimEnd(']');

            StringPos SectionPos;
            StringPos NextSectionPos;
            StringPos KeyPos;
            StringPos EqualCharacterPos;

            if (SectionName == null)
            {
                ;
            }

            //搜索节名
            SectionName = '[' + SectionName + ']';
            SectionPos = FindString(ref FileBody, 0, 0, SectionName,true);

            //如果没找到节名，直接追加
            if (SectionPos.StartPosition == -1)//如果没有找到节名
            {
                FileBody += SectionName + "\r\n";
                FileBody += KeyName + " = " + KeyValue + "\r\n";
                goto Exit;
            }

            //如果找到节名，继续找下一节的位置
            NextSectionPos = FindString(ref FileBody, SectionPos.NextStartPosition, null, "[",true);
            if (NextSectionPos.StartPosition == -1)//如果没有下一节
            {
                KeyPos = FindString(ref FileBody, SectionPos.NextStartPosition, null, KeyName,true,true);//寻找关键字
                if (KeyPos.StartPosition == -1)//如果没有键名，直接追加
                {
                    FileBody += KeyName + " = " + KeyValue + "\r\n";
                    goto Exit;
                }
                else
                {
                    EqualCharacterPos = FindString(ref FileBody, KeyPos.NextStartPosition, null, "=");
                }
            }
            else
            {
                KeyPos = FindString(ref FileBody, SectionPos.NextStartPosition, NextSectionPos.StartPosition, KeyName,true,true);
                if (KeyPos.StartPosition == -1)
                {
                    FileBody = FileBody.Insert(NextSectionPos.StartPosition, KeyName + " = " + KeyValue + "\r\n");//插入数据
                    goto Exit;
                }
                EqualCharacterPos = FindString(ref FileBody, KeyPos.NextStartPosition, NextSectionPos.StartPosition, "=");
            }

            string OriginalKeyName;
            string strKeyValuePair = KeyName + " = " + KeyValue + "\r\n";

            int iValueEndPos;
            StringPos EndPos;
            if (EqualCharacterPos.StartPosition == -1)
            {
                FileBody = FileBody.Insert(NextSectionPos.StartPosition, strKeyValuePair);
            }
            else
            {
                OriginalKeyName = FileBody.Substring(KeyPos.StartPosition, EqualCharacterPos.StartPosition - KeyPos.StartPosition);
                OriginalKeyName = OriginalKeyName.Trim();
                if (KeyName == OriginalKeyName)
                {
                    EndPos = FindString(ref FileBody, EqualCharacterPos.NextStartPosition, null, "\r\n");
                    iValueEndPos = EndPos.StartPosition - EqualCharacterPos.NextStartPosition;
                    if (EndPos.StartPosition == -1)
                    {
                        FileBody = FileBody.Remove(EqualCharacterPos.NextStartPosition);
                    }
                    else
                    {
                        FileBody = FileBody.Remove(EqualCharacterPos.NextStartPosition, iValueEndPos);
                    }

                    FileBody = FileBody.Insert(EqualCharacterPos.NextStartPosition, ' ' + KeyValue);//插入数据
                }
                else
                {
                    FileBody = FileBody.Insert(NextSectionPos.StartPosition, strKeyValuePair);
                }
            }
        Exit:
            return mFileOperator.Cover(FileBody);  /*mFileOperator.Write(FileBody,System.IO.SeekOrigin.Begin);*/
        }
        public List<string> GetSections()
        {
            return new List<string>();
        }
        public List<KeyValuePair<string, string>> GetKeyValuePair(string SectionName)
        {
            return new List<KeyValuePair<string, string>>();
        }


        struct StringPos
        {
            public int StartPosition;
            public int NextStartPosition;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="StartOffset"></param>
        /// <param name="EndOffset">如果为NULL或者为零，则一直搜索到最后</param>
        /// <param name="Key"></param>
        /// <param name="lineCorner">行切换判断</param>
        /// <param name="keySeparate">key分隔判断</param>
        /// <returns>返回字符串的起始位置和字符结束的下一位置</returns>
        private StringPos FindString(ref string Text, int StartOffset, int? EndOffset, string Key, bool lineCorner = false, bool keySeparate = false)
        {
            int iTxtLen = Text.Length;
            int iKeyLen = Key.Length;
            int iTheSameCharCount = 0;

            //KeyValuePair<int, int> KeyPos = new KeyValuePair<int, int>(-1, -1);
            StringPos KeyPos = new StringPos();
            KeyPos.StartPosition = -1;
            KeyPos.NextStartPosition = -1;

            //if (StartOffset > EndOffset)
            //{
            //    goto Exit;
            //}
            if (EndOffset == null || EndOffset == 0)
            {
                EndOffset = iTxtLen;
            }


            for (int i = StartOffset; i < EndOffset; i++)
            {
                for (int j = 0; j < iKeyLen; j++)
                {
                    if (Key[j] == Text[i + j])
                    {
                        iTheSameCharCount++;
                        if (iTheSameCharCount == iKeyLen)
                        {
                            if( lineCorner? (i == 0 || Text[i - 1] == '\n'):true)//是否需要确保关键字前面换行符
                            {
                                //KeyPos = new KeyValuePair<int, int>(i, i + iKeyLen);

                                if (keySeparate ? ((i == iTxtLen-1) || (Text[i+iKeyLen]== ' ') || (Text[i+ iKeyLen] == '=')) : true) //键后分隔判断
                                {
                                    KeyPos.StartPosition = i;
                                    KeyPos.NextStartPosition = i + iKeyLen;
                                    goto Exit;
                                }
                            }
                        }
                        continue;
                    }
                    else
                    {
                        iTheSameCharCount = 0;
                        break;
                    }
                }
            }

        Exit:
            return KeyPos;
        }
    }
}
