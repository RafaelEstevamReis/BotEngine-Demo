using System;
using System.Threading.Tasks;
using Serilog;
using Simple.BotUtils.DI;
using Simple.BotUtils.Jobs;
using Telegram.Bot;

namespace Rasp.Test
{
    public class TelegramPing : JobBase
    {
        private readonly ILogger logger;
        private readonly TelegramBotClient bot;
        private readonly Config cfg;

        public TelegramPing()
        {
            logger = Injector.Get<ILogger>();
            bot = Injector.Get<TelegramBotClient>();
            cfg = Injector.Get<Config>();

            RunOnStartUp = true;
            CanBeScheduled = true;
            StartEvery = TimeSpan.FromMinutes(5);

            logger.Information("TelegramPing started");
        }

        public override async Task ExecuteAsync(ExecutionTrigger trigger, object parameter)
        {
            logger.Information("TelegramPing: {trigger}", trigger);

            try
            {
                var chat = new Telegram.Bot.Types.ChatId(cfg.TelegramAdmin);
                Telegram.Bot.Types.Message message;
                if (trigger == ExecutionTrigger.Startup)
                {
                    message = await bot.SendTextMessageAsync(chat, "StartUp");
                }
                else if (trigger == ExecutionTrigger.Scheduled)
                {
                    message = await bot.SendTextMessageAsync(chat, "The time has come...");
                }
                else return;

                logger.Information("Ping sent: {@message}", message);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "TelegramPing:ExecuteAsync error");
            }
        }
    }
}
