using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using ServerModule.Network;

namespace TestClient
{
    public class Event : ClientSocketEvent
    {
        public static Config config = new Config();
        public static int connect_count = 0;
        public static Dictionary<int, TestClient> dic_testclient = new Dictionary<int, TestClient>();
        public static LinkedList<int> ll_index = new LinkedList<int>();

        public void OnError(SocketErrorType _error_type, Exception _exception, ConnectSocket _connectsocket)
        {
            //if(true == _exception is ObjectDisposedException ||
            //    true == _exception is ArgumentException ||
            //    true == _exception is NullReferenceException ||
            //    true == _exception is InvalidOperationException ||
            //    true == _exception is System.Net.Sockets.SocketException)
            //{
            //    return;
            //}

            Console.WriteLine($"OnError. ErrorType={_error_type.ToString()}, Msg={_exception.Message}, StackTrace={_exception.StackTrace}, Source={_exception.Source}");
        }

        public void OnConnect(ConnectSocket _connectsocket)
        {
            Interlocked.Increment(ref connect_count);

            try
            {
                int index = 0;
                lock(ll_index)
                {
                    index = ll_index.First.Value;
                    ll_index.RemoveFirst();
                }

                TestClient tc = new TestClient((TestClient.LogicType)config.logictype, index);
                lock(dic_testclient)
                {
                    if(false == dic_testclient.ContainsKey(tc.m_index))
                    {
                        dic_testclient.Add(tc.m_index, tc);
                    }
                }

                _connectsocket.StateObject = tc;
                tc.m_socket = _connectsocket;

                tc.OnConnect();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnConnect Exception Msg={e.Message}");
            }
        }

        public void OnDisconnect(ConnectSocket _connectsocket)
        {
            Interlocked.Decrement(ref connect_count);

            try
            {
                TestClient tc = (TestClient)_connectsocket.StateObject;
                if (null == tc)
                {
                    Thread.Sleep(10000);
                    if (false == _connectsocket.Connect(Event.config.ip, Event.config.port))
                    {
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect TC is Null Connect False Index={tc.m_index}");
                    }

                    return;
                }

                //Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect Index={tc.m_index}");

                tc.OnDisconnect();

                lock (dic_testclient)
                {
                    dic_testclient.Remove(tc.m_index);
                }

                lock(ll_index)
                {
                    ll_index.AddFirst(tc.m_index);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect Exception Msg={e.Message}");
            }
        }

        public void OnSend(ConnectSocket _connectsocket)
        {

        }

        public void OnReceive(ConnectSocket _connectsocket, byte[] _data)
        {
            try
            {
                TestClient tc = (TestClient)_connectsocket.StateObject;
                if (null == tc)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnReceive TC is Null");
                    _connectsocket.Disconnect();
                    return;
                }

                tc.OnReceive(_data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnReceive Exception Msg={e.Message}");
            }
        }
    }

    public class TestClient
    {
        public enum LogicType
        {
            ReConnect = 1,
            SendData = 2,
            Mix = 3,
        }

        public TestClient(LogicType _logictype, int _index)
        {
            m_logictype = _logictype;
            m_index = _index;
            m_data = new byte[256];
            m_rand = new Random(DateTime.Now.Millisecond);
            m_send = DateTime.Now;
            m_recv = DateTime.Now;
        }

        public void OnConnect()
        {
            switch (m_logictype)
            {
                case LogicType.ReConnect:
                    {
                        //m_socket.Disconnect();
                    }
                    break;
                case LogicType.SendData:
                case LogicType.Mix:
                    {
                        //Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnConnect Index={m_index}");

                        //m_rand.NextBytes(m_data);
                        //m_socket.Send(m_data);
                    }
                    break;
            }
        }

        public void OnDisconnect()
        {
            switch (m_logictype)
            {
                case LogicType.ReConnect:
                    {
                        Thread.Sleep(m_rand.Next(5000, 10000));
                        if (false == m_socket.Connect(Event.config.ip, Event.config.port))
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect LogicType.ReConnect Connect False Index={m_index}");
                        }
                    }
                    break;
                case LogicType.SendData:
                    {
                        Thread.Sleep(m_rand.Next(5000, 10000));
                        while (false == m_socket.Connect(Event.config.ip, Event.config.port))
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect LogicType.SendData Connect False Index={m_index}");
                        }
                    }
                    break;
                case LogicType.Mix:
                    {
                        //Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect Index={m_index}");

                        Thread.Sleep(m_rand.Next(5000, 10000));
                        while (false == m_socket.Connect(Event.config.ip, Event.config.port))
                        {
                            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect LogicType.Mix Connect False Index={m_index}");
                        }
                    }
                    break;
            }
        }

