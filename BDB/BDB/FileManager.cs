using System.IO;
using System;

namespace BDB
{
    class Table
    {
        public string Name;// название таблицы
        private Row[] Rows; // перечень рядов, 0 - заголовок
        private string[] Ids; // Ключи записей и их позиции
        private string Path; // расположение файла таблицы 
        
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
                Path = FilePath;
                Name = tPath[tPath.Length-1].Split('.')[0];
            }
            MakeForConstruct();
        }
        private void GenerateNewFile()
        {
            string[] Files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Table*.bdbt");
            Name = "Table" + (Files.Length + 1);
            Path = Name + ".bdbt";
        }
        private void MakeForConstruct()
        {
            File.Create(Path).Close();
            Rows = new Row[0];
            Ids = new string[0];
        }
        public void SetColNames(string[] ColNames) 
        {
            if(Rows.Length == 0)
                Array.Resize(ref Rows, 1);
            string[] CL = { "id|"};
            int pos = 1;
            for (int i = 1; i < ColNames.Length; i++)
            {
                if (ColNames[i - pos].ToLower() != "id")
                {
                    Array.Resize(ref CL, CL.Length+1);
                    if (i != ColNames.Length - 1)
                        CL[i] = ColNames[i - pos] + "|";
                    else
                        CL[i] = ColNames[i - pos] + ";";
                }
                else
                {
                    pos--;
                }
            }
            Rows[0] = new Row(CL);
        }
        public void AddRow(string[] RowData)
        {
            if (RowData.Length > Rows[0].cols.Length - 1)
                throw new Exception("too many arguments");
            string[] arrtowrite = new string[RowData.Length + 1];
            arrtowrite[0] = Rows.Length.ToString();
            RowData.CopyTo(arrtowrite, 1);
            for(int i =0; i < arrtowrite.Length - 1; i++)
            {
                arrtowrite[i] += "|";
            }
            arrtowrite[arrtowrite.Length-1] += ";";
            Array.Resize(ref Rows, Rows.Length + 1);
            Rows[Rows.Length-1] = new Row(arrtowrite);
            Array.Resize(ref Ids, Ids.Length + 1);
            Ids[Ids.Length - 1] = arrtowrite[0]; 
        }
        public void SaveChanges()
        {
            File.Delete(Path);
            StreamWriter sWriter = File.AppendText(Path);
            for(int i=0; i < Rows.Length; i++)
            {
                string write = "";
                foreach (string data in Rows[i].cols)
                    write += data;
                sWriter.WriteLine(write);
                if (i > 0)
                    Ids[i - 1] += i;
            }
            sWriter.Close();
            File.AppendAllLines(Path,Ids);
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
