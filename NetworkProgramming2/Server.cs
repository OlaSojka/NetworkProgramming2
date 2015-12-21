using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetworkProgramming2
{
    public class Server
    {
        private static void UDPListener()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            var host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] ipList = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress ipchuj = null;
            foreach (var ip in ipList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.WriteLine(ip.ToString());
                    ipchuj = ip;
                }
            }
            EndPoint recv = (EndPoint) (new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            s.Bind(endPoint);

            while (true)
            {
                byte[] buff = new byte[20];
                int port = 5005;
                s.ReceiveFrom(buff, ref recv);
                String msg = Encoding.ASCII.GetString(buff).TrimEnd('\0');
                //Console.WriteLine(msg + "..............");
                if (msg.Equals("DISCOVER"))
                {
                    Console.WriteLine("Received broadcast from {0} : {1}", recv.ToString(), msg);
                    s.SendTo(Encoding.ASCII.GetBytes("OFFER-" + port), recv);
                    Thread tcp = new Thread(
                        o =>
                        {
                            tcpchuj(ipchuj, port);
                        });
                    tcp.Start();
                }
            }
        }

        public static void tcpchuj(IPAddress ip, int port)
        {
            String data = null;
            Socket tcpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpsocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint tcpendEndPoint = new IPEndPoint(ip, port);
            tcpsocket.Bind(tcpendEndPoint);
            tcpsocket.Listen(1);
            Socket handler = tcpsocket.Accept();
            byte[] bytes = new byte[1024];
            while (true)
            {
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Text received : {0}", data);
            }
            

        }

        public static int Main(String[] args)
        {
            //Thread udp = new Thread(new ThreadStart(UDPListener));
            UDPListener();
            //udp.Start();
            //Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            //IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7);
            //s.Bind(endPoint);
            //byte[] buff = new byte[20];

            //while (true)
            //{
            //    s.ReceiveFrom()
            //}

            Console.ReadKey();
            return 0;
        }
    }
    
}
