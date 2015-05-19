using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace WebSockets_Server
{
    partial class Serwer
    {

        #region server tcp methods
        //Method is used to accept incoming connection
        public void AcceptCallback(IAsyncResult result)
        {

            Connection connection = new Connection();
            lock (locker) // establish only one connection at time
            {
                try
                {
                    //we try to make a connection
                    connection = new Connection();
                    connection.tcpClient = TCPListener.EndAcceptTcpClient(result); // Ending accepting the connection
                    // Adding client to connections that are already established
                    Connections.Add(connection);

                    //Handshake based on WebSocket protocol
                    confirmConnectionByHandshake(connection);

                }
                catch (ObjectDisposedException e)
                {
                    Console.WriteLine(e.ToString()); // if anything went wrong
                }
            }
            try
            {
                //print that client has connected
                Console.WriteLine("Client connected: " +
                    ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Address +
                    ":" + ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);
            }
                catch (Exception)
                {
                    ///...
                }
            
            try
            {
                // start of the async data reading that are being send via TCP protocol
                connection.tcpClient.GetStream().BeginRead(connection.buffer, 0, connection.buffer.Length, 
                    new AsyncCallback(ReadCallBack), connection);
            }
            catch (Exception)
            {
                try
                {
                    // If connection is lost print information
                    Console.WriteLine("Connection lost: " + ((IPEndPoint)connection.tcpClient.
                        Client.RemoteEndPoint).Address + ":" +
                        ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);

                    //Delete information about the connection
                    disconnect(((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint));
                }
                catch (Exception)
                {
                    ///...
                }
            }
            // Start async method that waits for accepting another connection
            TCPListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null);
        }

        //This method is needed to confirm TCP connections from HTTP websockets
        public void confirmConnectionByHandshake(Connection con)
        {
            NetworkStream stream = con.tcpClient.GetStream();
            Byte[] bytes = new Byte[con.tcpClient.Available];
            stream.Read(bytes, 0, bytes.Length);

            //translate bytes of request to string
            String data = Encoding.UTF8.GetString(bytes);

            if (new Regex("^GET").IsMatch(data))
            {
                Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                    + "Connection: Upgrade" + Environment.NewLine
                    + "Upgrade: websocket" + Environment.NewLine
                    + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )
                        )
                    ) + Environment.NewLine
                    + Environment.NewLine);

                stream.Write(response, 0, response.Length);
            }
            else
            {
                disconnect(((IPEndPoint)con.tcpClient.Client.RemoteEndPoint));
            }
        }

        public void ReadCallBack(IAsyncResult result)
        {
            lock (locker)
            {
                Connection connection = (Connection)result.AsyncState;
                try
                {
                    connection.tcpClient.GetStream().
                        BeginRead(connection.buffer, 0, connection.buffer.Length, new AsyncCallback(ReadCallBack), connection);
                    DataHelper.unserializeData(connection.buffer, connection);

                    if (connection.RecipientId.StartsWith("----------"))
                        sendToAll(DataHelper.serializeData(connection.Id, connection.Message, connection.Date, 
                            "1", connection.RecipientId
                            ));
                    else
                        sendPrivateMessage(connection);


                }
                catch (Exception)
                {
                    try
                    {
                        Console.WriteLine("Connection lost: " + ((IPEndPoint)connection.tcpClient.
                        Client.RemoteEndPoint).Address + ":" +
                        ((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint).Port);
                        disconnect(((IPEndPoint)connection.tcpClient.Client.RemoteEndPoint)); // usunięcie informacji o połączeniu
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
            }

        }

        public void sendPrivateMessage(Connection con)
        {
            bool found = false;
            con.buffer = DataHelper.serializeData(con.Id, con.Message, con.Date,
                            "2", con.RecipientId
                            );
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
                byte[] errorData = DataHelper.serializeData(con.Id, "User has not been found!", DateTime.Now.ToString(), "1");
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


    }
}
