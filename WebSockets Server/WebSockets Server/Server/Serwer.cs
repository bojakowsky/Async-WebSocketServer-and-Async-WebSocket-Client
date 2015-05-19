using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WebSockets_Server
{
    partial class Serwer
    {
        #region fields, properties

        public List <Connection> Connections = new List<Connection>(); // list of all connected users
        private TcpListener TCPListener;                               // socket listener, listens if there're any messages incoming
        Object locker = new Object();                                  // locker is used to unable multiple threads work on some function
        private int port = 0;                                          // server port
        public IPAddress IPAddr { get; set; }                          // server IPAddress
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
        
        //Helper get method for IPAddress type
        public string getStringIPAddress()
        {
            return IPAddr.ToString();
        }

        //Helper set method for IPAddress type
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

        //Default constructor, starts a configuration method for a server
        public Serwer() 
        {
            Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.StartingInfo);
            configuration();
        }

        //Configuration method that handles console messages (see ServerConsoleMessages.cs)
        public void configuration()
        {
            string command = "";
            while (!command.Equals("run")) // don't run server until server admin types "run"
            {
                command = Console.ReadLine();
                commandExecutioner(command);
            }
        }

        # region ServerConsoleMethods
        //on typing help - prints help in the console
        public void HelpMethod()
        {
            Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.Help);
        }
        
        //set IP - on typing 'ip xxx.xxx.xxx.xxx' sets IP to this xxx address
        public void ipSetMethod(string ip)
        {
            setIPAddr(ip);
            //Exception is handled inside setIP
        }

        //Set Port - on typing 'port x' , sets port to x
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

        //runMethod starts the server on specified configuration
        public void runMethod()
        {
            if (IPAddr == null)
                setIPAddr("192.168.0.100");
            if (Port == 0)
                Port = 666; // just for fun
            try
            {
                TCPListener = new TcpListener(IPAddr, Port);
                TCPListener.Start(); // Initialize and start TCPListener, now we're waiting for users
                Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.ServerRunSuccessFully);
            }
            catch (Exception)
            {
                Server.ServerConsoleMessages.PrintMessage(Server.ServerConsoleMessagesEnum.ServerRunError); 
                // if something went bad, print error
            }

            Console.WriteLine(
                  "<---------------------------------------------> \n"
                + "Server IP Address: " + IPAddr.ToString() + "\t Port: " + Port + "\n"
                + "<---------------------------------------------> \n");
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null); // async method that waits for
                                                                                       // tcpClient to connect to the server
        }

        # region Console Logger Functions
        //Executes the command user types in
        public void commandExecutioner(string command)
        {
            string[] cmd = command.Split(' ');
            if (!(cmd.Length == 2 || cmd.Length == 1)) // command should be like 'run' or 'port x' don't accept other command formats
            {
                Server.ServerConsoleMessages.PrintMessage(
                    Server.ServerConsoleMessagesEnum.WrongCommandOrWrongNumberOfParams);
            }
            else
            {
                chooseCommand(cmd);
            }
        
        }

        //checks if typed command fits any of defined
        public void chooseCommand(string[] cmd)
        {
            //Run method
            if (cmd[0].Equals(ServerCommands.run.ToString()))
            {
                runMethod();
            }
            
            //Help method
            else if (cmd[0].Equals(ServerCommands.help.ToString()))
            {
                HelpMethod();
            }
            
            //IPset method
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
            
            //Port set method
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
            
            //Else tell user that command is incorrect
            else
            {
                Server.ServerConsoleMessages.PrintMessage(
                    Server.ServerConsoleMessagesEnum.CommandNotFound);
            }
        }

        //defined ServerCommands
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
