using System;
using System.Threading;
using System.Collections.Generic;

using ServerModule.Database;

namespace TestMySql
{
    public class DBEvent : MySqlEvent
    {
        public DBEvent()
        {

        }

        public void OnError(string _error_type, Exception _exception)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]ErrorType={_error_type}, ErrorMessage={_exception.Message}, CallStack={_exception.StackTrace}");
        }

        public void OnError(string _error_type, IMySqlExecutor _executor, Exception _exception)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]ErrorType={_error_type}, ErrorMessage={_exception.Message}, CallStack={_exception.StackTrace}");
        }
    }

    public class TestClient
    {
        public static MySqlManager manager = new MySqlManager(new DBEvent());
        public static int execute_count = 0;

        public TestClient(int _index)
        {
            m_rand = new Random(DateTime.Now.Millisecond);
            m_start = DateTime.Now;
            m_end = DateTime.Now;
            m_index = _index;
        }

        public void FuncReader(MySql.Data.MySqlClient.MySqlDataReader reader)
        {
            try
            {
                if (true == reader.Read())
                {
                    string data = reader.GetString(0);

                    if (true == reader.NextResult())
                    {
                        while (true == reader.Read())
                        {
                            int val1 = reader.GetInt32(0);
                            string val2 = reader.GetString(1);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Reader Exception Index={m_index}, Message={e.Message}, CallStack={e.StackTrace}");
            }
        }

        public void FuncResult(MySql.Data.MySqlClient.MySqlParameterCollection Parameters)
        {
            try
            {
                if (false == Parameters["v_output"].IsNullable &&
                    null != Parameters["v_output"].Value)
                {
                    string nickname = Parameters["v_output"].Value.ToString();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Result Exception Index={m_index}, Message={e.Message}, CallStack={e.StackTrace}");
            }
        }

        public void FuncNext(object _object_next)
        {
            try
            {
                m_end = DateTime.Now;
                Interlocked.Increment(ref execute_count);

                double delay = (m_end - m_start).TotalMilliseconds;
                if (5000.0 < delay)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Index={m_index}, Delay={delay}");
                }

                if (1 == m_index)
                {
                    Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Index={m_index}, Delay={delay}");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Next Exception Index={m_index}, Message={e.Message}, CallStack={e.StackTrace}");
            }
        }

        public void Execute()
        {
            IMySqlExecutor executor = manager.GetMySqlExecutor();
            executor.AddInputParameter("v_input", MySql.Data.MySqlClient.MySqlDbType.Int64, (Int64)m_rand.Next(138053, 138432));
            executor.AddOutputParameter("v_output", MySql.Data.MySqlClient.MySqlDbType.VarChar);

            m_start = DateTime.Now;

            if (false == executor.ExecuteAsync("spTest", FuncReader, FuncResult, this, FuncNext))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Index={m_index}, DB Fail");
                return;
            }
        }

        private Random m_rand;
        public DateTime m_start;
        public DateTime m_end;
        public int m_index;
    }

    class Program
    {
        static void Main(string[] args)
        {
            int poolcount = 100;
            int runcount = 10000;
            if (false == TestClient.manager.Initialize("192.168.1.238", 3306, "gamedb", "root", "MonsterM12#", true, poolcount, 10000))
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}] DBManager Initialize Fail");
                return;
            }

            List<TestClient> list = new List<TestClient>();

            for(int i = 0; i < runcount; ++i)
            {
                list.Add(new TestClient(i + 1));
            }

            Console.WriteLine("Test Start");
            DateTime t = DateTime.Now.AddSeconds(-1.0);
            while(true)
            {
                if(t.AddSeconds(1.0) > DateTime.Now)
                {
                    Thread.Sleep(100);
                    continue;
                }

                t = DateTime.Now;

                foreach(TestClient c in list)
                {
                    c.Execute();
                }
            }
        }
    }
}
