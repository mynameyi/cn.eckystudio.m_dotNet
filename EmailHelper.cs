/***********************************************
 * 名称：邮件发送模块/组件
 * 功能/用途：便于邮件的发送
 * 构建目标：打造最便捷、可持续改进的邮件发送组件
 * 应用情景：
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2014-12-14
 * 修改记录：
 * 1:完成基本邮件发送功能（2014-12-14）
 * 2.增加错误映射表 ERROR_TABLE , LoadErrorCodeMappingTable (2017-10-24)
 * 备注：
 **********************************************/
using System;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Net.Sockets;
using EckyStudio.M.FunctionComponentModel;
using EckyStudio.M.BaseModel;
using System.Collections.Generic;

public class EmailHelper : FCM
{

    private static readonly Dictionary<int, string> ERROR_TABLE = new Dictionary<int, string>() {
        {2000,"发送邮件发生未知错误：{0}" },
        {2001,"设置参数发生未知错误：{1}" }
    };

    public EmailHelper(ITracer tracer)
        : base(tracer)
    {
        mMailMessage = new MailMessage();
    }

    private MailMessage mMailMessage;   //主要处理发送邮件的内容（如：收发人地址、标题、主体、图片等等）
    private SmtpClient mSmtpClient; //主要处理用smtp方式发送此邮件的配置信息（如：邮件服务器、发送端口号、验证方式等等）
    private int mSenderPort;   //发送邮件所用的端口号（htmp协议默认为25）
    private string mSenderServerHost;    //发件箱的邮件服务器地址（IP形式或字符串形式均可）
    private string mSenderPassword;    //发件箱的密码
    private string mSenderUsername;   //发件箱的用户名（即@符号前面的字符串，例如：hello@163.com，用户名为：hello）
    private bool mEnableSsl;    //是否对邮件内容进行socket层加密传输
    private bool mEnablePwdAuthentication;  //是否对发件人邮箱进行密码验证

    ///<summary>
    /// 添加附件
    ///</summary>
    ///<param name="attachmentsPath">附件的路径集合，以分号分隔</param>
    public void AddAttachments(string attachmentsPath)
    {
        try
        {
            string[] path = attachmentsPath.Split(';'); //以什么符号分隔可以自定义
            Attachment data;
            ContentDisposition disposition;
            for (int i = 0; i < path.Length; i++)
            {
                data = new Attachment(path[i], MediaTypeNames.Application.Octet);
                disposition = data.ContentDisposition;
                disposition.CreationDate = File.GetCreationTime(path[i]);
                disposition.ModificationDate = File.GetLastWriteTime(path[i]);
                disposition.ReadDate = File.GetLastAccessTime(path[i]);
                mMailMessage.Attachments.Add(data);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    ///<summary>
    /// 邮件的发送
    ///</summary>
    public void Send()
    {
        try
        {
            if (mMailMessage != null)
            {
                mSmtpClient = new SmtpClient();
                //mSmtpClient.Host = "smtp." + mMailMessage.From.Host;

                mSmtpClient.Host = this.mSenderServerHost;
                mSmtpClient.Port = this.mSenderPort;
                mSmtpClient.UseDefaultCredentials = true;
                mSmtpClient.EnableSsl = this.mEnableSsl;

                if (this.mEnablePwdAuthentication)
                {
                    System.Net.NetworkCredential nc = new System.Net.NetworkCredential(this.mSenderUsername, this.mSenderPassword);
                    //mSmtpClient.Credentials = new System.Net.NetworkCredential(this.mSenderUsername, this.mSenderPassword);
                    //NTLM: Secure Password Authentication in Microsoft Outlook Express
                    mSmtpClient.Credentials = nc.GetCredential(mSmtpClient.Host, mSmtpClient.Port, "NTLM");
                }
                else
                {
                    //163邮箱不能带@163.com后缀
                    mSmtpClient.Credentials = new System.Net.NetworkCredential(this.mSenderUsername, this.mSenderPassword);
                }
                mSmtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                mSmtpClient.Send(mMailMessage);
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.ToString());  
            //Print("发送邮件发生未知错误：" + ex.Message);
            SetErrorCodeAndPrint(2000, ex.Message);
        }
    }

    public void setSmtpServer(string server, string port, string username, string password, bool sslEnable, bool pwdCheckEnable)
    {
        this.mSenderServerHost = server;
        this.mSenderUsername = username;
        this.mSenderPassword = password;
        this.mSenderPort = Convert.ToInt32(port);
        this.mEnableSsl = sslEnable;
        this.mEnablePwdAuthentication = pwdCheckEnable;
    }

    public void setMailMessage(string toMail, string fromMail, string subject, string emailBody)
    {
        try
        {
            mMailMessage.To.Clear();
            mMailMessage.To.Add(toMail);
            mMailMessage.From = new MailAddress(fromMail);
            mMailMessage.Subject = subject;
            mMailMessage.Body = emailBody;
            mMailMessage.IsBodyHtml = true;
            mMailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mMailMessage.Priority = MailPriority.Normal;
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.ToString());
            //Print( "设置参数发生错误 ：" +  ex.Message);
            SetErrorCodeAndPrint(2001, ex.Message);
        };
    }

    /// <summary>
    /// 初始化错误映射列表
    /// </summary>
    /// <param name="table"></param>
    protected override void InitErrorCodeMappingTable(out Dictionary<int, string> table)
    {
        table = ERROR_CODE_MAPPING_TABLE;
    }
}

