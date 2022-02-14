using Simple.BotUtils.Controllers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Rasp.Test
{
    public class Commands : IController
    {
        private readonly TelegramBotClient client;
        public Commands(TelegramBotClient client)
        {
            this.client = client;
        }

        public async void Ping(Message message)
        {
            await client.SendTextMessageAsync(message.Chat, "Pong", replyToMessageId: message.MessageId);
        }
        public async void Echo(Message message, params string[] content)
        {
            // rebuild text
            string text = string.Join(" ", content);
            await client.SendTextMessageAsync(message.Chat, $"Echo\n{text}", replyToMessageId: message.MessageId);
        }

    }
}
