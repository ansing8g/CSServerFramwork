using System;
using System.Data;

using MySql.Data.MySqlClient;

namespace ServerModule.Database
{
    public interface IMySqlExecutor
    {
        public void AddInputParameter(string _name, MySqlDbType _type, object _value);
        public void AddOutputParameter(string _name, MySqlDbType _type);
        public bool Execute(string _procedure_name, Action<MySqlParameterCollection> _func_result);
        public bool Execute(string _procedure_name, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next);
        public bool Execute(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result);
        public bool Execute(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next);
        public bool ExecuteAsync(string _procedure_name, Action<MySqlParameterCollection> _func_result);
        public bool ExecuteAsync(string _procedure_name, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next);
        public bool ExecuteAsync(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result);
        public bool ExecuteAsync(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next);
    }

    public partial class MySqlManager
    {
        public class MySqlExecutor : IMySqlExecutor
        {
            internal MySqlExecutor(MySqlManager _manager)
            {
                m_command = new MySqlCommand();
                m_manager = _manager;
                m_procedure_name = "";
                m_func_reader = null;
                m_func_result = null;
                m_object_next = null;
                m_func_next = null;
            }

            public void AddInputParameter(string _name, MySqlDbType _type, object _value)
            {
                try
                {
                    m_command.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = _name,
                        MySqlDbType = _type,
                        Value = _value
                    });
                }
                catch (Exception e)
                {
                    if (null != m_manager &&
                        null != m_manager.m_event)
                    {
                        m_manager.m_event.OnError("IMySqlExecutor.AddInputParameter", this, e);
                    }
                }
            }

            public void AddOutputParameter(string _name, MySqlDbType _type)
            {
                try
                {
                    m_command.Parameters.Add(new MySqlParameter()
                    {
                        ParameterName = _name,
                        MySqlDbType = _type,
                        Direction = ParameterDirection.Output
                    });
                }
                catch (Exception e)
                {
                    if (null != m_manager &&
                        null != m_manager.m_event)
                    {
                        m_manager.m_event.OnError("IMySqlExecutor.AddOutputParameter", this, e);
                    }
                }
            }

            public bool Execute(string _procedure_name, Action<MySqlParameterCollection> _func_result)
            {
                m_procedure_name = _procedure_name;
                m_func_reader = null;
                m_func_result = _func_result;
                m_object_next = null;
                m_func_next = null;

                return Execute();
            }

            public bool Execute(string _procedure_name, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next)
            {
                m_procedure_name = _procedure_name;
                m_func_reader = null;
                m_func_result = _func_result;
                m_object_next = _object_next;
                m_func_next = _func_next;

                return Execute();
            }

            public bool Execute(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result)
            {
                m_procedure_name = _procedure_name;
                m_func_reader = _func_reader;
                m_func_result = _func_result;
                m_object_next = null;
                m_func_next = null;

                return Execute();
            }

            public bool Execute(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next)
            {
                m_procedure_name = _procedure_name;
                m_func_reader = _func_reader;
                m_func_result = _func_result;
                m_object_next = _object_next;
                m_func_next = _func_next;

                return Execute();
            }

            public bool ExecuteAsync(string _procedure_name, Action<MySqlParameterCollection> _func_result)
            {
                MySqlExecutor executor = new MySqlExecutor(m_manager);
                executor.m_procedure_name = _procedure_name;
                executor.m_func_reader = null;
                executor.m_func_result = _func_result;
                executor.m_object_next = null;
                executor.m_func_next = null;
                foreach (MySqlParameter param in m_command.Parameters)
                {
                    executor.m_command.Parameters.Add(param);
                }

                m_command.Parameters.Clear();

                return m_manager.AddAsyncExecute(executor);
            }

            public bool ExecuteAsync(string _procedure_name, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next)
            {
                MySqlExecutor executor = new MySqlExecutor(m_manager);
                executor.m_procedure_name = _procedure_name;
                executor.m_func_reader = null;
                executor.m_func_result = _func_result;
                executor.m_object_next = _object_next;
                executor.m_func_next = _func_next;
                foreach (MySqlParameter param in m_command.Parameters)
                {
                    executor.m_command.Parameters.Add(param);
                }

                m_command.Parameters.Clear();

                return m_manager.AddAsyncExecute(executor);
            }

            public bool ExecuteAsync(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result)
            {
                MySqlExecutor executor = new MySqlExecutor(m_manager);
                executor.m_procedure_name = _procedure_name;
                executor.m_func_reader = _func_reader;
                executor.m_func_result = _func_result;
                executor.m_object_next = null;
                executor.m_func_next = null;
                foreach (MySqlParameter param in m_command.Parameters)
                {
                    executor.m_command.Parameters.Add(param);
                }

                m_command.Parameters.Clear();

                return m_manager.AddAsyncExecute(executor);
            }

            public bool ExecuteAsync(string _procedure_name, Action<MySqlDataReader> _func_reader, Action<MySqlParameterCollection> _func_result, object _object_next, Action<object?> _func_next)
            {
                MySqlExecutor executor = new MySqlExecutor(m_manager);
                executor.m_procedure_name = _procedure_name;
                executor.m_func_reader = _func_reader;
                executor.m_func_result = _func_result;
                executor.m_object_next = _object_next;
                executor.m_func_next = _func_next;
                foreach (MySqlParameter param in m_command.Parameters)
                {
                    executor.m_command.Parameters.Add(param);
                }

                m_command.Parameters.Clear();

                return m_manager.AddAsyncExecute(executor);
            }

            public bool Execute()
            {
                try
                {
                    MySqlConnection? connection = null;
                    if (false == m_manager.PopConnection(out connection))
                    {
                        throw new Exception("Get Connector Fail");
                    }

                    m_command.Connection = connection;
                    m_command.CommandText = m_procedure_name;
                    m_command.CommandType = CommandType.StoredProcedure;

                    MySqlDataReader reader = m_command.ExecuteReader();

                    if (null != m_func_reader)
                    {
                        m_func_reader(reader);
                    }

                    reader.Close();

                    if (null != m_func_result)
                    {
                        m_func_result(m_command.Parameters);
                    }

                    m_command.Parameters.Clear();

                    m_manager.PushConnection(connection!);

                    if (null != m_func_next)
                    {
                        m_func_next(m_object_next);
                    }
                }
                catch (Exception e)
                {
                    if (null != m_manager &&
                        null != m_manager.m_event)
                    {
                        m_manager.m_event.OnError("IMySqlExecutor.Execute", this, e);
                    }

                    return false;
                }

                return true;
            }

            private MySqlCommand m_command;
            private MySqlManager m_manager;
            public string m_procedure_name;
            private Action<MySqlDataReader>? m_func_reader;
            private Action<MySqlParameterCollection>? m_func_result;
            private object? m_object_next;
            private Action<object?>? m_func_next;
        }
    }
}
