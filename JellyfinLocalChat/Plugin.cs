using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JellyfinLocalChat
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly IXmlSerializer _xmlSerializer;

        public override string Name => "Local Chat";
        public override Guid Id => Guid.Parse("b3d8b5a2-7c2c-4a5a-9d52-111111111111");

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            _xmlSerializer = xmlSerializer;
            Instance = this;
        }

        public static Plugin Instance { get; private set; }

        // 👇 THIS injects script into Jellyfin automatically
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "chat-inject",
                    EmbeddedResourcePath = "JellyfinLocalChat.inject.html"
                },
                new PluginPageInfo
                {
                    Name = "chat-overlay.js",
                    EmbeddedResourcePath = "JellyfinLocalChat.chat-overlay.js"
                }
            };
        }

        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ChatService>(provider => new ChatService(ApplicationPaths.DataPath));
            services.AddSingleton<ChatWebSocketListener>();
        }
    }

    public class PluginConfiguration : BasePluginConfiguration { }
}