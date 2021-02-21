using System.IO;
using System;

namespace BDB
{
    class Table
    {
        public string name;// название таблицы
        private Row[] rows; // перечень рядов, 0 - заголовок
        private string[] Ids; // Ключи записей и их позиции
        private string path; // расположение файла таблицы 
        
        public Table()
        {
            GenerateNewFile();
            MakeForConstruct();
        }
        public Table(string FilePath)
        {
            if (FilePath == "" || FilePath == null)
            {
                GenerateNewFile();
            }
            else
            {
                string[] tPath = FilePath.Split('\\');
                path = FilePath;
                name = tPath[tPath.Length-1].Split('.')[0];
            }
            MakeForConstruct();
        }
        private void GenerateNewFile()
        {
            string[] Files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Table*.bdbt");
            name = "Table" + (Files.Length + 1);
            path = name + ".bdbt";
        }
        private void MakeForConstruct()
        {
            File.Create(path);
            rows = new Row[0];
            Ids = new string[0];
        }
        public void SetColNames(string[] ColNames) 
        {
            if(rows.Length == 0)
                Array.Resize(ref rows, 1);
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
            rows[0] = new Row(CL);
        }
        public void AddRow(string[] RowData)
        {
            Array.Resize(ref rows, rows.Length + 1);
            rows[rows.Length] = new Row(RowData);
        }
    }
    class Row
    {
        public string[] cols; //содержимое строки, столбцы
        public Row(string[] data)
        {
            int pos = 0;
            foreach(string d in data)
            {
                Array.Resize(ref cols, pos + 1);
                cols[pos] = d;
                pos++;
            }
        }
    }
}
