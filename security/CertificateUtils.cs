/***********************************************
 * 功能：证书导入类
 * 目标：自动导入证书，免去手动导入的麻烦，使操作更加透明
 * 版本号：V1.0;
 * 作者：Ecky Leung;
 * 立项时间：2015-4-18
 * 完成时间：2015-4-18
 * 修改信息：
 * 备注：
 **********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;

public class CertificateUtils
{

    /// <summary>
    /// 导入证书文件
    /// </summary>
    /// <param name="path">pfx/p12证书文件的路径</param>
    /// <param name="pwd">证书导入密钥</param>
    /// <returns>返回导入证书的序列号</returns>
    public static string ImportCertificate(string path,string pwd)
    {
        try
        {
            if (!Path.IsPathRooted(path))
            {
                path = System.AppDomain.CurrentDomain.BaseDirectory + path;
            }

            X509Certificate2 cert;
            if (pwd == null)
            {
                cert = new X509Certificate2(path);
            }
            else
            {
                cert = new X509Certificate2(path, pwd);
                //cert = new X509Certificate2(path, pwd,X509KeyStorageFlags.Exportable | X509KeyStorageFlags.UserKeySet); //标记私钥可导出
            }          

            //新建指向当前用户，个人证书存贮区的x509Store对象
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser); 
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();

            return cert.GetSerialNumberString(); 
            //X509Certificate2 x509Certificate2 = new X509Certificate2(
            //    System.Environment.CurrentDirectory + @"\Provision\oem_ovk_cert.pfx",   //证书路径
            //    "Hebin154833",     //证书的私钥保护密码
            //    X509KeyStorageFlags.DefaultKeySet);

            ////新建指向当前用户，个人证书存贮区的x509Store对象
            //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            //store.Open(OpenFlags.ReadWrite);
            //store.Add(x509Certificate2);
            //store.Close();
            //return true;
        }
        catch
        {
            ;
        }
        return null;
    }

    public static X509Certificate GetCertifacate(string serialNo)
    {
        if (string.IsNullOrEmpty(serialNo))
            return null;

        X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        certStore.Open(OpenFlags.ReadOnly);
        //X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "1900007051", false);
        X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySerialNumber, serialNo, false);
        return certCollection.Count > 0 ? certCollection[0] : null;
    }

    public static X509Certificate2 GetCertifacate(byte[] rawData,string pwd)
    {
        X509Certificate2 cert = new X509Certificate2(rawData,pwd);
        return cert;
    }

    public static bool IsExist(string serialNo,out X509Certificate cert)
    {
        cert = GetCertifacate(serialNo);
        if (cert == null)
            return false;

        return true;
        ////X509Certificate2 x509Certificate2 = new X509Certificate2();
        //X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        //store.Open(OpenFlags.ReadOnly);
        //try
        //{
        //    //轮询存储区中的所有证书
        //    foreach (X509Certificate2 myX509Certificate2 in store.Certificates)
        //    {
        //        //将证书的名称跟要导出的证书MyTestCert比较,找到要导出的证书
        //        if (myX509Certificate2.Subject == "CN=OEM OVK Signing Certificate")
        //        {
        //            return true;
        //        }
        //    }
        //}
        //catch
        //{
        //    ;
        //}
        //finally
        //{
        //    store.Close();
        //}
        //return false;
    }

    //public static bool makeSureCertificateOK()
    //{
    //    //if (isExist())
    //    //{
    //    //    return true;
    //    //}
    //    removeAllCertificate();

    //    try
    //    {
    //        //x1-coship Hebin1548331
    //        //x1-jp Hebin154833
    //        X509Certificate2 x509Certificate2 = new X509Certificate2(
    //            System.Environment.CurrentDirectory + @"\Provision\oem_ovk_cert.pfx",   //证书路径
    //            "Hebin154833",     //证书的私钥保护密码
    //            X509KeyStorageFlags.DefaultKeySet);

    //        //新建指向当前用户，个人证书存贮区的x509Store对象
    //        X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    //        store.Open(OpenFlags.ReadWrite);
    //        store.Add(x509Certificate2);
    //        store.Close();
    //        return true;
    //    }
    //    catch {
    //        ;
    //    }

    //    return false;
    //}

    //public static void removeAllCertificate()
    //{
    //    X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    //    store.Open(OpenFlags.ReadOnly);
    //    try
    //    {
    //        store.RemoveRange(store.Certificates);
    //    }
    //    catch
    //    {
    //        ;
    //    }
    //    finally
    //    {
    //        store.Close();
    //    }
    //}
}

