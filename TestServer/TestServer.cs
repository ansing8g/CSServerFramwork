using System;
using System.Linq;

using Shared;

using ServerModule.Server;
using ServerModule.Network;
using ServerModule.Database;

using StackExchange.Redis;

using MySql.Data.MySqlClient;

namespace TestServer
{
    public class TestServer : ServerBase<int>
    {
        public TestServer()
            : base()
        {
            m_updatetime = DateTime.MinValue;
        }

        protected override void OnStart()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnStart");
            m_logger?.WriteFile($"OnStart");
        }

        protected override void OnUpdate()
        {
            if (m_updatetime.AddSeconds(5) <= DateTime.Now)
            {
                m_updatetime = DateTime.Now;
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnUpdate");
                m_logger?.WriteFile($"OnUpdate");
            }
        }

        private void Timer_Redis()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Timer_Redis");
            m_logger?.WriteFile("Timer_Redis");

            IDatabase db = m_redis.GetDatabase(0);
            if(true == db.KeyExists("Key1"))
            {
                db.KeyDelete("Key1");
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Timer_Redis KeyDelete");
            }
            else
            {
                db.StringSet("Key1", "Value1");
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Timer_Redis StringSet");
            }
        }

        private void Timer_MySqlExecute()
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Timer_MySqlExecute");
            m_logger?.WriteFile($"Timer_MySqlExecute");

            if(0 >= m_mysql.Count)
            {
                return;
            }

            ServerBase_MySql mysql = m_mysql.Values.ToList()[0];
            IMySqlExecutor executor = mysql.GetMySqlExecutor();
            executor.ExecuteAsync("spTest", MySqlDataReader, MySqlResult);
        }

        private void MySqlDataReader(MySqlDataReader reader)
        {
            int row = 1;
            while (true == reader.Read())
            {
                long userseq = reader.GetInt64(0);
                string nickname = reader.GetString(1);
                int level = reader.GetInt32(2);
                DateTime regdate = reader.GetDateTime(3);
                long chip = reader.GetInt64(4);
                long gold = reader.GetInt64(5);
                int gem = reader.GetInt32(6);

                //Console.WriteLine($"[{row}]UserSeq={userseq}, NickName={nickname}, Level={level}, RegDate={regdate}, Chip={chip}, Gold={gold}, Gem={gem}");

                ++row;
            }
        }

        private void MySqlResult(MySqlParameterCollection collection)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]MySqlResult");
        }

        public void StoCServerEventError(SocketErrorType _error_type, Exception _exception, SessionSocket? _sessionsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]StoCServerEventError ErrorType={_error_type}, Exception={_exception}, SessionSocket={_sessionsocket}");
            m_logger?.WriteFile($"StoCServerEventError ErrorType={_error_type}, Exception={_exception}, SessionSocket={_sessionsocket}");
        }
        public void StoCServerEventAccept(SessionSocket _sessionsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]StoCServerEventAccept SessionSocket={_sessionsocket}");
            m_logger?.WriteFile($"StoCServerEventAccept SessionSocket={_sessionsocket}");
        }
        public void StoCServerEventDisconnect(SessionSocket _sessionsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]StoCServerEventDisconnect SessionSocket={_sessionsocket}");
            m_logger?.WriteFile($"StoCServerEventDisconnect SessionSocket={_sessionsocket}");
        }
        public void StoCServerEventSend(SessionSocket _sessionsocket)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]StoCServerEventDisconnect SessionSocket={_sessionsocket}");
            m_logger?.WriteFile($"StoCServerEventDisconnect SessionSocket={_sessionsocket}");
        }

        public void MySqlEventError(string _error_type, Exception? _exception)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]MySqlEventError ErrorType={_error_type}, Exception={_exception}");
            m_logger?.WriteFile($"MySqlEventError ErrorType={_error_type}, Exception={_exception}");
        }
        public void MySqlEventErrorExecutor(string _error_type, IMySqlExecutor _executor, Exception? _exception)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]MySqlEventError ErrorType={_error_type}, MySqlExecutor={_executor}, Exception={_exception}");
            m_logger?.WriteFile($"MySqlEventError ErrorType={_error_type}, MySqlExecutor={_executor}, Exception={_exception}");
        }

        public void RedisEventError(object _sender, RedisErrorEventArgs _e)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]RedisEventError Sender={_sender}, RedisErrorEventArgs={_e}");
            m_logger?.WriteFile($"RedisEventError Sender={_sender}, RedisErrorEventArgs={_e}");
        }
        public void EventDispatcher(string _channel, RedisValue _value)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]EventDispatcher Channel={_channel}, RedisValue={_value}");
            m_logger?.WriteFile($"EventDispatcher Channel={_channel}, RedisValue={_value}");
        }

        public void Handler_Message(SessionSocket _socket, ClientToServer _packet)
        {
            m_stoc_server.Send(_socket, _packet);
        }

        public void Initialize()
        {
            Config_Network_StoC_Server = new ServerBaseConfig_Network_Server()
            {
                Port = 23232,
                EventError = StoCServerEventError,
                EventAccept = StoCServerEventAccept,
                EventDisconnect = StoCServerEventDisconnect,
                EventSend = StoCServerEventSend
            };

            RegistHandlerStoCServer<ClientToServer>(1, Handler_Message);

            Config_MySql.Add(1, new ServerBaseConfig_MySql()
            {
                IP = "192.168.1.238",
                Port = 3306,
                DBname = "gamedb",
                ID = "root",
                PW = "MonsterM12#",
                UsePooling = true,
                PoolCount = 10,
                GetConnectorTimeoutMillisecond = 5000,
                EventError = MySqlEventError,
                EventErrorMysqlexecutor = MySqlEventErrorExecutor
            });

            Config_Redis = new ServerBaseConfig_Redis()
            {
                Addr = "127.0.0.1",
                Port = 6379,
                EventError = RedisEventError,
                EventDispatcher = EventDispatcher
            };

            TimerRegist(5000.0, Timer_Redis);
            TimerRegist(5000.0, Timer_MySqlExecute);
        }

        private DateTime m_updatetime;
    }
}
