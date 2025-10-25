using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SurguMailBot
{

    public class SimpleStringEncryptor
    {

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
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            using var memoryStream = new MemoryStream();

            memoryStream.Write(aes.IV, 0, aes.IV.Length);

            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }


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

}