        public void OnReceive(byte[] _data)
        {
            if (m_index == BitConverter.ToInt32(_data))
            {
                m_recv = DateTime.Now;
            }

            //if (false == m_data.SequenceEqual(_data))
            //{
            //    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnReceive Not Equals Index={m_index}");
            //}
        }

        public void Logic()
        {
            switch (m_logictype)
            {
                case LogicType.SendData:
                    {
                        m_rand.NextBytes(m_data);
                        m_socket.Send(m_data);
                        m_send = DateTime.Now;
                    }
                    break;
                case LogicType.Mix:
                    {
                        int rand = m_rand.Next(1, 10000);
                        if (1 == rand)
                        {
                            m_socket.Disconnect();
                        }
                        else
                        {
                            //m_rand.NextBytes(m_data);
                            m_socket.Send(BitConverter.GetBytes(m_index));
                            m_send = DateTime.Now;
                        }
                    }
                    break;
            }
        }

        public void Display(bool _display)
        {
            if (false == _display)
            {
                return;
            }

            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Delay={(m_recv - m_send).Ticks / 10000}");
        }

        public LogicType m_logictype;
        public int m_index;
        public ConnectSocket m_socket;
        public byte[] m_data;
        public Random m_rand;
        public DateTime m_send;
        public DateTime m_recv;
    }

    public class Config
    {
        public Config()
        {
            ip = "";
            port = 0;
            logictype = (int)TestClient.LogicType.SendData;
            client_count = 1;
        }

        public string ip;
        public int port;
        public int logictype;
        public int client_count;
    }

    class Program
    {
        static void InputData()
        {
            Console.Write("Server IP:");
            Event.config.ip = Console.ReadLine();
            Console.Write("Server Port:");
            Event.config.port = int.Parse(Console.ReadLine());
            Console.WriteLine("ReConnect = 1, DataSend = 2, Mix = 3");
            Console.Write("Logic Type:");
            Event.config.logictype = int.Parse(Console.ReadLine());
            Console.Write("Client Count:");
            Event.config.client_count = int.Parse(Console.ReadLine());
        }

        static void Main(string[] args)
        {
            FileInfo fi = new FileInfo("./config.txt");
            if (true == fi.Exists)
            {
                try
                {
                    FileStream fs = fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    StreamReader sr = new StreamReader(fs);
                    Event.config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                    sr.Close();
                    fs.Close();

                    Console.WriteLine("Server IP:" + Event.config.ip);
                    Console.WriteLine("Server Port:" + Event.config.port);
                    Console.WriteLine("ReConnect = 1, DataSend = 2, Mix = 3");
                    Console.WriteLine("Logic Type:" + Event.config.logictype);
                    Console.WriteLine("Client Count:" + Event.config.client_count);
                }
                catch (Exception)
                {
                    InputData();
                }
            }
            else
            {
                InputData();
            }

            Event _event = new Event();
            for (int i = 1; i <= Event.config.client_count; ++i)
            {
                ConnectSocket connectsocket = new ConnectSocket(_event);

                while (false == connectsocket.Connect(Event.config.ip, Event.config.port))
                {
                    Console.WriteLine("Connect Fail");
                }

                lock(Event.ll_index)
                {
                    Event.ll_index.AddLast(i);
                    Event.ll_index.AddLast(i + Event.config.client_count + 1);
                }

                Thread.Sleep(1);
            }

            DateTime checktime = DateTime.Now;
            while (true)
            {
                Thread.Sleep(100);

                if (DateTime.Now < checktime.AddSeconds(1))
                {
                    continue;
                }

                checktime = DateTime.Now;

                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]ConnectCount=" + Event.connect_count);

                bool is_display = true;
                lock (Event.dic_testclient)
                {
                    foreach (TestClient client in Event.dic_testclient.Values)
                    {
                        client.Display(is_display);
                        client.Logic();
                        is_display = false;
                    }
                }
            }
        }
    }
}
