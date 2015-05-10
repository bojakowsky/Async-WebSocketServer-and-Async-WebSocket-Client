using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace ClientTest
{
    class Program
    {
        class X
        {

            public IPAddress ip_address;
            public TcpClient client;
            public Byte[] readBytes = new Byte[295];
            public Byte[] sendBytes = new Byte[295];
            public string nickname;
            public int port = 666;

            private X() { }
            public X(string ip, int port)
            {
                try
                {
                    ip_address = IPAddress.Parse(ip);
                }
                catch (Exception)
                {
                    ip_address = IPAddress.Parse("192.168.0.100");
                }

                client = new TcpClient();

                try
                {
                    client.Connect(ip_address, port);
                    nicknameChecker();
                    nickname.PadRight(10, ' ');
                    initialMessage();
                }
                catch (Exception)
                {
                    Console.WriteLine("Server is down");
                    Console.WriteLine("Exiting application in 2s");
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
            }

            public void nicknameChecker()
            {
                Console.WriteLine("Type your nickname [10 characters MAX]");
                nickname = Console.ReadLine();
                if (nickname.Length > 10 || nickname.Length == 0)
                    nicknameChecker();
                
            }

            public byte[] serializeData(string id, string message, string date, string type, string recipient = "----------")
            {
                id = id.PadRight(10, ' ');
                Byte[] idBytes = Encoding.UTF8.GetBytes(id);

                //char[] charDate = date.ToCharArray();
                date = date.PadRight(19, ' ');
                Byte[] dateBytes = Encoding.UTF8.GetBytes(date);

                //char[] charType = type.ToCharArray();

                Byte[] typeBytes = Encoding.UTF8.GetBytes(type);

                //char[] charMess = message.ToCharArray();
                message = message.PadRight(255, ' ');
                Byte[] messBytes = Encoding.UTF8.GetBytes(message);

                recipient = recipient.PadRight(10, ' ');
                Byte[] recipientBytes = Encoding.UTF8.GetBytes(recipient);

                Byte[] sendBytes = new Byte[idBytes.Length + dateBytes.Length + typeBytes.Length + messBytes.Length + recipientBytes.Length];

                Array.Copy(idBytes, 0, sendBytes, 0, idBytes.Length);
                Array.Copy(dateBytes, 0, sendBytes, idBytes.Length, dateBytes.Length);
                Array.Copy(typeBytes, 0, sendBytes, idBytes.Length + dateBytes.Length, typeBytes.Length);
                Array.Copy(recipientBytes, 0, sendBytes, idBytes.Length + dateBytes.Length + typeBytes.Length, recipientBytes.Length);
                Array.Copy(messBytes, 0, sendBytes, idBytes.Length + dateBytes.Length + typeBytes.Length + recipientBytes.Length
                    , messBytes.Length);
                return sendBytes;
            }

            public void unserializeData( byte[] bytes, ref string id, ref string message, 
                ref string date, ref string type, ref string recipient)
            {

            byte[] buffer = new byte[10];
            Array.Copy(bytes, 0, buffer, 0 , 10);
            id = Encoding.UTF8.GetString(buffer);
            id.PadRight(10, ' ');

            buffer = new byte[19];
            Array.Copy(bytes, 10, buffer, 0, 19);
            date = Encoding.UTF8.GetString(buffer);
            

            buffer = new byte[1];
            Array.Copy(bytes, 29, buffer, 0, 1);
            string buf = Encoding.UTF8.GetString(buffer);
            type = buf;

            buffer = new byte[10];
            Array.Copy(bytes, 30, buffer, 0, 10);
            recipient = Encoding.UTF8.GetString(buffer);
            recipient.PadRight(10, ' ');

            buffer = new byte[255];
            Array.Copy(bytes, 40, buffer, 0, 255);
            message = Encoding.UTF8.GetString(buffer);
            message.PadRight(255, ' ');
        }

    

        enum MessageType
        {
            ShoutBox = 1,
            PrivateMessage = 2        
        }

            public void readStream()
            {  
                while (true)
                {
                    try
                    {
                        client.GetStream().Read(readBytes, 0, 295);
                        //string buf = Encoding.UTF8.GetString(readBytes);

                        string id = "";
                        string message = "";
                        string date = "";
                        string type = "";
                        string recipient = "";

                        unserializeData(readBytes, ref id, ref message, ref  date, ref type, ref recipient);
                        message = message.TrimEnd(' ');
                        id = id.TrimEnd(' ');
                        recipient = recipient.TrimEnd(' ');

                        if (recipient != "----------")
                            Console.WriteLine(id + " says to " + recipient + ": " + message);
                        else Console.WriteLine(id + " says:" + message);
                    }
                    catch
                    {
                        Console.WriteLine("Server is down");
                        Console.WriteLine("Exiting application in 2s");
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                    }
                }
            }
            public void initialMessage()
            {
                sendBytes = serializeData(nickname, "Hi server.", DateTime.Now.ToString(), "1");
                client.GetStream().Write(sendBytes, 0, sendBytes.Length);
            }

            public void parseMessageAndSend(ref string message)
            {
                if (message.StartsWith("/all"))
                {
                    message = message.Substring(4);
                    sendBytes = serializeData(nickname, message, DateTime.Now.ToString(), "1");
                    client.GetStream().Write(sendBytes, 0, sendBytes.Length);
                }
                else if (message.StartsWith("/"))
                {
                    
                    message = message.Substring(1, message.Length-1);
                    string [] buf = message.Split(' ');
                    string recipient = buf[0];

                    message = "";
                    for (int i = 1; i < buf.Length; i++)
                    {
                        message += buf[i];
                    }

                    sendBytes = serializeData(nickname, message, DateTime.Now.ToString(), "2", recipient);
                    client.GetStream().Write(sendBytes, 0, sendBytes.Length);
                }
            }

            public void writeStream()
            {
                Console.WriteLine(
                     "How to chat: \n"
                    +"/all messageToAll \n"
                    +"/nickname messageToPerson \n"
                    );

                Console.WriteLine(DateTime.Now);       
         
                string message;
                
                while (true)
                {
                    //Console.Write(nickname+"> ");
                    try
                    {
                        message = Console.ReadLine();
                        parseMessageAndSend(ref message);
                    }
                    catch(Exception)
                    {
                        Console.WriteLine("Server is down");
                        Console.WriteLine("Exiting application in 2s");
                        Thread.Sleep(2000);
                        Environment.Exit(0);
                    }

                }
            }
        };
        
        
        static void Main(string[] args)
        {
            
            Console.WriteLine("What is the IP address of the servr?");
            string ip = Console.ReadLine();
            if ( !(ip.Length > 7 && ip.Length < 16) )
            {
                 Console.WriteLine("Due error, IP will be set to 192.168.0.100");
            }


            Console.WriteLine("What is the server port?");
            string s_port = Console.ReadLine();
            int port;
            try {
                port = Convert.ToInt16(s_port);
            }
            catch(Exception)
            {
                Console.WriteLine("Due error, port will be set to 666");Console.WriteLine("Server is down");
                port = 666;
            }

            X x = new X(ip, port);
            
            Thread reader_trhead = new Thread(x.writeStream);
            Thread sender_thread = new Thread(x.readStream);

            try
            {
                sender_thread.Start();
                reader_trhead.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Server is down");
                Console.WriteLine("Exiting application in 2s");
                sender_thread.Abort();
                reader_trhead.Abort();
            }


        }
    }
}
