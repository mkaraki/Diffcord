using DiffPlex.DiffBuilder;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diffcord
{
    internal class BotFrame
    {
        internal BotFrame(string apikey)
        {
            ApiKey = apikey ?? throw new ArgumentNullException("No Token Provided");
            ReadConfig();

            var dcfg = new DiscordSocketConfig();
            dcfg.MessageCacheSize = 10000;
            Client = new(dcfg);

            Client.Log += Client_Log;
            Client.Ready += Client_Ready;
            Client.MessageReceived += Client_MessageReceived;
            Client.MessageDeleted += Client_MessageDeleted;
            Client.MessageUpdated += Client_MessageUpdated;
        }

        private Task Client_MessageUpdated(Discord.Cacheable<Discord.IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
        {
            if (config.logging_subsystem_edit != true) return Task.CompletedTask;
            if (config.logging_channel == 0) return Task.CompletedTask;

            var c = Client.GetChannel(config.logging_channel) as IMessageChannel;

            if (arg1.Value == null)
                c.SendMessageAsync($"Detected <@{arg2.Author.Id}> Update Message in <#{arg3.Id}>: NO CACHE: {arg2.GetJumpUrl()}");
            else
            {
                if (arg1.Value.Content == arg2.Content) return Task.CompletedTask;
                if (arg1.Value.Author.Id == Client.CurrentUser.Id) return Task.CompletedTask;
                var diff = InlineDiffBuilder.Diff(arg1.Value.Content, arg2.Content);
                string unidiff = string.Empty;
                foreach (var l in diff.Lines)
                {
                    switch (l.Type)
                    {
                        case DiffPlex.DiffBuilder.Model.ChangeType.Inserted:
                            unidiff += '+' + l.Text + '\n';
                            break;

                        case DiffPlex.DiffBuilder.Model.ChangeType.Deleted:
                            unidiff += '-' + l.Text + '\n';
                            break;
                    }
                }
                c.SendMessageAsync($"Detected <@{arg2.Author.Id}> Update Message in <#{arg3.Id}>: {arg2.GetJumpUrl()}\n```diff\n" +
                    unidiff + "```");
            }



            return Task.CompletedTask;
        }

        private Task Client_MessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> arg1, Discord.Cacheable<Discord.IMessageChannel, ulong> arg2)
        {
            if (config.logging_subsystem_delete != true) return Task.CompletedTask;
            if (config.logging_channel == 0) return Task.CompletedTask;

            var c = Client.GetChannel(config.logging_channel) as IMessageChannel;

            if (arg1.Value == null)
                c.SendMessageAsync("Detected Message Deletion: NO CACHE IN SERVICE");
            else
            {
                if (arg1.Value.Author.Id == Client.CurrentUser.Id) return Task.CompletedTask;
                c.SendMessageAsync($"Detected <@{arg1.Value.Author.Id}>'s message deleted in <#{arg2.Value.Id}>\n```" +
                    arg1.Value.Content.Replace("```", "\\```") + "\n```");
            }

            return Task.CompletedTask;
        }

        private Task Client_MessageReceived(SocketMessage msg)
        {
            if (msg.Author.IsBot) return Task.CompletedTask;

            if (!msg.Content.StartsWith("+=")) return Task.CompletedTask;

            string evm = msg.Content.Substring(2);
            string[] args = evm.Split(' ');

            bool? confbool = null;

            switch (args[0])
            {
                case "logging":
                    if (args.Length < 2) return Task.CompletedTask;
                    switch (args[1])
                    {
                        case "subsystem":
                            if (args.Length < 4) return Task.CompletedTask;
                            confbool = YesNoBool(args[3]);
                            if (!confbool.HasValue) return Task.CompletedTask;
                            switch (args[2])
                            {
                                case "edit":
                                    config.logging_subsystem_edit = confbool.Value;
                                    break;
                                case "delete":
                                    config.logging_subsystem_delete = confbool.Value;
                                    break;
                            }
                            break;

                        case "here":
                            config.logging_channel = msg.Channel.Id;
                            break;
                    }
                    break;

                case "write":
                    msg.Channel.SendMessageAsync("Building Configuration");
                    WriteConfig();
                    break;
            }

            return Task.CompletedTask;
        }

        private Task Client_Ready()
        {
            Console.WriteLine("Connected");
            return Task.CompletedTask;
        }

        private Task Client_Log(Discord.LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private readonly DiscordSocketClient Client;

        private readonly string ApiKey;

        private Config config = new Config();

        private bool? YesNoBool(string yesno)
        { 
            yesno = yesno.ToLower();
            yesno = yesno.Trim();

            if (yesno == "yes")
                return true;
            else if (yesno == "no")
                return false;
            else
                return null;
        }

        private void ReadConfig()
        {
            var ser = new YamlDotNet.Serialization.DeserializerBuilder().Build();
            if (!File.Exists("config.yaml"))
                WriteConfig();
            var file = new StreamReader("config.yaml", Encoding.UTF8);
            config = ser.Deserialize<Config>(file);
        }

        private void WriteConfig()
        {
            var ser = new YamlDotNet.Serialization.SerializerBuilder().Build();
            var yml = ser.Serialize(config);

            File.WriteAllText("config.yaml", yml, Encoding.UTF8);
        }

        internal async Task MainAsync()
        {
            await Client.LoginAsync(Discord.TokenType.Bot, ApiKey);

            await Client.StartAsync();

            await Task.Delay(Timeout.Infinite);
        }
    }

    public class Config
    {
        public bool logging_subsystem_delete { get; set; } = false;

        public bool logging_subsystem_edit { get; set; } = false;

        public ulong logging_channel { get; set; } = 0;
    }
}
