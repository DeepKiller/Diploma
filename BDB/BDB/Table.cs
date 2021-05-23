using System;
using System.Collections;
using System.IO;
using System.Text.Json;
//TODO: Журнализация!
namespace BDB
{
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
            try
            {
                string json = JsonSerializer.Serialize(this, jsonSerializer);
                File.AppendAllText(Path, json);
            }
            catch
            {
                File.Create(Path);
            }
        }
        /// <summary>
        /// Загрузка данных из файла
        /// </summary>
        /// <param name="FilePath">Путь к файлу\\название.расширение</param>
        public void LoadTableData(string FilePath)
        {
            string ReadedJson = File.ReadAllText(FilePath);
            Table temp = (Table)JsonSerializer.Deserialize(ReadedJson, GetType());
            AuthorName = temp.AuthorName;
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
