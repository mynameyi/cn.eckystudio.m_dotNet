using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EckyStudio.M.BaseModel.log.email
{
    public static class WindowsUtils
    {

        private const int INTERNET_CONNECTION_MODEM = 1;
        private const int INTERNET_CONNECTION_LAN = 2;

        [System.Runtime.InteropServices.DllImport("winInet.dll")]
        private static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        /// <summary>
        /// 判断本地的连接状态
        /// </summary>
        /// <returns></returns>
        public static bool IsNetworkAvailable()
        {
            System.Int32 dwFlag = new Int32();
            if (!InternetGetConnectedState(ref dwFlag, 0))
            {
                Console.WriteLine("LocalConnectionStatus--未连网!");
                return false;
            }
            else
            {
                //if ((dwFlag & INTERNET_CONNECTION_MODEM) != 0)
                //{
                //    Console.WriteLine("LocalConnectionStatus--采用调制解调器上网。");
                //    return true;
                //}
                //else if ((dwFlag & INTERNET_CONNECTION_LAN) != 0)
                //{
                //    Console.WriteLine("LocalConnectionStatus--采用网卡上网。");
                //    return true;
                //}
                return true;
            }
            //return false;
        }
    }
}
