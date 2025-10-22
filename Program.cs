using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Data.Sqlite;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

try
{
    var db = new DataBaseService("database.db");
    var bot = new BotService("8419148700:AAH_7mzd9E29MnKTA_BTR1UJ059CS9FHVUw", db);

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddHostedService<TimedService>(provider => new TimedService(bot, db));

    builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
    var host = builder.Build();

    await host.StartAsync();
    // bot.SendROFL("💅сколько люкана не корми, а все равно хуесос💅");
    Console.ReadLine();
    await host.StopAsync();
    //bot.Stop();
}
catch (Exception e)
{
    Console.WriteLine(e);
}

public class SimpleStringEncryptor
{
    /// <summary>
    /// Шифрует строку используя ключ из файла
    /// </summary>
    public static string Encrypt(string plainText, string keyFilePath)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("Текст не может быть пустым");

        if (!File.Exists(keyFilePath))
            throw new FileNotFoundException("Файл с ключом не найден", keyFilePath);

        byte[] key = File.ReadAllBytes(keyFilePath);

        if (key.Length != 32)
            throw new ArgumentException("Ключ должен быть 32 байта (256 бит) для AES-256");

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV(); // Генерируем случайный IV для каждого шифрования
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();

        // Записываем IV в начало потока
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    /// <summary>
    /// Дешифрует строку используя ключ из файла
    /// </summary>
    public static string Decrypt(string encryptedText, string keyFilePath)
    {
        if (string.IsNullOrEmpty(encryptedText))
            throw new ArgumentException("Зашифрованный текст не может быть пустым");

        if (!File.Exists(keyFilePath))
            throw new FileNotFoundException("Файл с ключом не найден", keyFilePath);

        byte[] key = File.ReadAllBytes(keyFilePath);

        if (key.Length != 32)
            throw new ArgumentException("Ключ должен быть 32 байта (256 бит) для AES-256");

        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Извлекаем IV из начала данных (первые 16 байт)
        byte[] iv = new byte[16];
        byte[] cipherBytes = new byte[encryptedBytes.Length - 16];
        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var memoryStream = new MemoryStream(cipherBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);

        return streamReader.ReadToEnd();
    }

    public static void GenerateKey(string keyFilePath)
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        File.WriteAllBytes(keyFilePath, aes.Key);
        Console.WriteLine($"Ключ сохранен в: {keyFilePath}");
    }
}
public class UserModel
{
    // private enum Status
    // {
    //     on,
    //     off,
    //     error,
    // }

    // Status _status;
    public bool _status { get; }
    public Int64 _chatId { get; }

    public string? _email { get; }
    public string? _password { get; }
    public DateTime _last_check { get; }
    // Int64 userId;

    public UserModel(Int64 chat_id)
    {
        _chatId = chat_id;
        _email = null;
        _password = null;
        _status = false;
    }

    public UserModel(Int64 chat_id, string email, string password, bool status, DateTime last_check)
    {
        _chatId = chat_id;
        _email = email;
        _password = password;
        _status = status;
        _last_check = last_check;
    }
}


