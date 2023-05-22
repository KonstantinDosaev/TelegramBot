using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var client =new TelegramBotClient("tocken");

            client.StartReceiving(Update, HandleErrorAsync);

            
            Console.ReadLine();
        }




        static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {


            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Message.ReplyMarkup.InlineKeyboard.Count() == 1)
            {
                Console.WriteLine($"{update.CallbackQuery.Message.Chat.FirstName}");
                await Field.HandleMessage(botClient, update.CallbackQuery, token);
                return;
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                 
                await Field.HandleCallbackQuery(botClient, update.CallbackQuery,token);

                return;

            }
            InlineKeyboardMarkup keyboardMenu = new(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("3 x 3", "3"),
                    InlineKeyboardButton.WithCallbackData("5 x 5", "5"),
                });

            await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "tic tac toe",
                replyMarkup: keyboardMenu,
                cancellationToken: token);
            return;



        }

        static Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
