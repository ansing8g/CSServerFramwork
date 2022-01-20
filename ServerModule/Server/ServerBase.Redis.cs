using System;
using System.Collections.Generic;
using System.Threading;

using StackExchange.Redis;

namespace ServerModule.Server
{
    public class ServerBase_Redis
    {
        internal ServerBase_Redis()
        {
            EventError = null;
            EventDispatcher = null;

            m_options = new ConfigurationOptions();
            m_connector = null;
            m_dispatcher = new Dictionary<string, Action<RedisValue>>();
            m_rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        }

        private void OnError(object _sender, RedisErrorEventArgs _e)
        {
            if(null != EventError)
            {
                EventError(_sender, _e);
            }
        }

        private void OnErrorDispatcher(string _channel, RedisValue _value)
        {
            if(null != EventDispatcher)
            {
                EventDispatcher(_channel, _value);
            }
        }

        public bool Start(string _addr, uint _port)
        {
            m_options.EndPoints.Add($"{_addr}:{_port}");

            m_connector = ConnectionMultiplexer.Connect(m_options);
            if(null == m_connector ||
                false == m_connector.IsConnected)
            {
                return false;
            }

            ISubscriber? subscriber = m_connector?.GetSubscriber();
            if(null == subscriber)
            {
                return false;
            }

            foreach (KeyValuePair<string, Action<RedisValue>> handler in m_dispatcher)
            {
                subscriber?.Subscribe(handler.Key, Receive);
            }

            return true;
        }

        public IDatabase GetDatabase(int _dbindex)
        {
            if (null == m_connector ||
                false == m_connector.IsConnected)
            {
                m_connector = ConnectionMultiplexer.Connect(m_options);
            }

            return m_connector.GetDatabase(_dbindex);
        }

        public IServer? GetServer()
        {
            if (null == m_connector ||
                false == m_connector.IsConnected)
            {
                m_connector = ConnectionMultiplexer.Connect(m_options);
            }

            System.Net.EndPoint[] endpoint = m_connector.GetEndPoints();
            if(0 >= endpoint.Length)
            {
                return null;
            }

            return m_connector.GetServer(endpoint[0]);
        }

        public void Publish(string _channel, RedisValue _value)
        {
            ISubscriber? subscriber = m_connector?.GetSubscriber();
            subscriber?.PublishAsync(_channel, _value);
        }

        private void Receive(RedisChannel _channel, RedisValue _value)
        {
            m_rwlock.EnterReadLock();

            if(false == m_dispatcher.ContainsKey(_channel))
            {
                m_rwlock.ExitReadLock();

                OnErrorDispatcher(_channel, _value);
                return;
            }

            Action<RedisValue> func = m_dispatcher[_channel];

            m_rwlock.ExitReadLock();

            func(_value);
        }

        public void RegistSubscribeHandler(string _channel, Action<RedisValue> _func)
        {
            m_rwlock.EnterWriteLock();

            if(false == m_dispatcher.ContainsKey(_channel))
            {
                m_dispatcher.Add(_channel, _func);
            }
            else
            {
                m_dispatcher[_channel] = _func;
            }

            m_rwlock.ExitWriteLock();
        }

        public Action<object, RedisErrorEventArgs>? EventError { get; set; }
        public Action<string, RedisValue>? EventDispatcher { get; set; }

        private ConfigurationOptions m_options;
        private ConnectionMultiplexer? m_connector;
        private Dictionary<string, Action<RedisValue>> m_dispatcher;
        private ReaderWriterLockSlim m_rwlock;
    }
}
