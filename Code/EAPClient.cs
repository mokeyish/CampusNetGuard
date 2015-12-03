using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CampusNetGuard.Code.Log;
using CampusNetGuard.Code.Libraries;
using CampusNetGuard.Libraries;
using LitePcap;
using System.Net;

namespace CampusNetGuard.Code
{
    enum EAPoLAuthType : byte
    {
        Verifing,
        Running,
        Success =100,
        Failure=200,
        UsernameNotExist=101,
        PasswordWrong=102,
        OperateTimeIntervalTooShort = 123,
        NoPermissionForTheTime =207,
        ServerNoResponding =250,
        DhcpServerNoResponding = 251,
        ClientSendFaild =252,
        GetIPAddressSuccess = 253,
    }

    class EAPClient
    {
        private string u;
        private string p;
        private byte[] username;
        private byte[] password;
        public string Username
        {
            get
            {
                return u;
            }
            set
            {
                username = value == null ? null : RunTime.Encode.GetBytes(value);
                u = value;
            }
        }
        public string Password
        {
            get
            {
                return p;
            }
            set
            {
                password = value == null ? null : RunTime.Encode.GetBytes(value);
                p = value;
            }
        }
        public string PcapName
        {
            get
            {
                return pcapHelper.Name;
            }
        }
        private bool IsOk
        {
            get
            {
                return username != null && password != null;
            }
        }
        public bool CmdOnline { get; private set; }
        public long OnlineTimeTicks { get; private set; }
        public void SetNow()
        {
            OnlineTimeTicks = DateTime.Now.Ticks;
        }
        public NetCardInterface Interface { get; private set; }

        private IPcapHelper pcapHelper;
        private readonly ILog logger;
        public  EAPClient(ILog logger,string name)
        {
            this.logger = logger;
            pcapHelper = new PcapHelper(name);
            pcapHelper.Filter = "ether proto 0x888e or (udp src port 67 and udp dst port 68)";
            pcapHelper.OnPacketCaptured += PacketHandler;
        }
        private Timer timer;
        private bool hasSent;
        private bool serverReponsed;
        public int Timeout = 15;
        private void SendPacket(ICaptureDevice device,byte[] packet)
        {
            hasSent = false;
            serverReponsed = false;
            if (timer == null)
            {
                timer = new Timer((o) => {
                    if (!isActive) return;
                    if (!hasSent)
                    {
                        FireEvent(EAPoLAuthType.ClientSendFaild);
                    }else if (!serverReponsed)
                    {
                        FireEvent(EAPoLAuthType.ServerNoResponding);
                    }
                });
            }
            timer.Change(TimeSpan.FromSeconds(Timeout), TimeSpan.FromSeconds(0));
            pcapHelper.SendPacket(device, packet);
        }

