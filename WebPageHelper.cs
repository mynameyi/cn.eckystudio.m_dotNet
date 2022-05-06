/***********************************************
 * 功能：WebPage 获取辅助类
 * 构建目标：用于获取网页相关内容辅助类
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2014-12-14
 * 最后修改时间：2014-12-14
 * 修改信息：
 * 1:设置了基本功能组件的架构（2014-12-14）
 * 2.添加POST方法和代理设置的功能 2015-5-9
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using EckyStudio.M.FunctionComponentModel;
using EckyStudio.M.BaseModel;

public class WebPageHelper :FCM
{
    private string mDefaultEncoding = "utf-8";
    private HttpWebRequest mHttpRequest;
    private HttpWebResponse mHttResponse;
    private byte[] mFormData = null;

    public WebPageHelper(ITracer tracer):base(tracer)
    {
        ERROR_CODE_MAPPING_TABLE.Add(1000,"网络连接失败,错误代码{0}");
        ERROR_CODE_MAPPING_TABLE.Add(1001,"获取网页数据失败");
        ERROR_CODE_MAPPING_TABLE.Add(1002, "发生未知错误，错误：{0}");
    }

    public void setFormText(string text)
    {
        if (text == null)
            return;

        mFormData = Encoding.UTF8.GetBytes(text);
    }
    public bool getHtmlStream(out Stream result,string url, params KeyValuePair<string, string>[] headers)
    {
        mHttpRequest = (HttpWebRequest)WebRequest.Create(url);
 
        WebProxy proxy = new WebProxy("127.0.0.1", 8087);
        mHttpRequest.Proxy = proxy;

        mHttpRequest.Timeout = 60000; 
        mHttpRequest.AllowAutoRedirect = false;
        mHttpRequest.KeepAlive = true;
        mHttpRequest.Method = "POST";
        mHttpRequest.ContentType = "application/x-www-form-urlencoded";
        //string requestForm = "__VIEWSTATE=%2FwEPDwUKMTE4NTM4MDg3MGRk5p%2FmNmsOnVCJh3AYgUhW4wHxc5A%3D&txtRandomNum=&selectDrawID=10&hiddenSelectDrawID=10&radioDrawRange=GetDrawDate&_ctl0%3AContentPlaceHolder1%3AresultsMarkSix%3AselectDrawFromMonth=01&hiddenSelectDrawFromMonth=01&_ctl0%3AContentPlaceHolder1%3AresultsMarkSix%3AselectDrawFromYear=2002&hiddenSelectDrawFromYear=2002&_ctl0%3AContentPlaceHolder1%3AresultsMarkSix%3AselectDrawToMonth=05&hiddenSelectDrawToMonth=05&_ctl0%3AContentPlaceHolder1%3AresultsMarkSix%3AselectDrawToYear=2015&hiddenSelectDrawToYear=2015&radioResultType=GetAll";
        //byte[] postdatabyte = Encoding.UTF8.GetBytes(requestForm);
        //mHttpRequest.ContentLength = postdatabyte.Length;
        //Stream stream = mHttpRequest.GetRequestStream();
        //stream.Write(postdatabyte, 0, postdatabyte.Length); //设置请求主体的内容
        //stream.Close();

        if (mFormData != null)
        {
            mHttpRequest.ContentLength = mFormData.Length;
            Stream stream = mHttpRequest.GetRequestStream();
            stream.Write(mFormData, 0, mFormData.Length); //设置请求主体的内容
            stream.Close();
        }

        if (headers != null)
        {
            foreach(KeyValuePair<string,string> header in headers)
            {
                mHttpRequest.Headers.Add(header.Key, header.Value);
            }    
        }

        try
        {
            mHttResponse = (HttpWebResponse)mHttpRequest.GetResponse();
        }
        catch(Exception ex)
        {
            SetErrorCodeAndPrint(1002, ex.Message);
            result = null;
            return false;
        }
        if (mHttResponse.StatusCode != HttpStatusCode.OK)
        {
            SetErrorCodeAndPrint(1000,mHttResponse.StatusCode);
            result = null;
            return false;
        }
        
        result = mHttResponse.GetResponseStream();
        return true;
    }

    public HttpStatusCode getStatusCode()
    {
        return mHttResponse.StatusCode;
    }

    public bool getHtmlText(out String result,string url, params KeyValuePair<string, string>[] headers)
    {
        //String strEncoding = mHttResponse.Headers.Get("Content-Type");
        //Regex exEncoding = new Regex(@"charset=(?<encoding>.*?)");
        //String encoding = exEncoding.Match(strEncoding).Groups["encoding"].Value;
        Stream stream;
        if (getHtmlStream(out stream,url, headers))
        {
            Print("Encoding(mHttResponse.CharacterSet) is {0},mHttResponse.ContentEncoding ={1}", mHttResponse.CharacterSet, mHttResponse.ContentEncoding);
            String encoding = mHttResponse.CharacterSet;
            if (String.IsNullOrEmpty(encoding))
            {
                encoding = mDefaultEncoding;
            }

            StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(encoding));
            String content = reader.ReadToEnd();
            reader.Close();
            result = content;
            return true;
        }
        result = null;
        return false;
    }
}

