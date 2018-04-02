using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Core;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.AspNetCore.SignalR.Internal.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Playground.core.Hubs
{
    public class MyHubConnectionHandler<THub, TProtocol> : ConnectionHandler 
        where THub : Hub
        where TProtocol : IHubProtocol
    {
        private readonly HubLifetimeManager<THub> _lifetimeManager;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<HubConnectionHandler<THub>> _logger;
        private readonly HubOptions<THub> _hubOptions;
        private readonly HubOptions _globalHubOptions;
        private readonly IUserIdProvider _userIdProvider;
        private readonly HubDispatcher<THub> _dispatcher;
        private readonly TProtocol _protocol;
        private readonly bool _enableDetailedErrors;

        public MyHubConnectionHandler(HubLifetimeManager<THub> lifetimeManager,
                                    IOptions<HubOptions> globalHubOptions,
                                    IOptions<HubOptions<THub>> hubOptions,
                                    ILoggerFactory loggerFactory,
                                    IUserIdProvider userIdProvider,
                                    HubDispatcher<THub> dispatcher,
                                    TProtocol protocol)
        {
            _lifetimeManager = lifetimeManager;
            _loggerFactory = loggerFactory;
            _hubOptions = hubOptions.Value;
            _globalHubOptions = globalHubOptions.Value;
            _logger = loggerFactory.CreateLogger<HubConnectionHandler<THub>>();
            _userIdProvider = userIdProvider;
            _dispatcher = dispatcher;
            _protocol = protocol;
            _enableDetailedErrors = false;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            // We check to see if HubOptions<THub> are set because those take precedence over global hub options.
            // Then set the keepAlive and handshakeTimeout values to the defaults in HubOptionsSetup incase they were explicitly set to null.
            var keepAlive = _hubOptions.KeepAliveInterval ?? _globalHubOptions.KeepAliveInterval ?? TimeSpan.FromSeconds(5);
            var handshakeTimeout = _hubOptions.HandshakeTimeout ?? _globalHubOptions.HandshakeTimeout ?? TimeSpan.FromSeconds(10);
            
            var connectionContext = new HubConnectionContext(connection, keepAlive, _loggerFactory);

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

        private async Task RunHubAsync(HubConnectionContext connection)
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

        private async Task DispatchMessagesAsync(HubConnectionContext connection)
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
                            while (_protocol.TryParseMessage(ref buffer, _dispatcher, out var message))
                            {
                                // Don't wait on the result of execution, continue processing other
                                // incoming messages on this connection.
                                _ = _dispatcher.DispatchMessageAsync(connection, message);
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
