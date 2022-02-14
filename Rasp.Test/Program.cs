using Serilog;
using Simple.BotUtils.Data;
using Simple.BotUtils.DI;
using Simple.BotUtils.Jobs;
using Simple.BotUtils.Startup;
using Telegram.Bot.Extensions.Polling;
using System;
using System.Threading;
using Telegram.Bot;
using Simple.BotUtils.Controllers;

namespace Rasp.Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing... v0.1");

            var cancellationSource = new CancellationTokenSource();

            setupConfig(args);
            setupLogs();
            setupTelegramBot(cancellationSource.Token);
            setupTelegramCommands();
            var sch = setupScheduler();
            Injector.Get<ILogger>().Information("Initialization complete");

            sch.RunJobsSynchronously(cancellationSource.Token);
        }

        private static void setupConfig(string[] args)
        {
            // Load saved config (or create a empty one)
            var cfg = Config.Load("config.xml");
            // Update config with arguments, if any
            if (args.Length > 0)
            {
                ArgumentParser.ParseInto(args, cfg);
                // and save to next boot
                cfg.Save();
            }

            Injector.AddSingleton(cfg);
        }
        private static void setupLogs()
        {
            ILogger log = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("tests.log", rollingInterval: RollingInterval.Month)
                .CreateLogger();
            log.Information("Logging started");
            Injector.AddSingleton(log);
        }
        private static Scheduler setupScheduler()
        {
            var tasker = new Scheduler();
            AddJobs(tasker);

            Injector.Get<ILogger>().Information("SETUP Scheduler init {TaskCount} tasks", tasker.JobCount);

            Injector.AddSingleton(tasker);
            return tasker;
        }
        private static void AddJobs(Scheduler tasker)
        {
            tasker.Add(new TelegramPing());
        }
        private static void setupTelegramBot(CancellationToken token)
        {
            var cfg = Injector.Get<Config>();

            if (cfg.TelegramToken == null) throw new Exception("Telgram bot not configured");
            if (cfg.TelegramAdmin == 0) throw new Exception("Telgram admin not configured");

            var client = new TelegramBotClient(cfg.TelegramToken);
            client.StartReceiving(new DefaultUpdateHandler(UpdateHandler.HandleUpdateAsync, UpdateHandler.HandleErrorAsync), cancellationToken: token);

            Injector.Get<ILogger>().Information("SETUP Telegram bot initialized");

            Injector.AddSingleton(client);
        }
        private static void setupTelegramCommands()
        {
            var ctrl = new ControllerManager()
                        .AddController<Commands>()
                        .AddController<HelpCommands>();
            ctrl.AcceptSlashInMethodName = true;

            Injector.AddSingleton(ctrl);
        }

    }
}
