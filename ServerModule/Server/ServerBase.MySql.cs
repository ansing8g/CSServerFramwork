using System;

using ServerModule.Database;

namespace ServerModule.Server
{
    public class ServerBase_MySql : MySqlEvent
    {
        internal ServerBase_MySql()
        {
            EventError = null;
            EventError_MySqlExecutor = null;

            m_manager = new MySqlManager(this);
        }

        public virtual void OnError(string _error_type, Exception _exception)
        {
            if(null != EventError)
            {
                EventError(_error_type, _exception);
            }
        }

        public virtual void OnError(string _error_type, IMySqlExecutor _executor, Exception _exception)
        {
            if(null != EventError_MySqlExecutor)
            {
                EventError_MySqlExecutor(_error_type, _executor, _exception);
            }
            else if(null != EventError)
            {
                EventError(_error_type, _exception);
            }
        }

        public bool Initialize(string _ip, uint _port, string _dbname, string _id, string _pw, bool _use_pooling, int _pool_count, int _get_connector_timeout_millisecond)
        {
            return m_manager.Initialize(_ip, _port, _dbname, _id, _pw, _use_pooling, _pool_count, _get_connector_timeout_millisecond);
        }

        public IMySqlExecutor GetMySqlExecutor()
        {
            return m_manager.GetMySqlExecutor();
        }

        public Action<string, Exception?>? EventError { get; set; }
        public Action<string, IMySqlExecutor, Exception?>? EventError_MySqlExecutor { get; set; }

        private MySqlManager m_manager;
    }
}
