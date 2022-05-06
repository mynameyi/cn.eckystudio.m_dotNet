using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class StringUtils
{
    public static byte[] HexStringToBytes(string hexString) {
        hexString = hexString.Replace(" ", "");
        if ((hexString.Length % 2) != 0)
            hexString += " ";

        byte[] ret = new byte[hexString.Length / 2];
        for (int i = 0; i < ret.Length; i++)
            ret[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        return ret;
    }

    public static string BytesToHexString(byte[] bytes) {
        int count = bytes.Length;
        StringBuilder strBuider = new StringBuilder();
        for (int index = 0; index < count; index++)
        {
            strBuider.Append(((int)bytes[index]).ToString("X2"));
        }

        return strBuider.ToString();
    }

    /// <summary>
    /// 加密字符串，采用倒序、奇偶、首尾的处理工序对字符串进行转换
    /// </summary>
    /// <param name="str"></param>
    public static string Encrypt(string plain) {
        Random r = new Random();

        StringBuilder s1 = new StringBuilder(plain.Length+2);


        s1.Append((char)r.Next(65, 90));//首位加一个随机数,用于混淆字符数量

        //工序1:反转
        s1.Append(string.Join("", plain.Reverse()));


        //工序2：奇数+2，偶数作大小写字母变换并且+1
        for (int i = 0; i < s1.Length; i++) {

            if (i % 2 == 0)
            {
                s1[i] = (char)(s1[i] + 2);
            }
            else
            {
                if (s1[i] >= 65 && s1[i] <= 90)
                {
                    s1[i] =  (char)(s1[i] + 32);
                }
                else if (s1[i] >= 97 && s1[i] <= 122) {
                    s1[i] = (char) (s1[i]- 32);
                }

                s1[i] = (char)(s1[i] + 1);
            }

            switch (s1[i]) {

                case (char)91:
                    s1[i] = (char)65;           
                    break;
                case (char)92:
                    s1[i] = (char)66;
                    break;
                case (char)123:
                    s1[i] = (char)97;
                    break;
                case (char)124:
                    s1[i] = (char)98;
                    break;
            }
        }

        s1.Append((char)r.Next(65, 90));//尾部再加一个随机数,

        return s1.ToString();
    }

    public static string Decrypt(string cipher) {    
        StringBuilder sb = new StringBuilder(cipher.Length + 2);
        sb.Append (cipher);

        //奇偶位处理
        for (int i = 0; i < sb.Length; i++) {
            if (i % 2 == 0)
            {
                sb[i] -= (char)2;
            }
            else {
                if (sb[i] >= 65 && sb[i] <= 90)
                {
                    sb[i] = (char)(sb[i] + 32);
                }
                else if (sb[i] >= 97 && sb[i] <= 122)
                {
                    sb[i] = (char)(sb[i] - 32);
                }

                sb[i] -= (char)1;
            }


            switch (sb[i])
            {

                case (char)63:
                    sb[i] = (char)89;
                    break;
                case (char)64:
                    sb[i] = (char)90;
                    break;
                case (char)95:
                    sb[i] = (char)121;
                    break;
                case (char)96:
                    sb[i] = (char)122;
                    break;
            }

        }


        //移除首尾
        sb.Remove(0, 1);
        sb.Remove(sb.Length - 1, 1);

        return string.Join("",sb.ToString().Reverse());//反转
    }
}

