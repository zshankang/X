﻿using System;
using System.Diagnostics;
using System.Threading;
using NewLife.Collections;
using NewLife.CommonEntity;
using NewLife.Linq;
using NewLife.Log;
using NewLife.Messaging;
using NewLife.Net.Proxy;
using NewLife.Net.Sockets;
using NewLife.Reflection;
using NewLife.Threading;
using XCode.DataAccessLayer;
using System.Collections.Generic;

namespace Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            XTrace.UseConsole();
            while (true)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
#if !DEBUG
                try
                {
#endif
                Test1();
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
#endif

                sw.Stop();
                Console.WriteLine("OK! 耗时 {0}", sw.Elapsed);
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.C) break;
            }
        }

        static HttpProxy http = null;
        private static void Test1()
        {
            //var server = new HttpReverseProxy();
            //server.Port = 888;
            //server.ServerHost = "www.cnblogs.com";
            //server.ServerPort = 80;
            //server.Start();

            //var ns = Enum.GetNames(typeof(ConsoleColor));
            //var vs = Enum.GetValues(typeof(ConsoleColor));
            //for (int i = 0; i < ns.Length; i++)
            //{
            //    Console.ForegroundColor = (ConsoleColor)vs.GetValue(i);
            //    Console.WriteLine(ns[i]);
            //}

            NewLife.Net.Application.AppTest.Start();

            //http = new HttpProxy();
            //http.Port = 8080;
            ////http.OnResponse += new EventHandler<HttpProxyEventArgs>(http_OnResponse);
            //http.Start();

            //HttpProxy.SetIEProxy("127.0.0.1:" + http.Port);
            //Console.WriteLine("已设置IE代理，任意键结束测试，关闭IE代理！");

            //ThreadPoolX.QueueUserWorkItem(ShowStatus);

            //Console.ReadKey(true);
            //HttpProxy.SetIEProxy(null);

            ////server.Dispose();
            //http.Dispose();

            //var ds = new DNSServer();
            //ds.Start();

            //for (int i = 5; i < 6; i++)
            //{
            //    var buffer = File.ReadAllBytes("dns" + i + ".bin");
            //    var entity2 = DNSEntity.Read(buffer, false);
            //    Console.WriteLine(entity2);

            //    var buffer2 = entity2.GetStream().ReadBytes();

            //    var p = buffer.CompareTo(buffer2);
            //    if (p != 0)
            //    {
            //        Console.WriteLine("{0:X2} {1:X2} {2:X2}", p, buffer[p], buffer2[p]);
            //    }
            //}
        }

        static void ShowStatus()
        {
            //var pool = PropertyInfoX.GetValue<SocketBase, ObjectPool<NetEventArgs>>("Pool");
            var pool = NetEventArgs.Pool;

            while (true)
            {
                var asyncCount = 0;
                foreach (var item in http.Servers)
                {
                    asyncCount += item.AsyncCount;
                }
                foreach (var item in http.Sessions.Values.ToArray())
                {
                    var remote = (item as IProxySession).Remote;
                    if (remote != null) asyncCount += remote.Host.AsyncCount;
                }

                Int32 wt = 0;
                Int32 cpt = 0;
                ThreadPool.GetAvailableThreads(out wt, out cpt);
                Int32 threads = Process.GetCurrentProcess().Threads.Count;

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("异步:{0} 会话:{1} Thread:{2}/{3}/{4} Pool:{5}/{6}/{7}", asyncCount, http.Sessions.Count, threads, wt, cpt, pool.StockCount, pool.FreeCount, pool.CreateCount);
                Console.ForegroundColor = color;

                Thread.Sleep(3000);

                //GC.Collect();
            }
        }

        static void Test2()
        {
            var em = new ExceptionMessage();
            em.Value = new Exception("Error");
            var data = em.GetStream().ReadBytes();

            HttpClientMessageProvider client = new HttpClientMessageProvider();
            client.Uri = new Uri("http://localhost:8/Web/MessageHandler.ashx");

            LoginRequest request = new LoginRequest();
            request.UserName = "admin";
            request.Password = "admin";

            Message msg = client.SendAndReceive(request, 0);
            LoginResponse rs = msg as LoginResponse;
            Console.WriteLine("返回：" + rs.Admin);
        }

        #region 消息
        static readonly MessageKind YWS = MessageKind.UserDefine + 50;

        class LoginRequest : Message
        {
            public override MessageKind Kind { get { return YWS + 1; } }

            private String _UserName;
            /// <summary>用户名</summary>
            public String UserName { get { return _UserName; } set { _UserName = value; } }

            private String _Password;
            /// <summary>密码</summary>
            public String Password { get { return _Password; } set { _Password = value; } }
        }

        class LoginResponse : Message
        {
            public override MessageKind Kind { get { return YWS + 2; } }

            private IAdministrator _Admin;
            /// <summary>已登录的管理员对象</summary>
            public IAdministrator Admin { get { return _Admin; } set { _Admin = value; } }
        }
        #endregion

        static void Test3()
        {
            Int32 x = 2;
            Int32 y = 3;
            Int32 n = 0;

            var p = new Program();
            Func<Int32, Int32, Int32> add = Add;
            Func<Int32, Int32, Int32> mul = Mul;

            Console.WriteLine("Hook前：");
            n = add(x, y);
            Console.WriteLine(n);

            n = mul(x, y);
            Console.WriteLine(n);
            //Console.ReadKey(true);

            var hook = new ApiHook();
            hook.OriMethod = add.Method;
            hook.NewMethod = mul.Method;
            hook.Hook();

            Console.WriteLine("Hook后：");
            n = add(x, y);
            Console.WriteLine(n);

            n = mul(x, y);
            Console.WriteLine(n);
            //Console.ReadKey(true);

            hook.UnHook();

            Console.WriteLine("Hook还原：");
            n = add(x, y);
            Console.WriteLine(n);
            n = mul(x, y);
            Console.WriteLine(n);

            hook.Hook();
            n = add(x, y);
            Console.WriteLine(n);
            n = mul(x, y);
            Console.WriteLine(n);
        }

        static Int32 Add(Int32 x, Int32 y)
        {
            Console.WriteLine("Add");
            return x + y;
        }

        static Int32 Mul(Int32 x, Int32 y)
        {
            Console.WriteLine("Mul");
            return x * y;
        }
    }
}