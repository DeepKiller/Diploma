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
            test1.SetColNames(Cols);
            test1.AddRow(Data);
            test.LoadTableData("test.bdbt");
            test1.SaveChanges();
            DataBase.MakeBaseFile("test.bdb");
            DataBase.CompressByGlobalPath("Pass");
            //DataBase.DecompressByGlobalPath("Pass");
        }
    }
}