        private bool isActive = false;
        private void PacketHandler(object sender, CaptureEventArgs e)
        {
            if (e.Packet.Data[12] == 0x88 && e.Packet.Data[13] == 0x8e)
            {
                if (!IsOk) return;
                lock (this)
                {
                    //
                    switch (e.Packet.Data[15])
                    {
                        case 0x00:
                            //EAP Packet
                            switch (e.Packet.Data[18])
                            {
                                case 0x01:
                                    //Request
                                    switch (e.Packet.Data[22])
                                    {
                                        case 0x01:
                                            //Indetity
                                            if (!isActive) return;
                                            serverReponsed = true;
                                            FireEvent(EAPoLAuthType.Running);
                                            logger.Info("Server:Request Identity.");
                                            SendPacket(e.Device, ResponseIdentityPacket(e.Packet.Data));
                                            pcapHelper.SelectDevice(e.Device);
                                            break;
                                        case 0x02:
                                            //notification
                                            serverReponsed = true;
                                            FireEvent(EAPoLAuthType.Running);
                                            logger.Info(string.Format("Server Notification:{0}",FormatMessage(Encoding.UTF8.GetString(Utils.CutByteArray(e.Packet.Data, 23, e.Packet.Data[20] * 256 + e.Packet.Data[21] - 5)),false)));
                                            break;
                                        case 0x04:
                                            //Md5
                                            serverReponsed = true;
                                            FireEvent(EAPoLAuthType.Running);
                                            logger.Info("Server:Request Md5-Challenge.");
                                            SendPacket(e.Device, ResponseMd5Packet(e.Packet.Data));
                                            break;
                                    }
                                    break;
                                case 0x02:
                                    //Response
                                    switch (e.Packet.Data[22])
                                    {
                                        case 0x01:
                                            //Indentity
                                            hasSent = true;
                                            FireEvent(EAPoLAuthType.Running);
                                            logger.Info(string.Format("Client:Response Identity,{0}.", Username));
                                            break;
                                        case 0x04:
                                            //Md5
                                            hasSent = true;
                                            FireEvent(EAPoLAuthType.Running);
                                            logger.Info("Server:Response Md5-Challenge.");
                                            break;
                                    }
                                    break;
                                case 0x03:
                                    serverReponsed = true;
                                    OnlineTimeTicks = DateTime.Now.Ticks;
                                    logger.Info("Server:Success.");
                                    pcapHelper.SelectDevice(e.Device);
                                    RefreshInterface();
                                    FireEvent(EAPoLAuthType.Success);
                                    break;
                                case 0x04://失败
                                    serverReponsed = true;
                                    OnlineTimeTicks = 0;
                                    logger.Info(string.Format("Server：Failure.{0}", FormatMessage(Encoding.UTF8.GetString(Utils.CutByteArray(e.Packet.Data, 22, e.Packet.Data[20] * 256 + e.Packet.Data[21] - 4)), true)));
                                    
                                    break;
                            }
                            break;
                        case 0x01:
                            //Start
                            hasSent = true;
                            FireEvent(EAPoLAuthType.Running);
                            logger.Info("Client:EAPoL Start.");
                            break;
                        case 0x02:
                            //Logoff
                            hasSent = true;
                            FireEvent(EAPoLAuthType.Running);
                            logger.Info("Client:EAPoL Stop.");
                            break;
                        case 0x03:
                            switch (e.Packet.Data[18])
                            {
                                case 0x01:
                                    if (Utils.CompareByteArray(e.Device.MacAddress.GetAddressBytes(), e.Packet.Data, 0))
                                    {
                                        RunTime.Logger.Info("Server：Rc4Key!");
                                        SendPacket(e.Device, ResponseRc4KeyPacket(e.Packet.Data));
                                    }
                                    else
                                    {
                                        hasSent = true;
                                        serverReponsed = true;
                                        RunTime.Logger.Info("Client：Rc4Key!");
                                        FireEvent(EAPoLAuthType.Verifing);
                                    }
                                    break;
                            }
                            break;
                    }


                    //
                }
            }
            else
            {
                //dhcp
                //收到
                if (e.Packet.Data[42] == 0x02)
                {
                    //接收的
                    IsDHCPServerResponsed = true;
                    isOnDhcping = true;
                    Func<DhcpType> m =() => {
                        int p = 282;
                        while (p < e.Packet.Data.Length)
                        {
                            switch (e.Packet.Data[p++])
                            {
                                case 0x35:
                                    p++;
                                    switch (e.Packet.Data[p++])
                                    {
                                        case 0x01:
                                            return DhcpType.Discover;
                                        case 0x02:
                                            return DhcpType.Offer;
                                        case 0x03:
                                            return DhcpType.Request;
                                        case 0x05:
                                            return DhcpType.Ack;
                                        default:
                                            return DhcpType.None;
                                    }
                                default:
                                    p += ++e.Packet.Data[p++];
                                    break;
                            }
                        }
                        return DhcpType.Ack;
                    };
                    if (m() == DhcpType.Ack)
                    {
                        logger.Info("DHCP获取ip成功!");
                        RefreshInterface();
                        SendNetworkInformation();
                        FireEvent(EAPoLAuthType.GetIPAddressSuccess);
                        //保存ip记录
                        IPAddressRecords ipr = new IPAddressRecords();
                        ipr.Read();
                        ipr.Add(Interface);
                        ipr.Save();
                        isOnDhcping = false;

                    }
                }
            }
        }
        private void FireEvent(EAPoLAuthType t)
        {
            if (OnEventFired != null)
            {
                OnEventFired(this, (byte)t);
            }
        }
        public event EventHandler<byte> OnEventFired;
        public void RefreshInterface()
        {
            Interface.Refresh(PcapName);
        }
        private bool IsDHCPServerResponsed = false;
        private bool isOnDhcping = false;
        public async void GetIPAddressFromServer()
        {
            await Task.Run(() => {
                if (isOnDhcping) return;
                logger.Info("正在dhcp获取ip中...");
                IsDHCPServerResponsed = false;
                CmdNativeMethods.GetIPByDHCP();
                Thread.Sleep(10000);
                if (!IsDHCPServerResponsed)
                {
                    isOnDhcping = false;
                    logger.Info("DHCP服务器无响应！");
                    logger.Info("请根据已保存的历史IP记录手动设置！");
                }
            });
        }
        private bool opened = false;
        public void Open()
        {
            if (opened) return;
            pcapHelper.Open();
            Interface = new NetCardInterface();
            RefreshInterface();

            SendNetworkInformation();
            opened = true;
        }
        public void Close()
        {
            pcapHelper.Close();
        }
        public void Login()
        {
            if (!opened) Open();
            if (IsOk)
            {
                lock (this)
                {
                    isActive = true;
                    logger.Debug("内部认证开始");
                    SendPacket(null, StartPacket());
                }
            }
            else
            {
                logger.Info("用户名或者密码为空!");
            }
        }
        public void Logout()
        {
            lock (this)
            {
                isActive = false;
                logger.Debug("内部认证停止");
                SendPacket(null, StopPacket());
            }
        }


