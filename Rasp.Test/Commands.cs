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
    }
}
