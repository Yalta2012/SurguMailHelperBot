namespace SurguMailBot
{

    public class UserModel
    {

        public bool _status { get; }
        public Int64 _chatId { get; }

        public string? _email { get; }
        public string? _password { get; }
        public DateTime _last_check { get; }

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
}