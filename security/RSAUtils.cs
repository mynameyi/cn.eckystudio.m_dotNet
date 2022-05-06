using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class RSAUtils
{
    #region 签名方法

    /// <summary>
    /// 签名，默认使用SHA256哈希签名方法，默认使用UTF-8编码格式
    /// </summary>
    /// <param name="content">需要签名内容</param>
    /// <param name="privateKey">RSA私钥</param>
    /// <returns>Base64格式的签名字符串</returns>
    public static string Sign(string content, string privateKey) {   
        return Sign(content,new SHA256CryptoServiceProvider(),privateKey,"UTF-8");
    }

    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="content">需要签名的内容</param>
    /// <param name="hash">签名使用的hash算法</param>
    /// <param name="privateKey">RAS私钥</param>
    /// <param name="inputCharset">签名内容期望的编码格式</param>
    /// <returns>Base64格式的签名字符串</returns>
    public static string Sign(string content,HashAlgorithm hash, string privateKey, string inputCharset)
    {
        try
        {
            Encoding code = Encoding.GetEncoding(inputCharset);
            byte[] data = code.GetBytes(content);
            RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);

            //签名hash算法
            //SHA1 sh = new SHA1CryptoServiceProvider(); 

            byte[] signData = rsa.SignData(data, hash);
            return Convert.ToBase64String(signData);
        }
        catch(Exception ex)
        {
            return "";
        }
    }

    #endregion

    #region 验签函数

    /// <summary>
    /// 验签函数
    /// </summary>
    /// <param name="content"></param>
    /// <param name="signedString"></param>
    /// <param name="publicKey"></param>
    /// <param name="inputCharset"></param>
    /// <returns></returns>
    public static bool Verify(string content, string signedString, string publicKey, string inputCharset)
    {
        bool result = false;

        Encoding code = Encoding.GetEncoding(inputCharset);
        byte[] data = code.GetBytes(content);
        byte[] soureData = Convert.FromBase64String(signedString);
        RSAParameters paraPub = ConvertFromPublicKey(publicKey);
        RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
        rsaPub.ImportParameters(paraPub);
        SHA1 sh = new SHA1CryptoServiceProvider();
        result = rsaPub.VerifyData(data, sh, soureData);
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pemFileConent"></param>
    /// <returns></returns>
    internal static RSAParameters ConvertFromPublicKey(string pempublicKey)
    {
        byte[] keyData = Convert.FromBase64String(pempublicKey);
        if (keyData.Length < 162)
        {
            throw new ArgumentException("pem file content is incorrect.");
        }
        byte[] pemModulus = new byte[128];
        byte[] pemPublicExponent = new byte[3];
        Array.Copy(keyData, 29, pemModulus, 0, 128);
        Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
        RSAParameters para = new RSAParameters();
        para.Modulus = pemModulus;
        para.Exponent = pemPublicExponent;
        return para;
    }

    public static string DecodeBase64(string code_type, string code)
    {
        string decode = "";
        byte[] bytes = Convert.FromBase64String(code);  //将2进制编码转换为8位无符号整数数组. 
        try
        {
            decode = Encoding.GetEncoding(code_type).GetString(bytes);  //将指定字节数组中的一个字节序列解码为一个字符串。 
        }
        catch
        {
            decode = code;
        }
        return decode;
    }


    #endregion

    #region 内部辅助方法

    /// <summary>
    /// 解析PKCS8格式的pem私钥
    /// </summary>
    /// <param name="pemstr"></param>
    /// <returns></returns>
    internal static RSACryptoServiceProvider DecodePemPrivateKey(string pemstr)
    {
        RSACryptoServiceProvider rsa = null;
        byte[] pkcs8PrivteKey = Convert.FromBase64String(pemstr);
        if (pkcs8PrivteKey != null)
        {
            rsa = DecodePrivateKey(pkcs8PrivteKey);
        }
        return rsa;
    }

    /// <summary>
    /// 把PKCS8格式的私钥转成PEK格式私钥，然后再填充RSACryptoServiceProvider对象
    /// </summary>
    /// <param name="pkcs8"></param>
    /// <returns></returns>
    internal static RSACryptoServiceProvider DecodePrivateKey(byte[] pkcs8)
    {
        byte[] seqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
        byte[] seq = new byte[15];

        MemoryStream mem = new MemoryStream(pkcs8);
        RSACryptoServiceProvider rsacsp = null;

        int lenStream = (int)mem.Length;
        BinaryReader binReader = new BinaryReader(mem);
        byte bt = 0;
        ushort twoBytes = 0;

        try
        {
            twoBytes = binReader.ReadUInt16();
            if (twoBytes == 0x8130)        //data read as little endian order (actual data order for Sequence is 30 81)
                binReader.ReadByte();    //advance 1 byte
            else if (twoBytes == 0x8230)
                binReader.ReadInt16();    //advance 2 bytes
            else
                return null;

            bt = binReader.ReadByte();
            if (bt != 0x02)
                return null;

            twoBytes = binReader.ReadUInt16();

            if (twoBytes != 0x0001)
                return null;

            seq = binReader.ReadBytes(15);            //read the Sequence OID
            if (!CompareBytearrays(seq, seqOID))    //make sure Sequence for OID is correct
                return null;

            bt = binReader.ReadByte();
            if (bt != 0x04)                    //expect an Octet string 
                return null;

            bt = binReader.ReadByte();        //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
            if (bt == 0x81)
                binReader.ReadByte();
            else
                if (bt == 0x82)
                binReader.ReadUInt16();

            // at this stage, the remaining sequence should be the RSA private key
            byte[] rsaprivkey = binReader.ReadBytes((int)(lenStream - mem.Position));
            rsacsp = DecodeRSAPrivateKey(rsaprivkey);

            return rsacsp;
        }
        catch(Exception ex)
        {
            return null;
        }
        finally
        {
            binReader.Close();
        }
    }


    private static bool CompareBytearrays(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        int i = 0;
        foreach (byte c in a)
        {
            if (c != b[i])
                return false;
            i++;
        }
        return true;
    }

    /// <summary>
    /// PEM格式私钥转RSACryptoServiceProvider对象
    /// </summary>
    /// <param name="privkey"></param>
    /// <returns></returns>
    internal static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
    {
        byte[] modulus, e, d, p, q, dp, dq, iq;

        // Set up stream to decode the asn.1 encoded RSA private key
        MemoryStream mem = new MemoryStream(privkey);

        // wrap Memory Stream with BinaryReader for easy reading
        BinaryReader binReader = new BinaryReader(mem);
        byte bt = 0;
        ushort twoBytes = 0;
        int elems = 0;
        try
        {
            twoBytes = binReader.ReadUInt16();
            if (twoBytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
            {
                binReader.ReadByte(); // advance 1 byte
            }
            else if (twoBytes == 0x8230)
            {
                binReader.ReadInt16(); // advance 2 byte
            }
            else
            {
                return null;
            }

            twoBytes = binReader.ReadUInt16();
            if (twoBytes != 0x0102) // version number
                return null;
            bt = binReader.ReadByte();
            if (bt != 0x00)
                return null;

            // all private key components are Integer sequences
            elems = GetIntegerSize(binReader);
            modulus = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            e = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            d = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            p = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            q = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            dp = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            dq = binReader.ReadBytes(elems);

            elems = GetIntegerSize(binReader);
            iq = binReader.ReadBytes(elems);

            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters rsaParams = new RSAParameters();
            rsaParams.Modulus = modulus;
            rsaParams.Exponent = e;
            rsaParams.D = d;
            rsaParams.P = p;
            rsaParams.Q = q;
            rsaParams.DP = dp;
            rsaParams.DQ = dq;
            rsaParams.InverseQ = iq;
            rsa.ImportParameters(rsaParams);
            return rsa;
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            binReader.Close();
        }
    }


    internal static int GetIntegerSize(BinaryReader binReader)
    {
        byte bt = 0;
        byte lowByte = 0x00;
        byte highByte = 0x00;
        int count = 0;
        bt = binReader.ReadByte();
        if (bt != 0x02)        //expect integer
            return 0;
        bt = binReader.ReadByte();

        if (bt == 0x81)
        {
            count = binReader.ReadByte();    // data size in next byte
        }
        else
        {
            if (bt == 0x82)
            {
                highByte = binReader.ReadByte();    // data size in next 2 bytes
                lowByte = binReader.ReadByte();
                byte[] modint = { lowByte, highByte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;        // we already have the data size
            }
        }

        while (binReader.ReadByte() == 0x00)
        {    //remove high order zeros in data
            count -= 1;
        }
        binReader.BaseStream.Seek(-1, SeekOrigin.Current);        //last ReadByte wasn't a removed zero, so back up a byte
        return count;
    }


    #endregion
  
}

