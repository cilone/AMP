using System;

namespace AnyListen.AppCommunication.CommunicationEventArgs
{
    public class TCPConnectionChangedEventArgs : EventArgs
    {
        public TCPConnection Connection { get; set; }

        public TCPConnectionChangedEventArgs(TCPConnection connection)
        {
            Connection = connection;
        }
    }
}