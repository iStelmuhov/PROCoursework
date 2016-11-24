using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFRoomHost.Validation;

namespace ConsoleMainServerHost
{
    static class Program
    {
        private static ServiceHost _host;
        private static bool IsActive => _host!=null&&_host.State == CommunicationState.Opened;
        private static void Main(string[] args)
        {
            Console.WindowWidth = 60;
            Start();
        }

        #region StaticPrint

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.BackgroundColor= IsActive?ConsoleColor.Green:ConsoleColor.DarkRed;
            Console.WriteLine(
@"█████████████████████████████████████████████████
█─███─█────█───█─██─███───█───█────█─█─█───█────█
█──█──█─██─██─██──█─███─███─███─██─█─█─█─███─██─█
█─█─█─█────██─██─█──███───█───█────█─█─█───█────█
█─███─█─██─██─██─██─█████─█─███─█─██───█─███─█─██
█─███─█─██─█───█─██─███───█───█─█─███─██───█─█─██
█████████████████████████████████████████████████");
            Console.BackgroundColor=ConsoleColor.Black;
            if (IsActive)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor=ConsoleColor.Green;
                Console.WriteLine($@"Server online:{_host.BaseAddresses[0]}");
                Console.BackgroundColor=ConsoleColor.Black;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($@"Server down");
                Console.BackgroundColor = ConsoleColor.Black;

            }

            Console.WriteLine("\n\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void PrintMenuDialog()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"Select an action:");
            Console.WriteLine(!IsActive? "1) Start Service": "2) Stop Service");
            Console.WriteLine(@"3) About");
            Console.WriteLine("4) Exit \n");
        }

        private static void PrintAbout()
        {
            ClearConsole();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"//TODO TEXT");
            Console.ReadKey();
            MenuDialogAction();
        }

        private static void ClearConsole()
        {
            Console.Clear();
            PrintHeader();
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void FailureMessage(string message)
        {
            var colorF = Console.ForegroundColor;
            var colorB = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(message);
            Console.ForegroundColor = colorF;
            Console.BackgroundColor = colorB;
            Console.ReadKey();
        }

        private static void SuccessMessage(string message)
        {
            var colorF = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = colorF;

            Console.ReadKey();
        }
        #endregion

        #region Service

        private static void StartService()
        {
            ClearConsole();
            Console.ForegroundColor=ConsoleColor.DarkBlue;
            Console.WriteLine("Server Tuning:\n");
            string ip = InsertIp();
            string port= InsertPort();

            Console.WriteLine(@"Start configuration");
            Uri tcpAdrs = new Uri($"net.tcp://{ip}:{port}/Host/");

            Uri httpAdrs = new Uri($"http://{ip}:{int.Parse(port) + 1}/Host/");
            var baseAdrs = new[] { tcpAdrs, httpAdrs };

            Console.WriteLine($"TCP:{tcpAdrs}\nHTTP:{httpAdrs}");

            _host = new ServiceHost(typeof(WCFMainServerAssembly.ServerHostService), baseAdrs);

            NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None, true)
            {
                MaxBufferPoolSize = (int) 67108864,
                MaxBufferSize = 67108864,
                MaxReceivedMessageSize = (int) 67108864,
                TransferMode = TransferMode.Buffered,
                MaxConnections = 32,
                ReaderQuotas =
                {
                    MaxArrayLength = 67108864,
                    MaxBytesPerRead = 67108864,
                    MaxStringContentLength = 67108864
                }
            };


            var throttle = _host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (throttle == null)
            {
                throttle = new ServiceThrottlingBehavior
                {
                    MaxConcurrentCalls = 100,
                    MaxConcurrentSessions = 100
                };
                _host.Description.Behaviors.Add(throttle);
            }

            tcpBinding.ReceiveTimeout = new TimeSpan(20, 0, 0);
            tcpBinding.ReliableSession.Enabled = true;
            tcpBinding.ReliableSession.Ordered = false;
            tcpBinding.ReliableSession.InactivityTimeout =
                                       new TimeSpan(20, 0, 10);

            _host.AddServiceEndpoint(typeof(WCFMainServerAssembly.IService),
                tcpBinding, "tcp");

            ServiceMetadataBehavior mBehave =
               new ServiceMetadataBehavior();
            _host.Description.Behaviors.Add(mBehave);

            _host.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexTcpBinding(),
                $"net.tcp://{ip}:{int.Parse(port) - 1}/Host/mex");

            try
            {
                Console.WriteLine(@"Try to open connection...");
                _host.Open();
            }
            catch (Exception ex)
            {
                FailureMessage(ex.Message);
            }
            finally
            {
                if (_host.State == CommunicationState.Opened)
                {
                    SuccessMessage("Communication open!");
                }
            }

            MenuDialogAction();
        }

        private static string InsertIp()
        {
            string ip = String.Empty;
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(@"Enter IP-adress(localhost,192.168.0.3 and etc):");
                ip = Console.ReadLine();
                if (!new IpAdressValidationRule().Validate(ip, CultureInfo.CurrentCulture).IsValid)
                    throw new InvalidOperationException("IP adress is invalid");
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Invalid ip adress");
                return InsertIp();
            }

            return ip;
        }
        private static string InsertPort()
        {
            string port = string.Empty;
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write(@"Enter Port number(1 through 65535):");
                port = Console.ReadLine();
                if (!new PortValidationRule().Validate(port, CultureInfo.CurrentCulture).IsValid)
                    throw new InvalidOperationException("Port is invalid");               
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(@"Invalid port");
                return InsertPort();
            }

            return port;
        }

        private static void StopService(bool WithExit=false)
        {
            if(_host.State==CommunicationState.Opened)
            try
            {
               Console.WriteLine("Waiting! Ty to close connection...");
               _host?.Close();
            }
            catch (Exception ex)
            {
                FailureMessage(ex.Message);
            }
            finally
            {
                if (_host?.State == CommunicationState.Closed)
                {
                    SuccessMessage("Connection closed!");
                }
            }

            if(WithExit)
                System.Environment.Exit(0);

            MenuDialogAction();
        }
        #endregion

        private static void Start()
        {
            PrintHeader();
            MenuDialogAction();
        }

        private static void MenuDialogAction()
        {
            ClearConsole();
            PrintMenuDialog();

            while (true)
            {
                try
                {
                    Console.Write(@"Action number:");
                    int action = Int32.Parse(Console.ReadLine());
                    if (action < 1 || action > 4)
                        throw new InvalidOperationException("Invalid input range");

                    SwitchMenuAction(action);
                    break;
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("\nInvalid input, enter value from 1 to 3");
                    Thread.Sleep(1000);
                    ClearConsole();
                    PrintMenuDialog();
                }

            }

        }

        private static void SwitchMenuAction(int action)
        {
            switch (action)
            {
                case 1: StartService();     break;
                case 2: StopService();      break;
                case 3: PrintAbout();       break;
                case 4: StopService(true);  break;
                default: break;
            }
        }




    }
}
