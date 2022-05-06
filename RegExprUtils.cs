/***********************************************
 * 功能：正则表达式辅助工具类
 * 构建目标：封装正则表达式相关功能
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2017-10-19
 * 最后修改时间：
 * 修改信息：
 * 1:完成正则表达式相关函数  2017-10-19
 * 2.增加筛选一个函数 2017-10-20
 * 备注：
 **********************************************/
using EckyStudio.M.BaseModel;
using EckyStudio.M.FunctionComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class RegExprUtils
{
    public static bool IsMatch(string expr, string input,RegexOptions opt) {
        Regex ex = new Regex(expr,opt);
        return ex.IsMatch(input);
    }

    public static bool IsMatch(string expr, string input)
    {
        Regex ex = new Regex(expr);
        return ex.IsMatch(input);
    }

    /// <summary>
    /// 判断字符串是否匹配，并且提取命名分组
    /// </summary>
    /// <param name="expr">正则表达式</param>
    /// <param name="input">输入字符串</param>
    /// <param name="result">需要提取的分组</param>
    /// <returns></returns>
    public static bool IsMatch(string expr, string input, ref string result) {

        Regex ex = new Regex(expr);
        Match m = ex.Match(input);
        if (m.Success) {
            result = m.Groups[result].Value;
            return true;
        }
        return false;
    }


    #region -------文件名相关------------------------------
    /// <summary>
    /// 判断是否符合设定的格式的Windows下的文件名，Windows下文件名，忽略大小写。
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsMatchFileName(string expr, string input)
    {
        Regex ex = new Regex(expr,RegexOptions.IgnoreCase|RegexOptions.Singleline);
        return ex.IsMatch(input);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="fileList"></param>
    /// <returns></returns>
    public static LinkedList<string> MatchFileNameList(string expr, params string[] fileList) {
        LinkedList<string> ret = new LinkedList<string>();
        Regex ex = new Regex(expr, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //Match m;
        for (int i = 0; i < fileList.Length; i++) {
            //m = ex.Match(fileList[i]);
            //if (m.Success) {
            //    ret.AddLast(m.Value);
            //}
            if (ex.IsMatch(fileList[i])) {
                ret.AddLast(fileList[i]);
            }
        }
        return ret;
    }
    /// <summary>
    /// 是否能筛选出符合命名规则的一项
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="fileList"></param>
    /// <param name="chooseOne"></param>
    /// <returns></returns>
    public static bool CanScreenOutOneFileName(string expr, string[] fileList,out string screenedOutFileName) {
        screenedOutFileName = null;

        Regex ex = new Regex(expr, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        Match m;
        for (int i = 0; i < fileList.Length; i++)
        {
            m = ex.Match(fileList[i]);
            if (m.Success)
            {
                screenedOutFileName = m.Value;
                return true;
            }
        }
        return false;
    }

    #endregion

    /// <summary>
    /// 提取命名组的数据列表
    /// </summary>
    /// <param name="regExpr">正则表达式</param>
    /// <param name="input">需要匹配的文本</param>
    /// <returns>按照命名顺序返回命名组的匹配结果</returns>
    public static LinkedList<string[]> MatchNameGroupList(string regExpr, string input) {
        LinkedList<string[]> ret = new LinkedList<string[]>();
        //获取命名列表
        Regex reg = new Regex(@"\(\?\<(?<name>\S+?)\>\S+?\)");
        MatchCollection coll = reg.Matches(regExpr);
        string[] names = new string[coll.Count];
        for (int i = 0; i < names.Length; i++) {
            names[i] = coll[i].Groups["name"].Value;
        }

        //匹配数据
        reg = new Regex(regExpr);
        coll = reg.Matches(input);
        for (int i = 0; i < coll.Count; i++) {
            string[] values = new string[names.Length];
            for (int j = 0; j < values.Length; j++) {
               values[j] = coll[i].Groups[names[j]].Value;
            }
            ret.AddLast(values);
        }

        return ret;
    }

    /// <summary>
    /// 匹配一个命名组，名字必须为name
    /// </summary>
    /// <param name="regExpr">带命名组为name的正则表达式</param>
    /// <param name="input">需要匹配的字符串</param>
    /// <returns></returns>
    public static string MatchNameGroup(string regExpr, string input) {
        Regex ex = new Regex(regExpr);
        Match m = ex.Match(input);
        if (m.Success) {
            return m.Groups["name"].Value;
        }
        return null;
    }
}

