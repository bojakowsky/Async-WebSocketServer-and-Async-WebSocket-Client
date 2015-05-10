using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebSockets_Server
{
    class DataSent
    {
        
        public static byte[] serializeData(string id, string message, string date, string type, string recipient = "----------")
        {
            id = id.PadRight(10, ' ');
            Byte[] idBytes = Encoding.UTF8.GetBytes (id);

            //char[] charDate = date.ToCharArray();
            date = date.PadRight(19, ' ');
            Byte[] dateBytes = Encoding.UTF8.GetBytes(date);

            //char[] charType = type.ToCharArray();
            
            Byte[] typeBytes = Encoding.UTF8.GetBytes(type);

            //char[] charMess = message.ToCharArray();
            message = message.PadRight(255, ' ');
            Byte[] messBytes = Encoding.UTF8.GetBytes (message);

            recipient = recipient.PadRight(10, ' ');
            Byte[] recipientBytes = Encoding.UTF8.GetBytes(recipient);

            Byte[] sendBytes = new Byte [idBytes.Length + dateBytes.Length + typeBytes.Length + messBytes.Length + recipientBytes.Length];

            Array.Copy(idBytes, 0, sendBytes, 0, idBytes.Length);
            Array.Copy(dateBytes, 0, sendBytes, idBytes.Length, dateBytes.Length);
            Array.Copy(typeBytes, 0, sendBytes, idBytes.Length + dateBytes.Length, typeBytes.Length);
            Array.Copy(recipientBytes, 0, sendBytes, idBytes.Length + dateBytes.Length + typeBytes.Length, recipientBytes.Length);
            Array.Copy(messBytes, 0, sendBytes, idBytes.Length + dateBytes.Length + typeBytes.Length + recipientBytes.Length
                , messBytes.Length);
            return sendBytes;
        }

        public static void unserializeData( byte[] bytes, Connection con )
        {

            byte[] buffer = new byte[10];
            Array.Copy(bytes, 0, buffer, 0 , 10);
            con.Id = Encoding.UTF8.GetString(buffer);
            con.Id.PadRight(10, ' ');

            buffer = new byte[19];
            Array.Copy(bytes, 10, buffer, 0, 19);
            con.Date = Encoding.UTF8.GetString(buffer);
            

            buffer = new byte[1];
            Array.Copy(bytes, 29, buffer, 0, 1);
            string buf = Encoding.UTF8.GetString(buffer);
            if (buf == "1")
                con.Type = MessageType.ShoutBox;
            else con.Type = MessageType.PrivateMessage;

            buffer = new byte[10];
            Array.Copy(bytes, 30, buffer, 0, 10);
            con.RecipientId = Encoding.UTF8.GetString(buffer);
            con.RecipientId.PadRight(10, ' ');

            buffer = new byte[255];
            Array.Copy(bytes, 40, buffer, 0, 255);
            con.Message = Encoding.UTF8.GetString(buffer);
            con.Message.PadRight(255, ' ');
        }

    }

    enum MessageType
    {
        ShoutBox = 1,
        PrivateMessage = 2        
    }
}
