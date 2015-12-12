using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Client
    {
        private static String nick;

        static void UdpSend()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint send = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7);
            s.SendTo(Encoding.ASCII.GetBytes("DISCOVER"), send);
            Console.WriteLine("Discover message sent to the broadcast address");
        }

        static bool UdpReceive()
        {
             Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
             EndPoint recv = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
             IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7);
             s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
             s.Bind(endPoint);
            // s.ReceiveTimeout = 10000;

             byte[] buff = new byte[20];
             int i = 0;

             Console.WriteLine("Waiting for offer...");
             try
             {
                 s.ReceiveFrom(buff, ref recv);
                 String msg = Encoding.ASCII.GetString(buff).TrimEnd('\0');
                 if (msg.Equals("OFFER"))
                 {
                     Console.WriteLine("Received offer from {0}", recv.ToString());
                 }
             }
             catch (SocketException e)
             {
                 if (e.SocketErrorCode == SocketError.TimedOut)
                 {
                     Console.WriteLine("No server was found. Trying once again.");
                     s.Close();
                     return false;
                 }
             }

            /*bool done = false;

            UdpClient listener = new UdpClient(7);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, 7);
            listener.
            try
            {
                while (!done)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.WriteLine("Received broadcast from {0} :\n {1}\n",
                        groupEP.ToString(),
                        Encoding.ASCII.GetString(bytes, 0, bytes.Length));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }*/

            return true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Enter your nick:");
            nick = Console.ReadLine();

            bool serverFound = false;

            while (!serverFound)
            {
                UdpSend();
                serverFound = UdpReceive();
            }

            /*  while (!done)
            {
                try
                {
                    Console.WriteLine("Waiting for offer...");
                    if (s.Available != 0)
                    {
                        s.ReceiveFrom(buff, ref recv);
                        String msg = Encoding.ASCII.GetString(buff).TrimEnd('\0');
                        if (msg.Equals("OFFER"))
                        {
                            Console.WriteLine("Received offer from {0}\n", recv.ToString());
                            done = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("No server was found");
                    done = true;
                }
            }*/



            Console.ReadKey();
        }
    }
}
