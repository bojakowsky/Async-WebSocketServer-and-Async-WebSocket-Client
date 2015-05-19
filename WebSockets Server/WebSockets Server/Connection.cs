using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
namespace WebSockets_Server
{

    /// <summary>
    /// This class is like a mirror of established connection
    /// here we hold all the bufor data for the current user
    /// 
    /// ToDo: bufor data should be stored in database
    /// 
    /// </summary>


    class Connection
    {
        public TcpClient tcpClient = new TcpClient();   // Field responsible for holding TCP connection
        public byte[] buffer = new byte[306];           // Field for incoming data
        public string Len { get; set; }                 // 3 char which is the total length of the data 
        public string Id { get; set; }                  // 10 char unique ID for each connection (user)
        public string Message { get; set; }             // 255 char message data 
        private string date;                            // 19 char (day-month-year time)
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
        public MessageType Type;                        // Private or Broadcast
        public string RecipientId;                      // 10 char unique ID of recipient
    }
}
