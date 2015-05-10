using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSockets_Server.Server
{
    enum ServerConsoleMessagesEnum
    {
        //1 - 99 informational messages
        //100 - 199 OK messages
        //200 - 299 Error messages 

        Help = 1,
        StartingInfo = 2,

        IPSetSuccessFully = 100,
        PortSetSuccessFully = 101,
        ServerRunSuccessFully = 102,
        
        IPSetError = 200,
        PortSetError = 201,
        ServerRunError = 202,
        WrongCommandOrWrongNumberOfParams = 203,
        CommandNotFound = 204,
    }

    static class ServerConsoleMessages
    {
        static public void PrintMessage(ServerConsoleMessagesEnum cmd)
        {
            switch (cmd)
            {
                case ServerConsoleMessagesEnum.Help:
                    Console.WriteLine(
                      "Commands: \n"
                    + "ip 127.0.0.0 - set ip address to 127.0.0.0 \n"
                    + "port 20      - set port to 20\n"
                    + "run          - run server \n"
                    + "<---------------------------------------------> \n");
                    break;
                case ServerConsoleMessagesEnum.StartingInfo:
                    Console.WriteLine(
                    "<---------------------------------------------> \n"
                    + "Configure server parameters for your needs \n"
                    + "and start it typing \"run\". \n"
                    + "For help type \"help\". \n"
                    + "Default server address is 192.168.0.1:666 \n"
                    + "<---------------------------------------------> \n");
                    break;
                case ServerConsoleMessagesEnum.IPSetSuccessFully:
                    Console.WriteLine(
                          "Success: "
                        + "IP has been set. \n");
                    break;
                case ServerConsoleMessagesEnum.PortSetSuccessFully:
                    Console.WriteLine(
                          "Success: "
                        + "Port has been set. \n");
                    break;
                case ServerConsoleMessagesEnum.ServerRunSuccessFully:
                    Console.WriteLine(
                          "Success: "
                        + "Server is now running. \n");
                    break;
                case ServerConsoleMessagesEnum.IPSetError:
                    Console.WriteLine(
                          "Error: "
                        + "IP has NOT been set. \n");
                    break;
                case ServerConsoleMessagesEnum.PortSetError:
                    Console.WriteLine(
                          "Error: "
                        + "Port has NOT been set. \n");
                    break;
                case ServerConsoleMessagesEnum.ServerRunError:
                    Console.WriteLine(
                          "Error: "
                        + "Server is not running. \n");
                    break;
                case ServerConsoleMessagesEnum.WrongCommandOrWrongNumberOfParams:
                    Console.WriteLine(
                          "Error: "
                        + "Wrong command or wrong number of parameters. \n");
                    break;
                case ServerConsoleMessagesEnum.CommandNotFound:
                    Console.WriteLine(
                          "Error: "
                        + "Command has not been found. \n");
                    break;
                default:
                    break;
            }
        }
    }
}
