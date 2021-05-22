using System;
using System.Collections.Generic;

namespace BDB_Core
{
    public static class Database
    {
        private static readonly List<Table> Tables;
        public static string Name { get; private set; }
        public static string Path { get; private set; }
        public static string AuthorName { get; private set; }


        public static void CreateDatabase(string path)
        {
            Path = path;
            Name = SetDatabaseName();

            // Реализовывай через доп.методы.
        }

        public static void SaveDatabase()
        {
            // 
        }

        /// <summary>
        /// Устанавливает имя базы данных из пути к ней.
        /// </summary>
        /// <returns>Возвращает имя без расширения файла</returns>
        private static string SetDatabaseName()
        {
            char symbol = '\\';
            int countNameLetter = 0;
            int fileExtension = 4;

            for (int i = Path.Length - 1; i >= 0; i--)
            {
                if (Path[i] != symbol)
                    countNameLetter++;
                else
                    break;
            }

            int countPathLetter = Path.Length - countNameLetter;

            return Path.Substring(countPathLetter, countNameLetter - fileExtension);
        }

        public static void CreateTable(string name)
        {
            var table = new Table(name);

            Tables.Add(table);
        }

        public static List<Table> GetTables()
        {
            return Tables;
        }

        public static void SaveHowDatabase(string path, string name)
        {
            Path = path;
            Name = name;
            // Сохранение.
        }

    }
}
