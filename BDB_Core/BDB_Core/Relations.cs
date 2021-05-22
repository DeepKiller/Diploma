namespace BDB_Core
{
    public class Relation
    {
        public Table ConnectedTable { get; set; }
        public Relation()
        {
        }

        public Relation(Table tableToConnect)
        {
            ConnectedTable = tableToConnect;
        }
    }
}