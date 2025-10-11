// // using Telegram.Bot;
// // using Telegram.Bot.Polling;
// // using Telegram.Bot.Types.Enums;
// // using System;
// // using System.IO;

// // class Program
// // {
// //     // private static ITelegramBotClient _botClient;

// //     // private static ReceiverOptions _receiverOptions;
// //     public static async Task Main()

// //     // static async Task Main()
// //     {
// //         string token = File.ReadLines("token.txt").First();
// //         Console.WriteLine(token);

// //         // foreach (string line in File.ReadLines(filePath))
// //         // {
// //         //     Console.WriteLine(line);
// //         // }



// //         var bot = new TelegramBotClient(token);
// //         var me = await bot.GetMe();
// //         Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
// //         // _botClient = new TelegramBotClient("<token>");
// //         // _receiverOptions = new ReceiverOptions
// //         // {
// //         //     AllowedUpdates = new[]{
// //         //         UpdateType.Message,
// //         //     },
// //         // }
// //     }
// // }

// using Telegram.Bot;
// using Telegram.Bot.Types;
// using Telegram.Bot.Types.Enums;

// using var cts = new CancellationTokenSource();
// var bot = new TelegramBotClient("", cancellationToken: cts.Token);
// var me = await bot.GetMe();
// bot.OnMessage += OnMessage;

// Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
// Console.ReadLine();
// cts.Cancel();

// async Task OnMessage(Message msg, UpdateType type)
// {
//     await bot.SendMessage(msg.Chat, "абоба");
// }



//ПОЧТА
// using MailKit.Net.Imap;
// using MailKit.Search;
// using MailKit;

// // Создаем клиент
// var client = new ImapClient();

// // Подключаемся к серверу
// await client.ConnectAsync("mail.surgu.ru", 993, true);

// // Аутентифицируемся
// await client.AuthenticateAsync("gavrilov_ee@edu.surgu.ru", "");

// // Открываем папку Inbox
// var inbox = client.Inbox;
// await inbox.OpenAsync(FolderAccess.ReadOnly);

// // Ищем все непрочитанные письма
// var uids = await inbox.SearchAsync(SearchQuery.All);

// // Если есть письма, берем первое
// foreach (var i in uids)
// {
//     var message = await inbox.GetMessageAsync(i);
//     Console.WriteLine($"От: {message.From}");
//     Console.WriteLine($"Тема: {message.Subject}");
//     Console.WriteLine($"Дата: {message.Date}");
//     Console.WriteLine($"Текст: {message.TextBody}");
// }


// // Отключаемся
// await client.DisconnectAsync(true);












using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

string token = File.ReadLines("token.txt").First();

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

// method to handle errors in polling or in your OnMessage/OnUpdate code
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // just dump the exception to the console
}

// method that handle messages received by the bot:
async Task OnMessage(Message msg, UpdateType type)
{
    await bot.SendMessage(msg.Chat, $"BRO SAID: \"{msg.Text}\"", replyParameters: msg.Id);
    // if (msg.Text == "/start")
    // {
    //     await bot.SendMessage(msg.Chat, "Welcome! Pick one direction",
    //         replyMarkup: new InlineKeyboardButton[] { "Left", "Right" });
    // }

}

// method that handle other types of updates received by the bot:
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
        await bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
    }
}