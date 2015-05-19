
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebSockets_Server
{
    /// <summary>
    /// DataHelper class is responsible for serializing data we send, 
    /// and unserializign data we receive.
    /// </summary>
    class DataHelper
    {

        /// Data are serialized in this order:
        /// <param name="len"></param>          3 char length of the whole data
        /// <param name="id"></param>           10 char unique ID of the user
        /// <param name="date"></param>         19 char date
        /// <param name="type"></param>         1 char typ
        /// <param name="recipient"></param>    10 char unique ID of the recipient user, 
        ///if recipient is not included then it's marked as 10 char string = "----------"
        /// <param name="message"></param>      255 char message      
        public static byte[] serializeData(string id, string message, string date, string type, string recipient = "----------")
        {
            /*
             * 
             * Commented code is a serialization type for C# clients
             * code below (uncommented) is for javascript websockets
             * 
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
            
            // Below we put all the byte arrays into one big one that total length is 295
            Array.Copy(idBytes, 0, sendBytes, 0, idBytes.Length);
            Array.Copy(dateBytes, 0, sendBytes, idBytes.Length, dateBytes.Length);
            Array.Copy(typeBytes, 0, sendBytes, idBytes.Length + dateBytes.Length, typeBytes.Length);
            Array.Copy(recipientBytes, 0, sendBytes, idBytes.Length + dateBytes.Length + typeBytes.Length, recipientBytes.Length);
            Array.Copy(messBytes, 0, sendBytes, idBytes.Length + dateBytes.Length + typeBytes.Length + recipientBytes.Length
                , messBytes.Length);
            return sendBytes;
             */

            String message_to_serialize = id + date + type + recipient + message;
            string len = (message_to_serialize.Length + 3).ToString().PadLeft(3, '0');
            message_to_serialize = len + message_to_serialize;

            Byte[] response;
            Byte[] bytesRaw = Encoding.UTF8.GetBytes(message_to_serialize);
            Byte[] frame = new Byte[10];

            Int32 indexStartRawData = -1;
            Int32 length = bytesRaw.Length;

            frame[0] = (Byte)129;
            if (length <= 125)
            {
                frame[1] = (Byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = (Byte)126;
                frame[2] = (Byte)((length >> 8) & 255);
                frame[3] = (Byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = (Byte)127;
                frame[2] = (Byte)((length >> 56) & 255);
                frame[3] = (Byte)((length >> 48) & 255);
                frame[4] = (Byte)((length >> 40) & 255);
                frame[5] = (Byte)((length >> 32) & 255);
                frame[6] = (Byte)((length >> 24) & 255);
                frame[7] = (Byte)((length >> 16) & 255);
                frame[8] = (Byte)((length >> 8) & 255);
                frame[9] = (Byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new Byte[indexStartRawData + length];

            Int32 i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        //According to upper method - this method is unserializing 295 of the data we receive
        public static void unserializeData(byte[] bytes, Connection con)
        {
            //Unserializing data alogirthm - websocket protocol 
            String incomingData = String.Empty;
            Byte secondByte = bytes[1];
            Int32 dataLength = secondByte & 127;
            Int32 indexFirstMask = 2;
            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;

            IEnumerable<Byte> keys = bytes.Skip(indexFirstMask).Take(4);
            Int32 indexFirstDataByte = indexFirstMask + 4;

            Byte[] decoded = new Byte[bytes.Length];

            decoded[0] = (Byte)(bytes[indexFirstDataByte] ^ keys.ElementAt(0 % 4));
            decoded[1] = (Byte)(bytes[indexFirstDataByte+1] ^ keys.ElementAt(1 % 4));
            decoded[2] = (Byte)(bytes[indexFirstDataByte+2] ^ keys.ElementAt(2 % 4));
            try
            {
                con.Len = Encoding.UTF8.GetString(decoded, 0, 3);
            }
            catch (Exception)
            {
                return;
            }
            int proper_length = Convert.ToInt16(con.Len);
            if (proper_length < 41) return;
            else if (proper_length > 298) return; // ToDo, chunking packets
            for (Int32 i = indexFirstDataByte + 3, j = 3; i < proper_length + indexFirstDataByte; i++, j++)
            {
                decoded[j] = (Byte)(bytes[i] ^ keys.ElementAt(j % 4));
                if (decoded[j] > 128 || decoded[j] < 9) return; // if there're strange ASCIIs then return
            }

            incomingData = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            // return incomingData = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            //byte[] buffer = new byte[3];
            //Array.Copy(decoded, 0, buffer, 0, 3);
            //con.Len = Encoding.UTF8.GetString(buffer);
            //con.Len = dataLength.ToString();
            //con.Len = con.Len.PadLeft(3, '0');

            byte[] buffer = new byte[10];
            Array.Copy(decoded, 3, buffer, 0, 10);
            con.Id = Encoding.UTF8.GetString(buffer);
            con.Id = con.Id.PadRight(10, ' ');

            buffer = new byte[19];
            Array.Copy(decoded, 13, buffer, 0, 19);
            con.Date = Encoding.UTF8.GetString(buffer);


            buffer = new byte[1];
            Array.Copy(decoded, 32, buffer, 0, 1);
            string buf = Encoding.UTF8.GetString(buffer);
            if (buf == "1")
                con.Type = MessageType.ShoutBox;
            else con.Type = MessageType.PrivateMessage;

            buffer = new byte[10];
            Array.Copy(decoded, 33, buffer, 0, 10);
            con.RecipientId = Encoding.UTF8.GetString(buffer);
            con.RecipientId = con.RecipientId.PadRight(10, ' ');

            buffer = new byte[255];
            Array.Copy(decoded, 43, buffer, 0, proper_length - 43);
            con.Message = Encoding.UTF8.GetString(buffer);
            con.Message = con.Message.Substring(0, proper_length - 43);
            //con.Message = con.Message.PadRight(255, ' ');
        }

    }

    //ShoutBox means message that should be printed on the general chat
    //Private means it should be sent to the specific user
    enum MessageType
    {
        ShoutBox = 1,
        PrivateMessage = 2
    }
}
