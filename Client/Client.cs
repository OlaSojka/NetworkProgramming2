using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using System.Timers;

namespace Client
{
    class Client
    {
        private static Socket udpSocket;

        static void Main(string[] args)
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpSocket.ReceiveTimeout = 1000;
            IPAddress ip = IPAddress.Broadcast;
            IPEndPoint udpSend = new IPEndPoint(ip, 7);
            udpSocket.SendTo(Encoding.ASCII.GetBytes("DISCOVER"), udpSend);
            Console.WriteLine("Broadcast DISCOVER message sent.");
            byte[] buff = new byte[20];
            EndPoint recv = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
            //while (true)
            Stopwatch s = new Stopwatch();
            s.Start();
            int tcpport = 0;
            while (s.Elapsed < TimeSpan.FromSeconds(10))
            {
                try
                {
                    udpSocket.ReceiveFrom(buff, ref recv);
                    String[] msg = Encoding.ASCII.GetString(buff).TrimEnd('\0').Split('-');
                    tcpport = Convert.ToInt32(msg[1]);
                    Console.WriteLine(msg[0]);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut) { }
                    else
                        Console.WriteLine(e.StackTrace);
                }
            }
            Console.WriteLine("Olka chuj");
            s.Stop();
            IPAddress tcpip = IPAddress.Parse(recv.ToString().Split(':')[0].TrimEnd(' '));
            IPEndPoint tcEndPoint = new IPEndPoint(tcpip, tcpport);
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sender.Connect(tcEndPoint);
                while (true)
                {
                    sender.Send(Encoding.ASCII.GetBytes("dupa"));
                    Console.WriteLine("Wyslalem dupe");
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused ||
                    e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Console.WriteLine("no nie pyklo");
                }
            }
            Console.ReadKey();
        }
    }
}
