﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WPFRoomHost.Models;
using WPFRoomHost.Validation;

namespace WPFRoomHost.ViewModels
{
    public class MainViewModel:ViewModelBase
    {
        private ServiceHost _host;

        private bool _isStartActive = true;
        public bool IsStartActive
        {
            get
            {
                return _isStartActive;
            }

            set
            {
                if (_isStartActive == value)
                {
                    return;
                }

                _isStartActive = value;
                RaisePropertyChanged(nameof(IsStartActive));
            }
        }

        private bool _isStopActive = false;
        public bool IsStopActive
        {
            get
            {
                return _isStopActive;
            }

            set
            {
                if (_isStopActive == value)
                {
                    return;
                }

                _isStopActive = value;
                RaisePropertyChanged(nameof(IsStopActive));
            }
        }

        private string  _listenIp="localhost";
        public string ListenIp
        {
            get
            {
                return _listenIp;
            }

            set
            {
                if ( _listenIp == value)
                {
                    return;
                }

                 _listenIp = value;
                RaisePropertyChanged(nameof(ListenIp));
            }
        }

        private string _port="7997";
        public string Port
        {
            get
            {
                return _port;
            }

            set
            {
                if (_port == value)
                {
                    return;
                }

                _port = value;
                RaisePropertyChanged(nameof(Port));
            }
        }



        private ObservableCollection<LogMessage> _log = new ObservableCollection<LogMessage>();
        public ObservableCollection<LogMessage> Log
        {
            get
            {
                return _log;
            }

            set
            {
                if (_log == value)
                {
                    return;
                }

                _log = value;
                RaisePropertyChanged(nameof(Log));
            }
        }

        private RelayCommand _startButtonCommand;
        public RelayCommand StartButton
        {
            get
            {
                return  _startButtonCommand
                    ?? ( _startButtonCommand = new RelayCommand(async () =>
                    {

                        if (!new IpAdressValidationRule().Validate(ListenIp, CultureInfo.CurrentCulture).IsValid ||
                            !new PortValidationRule().Validate(Port, CultureInfo.CurrentCulture).IsValid)
                        {
                            WarningMessage("Check Listen Ip adress and Port");
                            return;
                        }

                        IsStartActive = false;
                        NormalMessage("Start configuration");

                        Uri tcpAdrs = new Uri($"net.tcp://{ListenIp}:{Port}/Host/");

                        Uri httpAdrs = new Uri($"http://{ListenIp}:{int.Parse(Port)+1}/Host/");

                        var baseAdrs = new[] {tcpAdrs, httpAdrs};

                        NormalMessage($"TCP:{tcpAdrs}\nHTTP:{httpAdrs}");

                        _host=new ServiceHost(typeof(ServiceAssembly.GameRoomService),baseAdrs);

                        NetTcpBinding tcpBinding = new NetTcpBinding(SecurityMode.None, true)
                        {
                            MaxBufferPoolSize = (int) 67108864,
                            MaxBufferSize = 67108864,
                            MaxReceivedMessageSize = (int) 67108864,
                            TransferMode = TransferMode.Buffered,
                            ReaderQuotas =
                            {
                                MaxArrayLength = 67108864,
                                MaxBytesPerRead = 67108864,
                                MaxStringContentLength = 67108864
                            },
                            MaxConnections = 32
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
                        tcpBinding.SendTimeout = new TimeSpan(0,0,7);
                        tcpBinding.ReliableSession.Enabled = true;
                        tcpBinding.ReliableSession.Ordered = false;
                        tcpBinding.ReliableSession.InactivityTimeout =
                                                   new TimeSpan(20, 0, 10);

                        _host.AddServiceEndpoint(typeof(ServiceAssembly.IGame),
                            tcpBinding, "tcp");

                        ServiceMetadataBehavior mBehave =
                           new ServiceMetadataBehavior();
                        _host.Description.Behaviors.Add(mBehave);

                        _host.AddServiceEndpoint(typeof(IMetadataExchange),
                            MetadataExchangeBindings.CreateMexTcpBinding(),
                            $"net.tcp://{ListenIp}:{int.Parse(Port)-1}/Host/mex");

                        try
                        {
                            NormalMessage("Try to open connection...");
                            await Task.Run(() => _host.Open());
                        }
                        catch (Exception ex)
                        {
                            IsStartActive = true;
                           FailureMessage(ex.Message);
                        }
                        finally
                        {
                            if (_host.State == CommunicationState.Opened)
                            {
                                IsStopActive = true;
                                SuccessMessage("Communication open!");
                            }
                            else
                            {
                                IsStartActive = true;
                            }
                            
                        }
                    }));
            }
        }

        private RelayCommand _stopButtonCommand;
        public RelayCommand StopButton
        {
            get
            {
                return  _stopButtonCommand
                    ?? ( _stopButtonCommand = new RelayCommand(async () =>
                        {
                           
                            try
                            {
                                IsStopActive = false;
                                await Task.Run(()=>_host?.Close());
                            }
                            catch (Exception ex)
                            {
                                IsStopActive = true;
                                FailureMessage(ex.Message);
                               
                            }
                            finally
                            {
                                if (_host?.State == CommunicationState.Closed)
                                {
                                    IsStartActive = true;
                                    SuccessMessage("Connection closed");

                                }
                                else
                                {
                                    IsStopActive = true;
                                }
                            }
                    }));
            }
        }
        private void FailureMessage(string text)
        {
            _log.Add(LogMessage.FailureMessage(text));
        }

        private void WarningMessage(string text)
        {
            _log.Add(LogMessage.WarningMessage(text));
        }

        private void NormalMessage(string text)
        {
            _log.Add(LogMessage.NormalMessage(text));
        }

        private void SuccessMessage(string text)
        {
            _log.Add(LogMessage.SuccessMessage(text));
        }
    }

}