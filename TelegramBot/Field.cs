using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Linq;

namespace TelegramBot
{
    public class Field
    {
        static readonly string playerOne = "❌";
        static readonly string playerTwoOrAi = "⭕";
        static readonly string empty = " ";
        public static async Task HandleMessage(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken token)
        {
            var lengthField = Convert.ToInt32(callbackQuery.Data);

            var inlineArray = new InlineKeyboardButton[lengthField][];

            for (int i = 0, k = 0; i < lengthField; i++)
            {
                inlineArray[i] = new InlineKeyboardButton[lengthField];

                for (int j = 0; j < lengthField; j++)
                {
                    inlineArray[i][j] = new InlineKeyboardButton(text: empty) { CallbackData = $"{k}"};
                    k++;
                }
            }

            InlineKeyboardMarkup inlineKeyboard = new(inlineArray);

            if (callbackQuery.Message != null)
                await botClient.EditMessageReplyMarkupAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    replyMarkup: inlineKeyboard,
                    cancellationToken: token);
            return;
        }
        public static async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken token)
        {
            var inlineKb = callbackQuery.Message!.ReplyMarkup!.InlineKeyboard;
            var sr = new(string,string)[inlineKb.Count(), inlineKb.Count()];

            var i = 0; var j=0;
            foreach (var items in inlineKb)
            {
                foreach (var item in items)
                {
                    if (item.CallbackData == callbackQuery.Data && item.Text is "⭕" or "❌") return;

                    if (item.CallbackData == callbackQuery.Data && item.Text == empty)
                    {
                        item.Text = playerOne;
                    }

                    sr[i,j] = (item.Text, item.CallbackData);
                    j++;
                }
                i++;
                j = 0;
            }

            var (item1, item2) = MoveAi(sr);

                foreach (var items in inlineKb)
                {
                    foreach (var item in items)
                    {
                        if (item.CallbackData == item2)
                        {
                            item.Text = playerTwoOrAi;
                        }
                    }
                }

            if (item1 != empty)
            {
                var textResult = item1 switch
                {
                    "winOne" => "ПОБЕДИЛ ИГРОК",
                    "winAi" => "ПОБЕДИЛ AI",
                    _ => "НИЧЬЯ"
                };

                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"{textResult}: /RESET ", cancellationToken: token);

            }

            await botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: callbackQuery.Message.ReplyMarkup,
                cancellationToken: token);
            return;

        }

        public static (string, string) MoveAi((string, string)[,] fieldArr)
        {
            var temp = new List<List<(string, string)>>();
            var diagonalOne = new List<(string, string)>();
            var diagonalTwo = new List<(string, string)>();
            for (int i = 0; i < fieldArr.GetLongLength(0); i++)
            {
                var horizontal = new List<(string, string)>();
                var vertical = new List<(string, string)>();
                for (int j = 0; j < fieldArr.GetLongLength(1); j++)
                {
                    horizontal.Add(fieldArr[i, j]);
                    vertical.Add(fieldArr[j, i]);
                    if (i != 0) continue;
                    diagonalOne.Add(fieldArr[j, fieldArr.GetLength(1) - 1 - j]);
                    diagonalTwo.Add(fieldArr[j, j]);

                }
                temp.Add(horizontal);
                temp.Add(vertical);
            }
            temp.Add(diagonalOne);
            temp.Add(diagonalTwo);

            //win pl
            if (temp.Any(a => a.Count(c => c.Item1 == playerOne) == temp[0].Count))
            {
                return ("winOne","");
            }
           
            if (temp.All(a => a.Count(c => c.Item1 == empty) == 0))
            {
                return ("draw", "");
            }
            //win Ai
            if (temp.Any(a => a.Count(c => c.Item1 == playerTwoOrAi) == temp[0].Count - 1 && a.Count(c => c.Item1 == empty) > 0))
            {
               var vinCb = temp.First(a => a.Count(c => c.Item1 == playerTwoOrAi) == temp[0].Count - 1 && a.Count(c => c.Item1 == empty) > 0).
                    OrderBy(c => c.Item1).FirstOrDefault().Item2;
               return ("winAi", vinCb);
            }
            //counter - move
            if (temp.Any(a => a.Count(c => c.Item1 == playerOne) == temp[0].Count - 1 && a.Count(c => c.Item1 == empty) > 0))
            {
                return temp.Last(a => a.Count(c => c.Item1 == playerOne) == temp[0].Count -  1 && a.Count(c => c.Item1 == empty) > 0).
                    OrderBy(c => c.Item1).FirstOrDefault();
            }

            if (temp.Any(a => a.Count(c => c.Item1 == playerTwoOrAi) >= 1 && a.Count(c => c.Item1 == playerOne) == 0))
            {
                return temp.First(a => a.Count(c => c.Item1 == playerTwoOrAi) >= 1 && a.Count(c => c.Item1 == playerOne) == 0).
                    OrderBy(c => c.Item1).FirstOrDefault();
            }

            if (fieldArr[temp[0].Count / 2,temp[0].Count/2].Item1 == empty)
            {
                return fieldArr[temp[0].Count / 2,temp[0].Count / 2];
            }
            return temp.First(c => c.Any(a => a.Item1 == empty)).OrderBy(c => c.Item1).FirstOrDefault();

        }




    }
}
