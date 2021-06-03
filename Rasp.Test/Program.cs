using Serilog;
using Simple.BotUtils.Data;
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

            setupConfig(args);
            setupLogs();
            setupTelegramBot();
            var sch = setupScheduler();
            Injector.Get<ILogger>().Information("Initialization complete");

            var cancellationSource = new CancellationTokenSource();
            sch.RunJobsSynchronously(cancellationSource.Token);
        }

        private static void setupConfig(string[] args)
        {
            var cfg = ConfigBase.Load<Config>("config.xml");
            
            ArgumentParser.ParseInto(args, cfg);
            if (args.Length > 0) cfg.Save();

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

            if (cfg.TelegramToken == null) throw new Exception("Telgram bot not configured");
            if (cfg.TelegramAdmin == 0) throw new Exception("Telgram admin not configured");

            var client = new TelegramBotClient(cfg.TelegramToken);
            Injector.Get<ILogger>().Information("SETUP Telegram bot initialized");

            Injector.AddSingleton(typeof(TelegramBotClient), client);
        }
    }
}
