using Serilog;
using Simple.BotUtils.Controllers;
using Simple.BotUtils.Data;
using Simple.BotUtils.DI;
using Simple.BotUtils.Jobs;
using Simple.BotUtils.Startup;
using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;


/*
    Welcome to my Bot Demo Project!
    I Hope you can learn a lot from it. 
    If you have any sugestions about usability and learning, 
     feel free to colaborate with the project

    How to start?
    1. You must talk with Telegram's bot: @BotFather to create your bot and get your token
    2. Initialize this bot using command line arguments, and setting:
      a. Your token
      b. Your admin account (your account id)
      The easier way to do that is in visual studio's project settings
      But you can also uncomment down below
    3. Run the project and interact with the bot
      You can see the available commands in Commands.cs and HelpCommands.cs

    This project has a lot of built-in tools
    1. Configuration Manager
    2. DI Engine (Dependecy Injection - a simple and lightweight one)
    3. Built-in logs
    4. A versatile Task Scheduler (see TelegramPing.cs)
    5. All commands use an Endpoint-like mechanism 
       You can add commands simply adding a new "Route" to the Endpoint


    How can I add a new commands?
     All classes with commands are added in the `setupTelegramCommands` method
     Simply add a new Controller like the existing ones
     All controllers (classes) addded have it's methods added

    How can I add a new Scheduler?
     1. Create a new JobBase
     2. Setup it's triggers in the constructor (see TelegramPing.cs)
     3. Overrides the ExecuteAsync method
     4. Add the class to the Scheduler (method `AddJobs` below)

    So ... This bot has DI? How do I uset it?
     Call `Injector.Get<TheClassYouWant>()` 
     For example, you can get the Logger instance with: Injector.Get<ILogger>()
     • var cfg = Injector.Get<Config>(); // To get the config
     • var bot = Injector.Get<TelegramBotClient>(); // To get the TelegramBotClient
     And you can add your class with Injector.AddSingleton<MyClass>(myClassInstance);

    This bot-demo is OpenSource and available at https://github.com/RafaelEstevamReis/BotEngine-Demo
    The underlying library `BotUtils` is OpenSource and available at https://github.com/RafaelEstevamReis/Simple.BotUtils

 */


namespace Rasp.Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing... v0.1");

            var cancellationSource = new CancellationTokenSource();

            // Uncomment this part to setup the bot, you have to do this only once
            // BUT NEVER EVER commit your code with your token

            /*
                // Ask for the token 
                Console.WriteLine("Paste your @BotFather token");
                string token = Console.ReadLine();
                args = new string[] { "-token", token };
            */

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
            if (!cfg.TelegramToken.Contains(":")) throw new Exception("Invalid Token");
            if (cfg.TelegramAdmin == 0)
            {
                // Configure here if you want to setup a an admin
                // Uncomment below to not let you bot start without an admin setted
                //throw new Exception("Telgram admin not configured");

                // You can later use this to decide if someone is Worthy of accessing your bot
                Console.WriteLine("No Admin configured !!");
            }

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
