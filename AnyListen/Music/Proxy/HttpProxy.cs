using System.Net;

namespace AnyListen.Music.Proxy
{
    public class HttpProxy
    {
        public string Ip { get; private set; }
        public int Port { get; private set; }

        public WebProxy ToWebProxy()
        {
            return new WebProxy(Ip, Port);
        }

        public HttpProxy(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }
    }
}
