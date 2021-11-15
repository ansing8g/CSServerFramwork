using System;
using System.Threading;
using System.Collections.Generic;

using ServerModule.Network;

namespace TestServer
{
    public class Event : ServerSocketEvent
    {
        public static int connectcount = 0;
        public static int recvcount = 0;
        public static Dictionary<int, SessionSocket> dic = new Dictionary<int, SessionSocket>();
        public static ReaderWriterLockSlim lock_dic = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void OnError(SocketErrorType _error_type, Exception _exception, SessionSocket _sessionsocket)
        {
            //if(true == _exception is NullReferenceException ||
            //    true == _exception is OutOfMemoryException ||
            //    true == _exception is System.Net.Sockets.SocketException)
            //{
            //    return;
            //}

            if (true == _exception is System.Net.Sockets.SocketException)
            {
                System.Net.Sockets.SocketException ex = _exception as System.Net.Sockets.SocketException;
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnError. ErrorType={_error_type.ToString()}, ErrorCode={ex.ErrorCode}, Msg={ex.Message}, StackTrace={ex.StackTrace}, Source={ex.Source}");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnError. ErrorType={_error_type.ToString()}, Msg={_exception.Message}, StackTrace={_exception.StackTrace}, Source={_exception.Source}");
            }
        }

        public void OnAccept(SessionSocket _sessionsocket)
        {
            //Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnAccept");

            int index = Interlocked.Increment(ref Event.connectcount);

            _sessionsocket.StateObject = index;


            lock_dic.EnterWriteLock();

            if (false == dic.ContainsKey(index))
            {
                dic.Add(index, _sessionsocket);
            }

            lock_dic.ExitWriteLock();
        }

        public void OnDisconnect(SessionSocket _sessionsocket)
        {
            //Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]OnDisconnect");

            Interlocked.Decrement(ref Event.connectcount);

            int index = (int)_sessionsocket.StateObject;

            lock_dic.EnterWriteLock();

            dic.Remove(index);

            lock_dic.ExitWriteLock();
        }

        public void OnSend(SessionSocket _sessionsocket)
        {

        }

        public void OnReceive(SessionSocket _sessionsocket, byte[] _data)
        {
            lock_dic.EnterReadLock();

            foreach (SessionSocket socket in dic.Values)
            {
                socket.Send(_data);
                Interlocked.Increment(ref Event.recvcount);
            }

            lock_dic.ExitReadLock();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Port:");
            int port = int.Parse(Console.ReadLine());

            AcceptSocket serversocket = new AcceptSocket(new Event());
            if (false == serversocket.Start(port, 10))
            {
                Console.WriteLine("Server Start Fail");
                return;
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

                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Connect Count={Event.connectcount}");

                int count = Interlocked.Exchange(ref Event.recvcount, 0);
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}]Recv Count={count}");
            }
        }
    }
}
