using System.IO;
using System;
using System.Text.Json;
using System.Collections;

namespace BDB
{
    class DataBase // Управление общим файлом, сборка, шифрование, сжатие, и обратно
    {

    }
    class Table //управление таблицами
    {
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
                Name = tPath[tPath.Length-1].Split('.')[0];
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
            string[] CL = { "id"};
            int pos = 1;
            for (int i = 1; i < ColNames.Length; i++)
            {
                if (ColNames[i - pos].ToLower() != "id")
                {
                    Array.Resize(ref CL, CL.Length+1);
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
            arrtowrite[0] = (Rows.Count-1).ToString();
            RowData.CopyTo(arrtowrite, 1);
            Rows.Add(new Row(arrtowrite));
            Ids.Add(arrtowrite[0]);
        }
        public void SaveChanges() //запись в файл
        {
            File.Delete(Path);
            JsonSerializerOptions jsonSerializer = new JsonSerializerOptions();
            jsonSerializer.WriteIndented = true;
            string json = JsonSerializer.Serialize(this,jsonSerializer);
            File.AppendAllText(Path, json);
        }
        public void LoadTableData(string FilePath) //загрузка из файла
        {
            string ReadedJson = File.ReadAllText(FilePath);
            Table temp = (Table)JsonSerializer.Deserialize(ReadedJson,GetType());
            for (int i = 0; i < temp.Rows.Count; i++)
            {
                Row r = (Row)JsonSerializer.Deserialize(((JsonElement)temp.Rows[i]).ToString(), new Row().GetType());
                for(int j = 0; j < r.cols.Count; j++)
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
            File.Delete(emptyFile[emptyFile.Length-1]);
        }
        public void DeleteTable()//Удаление Файла таблицы
        {
            File.Delete(Path);
        }
    }
    class Row //подкласс для рядов
    {
        public ArrayList cols { get; set; } //содержимое строки, столбцы
        public Row()// НЕ ТРОГАТЬ !!!!!!!!!!!!
        {

        }
        public Row(string[] data) //запись данных в объект
        {
            cols = new ArrayList();
            for(int i = 0; i < data.Length; i++)
            {
                cols.Add(data[i]);
            }
        }
    }
}
