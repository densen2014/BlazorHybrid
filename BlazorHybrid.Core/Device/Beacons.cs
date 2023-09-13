using System;
using System.Linq;

namespace Beacons;

public class iBeaconDataBase
{
    public string? Name { get; set; }

    public Guid ID { get; set; }

    /// <summary>
    /// CompanyId[0~1]：0x004C（Raw中小端排列:4C00）
    /// </summary>
    public int CompanyID { get; set; }

    /// <summary>
    /// DeviceType[2]：0x02(表示Beacon)
    /// </summary>
    public int DeviceType { get; set; }

    /// <summary>
    /// [DataLen[3]: 0x15(表示数据长度，uuid+major+minor+measured power 字节数)
    /// </summary>
    public int DataLen { get; set; }

    /// <summary>
    /// UUID[4~19] : iBeacon UUID
    /// </summary>
    public Guid UUID { get; set; }

    /// <summary>
    /// Major[20~21]: iBeacon Major
    /// </summary>
    public ushort Major { get; set; }

    /// <summary>
    /// Minor[21~22]: iBeacon Minor
    /// </summary>
    public ushort Minor { get; set; }

    /// <summary>
    /// Measured power[23]: iBeacon 测量功率( RSSI At 1M) 发射端和接收端相隔1米时的信号强度
    /// </summary>
    public int TxPower { get; set; }


    public double Distance { get; set; }

    /// <summary>
    /// 无线接收的信号强度指示, 信号强度,蓝牙天线发射功率, -20、-16、-12、-8、-4、0、   4
    /// 接收信号强度（负值）
    /// </summary>
    public int Rssi { get; set; }

    public string? Tostring { get; set; }

    public DateTime? Timestamp { get; set; }

    public bool IsiBeacon{ get; set; }
}

public sealed class iBeaconData: iBeaconDataBase
{

    public string? CompleteLocalName { get; set; }

    /// <summary>
    /// [0]：电池电量百分比(0~100)
    /// </summary>
    public int 电池电量百分比 { get; set; }

    /// <summary>
    /// [1]:  标识(0x73)
    /// </summary>
    public string? 标识 { get; set; }

    /// <summary>
    /// [2]：广播间隔(ms)/100 的值(例如：2表示当前广播间隔是  200ms)
    /// </summary>
    public int 广播间隔 { get; set; }

    /// <summary>
    /// [3~8]: 6字节用户数据自定义段（默认为MAC地址）
    /// </summary>
    public string? MAC地址 { get; set; }

    //[9]: 位有效，传感器配置。
    //     Bit[5~7] 未使用
    //     bit[4] 0:当前是可连接模式,1:当前是不可连接模式(30    秒后只有广播包)
    //     bit[3] 加速度传感器是否已连接
    //     bit[2] 是否使能加速度传感器调整广播间隔功能
    //     bit[1] 温湿度传感器是否已连接
    //     bit[0] 是否打开温湿度传感器检测
    public int 传感器配置 { get; set; }

    /// <summary>
    /// [10]: 00:加速度未移动，01加上正在移动
    /// </summary>
    public int 加速度 { get; set; }

    /// <summary>
    /// [11]: 最后一次检测到的温度（有符号）
    /// </summary>
    public int 温度 { get; set; }

    /// <summary>
    /// [12]: 最后一次检测到的湿度%
    /// </summary>
    public int 湿度 { get; set; } 
     
}
public static class iBeaconExtensions
{
#if WINDOWS
    //public static void iBeaconSetAdvertisement(this BluetoothLEAdvertisement Advertisment, iBeaconData data)
    //{
    //    BluetoothLEManufacturerData manufacturerData = new BluetoothLEManufacturerData();

    //    // Set Apple as the manufacturer data
    //    manufacturerData.CompanyId = 76;

    //    var writer = new DataWriter();
    //    writer.WriteUInt16(0x0215); //bytes 0 and 1 of the iBeacon advertisment indicator

    //    if (data != null & data.UUID != Guid.Empty)
    //    {
    //        //If UUID is null scanning for all iBeacons
    //        writer.WriteBytes(data.UUID.ToByteArray());
    //        if (data.Major != 0)
    //        {
    //            //If Major not null searching with UUID and Major
    //            writer.WriteBytes(BitConverter.GetBytes(data.Major).Reverse().ToArray());
    //            if (data.Minor != 0)
    //            {
    //                //If Minor not null we are looking for a specific beacon not a class of beacons
    //                writer.WriteBytes(BitConverter.GetBytes(data.Minor).Reverse().ToArray());
    //                if (data.TxPower != 0)
    //                    writer.WriteBytes(BitConverter.GetBytes(data.TxPower));
    //            }
    //        }
    //    }

