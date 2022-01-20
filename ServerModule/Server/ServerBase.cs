using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using ServerModule.Network;
using ServerModule.Database;
using ServerModule.Utility;

using StackExchange.Redis;

namespace ServerModule.Server
{
    public class ServerBaseConfig_Network_Server
    {
        public ServerBaseConfig_Network_Server()
        {
            Converter = new JsonConverter();
            Port = 0;
            ListenCount = 1000;
            BufSize = 512;
            TotalBufSize = 5120;
            EventError = null;
            EventAccept = null;
            EventDisconnect = null;
            EventSend = null;
        }

        public IConverter Converter;
        public int Port;
        public int ListenCount = 1000;
        public uint BufSize = 512;
        public uint TotalBufSize = 5120;
        public Action<SocketErrorType, Exception, SessionSocket?>? EventError;
        public Action<SessionSocket>? EventAccept;
        public Action<SessionSocket>? EventDisconnect;
        public Action<SessionSocket>? EventSend;
    }

    public class ServerBaseConfig_Network_Client
    {
        public ServerBaseConfig_Network_Client()
        {
            Converter = new JsonConverter();
            IP = "";
            Port = 0;
            BufSize = 512;
            TotalBufSize = 5120;
            EventError = null;
            EventAccept = null;
            EventDisconnect = null;
            EventSend = null;
        }

        public IConverter Converter;
        public string IP;
        public int Port;
        public uint BufSize = 512;
        public uint TotalBufSize = 5120;
        public Action<SocketErrorType, Exception, ConnectSocket?>? EventError;
        public Action<ConnectSocket>? EventAccept;
        public Action<ConnectSocket>? EventDisconnect;
        public Action<ConnectSocket>? EventSend;
    }

    public class ServerBaseConfig_MySql
    {
        public ServerBaseConfig_MySql()
        {
            IP = "";
            Port = 0;
            DBname = "";
            ID = "";
            PW = "";
            UsePooling = false;
            PoolCount = 0;
            GetConnectorTimeoutMillisecond = 0;
            EventError = null;
            EventErrorMysqlexecutor = null;
        }

        public string IP;
        public uint Port;
        public string DBname;
        public string ID;
        public string PW;
        public bool UsePooling;
        public int PoolCount;
        public int GetConnectorTimeoutMillisecond;
        public Action<string, Exception?>? EventError;
        public Action<string, IMySqlExecutor, Exception?>? EventErrorMysqlexecutor;
    }

    public class ServerBaseConfig_Redis
    {
        public ServerBaseConfig_Redis()
        {
            Addr = "";
            Port = 0;
            EventError = null;
            EventDispatcher = null;
        }

        public string Addr;
        public uint Port;
        public Action<object, RedisErrorEventArgs>? EventError;
        public Action<string, RedisValue>? EventDispatcher;
    }

    public abstract class ServerBase<PacketIndex> where PacketIndex : notnull
    {
        public class ServerBaseConfig_Timmer
        {
            public ServerBaseConfig_Timmer()
            {
                interval = 0.0;
                func = null;
                loop = false;
            }

            public double interval;
            public Action? func;
            public bool loop;
        }

        public class ServerBaseConfig_Task_Timmer
        {
            public ServerBaseConfig_Task_Timmer()
            {
                interval = 0.0;
                func = null;
                loop = false;
            }

            public double interval;
            public Func<Task>? func;
            public bool loop;
        }

        protected ServerBase()
        {
            //--> Server
            m_is_start = false;

            //--> Network
            m_stoc_server = new ServerBase_Network_Server<PacketIndex>();
            Config_Network_StoC_Server = null;
            m_stos_server = new ServerBase_Network_Server<PacketIndex>();
            Config_Network_StoS_Server = null;
            m_stos_client = new ServerBase_Network_Client<PacketIndex>();
            Config_Network_StoS_Client = null;

            //--> Database
            Config_MySql = new Dictionary<int, ServerBaseConfig_MySql>();
            m_mysql = new Dictionary<int, ServerBase_MySql>();
            m_redis = new ServerBase_Redis();
            Config_Redis = null;

            //--> Log
            m_logger = new Logger();

            //--> Timer
            m_timer = new TimerManager();
            m_config_timer = new List<ServerBaseConfig_Timmer>();
            m_config_task_timer = new List<ServerBaseConfig_Task_Timmer>();
        }

        //--> ServerBase
        private bool m_is_start;

        protected abstract void OnStart();
        protected abstract void OnUpdate();

