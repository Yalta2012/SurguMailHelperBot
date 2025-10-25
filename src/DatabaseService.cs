using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SurguMailBot
{

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
                        CultureInfo cultureInfo = CultureInfo.InvariantCulture;
                        DateTime last_check = DateTime.ParseExact( reader.GetString(3),"yyyy-MM-dd HH:mm:ss",null);
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

        public void MassUpdate(List<UserModel> list)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={_db_name}");
                connection.Open();

                using var transaction = connection.BeginTransaction();


                foreach (var user in list)
                {

                    using var command = connection.CreateCommand();
                    if (user._status == false)
                    {
                        command.CommandText = "UPDATE Users SET status = 0 WHERE chat_id = @chat_id";
                    }
                    else
                    {
                        command.CommandText = "UPDATE Users SET last_check = @last_check WHERE chat_id = @chat_id";
                        command.Parameters.AddWithValue("@last_check", user._last_check.ToString("yyyy-MM-dd HH:mm:ss"));

                    }
                    command.Parameters.AddWithValue("@chat_id", user._chatId);

                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }

                transaction.Commit();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
