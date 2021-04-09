using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.Json;

namespace BDB
{
    static class DataBase // Управление общим файлом, сборка, шифрование, сжатие, и обратно
    {
        static string Path = ""; //путь к файлу базы
        static public void MakeBaseFile(string path)//сборка таблиц в файл базы
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
            JsonSerializerOptions jsonSerializer = new JsonSerializerOptions();
            jsonSerializer.WriteIndented = true;
            string json = JsonSerializer.Serialize(tables, jsonSerializer);
            File.AppendAllText(path, json);
            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bdbt"))
                File.Delete(file);
            Path = path;
        }
        static public void CompressByGlobalPath(string Password)//сжатие и шифрование базы
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
            writer.Write(Convert.ToByte(output,2));
            writer.Close();
            reader.Close();
            File.Delete(Path + ".r");
        }
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
        static public void DecompressByGlobalPath(string Password)//востановление базы
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
                if (dict.Contains(seq.ToString() + c))
                {
                    if (reader.BaseStream.Position == 1)
                        writer.Write(c);
                    seq.Append(c);
                }
                else
                {
                    writer.Write(c);
                    dict.Add(seq.ToString()+c[0]);
                    seq.Clear().Append(c);
                }
            }
            writer.Close();
            reader.Close();
            File.Delete(Path + ".r");
        }
        static private string ReadNextSection(ref ArrayList dict, ref string output, ref BinaryReader reader)
        {
            int bytesize = 0;
            for (int i = 8; i < 16; i++)
                if (dict.Count < Math.Pow(2, i)-1)
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
    }
    //TODO: добавть связи таблиц
    class Table //управление таблицами
    {
        class Row //подкласс для рядов
        {
            public ArrayList cols { get; set; } //содержимое строки, столбцы
            public Row()// НЕ ТРОГАТЬ !!!!!!!!!!!!
            {

            }
            public Row(string[] data) //запись данных в объект
            {
                cols = new ArrayList();
                for (int i = 0; i < data.Length; i++)
                {
                    cols.Add(data[i]);
                }
            }
        }
        public string Name { set; get; }// название таблицы
        public ArrayList Rows { set; get; }  // перечень рядов, 0 - заголовок
        public ArrayList Ids { set; get; } // Ключи записей и их позиции
        public string Path { set; get; } // расположение файла таблицы 

        public Table() //Конструктор для создания стандартного файла
        {
            GenerateNewFile();
            MakeForConstruct();
        }
        public Table(string FilePath) //конструктор для создания файла в конкретном месте
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
        private void GenerateNewFile() //Функция, генерации параметров стандартного файла
        {
            string[] Files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Table*.bdbt");
            Name = "Table" + (Files.Length + 1);
            Path = Name + ".bdbt";
        }
        private void MakeForConstruct() //Создание файла, и свойств класса
        {
            File.Create(Path).Close();
            Rows = new ArrayList();
            Ids = new ArrayList();
        }
        public void SetColNames(string[] ColNames) //Установка названий колонок, id всегда первая
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
        public void AddRow(string[] RowData) //Добавление нового ряда
        {
            Row row = (Row)Rows[0];
            if (RowData.Length > row.cols.Count - 1)
                throw new Exception("too many arguments");
            string[] arrtowrite = new string[RowData.Length + 1];
            arrtowrite[0] = (Rows.Count - 1).ToString();
            RowData.CopyTo(arrtowrite, 1);
            Rows.Add(new Row(arrtowrite));
            Ids.Add(arrtowrite[0]);
        }
        public void SaveChanges() //запись в файл
        {
            File.Delete(Path);
            JsonSerializerOptions jsonSerializer = new JsonSerializerOptions();
            jsonSerializer.WriteIndented = true;
            string json = JsonSerializer.Serialize(this, jsonSerializer);
            File.AppendAllText(Path, json);
        }
        public void LoadTableData(string FilePath) //загрузка из файла
        {
            string ReadedJson = File.ReadAllText(FilePath);
            Table temp = (Table)JsonSerializer.Deserialize(ReadedJson, GetType());
            Name = temp.Name;
            Path = temp.Path;
            for (int i = 0; i < temp.Rows.Count; i++)
            {
                Row r = (Row)JsonSerializer.Deserialize(((JsonElement)temp.Rows[i]).ToString(), new Row().GetType());
                for (int j = 0; j < r.cols.Count; j++)
                {
                    r.cols[j] = r.cols[j].ToString();
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
        public void DeleteTable()//Удаление Файла таблицы
        {
            File.Delete(Path);
        }
    }

}