        public bool Start()
        {
            //--> Network Server to Client => Server
            if (null != Config_Network_StoC_Server)
            {
                if(false == m_stoc_server.Start(Config_Network_StoC_Server.Converter, Config_Network_StoC_Server.Port, Config_Network_StoC_Server.ListenCount, Config_Network_StoC_Server.BufSize, Config_Network_StoC_Server.TotalBufSize))
                {
                    m_logger?.WriteFile($"Server Start Fail. Network StoC Server Converter={Config_Network_StoC_Server.Converter}, Port={Config_Network_StoC_Server.Port}, ListenCount={Config_Network_StoC_Server.ListenCount}, BufSize={Config_Network_StoC_Server.BufSize}, TotalBufSize={Config_Network_StoC_Server.TotalBufSize}");
                    return false;
                }

                m_stoc_server.EventError = Config_Network_StoC_Server.EventError;
                m_stoc_server.EventAccept = Config_Network_StoC_Server.EventAccept;
                m_stoc_server.EventDisconnect = Config_Network_StoC_Server.EventDisconnect;
                m_stoc_server.EventSend = Config_Network_StoC_Server.EventSend;
            }
            //--> Network Server to Server => Server
            if (null != Config_Network_StoS_Server)
            {
                if(false == m_stos_server.Start(Config_Network_StoS_Server.Converter, Config_Network_StoS_Server.Port, Config_Network_StoS_Server.ListenCount, Config_Network_StoS_Server.BufSize, Config_Network_StoS_Server.TotalBufSize))
                {
                    m_logger?.WriteFile($"Server Start Fail. Network StoS Server Converter={Config_Network_StoS_Server.Converter}, Port={Config_Network_StoS_Server.Port}, ListenCount={Config_Network_StoS_Server.ListenCount}, BufSize={Config_Network_StoS_Server.BufSize}, TotalBufSize={Config_Network_StoS_Server.TotalBufSize}");
                    return false;
                }

                m_stos_server.EventError = Config_Network_StoS_Server.EventError;
                m_stos_server.EventAccept = Config_Network_StoS_Server.EventAccept;
                m_stos_server.EventDisconnect = Config_Network_StoS_Server.EventDisconnect;
                m_stos_server.EventSend = Config_Network_StoS_Server.EventSend;
            }
            //--> Network Server to Server => Client
            if (null != Config_Network_StoS_Client)
            {
                if(false == m_stos_client.Connect(Config_Network_StoS_Client.Converter, Config_Network_StoS_Client.IP, Config_Network_StoS_Client.Port, Config_Network_StoS_Client.BufSize, Config_Network_StoS_Client.TotalBufSize))
                {
                    m_logger?.WriteFile($"Server Start Fail. Network StoS Client Converter={Config_Network_StoS_Client.Converter}, IP={Config_Network_StoS_Client.IP}, Port={Config_Network_StoS_Client.Port}, BufSize={Config_Network_StoS_Client.BufSize}, TotalBufSize={Config_Network_StoS_Client.TotalBufSize}");
                    return false;
                }

                m_stos_client.EventError = Config_Network_StoS_Client.EventError;
                m_stos_client.EventAccept = Config_Network_StoS_Client.EventAccept;
                m_stos_client.EventDisconnect = Config_Network_StoS_Client.EventDisconnect;
                m_stos_client.EventSend = Config_Network_StoS_Client.EventSend;
            }

            //--> Database
            foreach(KeyValuePair<int, ServerBaseConfig_MySql> config in Config_MySql)
            {
                if(false == AddMySqlDB(config.Key, config.Value.IP, config.Value.Port, config.Value.DBname, config.Value.ID, config.Value.PW, config.Value.UsePooling, config.Value.PoolCount, config.Value.GetConnectorTimeoutMillisecond, config.Value.EventError, config.Value.EventErrorMysqlexecutor))
                {
                    m_logger?.WriteFile($"Server Start Fail. Databse Mysql Index={config.Key}, IP={config.Value.IP}, Port={config.Value.Port}, DBName={config.Value.DBname}, ID={config.Value.ID}, PW={config.Value.PW}, UsePooling={config.Value.UsePooling}, PoolCount={config.Value.PoolCount}, GetConnectorTimeoutMillisecond={config.Value.GetConnectorTimeoutMillisecond}, EventError={config.Value.EventError}, EventErrorMysqlexecutor={config.Value.EventErrorMysqlexecutor}");
                    return false;
                }
            }
            if(null != Config_Redis)
            {
                if(false == m_redis.Start(Config_Redis.Addr, Config_Redis.Port))
                {
                    m_logger?.WriteFile($"Server Start Fail. Databse Redis Addr={Config_Redis.Addr}, Port={Config_Redis.Port}");
                    return false;
                }

                m_redis.EventError = Config_Redis.EventError;
                m_redis.EventDispatcher = Config_Redis.EventDispatcher;
            }

            //--> Timer
            foreach(ServerBaseConfig_Timmer config in m_config_timer)
            {
                m_timer.Regist(config.interval, config.func!, config.loop);
            }

            foreach(ServerBaseConfig_Task_Timmer config in m_config_task_timer)
            {
                m_timer.Regist(config.interval, config.func!, config.loop);
            }

            m_is_start = true;

            OnStart();

            while(true)
            {
                OnUpdate();
            }
        }