        private void SendNetworkInformation()
        {
            if (Interface.FriendlyName == null) return;
            logger.Info(string.Format("◎网络设备:{0}", Interface.FriendlyName));
            logger.Info(string.Format("◎Ip地址:{0}", Interface.IPAddress));
            logger.Info(string.Format("◎掩码:{0}", Interface.Netmask));
            logger.Info(string.Format("◎网关:{0}", Interface.GatewayAddress));
            foreach (var item in Interface.Dns) if (item != IPAddress.Any) logger.Info(string.Format("◎Dns:{0}", item));
        }
        private string FormatMessage(string source,bool isFireEvent)
        {
            if (string.IsNullOrEmpty(source))
            {
                if (isFireEvent) FireEvent(EAPoLAuthType.Failure);
                return string.Empty;
            }
            else if (source.Contains("101"))
            {
                username = null;
                if (isFireEvent) FireEvent(EAPoLAuthType.UsernameNotExist);
                return "错误码101: 用户名不存在";
            }
            else if (source.Contains("102"))
            {
                password = null;
                if (isFireEvent) FireEvent(EAPoLAuthType.PasswordWrong);
                return "错误码102:密码错误";
            }
            else if (source.Contains("207"))
            {
                if (isFireEvent) FireEvent(EAPoLAuthType.NoPermissionForTheTime);
                return "错误码207:账号没有在该时段接入的权限";
            }
            else if (source.Contains("123"))
            {
                if (isFireEvent) FireEvent(EAPoLAuthType.OperateTimeIntervalTooShort);
                return "错误码123:两次认证间隔时间过短";
            }
            if (isFireEvent) FireEvent(EAPoLAuthType.Failure);
            return source;
        }
        
