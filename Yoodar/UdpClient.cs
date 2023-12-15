using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp;
using System;
using System.Text;
using System.Threading;


namespace Yoodar
{
    public class UdpClient
    {
        public delegate void DelegateString(ushort index, SimplSharpString String);
        public DelegateString DelegateRx { get; set; }
        public DelegateString DelegateJsonRx { get; set; }

        UDPServer Server;

        public void init(String IP, ushort Port)
        {
            Server = new UDPServer(IP, Port, 5000);
            CrestronConsole.PrintLine("EnableServer for {0}:{1} {2}", IP, Port, Server.EnableUDPServer());
            CrestronConsole.PrintLine("ReceiveDataAsync for {0}:{1} {2}", IP, Port, Server.ReceiveDataAsync(DataReceived));
            HeartBeat();
        }
        public void HeartBeat()
        {
            byte[] heartbeat = new byte[3];
            heartbeat[0] = 0xcf;
            heartbeat[1] = 0x00;
            heartbeat[2] = 0xcf;
            Thread.Sleep(1000);
            Server.SendData(heartbeat, heartbeat.Length);
            while (true)
            {
                Thread.Sleep(10000);
                Server.SendData(heartbeat, heartbeat.Length);
            }
        }

        public void SendString(String s)
        {
            byte[] data = Encoding.UTF8.GetBytes(s);
            Server.SendData(data, data.Length);
        }

        public void InRx(ushort index,string rx)
        {
            string str = Substring(rx, "{", "}");
            DelegateJsonRx(index, str);
        }
        public string Substring(string text, string start, string end)
        {
            try
            {
                int IndexofA = text.IndexOf(start);
                int IndexofB = text.LastIndexOf(end);
                string NameText = "";
                if (IndexofA > 0 && IndexofB > 0)
                {
                    NameText = text.Substring(IndexofA, IndexofB + end.Length - IndexofA);
                }
                return NameText;
            }
            catch (Exception e)
            {
                throw;
                CrestronConsole.PrintLine(e.Message);
            }
        }

        public void DataReceived(UDPServer myUDPServer, int ByteCount)
        {
            byte[] rxBuffer = new byte[ByteCount];
            //var rxToSplus = new SimplSharpString();
            string rxToSplus;
            string[] ChanneData = new string[ByteCount];
            if (ByteCount > 10)//过滤掉心跳
            {
                rxBuffer = Server.IncomingDataBuffer;
                int Address = rxBuffer[1];
                rxToSplus = Encoding.UTF8.GetString(rxBuffer, 0, ByteCount);
                if (Address >= 0 && Address <= 7)
                {
                    //ChanneData[Address] = rxToSplus.ToString();
                    DelegateRx((ushort)Address, rxToSplus);
                    InRx((ushort)Address, rxToSplus);
                    //CrestronConsole.PrintLine(rxToSplus);
                }
            }
            Server.ReceiveDataAsync(DataReceived);
        }
    }
}
