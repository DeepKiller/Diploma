namespace BDB
{
    class Program
    {
        static void Main()
        {
            Table test = new Table("test.bdbt");
            string[] Cols = { "one", "two", "id" };
            string[] Data = { "1", "2"};
            test.SetColNames(Cols);
            test.AddRow(Data);
            test.SaveChanges();
            Table test1 = new Table("test1.bdbt");
            test1.LoadTableData("test.bdbt");
            test1.SaveChanges();
        }
    }
}