        private byte[] StartPacket()
        {
            return new byte[]{
                0x01, 0x80, 0xc2, 0x00, 0x00, 0x03, //  0   目标Mac地址
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  6   本机Mac地址
                0x88, 0x8e,                         //  12  协议类型
                0x01,                               //  14  版本
                0x01,                               //  15  数据包类型:Sart
                0x00, 0x00                          //  16  数据包长度
            };
        }
        private byte[] StopPacket()
        {
            return new byte[]{
                0x01, 0x80, 0xc2, 0x00, 0x00, 0x03, //  0   目标Mac地址
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  6   本机Mac地址
                0x88, 0x8e,                         //  12  协议类型
                0x01,                               //  14  版本
                0x02,                               //  15  数据包类型:logoff
                0x00, 0x00                          //  16  数据包长度
            };
        }
        private byte[] ResponseIdentityPacket(byte[] rawPacket)
        {

            //复制用户名到数据包尾部，并且更新数据包的长度信息
            byte[] packet, tmp;
            tmp = new byte[]{
                    0x01, 0x80, 0xc2, 0x00, 0x00, 0x03, //  0   目标Mac地址
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  6   本机Mac地址
                    0x88, 0x8e,                         //  12  协议类型
                    0x01,                               //  14  版本
                    0x00,                               //  15  EAP包类型：EAP Packet
                    0x00, 0x00,                         //  16  EAP包长度
                    0x02,                               //  18  EAP包Code：Response
                    0x00,                               //  19  EAP包ID
                    0x00, 0x00,                         //  20  EAP包长度
                    0x01                                //  22  EAP包类型：Identity
                };
            //生成一个可以存放用户名的packet
            packet = new byte[tmp.Length + username.Length];
            //复制数据包模板到packet
            Buffer.BlockCopy(tmp, 0, packet, 0, tmp.Length);
            //复制用户名到packet
            Buffer.BlockCopy(username, 0, packet, tmp.Length, username.Length);

            //设置数据包长度，为用户名长度+5；
            //长度一般不会大于256，省略此句
            //packet[16] = packet[20] = (byte)((acc.Length+5) >> 0x08);
            packet[17] = packet[21] = (byte)((username.Length + 5) & 0xff);

            //复制源数据包ID
            packet[19] = rawPacket[19];
            return packet;
        }

        private byte[] ResponseMd5Packet(byte[] rawPacket)
        {

            byte[] packet, tmp;
            tmp = new byte[]{
                    0x01, 0x80, 0xc2, 0x00, 0x00, 0x03,             //  0   目标Mac地址
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00,             //  6   本机Mac地址
                    0x88, 0x8e,                                     //  12  协议类型
                    0x01,                                           //  14  版本
                    0x00,                                           //  15  EAP包类型
                    0x00, 0x00,                                     //  16  EAP包长度
                    0x02,                                           //  18  EAP包Code：Response  
                    0x00,                                           //  19  EAP包ID
                    0x00, 0x00,                                     //  20  EAP包长度
                    0x04,                                           //  22  EAP包类型：MD5
                    0x10,                                           //  23  Md5值长度
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  24  MD5 前8位
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00  //  32  MD5 后8位
                };
            packet = new byte[tmp.Length + username.Length];
            //复制模板到packet
            Buffer.BlockCopy(tmp, 0, packet, 0, tmp.Length);
            //复制用户名到packet
            Buffer.BlockCopy(username, 0, packet, tmp.Length, username.Length);
            //设置数据包长度，为用户名长度+5；
            //长度一般不会大于256，省略此句
            //_Md5PacketCache[16] = _Md5PacketCache[20] = (byte)((_Acc.Length+22) >> 0x08);
            packet[17] = packet[21] = (byte)((username.Length + 22) & 0xff);

            //配置一下Md5加密源，由ID，_Pwd,Zte常量,源包md5value
            tmp = new byte[10 + password.Length + rawPacket[23]];

            //复制源数据包ID到此包和加密源
            packet[19] = tmp[0] = rawPacket[19];
            //复制密码到加密源
            Buffer.BlockCopy(password, 0, tmp, 1, password.Length);
            Buffer.BlockCopy(RunTime.Encode.GetBytes("zte142052"), 0, tmp, password.Length + 1, 9);
            //复制源数据包Md5值到加密源
            Buffer.BlockCopy(rawPacket, 24, tmp, tmp.Length - rawPacket[23], rawPacket[23]);
            using (System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                Buffer.BlockCopy(md5.ComputeHash(tmp), 0, packet, packet.Length - rawPacket[23] - username.Length, rawPacket[23]);
                return packet;
            }
        }


