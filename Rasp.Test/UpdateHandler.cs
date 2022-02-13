using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Simple.BotUtils.Controllers;
using Simple.BotUtils.DI;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rasp.Test
{
    public class UpdateHandler
    {
        // Full example at TelegramBot repository examples directory
        // https://github.com/TelegramBots/Telegram.Bot.Examples/blob/master/Telegram.Bot.Examples.Polling/Handlers.cs

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult),
                //UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
                //UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery),

                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            var ctrl = Injector.Get<ControllerManager>();
            try
            {
                ctrl.ExecuteFromText(message.Text);
            }
            catch (KeyNotFoundException)
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Echo [KNF]:\n{message.Text}", replyToMessageId: message.MessageId);
            }
            catch (NoSuitableMethodFound)
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Echo [NSM]:\n{message.Text}", replyToMessageId: message.MessageId);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat, $"Error:\n{ex.Message}", replyToMessageId: message.MessageId);
            }
        }

        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown type: {update.Type}");
            return Task.CompletedTask;
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);

            return Task.CompletedTask;
        }
    }
}
