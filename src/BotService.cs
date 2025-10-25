using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SurguMailBot
{


    public class BotService
    {
        private TelegramBotClient _botClient;
        private CancellationTokenSource _cts;

        private DataBaseService _dataBase;

        public BotService(string token, DataBaseService dataBase)
        {


            _cts = new CancellationTokenSource();
            _botClient = new TelegramBotClient(token, cancellationToken: _cts.Token);
            _dataBase = dataBase;
            _botClient.OnMessage += OnMessage;

        }
        public void Stop()
        {
            _cts.Cancel();
        }

        async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text == null) return;

            UserModel? user = null;
            try
            {
                user = _dataBase.GetUser(message.Chat.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (null == user)
            {
                try
                {

                    _dataBase.CreateIfNotExist(message.Chat.Id);
                    user = _dataBase.GetUser(message.Chat.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }

            Console.WriteLine("message: " + message.Text + " " + message.Chat.Id);
            if (null == user) Console.WriteLine("user = null");

            try
            {

                if (message.Text.StartsWith("/start"))
                {
                    await SendMessage(message.Chat.Id,
                                    """
                                Вас приветствует бот для тех кто не хочет настраивать уведомления на почте!
                                Что бы посмотреть список команд введите /help
                                """);
                }
                else if (message.Text.StartsWith("/help"))
                {
                    await SendMessage(message.Chat.Id,
                    """
            Список команд:
            /help — Вывести этот текст
            /setemail <адрес> — Указать адрес электронной почти
            /setpassword <пароль> — Указать пароль для электронной почты
            /enable — Включить бота
            /disable — Выключить бота
            /status — Посмотреть статус
            /clear — Стереть адрес и пароль
            """);
                }
                else if (message.Text.StartsWith("/status"))
                {
                    await SendMessage(message.Chat.Id,
                    $"""
            Пользователь: {(user._chatId)}
            Состояние: {(user._status ? "Активен" : "Неактивен")}
            Почта: {(user._email == null ? "Не установлена" : user._email)}
            Пароль: {(user._password == null ? "Не установлен" : "Установлен")}
            Последняя проверка: {(user._last_check)}
            """);
                }

                else if (message.Text.StartsWith("/enable"))
                {
                    if (user._email != null && user._password != null)
                    {
                        _dataBase.SetStatus(user._chatId, true);
                        await SendMessage(message.Chat.Id, "Бот успено включен");
                    }
                    else
                    {
                        await SendMessage(message.Chat.Id, "Установите email и пароль");
                    }
                }

                else if (message.Text.StartsWith("/disable"))
                {
                    _dataBase.SetStatus(user._chatId, false);
                    await SendMessage(message.Chat.Id, "Бот успено выключен");
                }

                else if (message.Text.StartsWith("/checkmail"))
                {

                }

                else if (message.Text.StartsWith("/clear"))
                {
                    _dataBase.SetEmail(user._chatId, null);
                    _dataBase.SetPassword(user._chatId, null);
                    _dataBase.SetStatus(user._chatId, false);
                    await SendMessage(message.Chat.Id, "email и пароль сброшены, бот выключен");

                }
                else if (message.Text.StartsWith("/setemail"))
                {
                    var messageTockens = message.Text.Split();
                    if (messageTockens.Length < 2)
                    {
                        await SendMessage(message.Chat.Id, "Формат команды: /setemail <адрес>");
                        return;
                    }

                    if (MailService.IsValidEmailUsingMailAddress(messageTockens[1]))
                    {
                        _dataBase.SetEmail(user._chatId, messageTockens[1]);
                        await SendMessage(message.Chat.Id, "Почта успешно установлена");
                        return;
                    }

                    await SendMessage(message.Chat.Id, "Неверный формат почты");


                }
                else if (message.Text.StartsWith("/setpassword"))
                {

                    var messageTockens = message.Text.Split();
                    if (messageTockens.Length < 2)
                    {
                        await SendMessage(message.Chat.Id, "Формат команды: /setpassword <адрес>");
                        return;
                    }

                    if (messageTockens[1] == "password")
                    {
                        await SendMessage(message.Chat.Id, "Ты серьезно?");
                    }


                    _dataBase.SetPassword(user._chatId, SimpleStringEncryptor.Encrypt(messageTockens[1], ".key"));
                    await SendMessage(message.Chat.Id, "Пароль успешно установлен");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка обработки сообщения:");
                Console.WriteLine(e);
            }


        }

        async public Task SendMessage(System.Int64 chatId, string text)
        {
            await _botClient.SendMessage(chatId, text);
        }

    }
}