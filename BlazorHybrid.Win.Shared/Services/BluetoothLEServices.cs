using BlazorHybrid.Core.Device;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;


namespace BlazorHybrid.Maui.Shared;

/// <summary>
/// 蓝牙服务
/// </summary>
public partial class BluetoothLEServices
{
    public List<BleDevice> Devices { get; set; } = new List<BleDevice>();

    /// <summary>
    /// 特定查找的设备名称
    /// </summary>
    public BleTagDevice TagDevice { get; set; } = new BleTagDevice();

    /// <summary>
    /// <c>IAdapter</c> 中设备事件的事件参数，
    /// 例如<c>已播发设备</c>、<c>已发现设备</c>、<c>已连接设备</c>和<c>已断开设备</c>
    /// </summary>
    //public IDevice? Device = null;

    /// <summary>
    /// 设备信息
    /// </summary>
    public string? TagDeviceInfo { get; private set; }

    /// <summary>
    /// 读取设备名称结果
    /// </summary>
    public string? ReadDeviceNameResult { get; private set; }

    /// <summary>
    /// 通知特征值已更新结果
    /// </summary>
    public List<string> HbResults { get; } = new List<string>();

    /// <summary>
    /// 消息
    /// </summary>
    public event Action<string>? OnMessage;

    /// <summary>
    /// 数据接收
    /// </summary>
    public event Action<string>? OnDataReceived;

    /// <summary>
    /// 连接状态
    /// </summary>
    public event Action<bool>? OnStateConnect;

    private CancellationTokenSource? _scanForAedCts;

    public BluetoothLEServices()
    {
    }

    #region BLE状态管理



    #endregion

    #region 扫描外设
    void xxx()
    {
        //处理蓝牙的对象
        BluetoothClient client = new BluetoothClient();
        //获取电脑蓝牙
        BluetoothRadio radio = BluetoothRadio.PrimaryRadio;
        //设置电脑蓝牙可被搜索到
        radio.Mode = RadioMode.Connectable;
        //需要连接的蓝牙模块的唯一标识符
        BluetoothAddress? blueAddress = null;//= new BluetoothAddress(new byte[] { 0x8e, 0xed, 0x10, 0xa3, 0xa8, 0xaa });
                                            //搜索蓝牙设备，10秒
        BluetoothDeviceInfo[] devices = client.DiscoverDevices();
        //从搜索到的所有蓝牙设备中选择需要的那个
        //BarCode Scanner HID =》蓝牙设备名称
        foreach (var item in devices)
        {
            //textBox1.Text += $"{item.DeviceName} [{item.DeviceAddress}]{Environment.NewLine}";

            //根据蓝牙名字找
            if (item.DeviceName.Equals("BarCode Scanner HID"))
            {
                Console.WriteLine(item.DeviceAddress);
                Console.WriteLine(item.DeviceName);
                //获得蓝牙模块的唯一标识符
                blueAddress = item.DeviceAddress;
                break;
            }
        }
        if (blueAddress != null)
        {
            //BluetoothService.SerialPort根本无用
            BluetoothEndPoint ep = new BluetoothEndPoint(blueAddress, Guid.Parse("00001124-0000-1000-8000-00805f9b34fb"));
            //BluetoothEndPoint ep = new BluetoothEndPoint(blueAddress, BluetoothService.SerialPort);
            client.Connect(ep);//开始配对
            if (client.Connected)
            {
                //连接成功

            }
        }
    }
    #endregion

}
