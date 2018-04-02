using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Core;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Serializer;
using MQTTnet.Server;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MqttHubConnectionHandler<THub> : ConnectionHandler 
        where THub : Hub
    {
        private readonly HubLifetimeManager<THub> _lifetimeManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MqttHubConnectionHandler<THub>> _logger;
        private readonly HubOptions<THub> _hubOptions;
        private readonly HubOptions _globalHubOptions;
        private readonly IUserIdProvider _userIdProvider;
        private readonly HubDispatcher<THub> _dispatcher;
        private readonly MqttHubProtocol _protocol;
        private readonly MqttPacketSerializer _serializer;
        private readonly bool _enableDetailedErrors;
        
        private Subject<MqttPublishPacket> _packets = new Subject<MqttPublishPacket>();

        public MqttHubConnectionHandler(HubLifetimeManager<THub> lifetimeManager,
                                    IOptions<HubOptions> globalHubOptions,
                                    IOptions<HubOptions<THub>> hubOptions,
                                    ILoggerFactory loggerFactory,
                                    IUserIdProvider userIdProvider,
                                    HubDispatcher<THub> dispatcher,
                                    MqttHubProtocol protocol,
                                    MqttPacketSerializer serializer)
        {
            _lifetimeManager = lifetimeManager;
            _loggerFactory = loggerFactory;
            _hubOptions = hubOptions.Value;
            _globalHubOptions = globalHubOptions.Value;
            _logger = loggerFactory.CreateLogger<MqttHubConnectionHandler<THub>>();
            _userIdProvider = userIdProvider;
            _dispatcher = dispatcher;
            _protocol = protocol;
            _serializer = serializer;
            _enableDetailedErrors = false;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            // We check to see if HubOptions<THub> are set because those take precedence over global hub options.
            // Then set the keepAlive and handshakeTimeout values to the defaults in HubOptionsSetup incase they were explicitly set to null.
            var keepAlive = _hubOptions.KeepAliveInterval ?? _globalHubOptions.KeepAliveInterval ?? TimeSpan.FromSeconds(5);
            var handshakeTimeout = _hubOptions.HandshakeTimeout ?? _globalHubOptions.HandshakeTimeout ?? TimeSpan.FromSeconds(10);
            
            var connectionContext = new MqttHubConnectionContext(connection, keepAlive, _loggerFactory);

            var p = connectionContext.GetType().GetProperty("Protocol", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            p.SetValue(connectionContext, _protocol);

            try
            {
                await _lifetimeManager.OnConnectedAsync(connectionContext);
                await RunHubAsync(connectionContext);
            }
            finally
            {
                await _lifetimeManager.OnDisconnectedAsync(connectionContext);
            }
        }

        private async Task RunHubAsync(MqttHubConnectionContext connection)
        {
            try
            {
                await _dispatcher.OnConnectedAsync(connection);
            }
            catch (Exception ex)
            {
                Log.ErrorDispatchingHubEvent(_logger, "OnConnectedAsync", ex);

                await SendCloseAsync(connection, ex);

                // return instead of throw to let close message send successfully
                return;
            }

            try
            {
                await DispatchMessagesAsync(connection);
            }
            catch (Exception ex)
            {
                Log.ErrorProcessingRequest(_logger, ex);

                await HubOnDisconnectedAsync(connection, ex);

                // return instead of throw to let close message send successfully
                return;
            }

            await HubOnDisconnectedAsync(connection, null);
        }

        private async Task HubOnDisconnectedAsync(HubConnectionContext connection, Exception exception)
        {
            // send close message before aborting the connection
            await SendCloseAsync(connection, exception);

            // We wait on abort to complete, this is so that we can guarantee that all callbacks have fired
            // before OnDisconnectedAsync

            try
            {
                // Ensure the connection is aborted before firing disconnect
                //await connection.AbortAsync();
            }
            catch (Exception ex)
            {
                Log.AbortFailed(_logger, ex);
            }

            try
            {
                await _dispatcher.OnDisconnectedAsync(connection, exception);
            }
            catch (Exception ex)
            {
                Log.ErrorDispatchingHubEvent(_logger, "OnDisconnectedAsync", ex);
                throw;
            }
        }

        private async Task SendCloseAsync(HubConnectionContext connection, Exception exception)
        {
            var closeMessage = CloseMessage.Empty;

            if (exception != null)
            {
                //var errorMessage = ErrorMessageHelper.BuildErrorMessage("Connection closed with an error.", exception, _enableDetailedErrors);
                //closeMessage = new CloseMessage(errorMessage);
            }

            try
            {
                await connection.WriteAsync(closeMessage);
            }
            catch (Exception ex)
            {
                Log.ErrorSendingClose(_logger, ex);
            }
        }

        private async Task DispatchMessagesAsync(MqttHubConnectionContext connection)
        {
            // Since we dispatch multiple hub invocations in parallel, we need a way to communicate failure back to the main processing loop.
            // This is done by aborting the connection.

            try
            {
                while (true)
                {
                    var result = await connection.Input.ReadAsync(connection.ConnectionAborted);
                    var buffer = result.Buffer;

                    try
                    {
                        if (!buffer.IsEmpty)
                        {
                            while (_serializer.Deserialize(ref buffer, out var packet))
                            {
                                HandleMqttPacket(connection, packet);
                            }
                        }
                        else if (result.IsCompleted)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        // The buffer was sliced up to where it was consumed, so we can just advance to the start.
                        // We mark examined as buffer.End so that if we didn't receive a full frame, we'll wait for more data
                        // before yielding the read again.
                        connection.Input.AdvanceTo(buffer.Start, buffer.End);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If there's an exception, bubble it to the caller
                //connection.AbortException?.Throw();
            }
        }

        private void HandleMqttPacket(MqttHubConnectionContext connection, MqttBasePacket packet) 
        {
            switch (packet)
            {
                case MqttConnectPacket connect:
                    OnConnect(connection, connect);
                    break;
                case MqttPublishPacket publish:
                    OnPublish(connection, publish);
                    break;
                case MqttPingReqPacket ping:
                    OnPing(connection, ping);
                    break;
                case MqttSubscribePacket subscribe:
                    OnSubscribe(connection, subscribe);
                    break;
                case null:
                    break;
                default:
                    break;
            }
        }

        public void OnConnect(MqttHubConnectionContext connection, MqttConnectPacket connect)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"connect");
            }

            connection.WriteAsync(new MqttConnAckPacket()
            {
                ConnectReturnCode = MqttConnectReturnCode.ConnectionAccepted
            });
        }


        public void OnPublish(MqttHubConnectionContext connection, MqttPublishPacket publish)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"publish {publish.Topic}");
            }

            _packets.OnNext(publish);

            if (publish.QualityOfServiceLevel == MqttQualityOfServiceLevel.AtMostOnce)
            {
                return;
            }

            connection.WriteAsync(new MqttPubAckPacket()
            {
                PacketIdentifier = publish.PacketIdentifier,
            });
        }

        public void OnPing(MqttHubConnectionContext connection, MqttPingReqPacket ping)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"ping");
            }
            connection.WriteAsync(new MqttPingRespPacket()
            {
            });
        }

        public async Task OnSubscribe(MqttHubConnectionContext connection, MqttSubscribePacket subscribe)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"subscribe {string.Join(", ", subscribe.TopicFilters.Select(t => t.Topic))}");
            }

            var ack = new MqttSubAckPacket()
            {
                PacketIdentifier = subscribe.PacketIdentifier,
            };

            foreach (var topic in subscribe.TopicFilters)
            {
                ack.SubscribeReturnCodes.Add(MqttSubscribeReturnCode.SuccessMaximumQoS0);
            }

            await connection.WriteAsync(ack);

            var filteredPublishPackets = _packets
                //.Do(p => logger.LogInformation($"publish {p.Topic}"))
                .Where(p =>
                {
                    var result = subscribe.TopicFilters.Any(f => MqttTopicFilterComparer.IsMatch(p.Topic, f.Topic));

                    // logger.LogInformation($"filter {p.Topic}: {result}");

                    return result;
                })
                .Do(p => connection.WriteAsync(p));
        }

        private static class Log
        {
            private static readonly Action<ILogger, string, Exception> _errorDispatchingHubEvent =
                LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "ErrorDispatchingHubEvent"), "Error when dispatching '{HubMethod}' on hub.");

            private static readonly Action<ILogger, Exception> _errorProcessingRequest =
                LoggerMessage.Define(LogLevel.Error, new EventId(2, "ErrorProcessingRequest"), "Error when processing requests.");

            private static readonly Action<ILogger, Exception> _abortFailed =
                LoggerMessage.Define(LogLevel.Trace, new EventId(3, "AbortFailed"), "Abort callback failed.");

            private static readonly Action<ILogger, Exception> _errorSendingClose =
                LoggerMessage.Define(LogLevel.Debug, new EventId(4, "ErrorSendingClose"), "Error when sending Close message.");

            public static void ErrorDispatchingHubEvent(ILogger logger, string hubMethod, Exception exception)
            {
                _errorDispatchingHubEvent(logger, hubMethod, exception);
            }

            public static void ErrorProcessingRequest(ILogger logger, Exception exception)
            {
                _errorProcessingRequest(logger, exception);
            }

            public static void AbortFailed(ILogger logger, Exception exception)
            {
                _abortFailed(logger, exception);
            }

            public static void ErrorSendingClose(ILogger logger, Exception exception)
            {
                _errorSendingClose(logger, exception);
            }
        }
    }
}
