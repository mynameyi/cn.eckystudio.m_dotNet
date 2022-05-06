using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


public class SetupapiWrapper
{
    [DllImport("kernel32.dll")]
    private extern static int GetLastError();

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, UInt32 iEnumerator, IntPtr hParent, UInt32 nFlags);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, UInt32 dwIndex, SP_DEVINFO_DATA devInfoData);

    [DllImport("setupapi.dll", SetLastError = true)]
    public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, UInt32 Property,
        out UInt32 PropertyRegDataType, byte[] PropertyBuffer, UInt32 PropertyBufferSize, out Int32 RequiredSize);

    //// 〈summary〉
    /// 设备信息数据
    /// 〈/summary〉
    [StructLayout(LayoutKind.Sequential)]
    public class SP_DEVINFO_DATA
    {
        public int cbSize;
        public Guid classGuid;
        public int devInst;
        public ulong reserved;
    };

    //DIGCF_DEFAULT = 0x00000001;          // 只返回与系统默认设备相关的设备
    //DIGCF_PRESENT = 0x00000002;          // 只返回当前存在的设备
    //DIGCF_ALLCLASSES = 0x00000004;       // 返回所有已安装的设备
    //DIGCF_PROFILE = 0x00000008;          // 只返回当前硬件配置文件中的设备
    //DIGCF_DEVICEINTERFACE = 0x00000010;  // 返回所有支持的设备

    private const int MAX_DEV_LEN = 1000;//返回值最大长度

    static byte[] mBuffer = new byte[2048];

    public static SetupComInfo[] GetDevice() {

        Guid guid = System.Guid.Parse("4d36e978-e325-11ce-bfc1-08002be10318");//获取Ports类型设备
        IntPtr hDevInfo = SetupDiGetClassDevs(ref guid, 0, IntPtr.Zero, 0x00000002);

        if (hDevInfo == IntPtr.Zero)
            return null;

        SP_DEVINFO_DATA devInfoData;
        devInfoData = new SP_DEVINFO_DATA();

        //DeviceInfoData.cbSize = 28;
        //if (Environment.Is64BitOperatingSystem)
        //    devInfoData.cbSize = 32;//(16,4,4,4)    
        //else
        devInfoData.cbSize = 28;

        devInfoData.devInst = 0;
        devInfoData.classGuid = guid;
        devInfoData.reserved = 0;

        //StringBuilder property = new StringBuilder("");
        //property.Capacity = MAX_DEV_LEN;
        //StringBuilder propBuffer = new StringBuilder();

        //UInt32 i;
        uint dataType;
        int receiveSize;
        string manufacturor = "";

        List<SetupComInfo> infos = new List<SetupComInfo>();

        for (uint i = 0; SetupDiEnumDeviceInfo(hDevInfo, i, devInfoData); i++) {
            if (SetupDiGetDeviceRegistryProperty(hDevInfo, devInfoData, (uint)SPDRP.SPDRP_MFG, out dataType, mBuffer, (uint)mBuffer.Length, out receiveSize))
            {
                manufacturor = Encoding.UTF8.GetString(mBuffer, 0, receiveSize - 1);
            }

            if (!"Qualcomm Incorporated".Equals(manufacturor)) {
                continue;
            }

            SetupComInfo info = new SetupComInfo();
            if (SetupDiGetDeviceRegistryProperty(hDevInfo, devInfoData, (uint)SPDRP.SPDRP_ADDRESS, out dataType, mBuffer, (uint)mBuffer.Length, out receiveSize))
            {
                info.Address = BitConverter.ToInt32(mBuffer, 0);
            }

            if (SetupDiGetDeviceRegistryProperty(hDevInfo, devInfoData, (uint)SPDRP.SPDRP_FRIENDLYNAME, out dataType, mBuffer, (uint)mBuffer.Length, out receiveSize))
            {
                info.Name = Encoding.UTF8.GetString(mBuffer, 0, receiveSize - 1);
            }

            infos.Add(info);
        }


        return infos.ToArray();
        //int i3 = GetLastError();
    }



    public enum DIGCF
    {
        DIGCF_DEFAULT = 0x1,
        DIGCF_PRESENT = 0x2,
        DIGCF_ALLCLASSES = 0x4,
        DIGCF_PROFILE = 0x8,
        DIGCF_DEVICEINTERFACE = 0x10
    }

    public enum SPDRP : uint
    {
        SPDRP_DEVICEDESC = 0,
        SPDRP_HARDWAREID = 0x1,
        SPDRP_COMPATIBLEIDS = 0x2,
        SPDRP_UNUSED0 = 0x3,
        SPDRP_SERVICE = 0x4,
        SPDRP_UNUSED1 = 0x5,
        SPDRP_UNUSED2 = 0x6,
        SPDRP_CLASS = 0x7,
        SPDRP_CLASSGUID = 0x8,
        SPDRP_DRIVER = 0x9,
        SPDRP_CONFIGFLAGS = 0xA,
        SPDRP_MFG = 0xB,
        SPDRP_FRIENDLYNAME = 0xC,
        SPDRP_LOCATION_INFORMATION = 0xD,
        SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0xE,
        SPDRP_CAPABILITIES = 0xF,
        SPDRP_UI_NUMBER = 0x10,
        SPDRP_UPPERFILTERS = 0x11,
        SPDRP_LOWERFILTERS = 0x12,
        SPDRP_BUSTYPEGUID = 0x13,
        SPDRP_LEGACYBUSTYPE = 0x14,
        SPDRP_BUSNUMBER = 0x15,
        SPDRP_ENUMERATOR_NAME = 0x16,
        SPDRP_SECURITY = 0x17,
        SPDRP_SECURITY_SDS = 0x18,
        SPDRP_DEVTYPE = 0x19,
        SPDRP_EXCLUSIVE = 0x1A,
        SPDRP_CHARACTERISTICS = 0x1B,
        SPDRP_ADDRESS = 0x1C,
        SPDRP_UI_NUMBER_DESC_FORMAT = 0x1E,
        SPDRP_MAXIMUM_PROPERTY = 0x1F
    }
    public enum DICS
    {
        DICS_ENABLE = 0x1,
        DICS_DISABLE = 0x2,
        DICS_PROPCHANGE = 0x3,
        DICS_START = 0x4,
        DICS_STOP = 0x5,
        DICS_FLAG_GLOBAL = DICS_ENABLE,
        DICS_FLAG_CONFIGSPECIFIC = DICS_DISABLE,
        DICS_FLAG_CONFIGGENERAL = DICS_START
    }
    public enum DIF
    {
        DIF_SELECTDEVICE = 0x1,
        DIF_INSTALLDEVICE = 0x2,
        DIF_ASSIGNRESOURCES = 0x3,
        DIF_PROPERTIES = 0x4,
        DIF_REMOVE = 0x5,
        DIF_FIRSTTIMESETUP = 0x6,
        DIF_FOUNDDEVICE = 0x7,
        DIF_SELECTCLASSDRIVERS = 0x8,
        DIF_VALIDATECLASSDRIVERS = 0x9,
        DIF_INSTALLCLASSDRIVERS = 0xA,
        DIF_CALCDISKSPACE = 0xB,
        DIF_DESTROYPRIVATEDATA = 0xC,
        DIF_VALIDATEDRIVER = 0xD,
        DIF_MOVEDEVICE = 0xE,
        DIF_DETECT = 0xF,
        DIF_INSTALLWIZARD = 0x10,
        DIF_DESTROYWIZARDDATA = 0x11,
        DIF_PROPERTYCHANGE = 0x12,
        DIF_ENABLECLASS = 0x13,
        DIF_DETECTVERIFY = 0x14,
        DIF_INSTALLDEVICEFILES = 0x15,
        DIF_UNREMOVE = 0x16,
        DIF_SELECTBESTCOMPATDRV = 0x17,
        DIF_ALLOW_INSTALL = 0x18,
        DIF_REGISTERDEVICE = 0x19,
        DIF_NEWDEVICEWIZARD_PRESELECT = 0x1A,
        DIF_NEWDEVICEWIZARD_SELECT = 0x1B,
        DIF_NEWDEVICEWIZARD_PREANALYZE = 0x1C,
        DIF_NEWDEVICEWIZARD_POSTANALYZE = 0x1D,
        DIF_NEWDEVICEWIZARD_FINISHINSTALL = 0x1E,
        DIF_UNUSED1 = 0x1F,
        DIF_INSTALLINTERFACES = 0x20,
        DIF_DETECTCANCEL = 0x21,
        DIF_REGISTER_COINSTALLERS = 0x22,
        DIF_ADDPROPERTYPAGE_ADVANCED = 0x23,
        DIF_ADDPROPERTYPAGE_BASIC = 0x24,
        DIF_RESERVED1 = 0x25,
        DIF_TROUBLESHOOTER = 0x26,
        DIF_POWERMESSAGEWAKE = 0x27
    }
}

public class SetupComInfo{
    public string Name;
    public int Address;
}

public abstract class DeviceInfo {
    public abstract void Fill();
}

