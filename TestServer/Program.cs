using System;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TestServer server = new TestServer();
            server.Initialize();
            if(false == server.Start())
            {
                Console.WriteLine("Start Fail");
                return;
            }
        }
    }
}