public class DataBaseService
{
    string _db_name;
    public DataBaseService(string db_name)
    {
        _db_name = "../../../" + db_name;
        try
        {
            using (var connection = new SqliteConnection("Data Source=" + _db_name))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand(
                "CREATE TABLE IF NOT EXISTS Users (" +
                "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, " +
                "chat_id INTEGER NOT NULL UNIQUE, " +
                "email TEXT, " +
                "password TEXT, " +
                "status INEGER NOT NULL, " +
                "last_check DATETIME DEFAULT '1970-01-01 00:00:00'" +
                ")",
                connection);

                command.ExecuteNonQuery();

            }
        }
        catch
        {
            throw;
        }
    }

    public void CreateIfNotExist(Int64 chat_id)
    {
        try
        {
            using (var connection = new SqliteConnection("Data Source=" + _db_name))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand($"INSERT OR IGNORE INTO Users (chat_id, email, password, status) VALUES ({chat_id}, null, null, 0)", connection);
                command.ExecuteNonQuery();

            }
        }
        catch
        {
            throw;
        }
    }

    public UserModel? GetUser(Int64 chat_id)
    {
        try
        {

            using (var connection = new SqliteConnection("Data Source=" + _db_name))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand($"SELECT chat_id, email, password, status, last_check FROM Users WHERE chat_id={chat_id}", connection);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new UserModel(
                        reader.GetInt64(0),
                        reader.IsDBNull(1) ? null : reader.GetString(1),
                        reader.IsDBNull(2) ? null : reader.GetString(2),
                        reader.GetBoolean(3),
                        reader.GetDateTime(4)
                    );
                }
                return null;
            }
        }
        catch
        {
            throw;
        }
    }

    public List<UserModel> GetActive()
    {
        List<UserModel> result = new List<UserModel>();
        try
        {
            using (var connection = new SqliteConnection("Data Source=" + _db_name))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand("SELECT chat_id, email, password, last_check FROM Users WHERE status=TRUE", connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Int64 chat_id = reader.GetInt64(0);
                    string email = reader.GetString(1);
                    string password = reader.GetString(2);
                    DateTime last_check = reader.GetDateTime(3);
                    result.Add(new UserModel(chat_id, email, password, true, last_check));

                }
            }
        }
        catch
        {
            throw;
        }
        return result;
    }

    private void SetField(Int64 chat_id, string field_name, object new_value)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_db_name}");
            connection.Open();

            using var command = new SqliteCommand($"UPDATE Users SET [{field_name}] = @value WHERE chat_id = @chatId", connection);

            command.Parameters.AddWithValue("@chatId", chat_id);

            if (new_value == null)
            {
                command.Parameters.AddWithValue("@value", DBNull.Value);
            }
            else
            {
                command.Parameters.AddWithValue("@value", new_value);
            }

            command.ExecuteNonQuery();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public void SetEmail(Int64 chat_id, string email)
    {
        SetField(chat_id, "email", email);
    }
    public void SetPassword(Int64 chat_id, string passord)
    {
        SetField(chat_id, "password", passord);

    }
    public void SetStatus(Int64 chat_id, bool status)
    {
        SetField(chat_id, "status", status);

    }

    public void SetLastTime(Int64 chat_id, DateTime time)
    {
        SetField(chat_id, "last_check", time);

    }

}
public static class MailService
{
    public static async Task<List<UserModel>> run(BotService bot, DataBaseService dataBaseService, List<UserModel> list)
    {
        var client = new ImapClient();
        var result = new List<UserModel>();
        try
        {
            await client.ConnectAsync("mail.surgu.ru", 993, true);
            foreach (var user in list)
            {
                try
                {
                    try
                    {
                        await client.AuthenticateAsync(user._email, SimpleStringEncryptor.Decrypt(user._password, ".key"));
                    }
                    catch
                    {
                        result.Add(new UserModel(user._chatId, user._email, user._password, false, user._last_check));
                        await bot.SendMessage(user._chatId, "Произошла ошибка аунтификации на почтовом сервисе. Проверьте правильность почтового адреса и пароля. Бот выключен.");
                        continue;
                    }


                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    var unreadCount = inbox.Search(SearchQuery.NotSeen).Count();

                    if (inbox.Count > 0)
                    {

                        var lastMessage = inbox.GetMessage(inbox.Count - 1);
                        DateTime lastEmailTime = lastMessage.Date.DateTime;
                        if (lastEmailTime > user._last_check)
                        {
                            await bot.SendMessage(user._chatId, $"Последнее письмо пришло: {lastEmailTime}\nНепрочитанных сообщений: {unreadCount}");
                            result.Add(new UserModel(user._chatId, user._email, user._password, true, lastEmailTime));
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            await client.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return result;
    }

    public static bool IsValidEmailUsingMailAddress(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}


public class TimedService : BackgroundService
{
    BotService _botService;
    DataBaseService _dataBaseService;
    public TimedService(BotService botService, DataBaseService dataBaseService)
    {
        _botService = botService;
        _dataBaseService = dataBaseService;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now} Start core service\n");
        await base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"{DateTime.Now} Start messaging");
            var list = _dataBaseService.GetActive();
            Console.WriteLine($"Active users: {list.Count()}");
            list = MailService.run(_botService, _dataBaseService, list).Result;
            Console.WriteLine($"Users to change: {list.Count()}");
            foreach (var user in list)
            {
                if (user._status == false) _dataBaseService.SetStatus(user._chatId, false);
                else _dataBaseService.SetLastTime(user._chatId, user._last_check);
            }
            Console.WriteLine($"{DateTime.Now} End messaging\n");

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stop Core");
        await base.StopAsync(cancellationToken);

    }
}

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
            }
        }

        Console.WriteLine("message: " + message.Text + " " + message.Chat.Id);
        if (null == user) Console.WriteLine("user = null");
        else
        {
            Console.WriteLine("email: " + (user._email == null ? "null" : user._email.ToString()));
            Console.WriteLine("password: " + (user._password == null ? "null" : user._password.ToString()));
        }

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
            /checkmail
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

