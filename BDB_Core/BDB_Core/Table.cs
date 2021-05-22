using System.Collections.Generic;

namespace BDB_Core
{
    public class Table
    {
        public string Name { get; set; }
        public List<Relation> Relations { get; set; }
        public List<Record> Records { get; set; }
        public Table(string name)
        {
            Name = name;
        }
        public Table()
        {

        }
        public Table(string name, List<Record> records)
        {
            Name = name;
            Records = records;
        }
        
        // Получить поля.
        public string[] GetFields()
        {
            string[] fieldsName = new string[Records.Count];

            for(int i = 0; i < Records.Count; i++)
            { 
                fieldsName[i] = Records[0].Field.Name;
            }

            return fieldsName;
        }

        // Получить записи.
        public string[] GetData()
        {
            string[] datas = new string[Records.Count];

            for (int i = 0; i < Records.Count; i++)
            {
                datas[i] = Records[0].Data;
            }

            return datas;
        }

        public string[,] SetFullArray()
        {

            /* Генерировать табличку, типа:
             
             |id|name |secondName | <-- имена полей
             | 0|vlad |kostenok   | <-- данные
             | 0|kolya|virahovskiy| <-- данные

            */

            return new string[3,3];
        }

        public void SaveTable()
        {

        }
        public void DeleteTable()
        {

        }

        public void AddRelation(ref Table tableToConnect)
        {
            var relation = new Relation(tableToConnect);

            relation.ConnectedTable = this;

            Relations.Add(relation);
        }
        public void DeleteRelation(ref Table tableToDelete)
        {
            var relation = new Relation(tableToDelete);

            Relations.Remove(Relations.Find(x => x.ConnectedTable == this));
        }
    }
}