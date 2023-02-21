using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileEncrypter
{
    class Program
    {
        static List<string[]> encrypted_files = new List<string[]>();
        static List<string> decrypted_file_paths = new List<string>();
        static List<byte[]> decrypted_file_contents = new List<byte[]>();
        static List<string> paths = new List<string>();
        static void Main(string[] args)
        {
            Console.WriteLine("Select mode (0 - encrypt, 1 - decrypt): ");
            int mode = Int32.Parse(Console.ReadLine());
            if (mode == 0)
            {
                Console.WriteLine("Enter folder path you need to encrypt: ");
                string path = Console.ReadLine();
                ScanFolder(path);
                Console.WriteLine("___________");
                foreach (string p in paths)
                {
                    Console.WriteLine(p);
                    encrypted_files.Add(new string[] { p, EncryptFile(p) });
                }
                Console.ReadLine();
            } else
            {
                Console.WriteLine("Enter folder path you need to decrypt: ");
                string path = Console.ReadLine();
                ScanFolder(path);
                Console.WriteLine("___________");
                foreach (string p in paths)
                {
                    Console.WriteLine(p);
                    decrypted_file_paths.Add(p);
                    decrypted_file_contents.Add(DecryptFile(p));
                }
                Console.ReadLine();
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

        static void OverwriteFolder()
        {

        }
        static string EncryptFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            return ToBase64(GetFileContent(path));
        }
        static byte[] DecryptFile(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fs);
            return FromBase64(reader.ReadToEnd());
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
    }
}