    //    manufacturerData.Data = writer.DetachBuffer();

    //    Advertisment.ManufacturerData.Clear();
    //    Advertisment.ManufacturerData.Add(manufacturerData);
    //}
#endif

    /// <summary>
    /// iBeacon规范的内容 
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="beacon"></param>
    /// <returns></returns>
    public static iBeaconData iBeaconParseAdvertisement(this byte[] bytes, iBeaconData beacon)
    {
        beacon.CompanyID = BitConverter.ToUInt16(bytes.Take(2).ToArray(), 0); //76 apple

        bytes =bytes.Skip(2).ToArray();

        //0x02 表示Beacon, 0x15 数据长度
        if (bytes[0] == 0x02 && bytes[1] == 0x15 && bytes.Length == 23)
        {
            beacon.DeviceType = (sbyte)bytes[0];
            beacon.DataLen = (sbyte)bytes[1];

            //iBeacon Data
            beacon.UUID = new Guid(bytes.Skip(2).Take(16).ToArray());
            beacon.Major = BitConverter.ToUInt16(bytes.Skip(18).Take(2).Reverse().ToArray(), 0);
            beacon.Minor = BitConverter.ToUInt16(bytes.Skip(20).Take(2).Reverse().ToArray(), 0);
            beacon.TxPower = (short)(sbyte)bytes[22]; 

            //Estimated value
            //Read this article http://developer.radiusnetworks.com/2014/12/04/fundamentals-of-beacon-ranging.html 
            beacon.Distance = CalculateDistance(beacon.TxPower, beacon.Rssi);
            beacon.Tostring = "UUID: " + beacon.UUID.ToString() + " Major: " + beacon.Major + " Minor:" + beacon.Minor + " Power: " + beacon.TxPower + " Rssi: " + beacon.Rssi + " Distance:" + beacon.Distance+ " MAC: " + beacon.MAC地址 + " ID: " + beacon.ID;
            //Debug.WriteLine(beacon.Tostring);
            beacon.Timestamp = DateTime.Now;
        }

        return beacon;
    }


    /// <summary>
    /// 蓝牙附加信息 
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="beacon"></param>
    /// <returns></returns>
    public static iBeaconData btParseAdvertisement(this byte[] bytes, iBeaconData beacon)
    {
        if (bytes.Length == 13)
        {
            //iBeacon Data
            beacon.电池电量百分比 = (sbyte)bytes[0];
            beacon.标识 = Convert.ToChar(bytes[1]).ToString();
            beacon.广播间隔 = (sbyte)bytes[2];
            var MAC地址 = bytes.Skip(3).Take(6).ToArray().ToHexString();
            if (!string.IsNullOrWhiteSpace (MAC地址))  beacon.MAC地址 = MAC地址;
            beacon.传感器配置 = (sbyte)bytes[9];
            beacon.加速度 = (sbyte)bytes[10];
            beacon.温度 = (sbyte)bytes[11];
            beacon.湿度 = (sbyte)bytes[12]; 
            //Debug.WriteLine("MAC地址: " + beacon.MAC地址 + " ID: " + beacon.ID);
            beacon.Timestamp = DateTime.Now;
        }

        return beacon;
    }

    /// <summary>
    /// 蓝牙RSSI转距离计算工具
    /// </summary>
    /// <param name="txPower">发射端和接收端相隔1米时的信号强度</param>
    /// <param name="rssi">接收信号强度（负值）</param>
    /// <returns></returns>
    internal static double CalculateDistance(int txPower, double rssi)
    {
        if (rssi == 0)
        {
            return -1.0; // if we cannot determine accuracy, return -1.
        }

        double ratio = rssi * 1.0 / txPower;
        if (ratio < 1.0)
        {
            return Math.Pow(ratio, 10);
        }
        else
        {
            double accuracy = (0.89976) * Math.Pow(ratio, 7.7095) + 0.111;
            return accuracy;
        }
    }

    /// <summary>
    /// Convert a byte array to a hex string.
    /// </summary>
    static string ToHexString(this byte[] bytes)
    {
        return bytes != null ? BitConverter.ToString(bytes) : string.Empty;
    }
}
