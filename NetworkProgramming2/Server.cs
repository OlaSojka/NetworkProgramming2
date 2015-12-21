using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace NetworkProgramming2
{
    public class Server
    {
        private static void UDPListener()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress[] ipList = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress ip = null;
            foreach (var x in ipList)
            {
                if (x.AddressFamily == AddressFamily.InterNetwork)
                {
                 //   Console.WriteLine(ip.ToString());
                    ip = x;
                }
            }
            Console.WriteLine("My IP address is: {0}, and I'm listening on port 7", ip);
            EndPoint recv = (EndPoint) (new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            s.Bind(endPoint);
            int port = 5005;

            while (true)
            {
                byte[] buff = new byte[20];
                s.ReceiveFrom(buff, ref recv);
                String msg = Encoding.ASCII.GetString(buff).TrimEnd('\0');
                //Console.WriteLine(msg + "..............");
                if (msg.Equals("DISCOVER"))
                {
                    int nowyport = port;
                    Console.WriteLine("Received broadcast from {0} : {1}", recv.ToString(), msg);
                    s.SendTo(Encoding.ASCII.GetBytes("OFFER-" + port), recv);
                    Thread tcp = new Thread(
                        o =>
                        {
                            tcpConnect(ip, nowyport);
                        });
                    port++;
                    tcp.Start();
                }
            }
        }

        public static bool PortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        public static void tcpConnect(IPAddress ip, int port)
        {
           // Console.WriteLine(port);
            Socket tcpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint tcpendEndPoint = new IPEndPoint(ip, port);
            tcpsocket.Bind(tcpendEndPoint);
            tcpsocket.Listen(1);
            String nick = "";
            try
            {
                Socket handler = tcpsocket.Accept();
                byte[] bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                nick = Encoding.ASCII.GetString(bytes, 0, bytesRec).Split(':')[1];
                while (true)
                {
                    int bytesRec2 = handler.Receive(bytes);
                    Console.WriteLine(Encoding.ASCII.GetString(bytes, 0, bytesRec2));
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused ||
                    e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Console.WriteLine("{0} disconnected.", nick);
                }
            }
        }

       /* static void CheckTcp()
        {
            Socket tcpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            while(!tcpsocket.Poll(0, SelectMode.SelectRead))
                Console.WriteLine(tcpsocket.Poll(0, SelectMode.SelectRead));
        }*/

        public static int Main(String[] args)
        {
            Thread udp = new Thread(new ThreadStart(UDPListener));
            //UDPListener();
            udp.Start();

            Console.ReadKey();
            return 0;

        }
    }
    
}