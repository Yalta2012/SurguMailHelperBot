using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using System.Net.Mail;

namespace SurguMailBot
{

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
}