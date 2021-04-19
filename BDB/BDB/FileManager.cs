using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
//TODO: Вынести сжатие отдельно, переписать все работы с битами!
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
        static string Path = "";
        /// <summary>
        /// Собирает все таблицы в папке назначения в один файл
        /// </summary>
        /// <param name="path">путь\\названиефайла.расширение</param>
        static public void MakeBaseFile(string path)
        {
            ArrayList tables = new ArrayList();
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bdbt"))
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
            string json = JsonSerializer.Serialize(tables, jsonSerializer);
            File.AppendAllText(path, json);
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bdbt"))
                File.Delete(file);
            Path = path;
        }
        /// <summary>
        /// Разбивает базу данных на таблицы
        /// </summary>
        static public void DisassembleBaseFile()
        {
            string json = File.ReadAllText(Path);
            Table[] tables = new Table[0];
            tables = (Table[])JsonSerializer.Deserialize(json, tables.GetType());
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bdbt"))
                File.Delete(file);
            foreach (Table table in tables)
                table.SaveChanges();
            File.Delete(Path);
        }
        /// <summary>
        /// Сжатие данных
        /// </summary>
        static public void CompressByGlobalPath()
        {
            ArrayList dict = new ArrayList();
            FillDict(ref dict); //заполнение словаря всеми доступными одиночными символами
            StringBuilder seq = new StringBuilder();
            if (File.Exists(Path + ".r"))
                File.Delete(Path + ".r");
            File.Move(Path, Path + ".r");
            StreamReader reader = new StreamReader(Path + ".r");
            BinaryWriter writer = new BinaryWriter(File.Open(Path, FileMode.OpenOrCreate));
            string output = "";
            char c;
            while (reader.Peek() != -1)
            {
                int bytesize = 0;
                for (int i = 8; i < 16; i++)
                    if (dict.Count < Math.Pow(2, i))
                    {
                        bytesize = i; break;
                    }
                c = (char)reader.Read();
                if (dict.Contains(seq.ToString() + c))
                {
                    seq.Append(c);
                }
                else
                {
                    string temp = Convert.ToString(dict.IndexOf(seq.ToString()), 2);
                    while (temp.Length < bytesize)
                    {
                        temp = "0" + temp;
                    }
                    output += temp;
                    while (output.Length >= 8)
                    {
                        byte id = Convert.ToByte(output.Substring(0, 8), 2);
                        output = output.Remove(0, 8);
                        writer.Write(id);
                    }
                    dict.Add(seq.ToString() + c);
                    seq.Clear().Append(c);
                }
            }
            while (output.Length < 8)
                output += "0";
            output += Convert.ToString(dict.IndexOf(seq.ToString()), 2);
            while (output.Length >= 8)
            {
                byte id = Convert.ToByte(output.Substring(0, 8), 2);
                output = output.Remove(0, 8);
                writer.Write(id);
            }
            while (output.Length < 8)
                output += "0";
            writer.Write(Convert.ToByte(output, 2));
            writer.Close();
            reader.Close();
            File.Delete(Path + ".r");
        }
        /// <summary>
        /// Заполнение словаря необходимыми для сжатия символами
        /// </summary>
        /// <param name="dict">Словарь для заполнения</param>
        static private void FillDict(ref ArrayList dict)
        {
            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                char c = Convert.ToChar(i);
                if ((i >= 0 && i <= 126) || (i >= 160 && i <= 191) || (i >= 247 && i <= 248) || (i >= 1040 && i <= 1103))
                {
                    dict.Add(c.ToString());
                }
            }
        }
        /// <summary>
        /// Разжатие данных
        /// </summary>
        static public void DecompressByGlobalPath()
        {
            ArrayList dict = new ArrayList();
            FillDict(ref dict); //заполнение словаря всеми доступными одиночными символами
            StringBuilder seq = new StringBuilder();
            if (File.Exists(Path + ".r"))
                File.Delete(Path + ".r");
            File.Move(Path, Path + ".r");
            BinaryReader reader = new BinaryReader(File.Open(Path + ".r", FileMode.Open));
            StreamWriter writer = new StreamWriter(File.Open(Path, FileMode.OpenOrCreate));
            string output = "";
            string c;
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                c = ReadNextSection(ref dict, ref output, ref reader);
                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    int charCode = c[0];
                    charCode <<= 7;
                    charCode |= Convert.ToInt32(output.Substring(0,output.Length-2), 2);
                    c = ((char)charCode).ToString();
                }
                if (dict.Contains(seq.ToString() + c))
                {
                    if (reader.BaseStream.Position == 1)
                        writer.Write(c);
                    seq.Append(c);
                }
                else
                {
                    writer.Write(c);
                    dict.Add(seq.ToString() + c[0]);
                    seq.Clear().Append(c);
                }
            }
            writer.Close();
            reader.Close();
            File.Delete(Path + ".r");
        }
        /// <summary>
        /// Считывает следующую секцию данных
        /// </summary>
        /// <param name="dict">Словарь символов</param>
        /// <param name="output">Строка для выходных данных</param>
        /// <param name="reader">Поток данных для чтения</param>
        /// <returns>Считанная секция ввиде символа</returns>
        static private string ReadNextSection(ref ArrayList dict, ref string output, ref BinaryReader reader)
        {
            int bytesize = 0;
            for (int i = 8; i < 16; i++)
                if (dict.Count < Math.Pow(2, i) - 1)
                {
                    bytesize = i; break;
                }
            while (output.Length < bytesize)
            {
                string readed = Convert.ToString(reader.ReadByte(), 2);
                while (readed.Length < 8)
                    readed = "0" + readed;
                output += readed;
            }
            string temp = output.Substring(0, bytesize);
            output = output.Remove(0, bytesize);
            string c = (string)dict[Convert.ToInt32(temp, 2)];
            return c;
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
            while (reader.BaseStream.Position !=FileSize)
                writer.Write(reader.ReadByte());
            reader.Close();
            writer.Close();
            File.Delete(Path + ".r");
        }
    }
    /// <summary>
    /// Класс для управления таблицами
    /// </summary>
    public class Table
    {
        class TooManyArgumentsException : Exception
        {
            public TooManyArgumentsException() : base("Too many arguments!")
            {

            }
        }
        /// <summary>
        /// Подкласс для рядов в таблице
        /// </summary>
        public class Row
        {
            /// <summary>
            /// Содержимое строки, столбцы
            /// </summary>
            public ArrayList Cols { get; set; }
            /// <summary>
            /// Пустой конструктор необходим для сериализации
            /// </summary>
            public Row()
            {

            }
            /// <summary>
            /// Конструктор для инициализации объекта
            /// </summary>
            /// <param name="data">Данные для внесения в объект</param>
            public Row(string[] data)
            {
                Cols = new ArrayList();
                for (int i = 0; i < data.Length; i++)
                {
                    Cols.Add(data[i]);
                }
            }
            /// <summary>
            /// Заменяет значения ряда, на новые
            /// </summary>
            /// <param name="data">Массив значений для замены</param>
            public void ChangeRow(string[] data)
            {
                ArrayList arrayList = new ArrayList
                {
                    Cols[0]
                };
                Cols.Clear();
                for (int i = 0; i < data.Length; i++)
                {
                    arrayList.Add(data[i]);
                }
                Cols = arrayList;
            }
        }
        /// <summary>
        /// Класс организации связей
        /// </summary>
        class Relation
        {
            /// <summary>
            /// Объект свзяанной таблицы
            /// </summary>
            public Table ConnectedTable { get; set; }
            public Relation() { }
            public Relation(Table tableToConnect)
            {
                ConnectedTable = tableToConnect;
            }
        }
        /// <summary>
        /// Хранит имя пользователя создавшего таблицу, по-умолчанию имя пользователя Windows. Может быть изменено.
        /// </summary>
        public string AuthorName { get; set; }
        /// <summary>
        /// Свойство для хранения связей
        /// </summary>
        public ArrayList Relations { get; set; }
        /// <summary>
        /// Метод добавленяи свзи с таблицей
        /// </summary>
        /// <param name="tableToAdd">Таблица для связывания</param>
        public void AddRelation(ref Table tableToAdd)
        {
            Relations.Add(new Relation(tableToAdd));
            tableToAdd.Relations.Add(this);
        }
        /// <summary>
        /// Метод для удаления связи
        /// </summary>
        /// <param name="tableToDelete">Таблица связь с которой нужно удалить</param>
        public void DeleteRelation(ref Table tableToDelete)
        {
            Relations.Remove(tableToDelete);
            tableToDelete.Relations.Remove(this);
        }
        /// <summary>
        /// Удаление ряда
        /// </summary>
        /// <param name="id">ID ряда для удаления</param>
        public void DeleteRow(int id)
        {
            for (int i = 0; i < Rows.Count; i++)
                if (((Row)Rows[i]).Cols[0].ToString() == id.ToString())
                {
                    Ids.Remove(((Row)Rows[i]).Cols[0]);
                    Rows.RemoveAt(i);
                }
        }
        /// <summary>
        /// Свойство названия таблицы
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// Свойство для хранения рядов, 0 - заголовок
        /// </summary>
        public ArrayList Rows { set; get; }
        /// <summary>
        /// Свойство для хранения ключей записи и их позиции
        /// </summary>
        public ArrayList Ids { set; get; }
        /// <summary>
        /// Свойство для хранения расположения файла, относительный путь, содержит название таблицы и расширение
        /// </summary>
        public string Path { set; get; }
        /// <summary>
        /// Конструктор для создания стандартного файла
        /// </summary>
        public Table()
        {
            GenerateNewFile();
            MakeForConstruct();
        }
        /// <summary>
        /// Конструктор для создания файла в конкретном месте
        /// </summary>
        /// <param name="FilePath">Путь к файлу\\название.расширение</param>
        public Table(string FilePath)
        {
            if (FilePath == "" || FilePath == null)
            {
                GenerateNewFile();
            }
            else
            {
                string[] tPath = FilePath.Split('\\');
                Path = FilePath;
                Name = tPath[tPath.Length - 1].Split('.')[0];
            }
            MakeForConstruct();
        }
        /// <summary>
        /// Функция генерации стандартного файла
        /// </summary>
        private void GenerateNewFile()
        {
            string[] Files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Table*.bdbt");
            Name = "Table" + (Files.Length + 1);
            Path = Name + ".bdbt";
        }
        /// <summary>
        /// Создание файла, и свойств класса
        /// </summary>
        private void MakeForConstruct()
        {
            File.Create(Path).Close();
            Rows = new ArrayList();
            Ids = new ArrayList();
            Relations = new ArrayList();
            AuthorName = Environment.UserName;
        }
        /// <summary>
        /// Устанавливает названия колонок, id всегда первая, при наличии id в списке пропускает
        /// </summary>
        /// <param name="ColNames">Массив названий для колонок</param>
        public void SetColNames(string[] ColNames)
        {
            string[] CL = { "id" };
            int pos = 1;
            for (int i = 1; i < ColNames.Length; i++)
            {
                if (ColNames[i - pos].ToLower() != "id")
                {
                    Array.Resize(ref CL, CL.Length + 1);
                    CL[i] = ColNames[i - pos];
                }
                else
                {
                    pos--;
                }
            }
            Rows.Add(new Row(CL));
        }
        /// <summary>
        /// Добавление нового ряда
        /// </summary>
        /// <param name="RowData">Поля по порядку, id устанавливается сам</param>
        public void AddRow(string[] RowData)
        {
            Row row = (Row)Rows[0];
            if (RowData.Length > row.Cols.Count - 1)
                throw new TooManyArgumentsException();
            string[] arrtowrite = new string[RowData.Length + 1];
            arrtowrite[0] = (Rows.Count - 1).ToString();
            RowData.CopyTo(arrtowrite, 1);
            Rows.Add(new Row(arrtowrite));
            Ids.Add(arrtowrite[0]);
        }
        /// <summary>
        /// Сохранение данных в файле
        /// </summary>
        public void SaveChanges()
        {
            File.Delete(Path);
            JsonSerializerOptions jsonSerializer = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(this, jsonSerializer);
            File.AppendAllText(Path, json);
        }
        /// <summary>
        /// Загрузка данных из файла
        /// </summary>
        /// <param name="FilePath">Путь к файлу\\название.расширение</param>
        public void LoadTableData(string FilePath)
        {
            string ReadedJson = File.ReadAllText(FilePath);
              Table temp = (Table)JsonSerializer.Deserialize(ReadedJson, GetType());
            Name = temp.Name;
            Path = temp.Path;
            for (int i = 0; i < temp.Rows.Count; i++)
            {
                Row r = (Row)JsonSerializer.Deserialize(((JsonElement)temp.Rows[i]).ToString(), new Row().GetType());
                for (int j = 0; j < r.Cols.Count; j++)
                {
                    r.Cols[j] = r.Cols[j].ToString();
                }
                Rows.Add(r);
            }
            for (int i = 0; i < temp.Ids.Count; i++)
            {
                Ids.Add(temp.Ids[i].ToString());
            }
            string[] emptyFile = Directory.GetFiles(Directory.GetCurrentDirectory(), "Table*.bdbt"); //багфикс, удаление файла после вызова сериалазером пустого конструктора
            File.Delete(emptyFile[emptyFile.Length - 1]);
        }
        /// <summary>
        /// Удаление файла текущей таблицы
        /// </summary>
        public void DeleteTable()
        {
            File.Delete(Path);
        }
        /// <summary>
        /// Получает ряд по его id
        /// </summary>
        /// <param name="id">id для поиска</param>
        /// <returns>возвращает объект Row, либо null</returns>
        public Row GetRowByID(int id)
        {
            foreach (Row row in Rows)
                if (row.Cols[0].ToString() == id.ToString())
                    return row;
            return null;
        }
    }

}
