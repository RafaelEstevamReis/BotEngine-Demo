using Simple.BotUtils.Controllers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Rasp.Test
{
    public class Commands : IController
    {
        public async void Ping([FromDI] TelegramBotClient client, Message message)
        {
            await client.SendTextMessageAsync(message.Chat, "Pong", replyToMessageId: message.MessageId);
        }

        public async void Help([FromDI] TelegramBotClient client, Message message)
        {
            await client.SendTextMessageAsync(message.Chat, "So much help!", replyToMessageId: message.MessageId);
        }
        public async void Help([FromDI] TelegramBotClient client, Message message, string commandName)
        {
            await client.SendTextMessageAsync(message.Chat, $"Help for {commandName}", replyToMessageId: message.MessageId);
        }
    }
}
