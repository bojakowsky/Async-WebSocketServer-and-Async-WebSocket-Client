using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WebSockets_Server
{
    class Serwer
    {
        #region fields, properties

        public List <Connection> Connections = new List<Connection>();
        private TcpListener TCPListener;
        Object locker = new Object();
        private int port = 0;
        public IPAddress IPAddr { get; set; }
        public int Port {
            get 
            {
                return port;
            }
            set
            {
                if (value > 9000 || value < 1)
                    Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.PortSetError);
                else
                {
                    Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.PortSetSuccessFully);
                    port = value;
                }
            }
        
        }
        

        public string getStringIPAddress()
        {
            return IPAddr.ToString();
        }

        public void setIPAddr(string ip)
        {
            try
            {
                if (! (ip.Length < 7 || ip.Length > 15))
                {
                    IPAddr = IPAddress.Parse(ip);
                    Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.IPSetSuccessFully);
                }
                else
                {
                    Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.IPSetError);
                }
            }
            catch (Exception)
            {
                Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.IPSetError);
            }
        }

        #endregion

        public Serwer() 
        {
            Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.StartingInfo);
            configuration();
        }

        public void configuration()
        {
            string command = "";
            while (!command.Equals("run"))
            {
                command = Console.ReadLine();
                commandExecutioner(command);
            }
        }

        # region ServerConsoleMethods
        public void HelpMethod()
        {
            Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.Help);
        }

        public void ipSetMethod(string ip)
        {
            setIPAddr(ip);
            //Exception is handled inside setIP
        }

        public void portSetMethod(string port)
        {
            try
            {
                Port = Convert.ToInt32(port);
            }
            catch (Exception)
            {
                Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.PortSetError);
            }
        }
        #endregion

        public void runMethod()
        {
            if (IPAddr == null)
                setIPAddr("192.168.0.100");
            if (Port == 0)
                Port = 666; // just for fun
            try
            {
                TCPListener = new TcpListener(IPAddr, Port);
                TCPListener.Start();
                Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.ServerRunSuccessFully);
            }
            catch (Exception)
            {
                Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.ServerRunError);
            }

            Console.WriteLine(
                  "<---------------------------------------------> \n"
                + "Server IP Address: " + IPAddr.ToString() + "\t Port: " + Port + "\n"
                + "<---------------------------------------------> \n");
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null); 
        }

        #region server tcp methods

        public void ReadCallBack(IAsyncResult result)
        {
            lock (locker)
            {
                Connection connection = (Connection)result.AsyncState;
                try
                {
                    connection.tcpClient.GetStream().
                        BeginRead(connection.buffer, 0, connection.buffer.Length, new AsyncCallback(ReadCallBack), connection);
                    DataSent.unserializeData(connection.buffer, connection);

                    if (connection.RecipientId.StartsWith("----------"))
                        sendToAll(connection.buffer);
                    else
                        sendPrivateMessage(connection);


                }
                catch (Exception)
                {
                    Console.WriteLine("Connection lost: " + ((IPEndPoint)connection.tcpClient.
                    Client.RemoteEndPoint).Address + ":" +
                    ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);
                    disconnect(((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint)); // usunięcie informacji o połączeniu
                }
            }
        }

        public void sendPrivateMessage(Connection con)
        {
            bool found = false;
            foreach (var connection in Connections)
            {
                if (connection.Id.Equals(con.RecipientId))
                {
                    found = true;
                    connection.tcpClient.GetStream().BeginWrite(con.buffer, 0, con.buffer.Length,
                        new AsyncCallback(sendPrivate), connection);
                    if (con != connection)
                        con.tcpClient.GetStream().BeginWrite(con.buffer, 0, con.buffer.Length,
                            new AsyncCallback(sendPrivate), con);
                }
            }
            if (!found)
            {
                byte[] errorData = DataSent.serializeData(con.Id, "User has not been found!", DateTime.Now.ToString(), "1");
                con.tcpClient.GetStream().BeginWrite(errorData, 0, errorData.Length,
                    new AsyncCallback(sendPrivate), con);
            }
            
        }

        public void sendPrivate(IAsyncResult result)
        {
            Connection connection = (Connection)result.AsyncState;
            connection.tcpClient.GetStream().EndWrite(result);
        }

        public void sendToAll(byte[] buffer)
        {

            foreach (var con in Connections)
            {
                con.tcpClient.GetStream().BeginWrite(buffer, 0, buffer.Length,
                        new AsyncCallback(sendBroadcast), con);
                
            }
        }

        public void sendBroadcast(IAsyncResult result)
        {
            Connection connection = (Connection)result.AsyncState;
            connection.tcpClient.GetStream().EndWrite(result);
        }

        public void AcceptCallback(IAsyncResult result)
        {
            Connection connection = new Connection();
            lock (locker)
            {
                try
                {
                    connection = new Connection();
                    connection.tcpClient = TCPListener.EndAcceptTcpClient(result); // zakończenie akceptowania klienta
                    // dodanie klienta do listy połączonych klientów
                    Connections.Add(connection);
                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            Console.WriteLine("Client connected: " + 
                ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Address + 
                ":" + ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);
            try
            {
                // rozpoczęcie asynchronicznego czytania danych wysyłanych przez protokół TCP
                connection.tcpClient.GetStream().BeginRead(connection.buffer, 0, connection.buffer.Length, new AsyncCallback(ReadCallBack), connection);
            }
            catch (Exception)
            {
                Console.WriteLine("Connection lost: " + ((IPEndPoint)connection.tcpClient.
                    Client.RemoteEndPoint).Address + ":" + 
                    ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);
                disconnect(((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint)); // usunięcie informacji o połączeniu
            }
            // rozpoczęcie asynchronicznego oczekiwania na następnego klienta
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null);
        }



        public void disconnect(IPEndPoint endpoint)
        {
            lock (locker)
            {
                Connection connectionToDel = null;
                foreach (Connection connection in Connections)
                {
                    if (connection != null && ((IPEndPoint)(connection.tcpClient.Client.RemoteEndPoint)).Equals(endpoint))
                    {
                        if (connection.tcpClient.Connected)
                        {
                            Console.WriteLine("Client disconnected: " + ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Address + 
                                ":" + ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);
                            connection.tcpClient.GetStream().Close();
                            connection.tcpClient.Close();
                        }
                        connectionToDel = connection;
                    }
                }
                if (connectionToDel != null)
                {
                    Connections.Remove(connectionToDel);
                }
            }
        }
        
        #endregion

        # region Console Logger Functions
        public void commandExecutioner(string command)
        {
            string[] cmd = command.Split(' ');
            if (!(cmd.Length == 2 || cmd.Length == 1))
            {
                Server.ServerConsoleMessages.PrintMessage(
                    Server.ServerConsoleMessagesEnum.WrongCommandOrWrongNumberOfParams);
            }
            else
            {
                chooseCommand(cmd);
            }
        
        }

        public void chooseCommand(string[] cmd)
        {
            if (cmd[0].Equals(ServerCommands.run.ToString()))
            {
                runMethod();
            }

            else if (cmd[0].Equals(ServerCommands.help.ToString()))
            {
                HelpMethod();
            }

            else if (cmd[0].Equals(ServerCommands.ip.ToString()) )
            {
                if (cmd.Length != 2)
                {
                    Server.ServerConsoleMessages.PrintMessage(
                        Server.ServerConsoleMessagesEnum.WrongCommandOrWrongNumberOfParams);
                }
                else 
                    ipSetMethod(cmd[1]);
            }

            else if (cmd[0].Equals(ServerCommands.port.ToString()) )
            {
                if (cmd.Length != 2)
                {
                    Server.ServerConsoleMessages.PrintMessage(
                        Server.ServerConsoleMessagesEnum.WrongCommandOrWrongNumberOfParams);
                }
                else 
                    portSetMethod(cmd[1]);
            }

            else
            {
                Server.ServerConsoleMessages.PrintMessage(
                    Server.ServerConsoleMessagesEnum.CommandNotFound);
            }
        }

        enum ServerCommands
        {
            run,
            help,
            ip,
            port
        }
        #endregion
    }
}
