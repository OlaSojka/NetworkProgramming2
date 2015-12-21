using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

namespace Client
{
    class Client
    {
        private static Socket udpSocket;

        static void UdpSend()
        {
            IPAddress ip = IPAddress.Broadcast;
            IPEndPoint udpSend = new IPEndPoint(ip, 7);
            udpSocket.SendTo(Encoding.ASCII.GetBytes("DISCOVER"), udpSend);
            Console.WriteLine("Broadcast DISCOVER message sent.");
        }

        static List<Tuple<EndPoint,int>> UdpReceive() //tuple(recv, port)
        {
            byte[] buff = new byte[20];
            EndPoint recv = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));
            List<Tuple<EndPoint, int>> servers = new List<Tuple<EndPoint, int>>();
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
                    Console.WriteLine("Received {0} from: {1}", msg[0], recv);
                    servers.Add(Tuple.Create<EndPoint, int>(recv, tcpport));
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut) { }
                    else
                        Console.WriteLine(e.StackTrace);
                }
            }
            s.Stop();
            if (servers.Count == 0)
            {
                Console.WriteLine("No server found, trying once again");
                return null;
            }
            return servers;
        }

        private static void writeLastServer(IPEndPoint ep)
        {
            string path = "lastserver.txt";

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            FileStream fs = null;
            fs = new FileStream(path, FileMode.CreateNew);
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(ep.ToString());
            }
        }

        static bool TcpSender(EndPoint recv, int tcpport)
        {
            IPAddress tcpip = IPAddress.Parse(recv.ToString().Split(':')[0].TrimEnd(' '));
            IPEndPoint tcpEndPoint = new IPEndPoint(tcpip, tcpport);
            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                sender.Connect(tcpEndPoint);

                writeLastServer(tcpEndPoint);

                Console.WriteLine("Connection established. Enter your nick:");
                String nick = Console.ReadLine();
                Console.WriteLine("Give the frequency in which you want to send data (in miliseconds, between 0 and 10000):");
                bool flag = false;
                int frequency = 0;
                while (!flag)
                {
                    String input = Console.ReadLine();
                    try
                    {
                        int number = Int32.Parse(input);
                        if (number < 0)
                            Console.WriteLine("Frequency cannot be nagative, try again");
                        else if(number > 10000)
                            Console.WriteLine("Frequency cannot be bigger than 10000ms, try again");
                        else
                        {
                            frequency = number;
                            flag = true;
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("It's not a number, try again.");
                    }
                }

                sender.Send(Encoding.ASCII.GetBytes("NICK:" + nick));

                while (true)
                {
                    Random rnd = new Random();
                    int value = rnd.Next(0, 101);
                    sender.Send(Encoding.ASCII.GetBytes(value.ToString()));
                    Console.WriteLine("VALUE {0} sent", value);
                    Thread.Sleep(frequency);
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused ||
                    e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    Console.WriteLine("Server disconnected.");
                    return false;
                }
            }
            return true;
        }

        static void SuggestServer()
        {
            string path = "lastserver.txt";
            String server = null;

            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (sr.Peek() >= 0)
                        {
                            server = sr.ReadLine();
                        }
                    }
                }
                if (server != null)
                {
                    Console.WriteLine("Suggested connection: {0}.\nDo you want to connect? y/n", server);
                    bool flag = false;
                    while (!flag)
                    {
                        ConsoleKeyInfo answer = Console.ReadKey();
                        if (answer.KeyChar == 'y')
                        {
                            int port = Int32.Parse(server.Split(':')[1]);
                            EndPoint ep = new IPEndPoint(IPAddress.Parse(server.Split(':')[0]), port);
                            flag = true;
                            TcpSender(ep, port);
                        }
                        else if (answer.KeyChar == 'n')
                            flag = true;
                        else
                            flag = false;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpSocket.ReceiveTimeout = 1000;
            
            bool serverConnected = false;
            while (!serverConnected)
            {
                SuggestServer();

                bool serverFound = false;
                List<Tuple<EndPoint, int>> serversList = new List<Tuple<EndPoint, int>>();

                while (!serverFound)
                {
                    UdpSend();
                    serversList = UdpReceive();
                    if (serversList != null)
                        serverFound = true;
                }
                Console.WriteLine("List of available servers:");
                int counter = 1;
                foreach (var server in serversList)
                {
                    Console.WriteLine("{0}. {1}:{2}", counter, server.Item1.ToString().Split(':')[0], server.Item2);
                    counter++;
                }
                Console.WriteLine("Select server you want to connect to:");
                bool flag = false;
                int selectedServer = 0;
                while (!flag)
                {
                    String input = Console.ReadLine();
                    try
                    {
                        int server = Int32.Parse(input);
                        if (server > serversList.Count)
                            Console.WriteLine("The number is too big, try again.");
                        else if (server < 0)
                            Console.WriteLine("The number cannot be nagative, try again");
                        else
                        {
                            selectedServer = server - 1;
                            flag = true;
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("It's not a number, try again.");
                    }
                }

                serverConnected = TcpSender(serversList.ElementAt(selectedServer).Item1, serversList.ElementAt(selectedServer).Item2);
            }
            Console.ReadKey();
        }
    }
}
