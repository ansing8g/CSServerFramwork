using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

using MySql.Data.MySqlClient;

namespace ServerModule.Database
{
    // 커넥션 플링, 비동기 실행
    public partial class MySqlManager
    {
        public MySqlManager(MySqlEvent _event)
        {
            m_string_builder = new MySqlConnectionStringBuilder();
            m_event = _event;
            m_use_pooling = false;
            m_connection_pool = null;
        }

        public bool Initialize(string _ip, uint _port, string _dbname, string _id, string _pw, bool _use_pooling, int _pool_count, int _get_connector_timeout_millisecond)
        {
            try
            {
                m_string_builder.Server = _ip;
                m_string_builder.Port = _port;
                m_string_builder.Database = _dbname;
                m_string_builder.UserID = _id;
                m_string_builder.Password = _pw;
                m_string_builder.Pooling = _use_pooling;

                m_use_pooling = _use_pooling;
                m_get_connector_timeout_millisecond = Math.Max(0, _get_connector_timeout_millisecond);

                if (false == _use_pooling ||
                    0 == _pool_count)
                {
                    return true;
                }

                m_connection_pool = new List<MySqlConnection>();
                m_connection_pool.Capacity = _pool_count;
                
                for (int i = 0; i < _pool_count; ++i)
                {
                    MySqlConnection connection = new MySqlConnection(m_string_builder.ConnectionString);

                    connection.Open();

                    if (ConnectionState.Open != connection.State)
                    {
                        throw new Exception($"데이터베이스 풀링 생성 중 데이터베이스 커넥션 실패 ConnectionString={m_string_builder.ConnectionString}");
                    }

                    lock (m_connection_pool)
                    {
                        m_connection_pool.Add(connection);
                    }
                }
            }
            catch (Exception e)
            {
                if (null != m_event)
                {
                    m_event.OnError("MySqlManager.Initialize", e);
                }

                lock (m_connection_pool)
                {
                    foreach (MySqlConnection connection in m_connection_pool)
                    {
                        connection.Close();
                    }
                }

                return false;
            }

            return true;
        }

        public IMySqlExecutor GetMySqlExecutor()
        {
            return new MySqlExecutor(this);
        }

        private bool PopConnection(out MySqlConnection _connection)
        {
            _connection = null;

            try
            {
                if (false == m_use_pooling)
                {
                    _connection = new MySqlConnection(m_string_builder.ConnectionString);
                    _connection.Open();
                    if(ConnectionState.Open != _connection.State)
                    {
                        return false;
                    }
                }
                else
                {
                    DateTime start = DateTime.Now;

                    try
                    {
                        if(false == Monitor.TryEnter(m_connection_pool, m_get_connector_timeout_millisecond))
                        {
                            return false;
                        }
                        
                        while(0 >= m_connection_pool.Count)
                        {
                            if(DateTime.Now < start.AddMilliseconds(m_get_connector_timeout_millisecond))
                            {
                                Monitor.Exit(m_connection_pool);
                                return false;
                            }
                        }

                        _connection = m_connection_pool[m_connection_pool.Count - 1];
                        m_connection_pool.RemoveAt(m_connection_pool.Count - 1);

                        Monitor.Exit(m_connection_pool);

                        if (ConnectionState.Open != _connection.State)
                        {
                            _connection.Open();
                            if (ConnectionState.Open != _connection.State)
                            {
                                return false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (null != m_event)
                        {
                            m_event.OnError("MySqlManager.PopConnection", e);
                        }

                        if (true == Monitor.IsEntered(m_connection_pool))
                        {
                            Monitor.Exit(m_connection_pool);
                        }

                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                if(null != m_event)
                {
                    m_event.OnError("MySqlManager.PopConnection", e);
                }

                return false;
            }

            return true;
        }

        private void PushConnection(MySqlConnection _connection)
        {
            try
            {
                if (false == m_use_pooling)
                {
                    _connection.Close();
                }
                else
                {
                    lock (m_connection_pool)
                    {
                        m_connection_pool.Add(_connection);
                    }
                }
            }
            catch(Exception e)
            {
                if (null != m_event)
                {
                    m_event.OnError("MySqlManager.PushConnection", e);
                }
            }
        }

        private bool AddAsyncExecute(MySqlExecutor _executor)
        {
            return ThreadPool.QueueUserWorkItem<MySqlExecutor>(Worker, _executor, false);
        }

        private void Worker(MySqlExecutor _executor)
        {
            _executor.Execute();
        }

        internal MySqlEvent Event
        {
            get
            {
                return m_event;
            }           
        }

        private MySqlConnectionStringBuilder m_string_builder;
        private bool m_use_pooling;
        private List<MySqlConnection> m_connection_pool;
        private MySqlEvent m_event;
        private int m_get_connector_timeout_millisecond;
    }
}