        private byte[] _ResponseRc4KeyT;
        private byte[] _ResponseRc4KeySourceT;
        private byte[] _ResponseRc4KeyPacketSourceT;
        private byte[] _ResponseRc4KeyPacketT;
        private byte[] _KeyIndex;
        private System.Security.Cryptography.HMACMD5 _HmacMd5;
        private RC4Crypt _Rc4Crypt;
        //需要缓存
        private byte[] ResponseRc4KeyPacket(byte[] rawPacket)
        {

            if (_ResponseRc4KeyPacketT == null)
            {
                _ResponseRc4KeyPacketT = new byte[]{
                        0x01, 0x80, 0xc2, 0x00, 0x00, 0x03,             //  0   目标Mac地址
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00,             //  6   本机Mac地址
                        0x88, 0x8e,                                     //  12  协议类型
                        0x01,                                           //  14  版本
                        0x03,                                           //  15  EAP包类型
                        0x00, 0x30,                                     //  16  EAP包长度
                        0x01,                                           //  18  Key Descripitor Type:RC4 Descriptor(1)
                        0x00, 0x04,                                     //  19  Key的长度
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  21  Replay Counter
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  29  Key IV 前8位
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  37  Key Iv 后8位
                        0x00,                                           //  45  Key Index
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  46  Key Signature 前8位
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //  54  Key Signature 后8位
                        0x00, 0x00, 0x00, 0x00                          //  62  Key
                    };

                _ResponseRc4KeySourceT = new byte[] { 0x02, 0x02, 0x14, 0x00 };
                _ResponseRc4KeyT = new byte[20];
                _KeyIndex = new byte[1];
                _ResponseRc4KeyPacketSourceT = new byte[_ResponseRc4KeyPacketT.Length - 14];
                _HmacMd5 = new System.Security.Cryptography.HMACMD5();
            }
            if (_Rc4Crypt == null)
            {
                _Rc4Crypt = new RC4Crypt();
            }

            //存入Replay Counter  +  Key IV ，24字节
            Buffer.BlockCopy(rawPacket, 21, _ResponseRc4KeyPacketT, 21, 24);

            //取出keyIndex到key，并存入keyIndex
            _ResponseRc4KeyPacketT[45] = _KeyIndex[0] = rawPacket[45];

            //取出Rc4Key基于（Key IV + Key IV最后四个字节）==20字节
            Buffer.BlockCopy(rawPacket, 29, _ResponseRc4KeyT, 0, 16);
            Buffer.BlockCopy(_ResponseRc4KeyT, 12, _ResponseRc4KeyT, 16, 4);


            //使用rc4算法生成4位的key,
            _Rc4Crypt.Key = _ResponseRc4KeyT;
            //生成并在数据包存入key
            Buffer.BlockCopy(_Rc4Crypt.EncryptByte(_ResponseRc4KeySourceT), 0, _ResponseRc4KeyPacketT, 62, 4);

            //使用hmac_md5算法生成Key Signature，此用于包的校验
            _HmacMd5.Key = _KeyIndex;
            Buffer.BlockCopy(_ResponseRc4KeyPacketT, 14, _ResponseRc4KeyPacketSourceT, 0, _ResponseRc4KeyPacketSourceT.Length);
            //将数据包的Key Signature归零
            for (int i = 32; i < 48; i++)
            {
                _ResponseRc4KeyPacketSourceT[i] = 0x00;
            }
            //计算Key Signature并向数据包存入Key Signature
            Buffer.BlockCopy(_HmacMd5.ComputeHash(_ResponseRc4KeyPacketSourceT), 0, _ResponseRc4KeyPacketT, 46, 16);
            return _ResponseRc4KeyPacketT;
        }

        


    }
}
