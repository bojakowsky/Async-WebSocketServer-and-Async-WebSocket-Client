using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
namespace WebSockets_Server
{
    class Connection
    {
        public TcpClient tcpClient = new TcpClient();
        public byte[] buffer = new byte[295];

        public string Id { get; set; }
        public string Message { get; set; }
        private string date;
        public string Date
        {
            get
            {
                return DateTime.Now.ToString();
            }
            set
            {
                date = value.ToString();
            }
        }
        public MessageType Type;
        public string RecipientId;


    }
}
