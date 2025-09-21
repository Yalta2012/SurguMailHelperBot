using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using System;
using System.IO;

class Program
{
    private static ITelegramBotClient _botClient;

    private static ReceiverOptions _receiverOptions;
    public static void Main()

    // static async Task Main()
    {
        string filePath = "token.txt";
        // foreach (string line in File.ReadLines(filePath))
        // {
        //     Console.WriteLine(line);
        // }
        Console.WriteLine(File.ReadLines(filePath).First());
        // _botClient = new TelegramBotClient("<token>");
        // _receiverOptions = new ReceiverOptions
        // {
        //     AllowedUpdates = new[]{
        //         UpdateType.Message,
        //     },
        // }
    }
}