using Serilog;
using Simple.BotUtils.DI;
using Simple.BotUtils.Jobs;
using Simple.BotUtils.Startup;
using System;
using System.Threading;
using Telegram.Bot;

namespace Rasp.Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing... v0.1");

            setupArgs(args);
            setupLogs();
            setupConfig();
            setupTelegramBot();
            var sch = setupScheduler();
            Injector.Get<ILogger>().Information("Initialization complete");

            var cancellationSource = new CancellationTokenSource();
            sch.RunJobsSynchronously(cancellationSource.Token);
        }

        private static void setupArgs(string[] args)
        {
            var arguments = ArgumentParser.Parse(args);
            Injector.AddSingleton(arguments);
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
        private static void setupConfig()
        {
            var cfg = Config.Load();
            Injector.AddSingleton(cfg);
        }
        private static Scheduler setupScheduler()
        {
            var tasker = new Scheduler();
            AddJobs(tasker);

            Injector.Get<ILogger>().Information("SETUP Scheduler init {TaskCount} tasks", tasker.JobCount);

            Injector.AddSingleton(typeof(Scheduler), tasker);
            return tasker;
        }
        private static void AddJobs(Scheduler tasker)
        {
            tasker.Add(new TelegramPing());
        }
        private static void setupTelegramBot()
        {
            var cfg = Injector.Get<Config>();

            if (cfg.TelegramToken == null)
            {
                var args = Injector.Get<Arguments>();
                cfg.TelegramToken = args.Get("-token");
                if (!string.IsNullOrEmpty(cfg.TelegramToken))
                {
                    cfg.Save();
                    Injector.Get<ILogger>().Information("New TelegramBot token registered");
                }
            }
            if (cfg.TelegramAdmin == 0)
            {
                var args = Injector.Get<Arguments>();
                string admin = args.Get("-admin");
                if (!string.IsNullOrEmpty(admin))
                {
                    cfg.TelegramAdmin = int.Parse(admin);
                    cfg.Save();
                    Injector.Get<ILogger>().Information("New Telegram admin token registered");
                }
            }

            if (cfg.TelegramToken == null) throw new Exception("Telgram bot not configured");
            if (cfg.TelegramAdmin == 0) throw new Exception("Telgram admin not configured");

            var client = new TelegramBotClient(cfg.TelegramToken);
            Injector.Get<ILogger>().Information("SETUP Telegram bot initialized");

            Injector.AddSingleton(typeof(TelegramBotClient), client);
        }
    }
}
