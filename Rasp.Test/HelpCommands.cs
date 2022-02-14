using Simple.BotUtils.Controllers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Rasp.Test
{
    public class HelpCommands : IController
    {
        private readonly TelegramBotClient client;
        public HelpCommands(TelegramBotClient client)
        {
            this.client = client;
        }

        public async void Help(Message message)
        {
            await client.SendTextMessageAsync(message.Chat, "So much help!", replyToMessageId: message.MessageId);
        }
        public async void Help(Message message, string commandName)
        {
            await client.SendTextMessageAsync(message.Chat, $"Help for {commandName}", replyToMessageId: message.MessageId);
        }
    }
}
