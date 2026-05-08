using MediaBrowser.Controller.Net;
using MediaBrowser.Controller.Net.WebSocketMessages;
using MediaBrowser.Model.Session;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JellyfinLocalChat
{
    public class ChatWebSocketListener : IWebSocketListener
    {
        private readonly ChatService _chatService;

        public ChatWebSocketListener(ChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task ProcessWebSocketConnectedAsync(
            IWebSocketConnection connection, 
            HttpContext httpContext)
        {
            var username = connection.AuthorizationInfo?.User?.Username ?? "Unknown";
            var userId = connection.AuthorizationInfo?.UserId ?? Guid.Empty;

            var client = new ClientConnection
            {
                Username = username,
                UserId = userId,
                Connection = connection
            };

            _chatService.Clients.Add(client);
            
            // Send chat history
            var history = _chatService.GetMessages();
            var historyMessage = new OutboundWebSocketMessage<object>
            {
                MessageType = SessionMessageType.GeneralCommand,
                Data = new { type = "history", messages = history }
            };
            await connection.SendAsync(historyMessage, CancellationToken.None);

            await BroadcastUsers();

            // Register message handler
            connection.OnReceive = ProcessMessageAsync;
        }

        public async Task ProcessMessageAsync(WebSocketMessageInfo message)
        {
            if (message == null || string.IsNullOrEmpty(message.Data))
                return;

            var connection = message.Connection;
            var username = connection.AuthorizationInfo?.User?.Username ?? "Unknown";

            try
            {
                var doc = JsonDocument.Parse(message.Data);
                var type = doc.RootElement.GetProperty("type").GetString();

                if (type == "message")
                {
                    var text = doc.RootElement.GetProperty("text").GetString();
                    var msg = _chatService.AddMessage(username, text);

                    await Broadcast(new
                    {
                        type = "message",
                        id = msg.Id,
                        user = username,
                        text = msg.Message
                    });
                }

                if (type == "typing")
                {
                    _chatService.TypingUsers.Add(username);
                    await Broadcast(new { type = "typing", user = username });
                }

                if (type == "stopTyping")
                {
                    _chatService.TypingUsers.Remove(username);
                }

                if (type == "delete")
                {
                    var id = doc.RootElement.GetProperty("id").GetGuid();
                    _chatService.DeleteMessage(id);
                    await Broadcast(new { type = "delete", id });
                }
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error processing WebSocket message: {ex.Message}");
            }
        }

        private async Task Broadcast(object obj)
        {
            var message = new OutboundWebSocketMessage<object>
            {
                MessageType = SessionMessageType.GeneralCommand,
                Data = obj
            };
            
            var clientsToRemove = new List<ClientConnection>();
            
            foreach (var client in _chatService.Clients.ToList())
            {
                try
                {
                    await client.Connection.SendAsync(message, CancellationToken.None);
                }
                catch
                {
                    clientsToRemove.Add(client);
                }
            }

            foreach (var client in clientsToRemove)
            {
                _chatService.Clients.Remove(client);
            }
        }

        private async Task BroadcastUsers()
        {
            var users = _chatService.Clients.Select(c => new { c.Username, TypingUsers = _chatService.TypingUsers.ToList() });
            await Broadcast(new { type = "users", users });
        }
    }

    // Simple message wrapper for SendAsync
    public class WebSocketMessage<T>
    {
        public string MessageType { get; set; }
        public T Data { get; set; }
    }
}