        //--> Network Server to Client => Server
        protected ServerBase_Network_Server<PacketIndex> m_stoc_server;
        public ServerBaseConfig_Network_Server? Config_Network_StoC_Server;
        public void RegistHandlerStoCServer<PacketObject>(PacketIndex _packet_index, Action<SessionSocket, PacketObject> _func) where PacketObject : PacketBase<PacketIndex>
        {
            m_stoc_server.RegistHandler<PacketObject>(_packet_index, _func);
        }
        //--> Network Server to Server => Server
        protected ServerBase_Network_Server<PacketIndex> m_stos_server;
        public ServerBaseConfig_Network_Server? Config_Network_StoS_Server;
        public void RegistHandlerStoSServer<PacketObject>(PacketIndex _packet_index, Action<SessionSocket, PacketObject> _func) where PacketObject : PacketBase<PacketIndex>
        {
            m_stos_server.RegistHandler<PacketObject>(_packet_index, _func);
        }
        //--> Network Server to Server => Client
        protected ServerBase_Network_Client<PacketIndex> m_stos_client;
        public ServerBaseConfig_Network_Client? Config_Network_StoS_Client;
        public void RegistHandlerStoSClient<PacketObject>(PacketIndex _packet_index, Action<ConnectSocket, PacketObject> _func) where PacketObject : PacketBase<PacketIndex>
        {
            m_stos_client.RegistHandler<PacketObject>(_packet_index, _func);
        }

        //--> Database
        //--> MySql
        protected Dictionary<int, ServerBase_MySql> m_mysql;
        protected bool AddMySqlDB(int _index, string _ip, uint _port, string _dbname, string _id, string _pw, bool _use_pooling, int _pool_count, int _get_connector_timeout_millisecond, Action<string, Exception?>? _event_error, Action<string, IMySqlExecutor, Exception?>? _event_error_mysqlexecutor)
        {
            ServerBase_MySql mysql = new ServerBase_MySql();
            if(false == mysql.Initialize(_ip, _port, _dbname, _id, _pw, _use_pooling, _pool_count, _get_connector_timeout_millisecond))
            {
                return false;
            }

            mysql.EventError = _event_error;
            mysql.EventError_MySqlExecutor = _event_error_mysqlexecutor;

            if (true == m_mysql.ContainsKey(_index))
            {
                m_mysql[_index] = mysql;
            }
            else
            {
                m_mysql.Add(_index, mysql);
            }

            return true;
        }
        public Dictionary<int, ServerBaseConfig_MySql> Config_MySql;
        public void AddMySqlConfig(int _index, ServerBaseConfig_MySql _config)
        {
            if (false == Config_MySql.ContainsKey(_index))
            {
                Config_MySql.Add(_index, _config);
            }

            Config_MySql[_index] = _config;
        }

        //--> Redis
        protected ServerBase_Redis m_redis;
        public ServerBaseConfig_Redis? Config_Redis;
        public void RegistHandlerRedis(string _channel, Action<RedisValue> _func)
        {
            m_redis.RegistSubscribeHandler(_channel, _func);
        }

        //--> Log
        protected Logger? m_logger;
        public void SetLogConfig(string _path_directory, string _filename, uint _delay_second = 0, long _limit_filesize = 0)
        {
            m_logger = new Logger(_path_directory, _filename, _delay_second, _limit_filesize);
        }

        //--> Timer
        private TimerManager m_timer;
        private List<ServerBaseConfig_Timmer> m_config_timer;
        private List<ServerBaseConfig_Task_Timmer> m_config_task_timer;
        protected void TimerRegist(double _interval_milliseconds, Action _func, bool _loop = true)
        {
            if(false == m_is_start)
            {
                m_config_timer.Add(new ServerBaseConfig_Timmer()
                {
                    interval = _interval_milliseconds,
                    func = _func,
                    loop = _loop
                });
            }
            else
            {
                m_timer.Regist(_interval_milliseconds, _func, _loop);
            }
        }
        protected void TimerRegist(double _interval_milliseconds, Func<Task> _func, bool _loop = true)
        {
            if (false == m_is_start)
            {
                m_config_task_timer.Add(new ServerBaseConfig_Task_Timmer()
                {
                    interval = _interval_milliseconds,
                    func = _func,
                    loop = _loop
                });
            }
            else
            {
                m_timer.Regist(_interval_milliseconds, _func, _loop);
            }
        }
    }
}
