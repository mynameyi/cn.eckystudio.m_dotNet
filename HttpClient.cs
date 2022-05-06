/***********************************************
 * 功能：HttpClient 获取辅助类
 * 构建目标：用于获取网页相关内容辅助类
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 完成时间：2016-9-15
 * 最后修改时间：2016-9-15
 * 修改信息：
 * 1:基于WebPageHelper进行更细致的修改
 * 2.增加WebException处理 2016-9-29
 * 3.增加双向证书认证 REQUEST_PROP_CERTIFICATE  2019-9-3
 * 4.增加WebException 错误返回 2021-6-12
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
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

/// <summary>
/// 1.支持代理设置
/// 2.支持双向认证通信
/// </summary>
public class HttpClient : FCM
{
    private string mDefaultEncoding = "utf-8";
    private HttpWebRequest mHttpRequest;
    private HttpWebResponse mHttpResponse;
    private CookieContainer mCookieContainer = new CookieContainer();
    private byte[] mFormData = null;
    private string _FormText;
    private Dictionary<string, string> mHeaders = new Dictionary<string, string>();


    #region--------------- request property----------------------------
    /// <summary>
    /// 请求的表单文本
    /// </summary>
    public string REQUEST_PROP_FORM_TEXT
    {
        set {
            if (value == null)
            {
                mFormData = null;
                _FormText = null;
                return;
            }

            _FormText = value;
            mFormData = Encoding.UTF8.GetBytes(_FormText);
        }

        get {
            return _FormText;
        }
    }
    /// <summary>
    /// 设置代理服务器 格式如下 REQUEST_PROP_WEB_PROXY = new WebProxy("127.0.0.1", 8087);
    /// </summary>
    public WebProxy REQUEST_PROP_WEB_PROXY;
    /// <summary>
    /// 设置连接超时时间，默认为60000
    /// </summary>
    public int REQUEST_PROP_TIMEOUT = 60000;
    /// <summary>
    /// 设置是否允许跳转，默认为false
    /// </summary>
    public bool REQEUST_PROP_ALLOW_AUTO_REDIRECT = false;
    /// <summary>
    /// 设置是否保持连接，默认为true 
    /// </summary>
    public bool REQUEST_PROP_KEEP_ALIVE = true;
    /// <summary>
    /// 设置HTTP的请求方法，有“GET”和“POST”两种，默认为“GET”
    /// </summary>
    public string REQUEST_PROP_METHOD = "GET";
    /// <summary>
    /// 设置内容类型，表单请求可设置为 "application/x-www-form-urlencoded","application/xml","application/json"
    /// </summary>
    public string REQUEST_PROP_CONTENT_TYPE;
    /// <summary>
    /// 设置User-Agent，由于不设置User-Agent很多服务器直接拒绝连接,默认设置为 "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)"
    /// </summary>
    public string REQUEST_PROP_USER_AGENT = "Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; Lumia 650) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Mobile Safari/537.36 Edge/15.14900";
    public string REQUEST_PROP_ACCEPT = null;
    public string REQUEST_ACCEPT_ENCODING = null;

    /// <summary>
    /// 设置双向SSL认证,X509 3.0证书
    /// </summary>
    public X509Certificate REQUEST_PROP_CERTIFICATE = null;

    /// <summary>
    /// 设置http协议版本,http协议包括 http1.0、http1.1、http2.0
    /// </summary>
    public Version REQUEST_HTTP_PROTOCOL_VERSION = null;

    /// <summary>
    /// 设置SSL版本，SystemDefault =0,Ssl3 = 48,Tls=192,Tls11=768,Tls12=3072
    /// </summary>
    public SecurityProtocolType REQUEST_SSL_VERSION = (SecurityProtocolType)3072;

    /// <summary>
    /// 强制指定编码解释返回数据流，因为有部分服务器配置编码错误导致解析出现乱码
    /// </summary>
    public string RESPONSE_MANDOTORY_ENCODING = null;
    #endregion

    public HttpClient(ITracer tracer):base(tracer)
    {
        ERROR_CODE_MAPPING_TABLE.Add(1000,"网络连接失败,错误代码{0}");
        ERROR_CODE_MAPPING_TABLE.Add(1001,"获取网页数据失败");
        ERROR_CODE_MAPPING_TABLE.Add(1002, "发生未知错误，错误：{0}");
        ERROR_CODE_MAPPING_TABLE.Add(1003, "服务器连接发生错误，错误码：{0},错误信息：{1}");
    }

    public bool getHtmlStream(out Stream result, string url, params KeyValuePair<string, string>[] headers)
    {
        string[] strHeaders = null;
        if (headers != null)
        {
            strHeaders = new string[headers.Length];
            int i = -1;
            foreach (KeyValuePair<string, string> header in headers)
            {
                //mHttpRequest.Headers.Add(header.Key, header.Value);
                strHeaders[i++] = header.Key + ": " + header.Value;
            }
        }

        return getHtmlStream(out result,url, strHeaders);
    }

    public bool getHtmlStream(out Stream result,string url, string[] headers)
    {

        if (REQUEST_PROP_CERTIFICATE != null)
        {
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback; //添加证书验证链
            ServicePointManager.SecurityProtocol = REQUEST_SSL_VERSION;//设置SSL协议版本 Ssl3 = 48,SystemDefault =0,Tls=192,Tls11=768,Tls12=3072

            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.DefaultConnectionLimit = 512;
            ServicePointManager.Expect100Continue = false;      
        }

        //mHttpRequest = WebRequest.CreateHttp(url); //just can use above .net 4.5
        mHttpRequest = (HttpWebRequest)WebRequest.Create(url);

        if (REQUEST_PROP_WEB_PROXY != null)
        {
            mHttpRequest.Proxy = REQUEST_PROP_WEB_PROXY;//设置代理服务器
        }

        mHttpRequest.Timeout = REQUEST_PROP_TIMEOUT;
        mHttpRequest.AllowAutoRedirect = REQEUST_PROP_ALLOW_AUTO_REDIRECT;
        mHttpRequest.KeepAlive = REQUEST_PROP_KEEP_ALIVE;
        mHttpRequest.Method = REQUEST_PROP_METHOD;
        mHttpRequest.ContentType = REQUEST_PROP_CONTENT_TYPE;
        mHttpRequest.CookieContainer = mCookieContainer;

        //mHttpRequest.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
        //mHttpRequest.Headers.Add("POSTMAN_TOKEN", "ebe3e01b-a66f-4ca0-88d7-44afc4a068a9");

        if (REQUEST_PROP_ACCEPT != null)
        {
            mHttpRequest.Accept = REQUEST_PROP_ACCEPT;
        }

        if (REQUEST_PROP_USER_AGENT != null)
        {
            mHttpRequest.UserAgent = REQUEST_PROP_USER_AGENT;
        }

        if (REQUEST_ACCEPT_ENCODING != null)
        {
            mHttpRequest.Headers.Add("Accept-Encoding",REQUEST_ACCEPT_ENCODING);
        }

        if (headers != null)
        {
            foreach (string header in headers)
            {
                mHttpRequest.Headers.Add(header);
            }
        }

        if (mHeaders != null) {
            foreach (KeyValuePair<string,string> header in mHeaders)
            {
                //mHttpRequest.Headers.Add(header.Key + ":" + header.Value);
                mHttpRequest.Headers.Add(header.Key, header.Value);
            }
        }


        if (REQUEST_HTTP_PROTOCOL_VERSION != null) {
            mHttpRequest.ProtocolVersion = REQUEST_HTTP_PROTOCOL_VERSION;
        }

        if (REQUEST_PROP_CERTIFICATE != null)
        {
            mHttpRequest.ClientCertificates.Add(REQUEST_PROP_CERTIFICATE);
        }

        if (mFormData != null)
        {
            if (mHttpRequest.ContentType == null)
            {
                mHttpRequest.ContentType = "application/x-www-form-urlencoded";
            }
  
            mHttpRequest.ContentLength = mFormData.Length;
            Stream stream = mHttpRequest.GetRequestStream();
            stream.Write(mFormData, 0, mFormData.Length); //设置请求主体的内容
            stream.Close();
        }

        try
        {
            mHttpResponse = (HttpWebResponse)mHttpRequest.GetResponse(); 
            mCookieContainer.Add(mHttpResponse.Cookies);
        }
        catch (WebException wex)
        {
            SetErrorCodeAndPrint(1003,wex.Status,wex.Message);
            mHttpResponse = (HttpWebResponse)wex.Response;
            result = mHttpResponse.GetResponseStream();
            return false;
        }
        catch (Exception ex)
        {
            SetErrorCodeAndPrint(1002, ex.Message);
            result = null;
            return false;
        }
        if (mHttpResponse.StatusCode != HttpStatusCode.OK)
        {
            SetErrorCodeAndPrint(1000,mHttpResponse.StatusCode);
            result = null;
            return false;
        }
        
        result = mHttpResponse.GetResponseStream();
        return true;
    }

    public HttpStatusCode getStatusCode()
    {
        return mHttpResponse.StatusCode;
    }

    public bool getHtmlText(out String result,string url,string[] headers = null)
    {
        //String strEncoding = mHttpResponse.Headers.Get("Content-Type");
        //Regex exEncoding = new Regex(@"charset=(?<encoding>.*?)");
        //String encoding = exEncoding.Match(strEncoding).Groups["encoding"].Value;
        Stream stream;
        if (getHtmlStream(out stream, url, headers))
        {
            result = ParseStream(stream);
            if (result == null)
                return false;
            return true;
        }

        if (stream != null)
        {
            result = ParseStream(stream);
        }
        else
        { 
            result = null;
        }
        return false;
    }

    private string ParseStream(Stream stream)
    {
        Print("Encoding(mHttpResponse.CharacterSet) is {0},mHttpResponse.ContentEncoding ={1}", mHttpResponse.CharacterSet, mHttpResponse.ContentEncoding);
        String streamEncoding = mHttpResponse.ContentEncoding.ToLower();

        Print("page encoding is " + streamEncoding);
        switch (streamEncoding)
        {
            case "gzip":
                GZipStream gs = new GZipStream(stream, CompressionMode.Decompress);
                stream = gs;
                break;
            case "deflate":
            //break;
            case "sdch":
                Print("didn't support encoding : " + streamEncoding);
                return null;
            default:
                break;
        }

        Encoding encoding = null;
        if (RESPONSE_MANDOTORY_ENCODING != null)
        {
            encoding = Encoding.GetEncoding(RESPONSE_MANDOTORY_ENCODING);
        }
        else if (!string.IsNullOrEmpty(mHttpResponse.CharacterSet) && IsExistEncoding(mHttpResponse.CharacterSet, out encoding))
        {

        }
        else if (!string.IsNullOrEmpty(streamEncoding) && IsExistEncoding(streamEncoding, out encoding))
        {
        }
        else
        {
            encoding = Encoding.GetEncoding(mDefaultEncoding);
        }

        string transferEncoding = mHttpResponse.Headers.Get("Transfer-Encoding").ToLower();
        string content;
        if ("chunked".Equals(transferEncoding))
        {
            StringBuilder sb = new StringBuilder();
            Byte[] buf = new byte[8192];
            int count = 0;
            do
            {
                count = mHttpResponse.GetResponseStream().Read(buf, 0, buf.Length);
                if (count != 0)
                {
                    sb.Append(Encoding.UTF8.GetString(buf, 0, count)); // just hardcoding UTF8 here
                }
            } while (count>0);
            content = sb.ToString();
        }
        else
        {
            StreamReader reader = new StreamReader(stream, encoding);
            content = reader.ReadToEnd();
            reader.Close();
        }

        return content;
    }

    private bool IsExistEncoding(string strEncoding,out Encoding encoding) {
        encoding = null;
        try
        {
            encoding = Encoding.GetEncoding(strEncoding);
        }
        catch (ArgumentException ae)
        {
            return false;
        }
        return true;
    }

    public void AddHeader(string key, string value) {
        mHeaders.Add(key, value);
    }

    public void ResetRequestProp()
    {
        ResetRequestProp(false);
    }

    /// <summary>
    /// 清空请求头
    /// </summary>
    /// <param name="keepUserAgent">是否</param>
    public void ResetRequestProp(bool keepUserAgent)
    {
        REQUEST_PROP_FORM_TEXT = null;
        REQUEST_PROP_WEB_PROXY = null;
        REQUEST_PROP_TIMEOUT = 60000;
        REQEUST_PROP_ALLOW_AUTO_REDIRECT = false;
        REQUEST_PROP_KEEP_ALIVE = true;
        REQUEST_PROP_METHOD = "GET";
        REQUEST_PROP_CONTENT_TYPE = null;

        mHeaders.Clear();//清除头部

        if (!keepUserAgent)
            REQUEST_PROP_USER_AGENT = null;
    }

    //create a new cookie
    public void ClearCookies()
    {
        mCookieContainer = new CookieContainer();//
    }

    public void Reset()
    {
        ResetRequestProp();
        ClearCookies();
    }

    private static bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;
        //return false;
        throw new Exception("导入证书发生策略异常");
    }

    public static X509Certificate LoadP12Certificate(string path,string pwd = null) {
        if (!Path.IsPathRooted(path))
        {
            path = System.AppDomain.CurrentDomain.BaseDirectory + path;
        }

        //X509Certificate cert;
        //if (pwd == null){
        //    cert = new X509Certificate(path);
        //}
        //else{
        //    cert = new X509Certificate(path, pwd);
        //}
        //return cert;

        X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);
        X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "1900007051", false);

        return certCollection.Count >0 ? certCollection[0] : null;
    }
}

