using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace BDB
{
    /// <summary>
    /// Класс для управления базой данных
    /// </summary>
    static public class DataBase
    {
        /// <summary>
        /// Класс исключения, при несовпадающем пароле
        /// </summary>
        class IncorrectPasswordException : Exception
        {
            public IncorrectPasswordException() : base("Incorrect password")
            {

            }
        }
        /// <summary>
        /// Поле для хранения пути к файлу
        /// </summary>
        static public string Path = "";
        /// <summary>
        /// Собирает все таблицы в папке назначения в один файл!
        /// </summary>
        /// <param name="path">путь\\названиефайла.расширение</param>
        static public void MakeBaseFile(string path)
        {
            ArrayList tables = new ArrayList();
            string[] split = path.Split('\\');
            string filepath = path;
            filepath = split.Length != 1 ? filepath.Remove(filepath.Length - split[split.Length - 1].Length, split[split.Length - 1].Length) : Directory.GetCurrentDirectory();
            foreach (string file in Directory.GetFiles(filepath, "*.bdbt"))
            {
                Table tab = new Table();
                tab.LoadTableData(file);
                tables.Add(tab);
            }
            if (File.Exists(path))
                File.Delete(path);
            JsonSerializerOptions jsonSerializer = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            try
            {
                string json = JsonSerializer.Serialize(tables, jsonSerializer);
                File.AppendAllText(path, json);
                foreach (string file in Directory.GetFiles(filepath, "*.bdbt"))
                    File.Delete(file);
            }
            catch
            {
                File.AppendAllText(path, "[]");
            }
            Path = path;
        }
        /// <summary>
        /// Разбивает базу данных на таблицы
        /// </summary>
        static public void DisassembleBaseFile()
        {
            string[] split = Path.Split('\\');
            string filepath = Path;
            filepath = split.Length != 1 ? filepath.Remove(filepath.Length - split[split.Length - 1].Length, split[split.Length - 1].Length) : Directory.GetCurrentDirectory();
            string json = File.ReadAllText(Path);
            Table[] tables = new Table[0];
            try
            {
                tables = (Table[])JsonSerializer.Deserialize(json, tables.GetType());
                foreach (string file in Directory.GetFiles(filepath, "*.bdbt"))
                    File.Delete(file);
                foreach (Table table in tables)
                    table.SaveChanges();
                File.Delete(Path);
            }
            catch { }
        }
        /// <summary>
        /// Шифрование данных, использует path
        /// </summary>
        /// <param name="Password">Пароль для шифрования</param>
        static public void CryptData(string Password)
        {
            BinaryWriter writer = new BinaryWriter(File.Open(Path, FileMode.Open));
            long FileSize = writer.BaseStream.Length;
            writer.BaseStream.Position = writer.BaseStream.Length;
            while (writer.BaseStream.Length % 16 != 0)
                writer.Write((byte)0);
            writer.Close();
            if (File.Exists(Path + ".r"))
                File.Delete(Path + ".r");
            File.Move(Path, Path + ".r");
            BinaryReader reader = new BinaryReader(File.Open(Path + ".r", FileMode.Open));
            writer = new BinaryWriter(File.Open(Path, FileMode.OpenOrCreate));
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                ulong A = reader.ReadUInt64();
                ulong B = reader.ReadUInt64();
                RC5.CryptData(Password, ref A, ref B);
                writer.Write(A);
                writer.Write(B);
            }
            byte[] passwordHash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(Password));
            byte[] hashToWrite = new byte[4];
            for (int i = 0; i < hashToWrite.Length; i++)
                hashToWrite[i] = passwordHash[i * 4];
            writer.Write(hashToWrite);
            writer.Write(FileSize);
            reader.Close();
            writer.Close();
            File.Delete(Path + ".r");
        }
        /// <summary>
        /// Расшифрование данных, использует path
        /// </summary>
        /// <param name="Password">Пароль для расшифрования</param>
        /// <exception cref="IncorrectPasswordException">Введён не правильный пароль</exception>
        static public void DeCryptData(string Password)
        {
            if (File.Exists(Path + ".r"))
                File.Delete(Path + ".r");
            File.Move(Path, Path + ".r");
            BinaryReader reader = new BinaryReader(File.Open(Path + ".r", FileMode.Open));
            BinaryWriter writer = new BinaryWriter(File.Open(Path, FileMode.OpenOrCreate));
            reader.BaseStream.Position = reader.BaseStream.Length - 12;
            byte[] hashToCheck = reader.ReadBytes(4);
            byte[] passwordHash = new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(Password));
            for (int i = 0; i < hashToCheck.Length; i++)
                if (hashToCheck[i] != passwordHash[i * 4])
                    throw new IncorrectPasswordException();
            long FileSize = reader.ReadInt64();
            reader.BaseStream.Position = 0;
            while (reader.BaseStream.Position != reader.BaseStream.Length - 12)
            {
                ulong A = reader.ReadUInt64();
                ulong B = reader.ReadUInt64();
                RC5.DeCryptData(Password, ref A, ref B);
                writer.Write(A);
                writer.Write(B);
            }
            reader.Close();
            writer.Close();
            File.Delete(Path + ".r");
            File.Move(Path, Path + ".r");
            reader = new BinaryReader(File.Open(Path + ".r", FileMode.Open));
            writer = new BinaryWriter(File.Open(Path, FileMode.OpenOrCreate));
            while (reader.BaseStream.Position != FileSize)
                writer.Write(reader.ReadByte());
            reader.Close();
            writer.Close();
            File.Delete(Path + ".r");
        }
        /// <summary>
        /// Сжатие по пути файла базы
        /// </summary>
        static public void CompresByGlobalPath()
        {
            LZW.CompresByPath(Path);
        }
        /// <summary>
        /// Разжатие по пути файла базы
        /// </summary>
        static public void DecompresByGlobalPath()
        {
            LZW.DecompresByPath(Path);
        }
        /// <summary>
        /// Метод возвращает массив таблиц из директории в которой находился файл базы данных
        /// </summary>
        /// <returns>Массив таблиц</returns>
        static public ArrayList GetTables()
        {
            ArrayList tables = new ArrayList();
            string[] split = Path.Split('\\');
            string filepath = Path;
            filepath = split.Length != 1 ? filepath.Remove(filepath.Length - split[split.Length - 1].Length, split[split.Length - 1].Length) : Directory.GetCurrentDirectory();
            foreach (string file in Directory.GetFiles(filepath, "*.bdbt"))
            {
                Table tab = new Table();
                tab.LoadTableData(file);
                tables.Add(tab);
            }
            return tables;
        }
    }
}
