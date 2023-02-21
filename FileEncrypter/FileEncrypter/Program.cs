using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Remoting.Messaging;

namespace FileEncrypter
{
    class Program
    {
        static byte[] password_bytes;
        private static readonly byte[] Salt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        

        static List<string[]> encrypted_files = new List<string[]>();
        static List<string> decrypted_file_paths = new List<string>();
        static List<byte[]> decrypted_file_contents = new List<byte[]>();
        static List<string> paths = new List<string>();
        static string enc_path = @"C:\Enc\";
        static string dec_path = @"C:\Users\User\Documents\Temp\";
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Console.WriteLine("Select mode (0 - encrypt, 1 - decrypt): ");
            int mode = Int32.Parse(Console.ReadLine());
            if (mode == 0)
            {
                Console.WriteLine("Enter folder path you need to encrypt: ");
                string path = Console.ReadLine();
                if (path[path.Length-1] != '\\')
                {
                    path += "\\";
                }
                Console.WriteLine("Enter your password: ");
                password_bytes = CreateKey(Console.ReadLine());
                ScanFolder(path);
                Console.WriteLine("___________");
                foreach (string p in paths)
                {
                    Console.WriteLine(p);
                    encrypted_files.Add(new string[] { p, EncryptFile(p) });
                }
                enc_path = path;
                WriteNewEncryptedFolder();
                Console.ReadLine();
            } else
            {
                Console.WriteLine("Enter folder path you need to decrypt: ");
                string path = Console.ReadLine();
                if (path[path.Length - 1] != '\\')
                {
                    path += "\\";
                }
                Console.WriteLine("Enter your password: ");
                password_bytes = CreateKey(Console.ReadLine());
                ScanFolder(path);
                Console.WriteLine("___________");
                foreach (string p in paths)
                {
                    Console.WriteLine(p);
                    decrypted_file_paths.Add(p);
                    decrypted_file_contents.Add(DecryptFile(p));
                }
                dec_path = path;
                WriteNewDecryptedFolder();
                Console.ReadLine();
            }
        }
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            while (true)
            {

            }
        }

        static void ScanFolder(string path)
        {
            DirectoryInfo d = new DirectoryInfo(path);

            FileInfo[] Files = d.GetFiles("*.*");
            string str = "";

            foreach (FileInfo file in Files)
            {
                paths.Add(file.FullName);
            }
            string[] directories = Directory.GetDirectories(path);
            foreach (string dir in directories)
            {
                ScanFolder(dir);
            }
        }

        static void WriteNewEncryptedFolder()
        {
            if (!Directory.Exists(enc_path))
            {
                Directory.Delete(enc_path, true);
            }
            Directory.CreateDirectory(enc_path);
            Console.WriteLine(encrypted_files.Count);
            foreach(string[] file in encrypted_files)
            {
                string dir_path = enc_path + GetDirectoryPath(file[0]);
                string file_path = enc_path + GetFilePath(file[0]);
                if (!Directory.Exists(dir_path))
                {
                    Directory.CreateDirectory(dir_path);
                }
                File.WriteAllText(file_path, file[1]);
            }
        }
        static void WriteNewDecryptedFolder()
        {
            if (!Directory.Exists(dec_path))
            {
                Directory.Delete(dec_path, true);
            }
            Directory.CreateDirectory(dec_path);
            Console.WriteLine(decrypted_file_paths.Count);
            for(int i = 0; i < decrypted_file_paths.Count; i++)
            {
                string dir_path = dec_path + GetDirectoryPath(decrypted_file_paths[i]);
                string file_path = dec_path + GetFilePath(decrypted_file_paths[i]);
                if (!Directory.Exists(dir_path))
                {
                    Directory.CreateDirectory(dir_path);
                }
                File.WriteAllBytes(file_path, decrypted_file_contents[i]);
            }
        }

        static string GetDirectoryPath(string path)
        {
            string[] splitted = path.Split('\\');
            string directory_path = "";
            for(int i = 2; i < splitted.Length-1; i++)
            {
                directory_path += splitted[i] + @"\";
            }
            return directory_path;
        }
        static string GetFilePath(string path)
        {
            string[] splitted = path.Split('\\');
            string file_path = "";
            for (int i = 2; i < splitted.Length; i++)
            {
                if(i == splitted.Length - 1)
                {
                    file_path += splitted[i];
                    break;
                }
                file_path += splitted[i] + @"\";
            }
            return file_path;
        }
        static string EncryptFile(string path)
        {
            return EncryptSymmetric(GetFileContent(path), password_bytes);
        }
        static byte[] DecryptFile(string path)
        {
            return DecryptSymmetric(Encoding.Default.GetString(GetFileContent(path)), password_bytes);
        }
        static byte[] GetFileContent(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            var bytes = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                reader.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }
            reader.Close();
            return bytes;
        }


        public static string ToBase64(byte[] input)
        {
            return Convert.ToBase64String(input);
        }
        public static byte[] FromBase64(string input)
        {
            return Convert.FromBase64String(input);
        }

        public static string EncryptSymmetric(byte[] data, byte[] key)
        {
            byte[] initializationVector = Encoding.ASCII.GetBytes("abcede0123456789");
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initializationVector;
                var symmetricEncryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream, symmetricEncryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream as Stream))
                        {
                            streamWriter.BaseStream.Write(data, 0, data.Length);
                        }
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }
        public static byte[] DecryptSymmetric(string cipherText, byte[] key)
        {
            byte[] initializationVector = Encoding.ASCII.GetBytes("abcede0123456789");
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = initializationVector;
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var memoryStream = new MemoryStream(buffer))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream as Stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream as Stream))
                        {
                            var bytes = default(byte[]);
                            using (var memstream = new MemoryStream())
                            {
                                streamReader.BaseStream.CopyTo(memstream);
                                bytes = memstream.ToArray();
                            }
                            return bytes;
                        }
                    }
                }
            }
        }
        public static byte[] CreateKey(string password, int keyBytes = 32)
        {
            const int Iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, Salt, Iterations);
            return keyGenerator.GetBytes(keyBytes);
        }
    }
}
