using Diffcord;

Console.WriteLine("Diffcord");

DotNetEnv.Env.Load();

new BotFrame(Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"))
    .MainAsync()
    .GetAwaiter()
    .GetResult();
