// using Telegram.Bot;
// using Telegram.Bot.Polling;
// using Telegram.Bot.Types.Enums;
// using System;
// using System.IO;

// // using var cts = new CancellationTokenSource();
// // var bot = new TelegramBotClient("", cancellationToken: cts.Token);
// // var me = await bot.GetMe();
// // bot.OnMessage += OnMessage;

// // Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
// // Console.ReadLine();
// // cts.Cancel();

// // async Task OnMessage(Message msg, UpdateType type)
// // {
// //     await bot.SendMessage(msg.Chat, "абоба");
// // }
// public class BotService
// {
//     private static ITelegramBotClient _botClient;
//     // private static ReceiverOptions _receiverOptions;
//     public BotService(string token)
//     {
//         _botClient = new TelegramBotClient(token);
//         // _receiverOptions = new ReceiverOptions
//         // {
//         //     AllowedUpdates = new[]
//         //     {
//         //         UpdateType.Message,
//         //     },
//         //     ThrowPendingUpdates = true,
//         // };

//     }
// }
