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
            EndPoint recv = (EndPoint) (new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7);
            s.Bind(endPoint);

            while (true)
            {
                byte[] buff = new byte[20];
                int i = 0;

                s.ReceiveFrom(buff, ref recv);
                String msg = Encoding.ASCII.GetString(buff).TrimEnd('\0');
                //Console.WriteLine(msg + "..............");
                if (msg.Equals("DISCOVER"))
                {
                    Console.WriteLine("Received broadcast from {0} : {1}", recv.ToString(), msg);
                    s.SendTo(Encoding.ASCII.GetBytes("OFFER"), recv);
                }
            }
        }

        public static int Main(String[] args)
        {
            Thread udp = new Thread(new ThreadStart(UDPListener));
            udp.Start();


            Console.ReadKey();
            return 0;
        }
    }
    
}
