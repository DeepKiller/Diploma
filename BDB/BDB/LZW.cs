using System;
using System.Collections;
using System.IO;
using System.Text;
namespace BDB
{
    class LZW
    {
        /// <summary>
        /// Разжатие данных
        /// </summary>
        static public void DecompresByPath(string Path)
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
                    c = ((char)93).ToString();
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
        /// Сжатие данных
        /// </summary>
        static public void CompresByPath(string Path)
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
    }
}
