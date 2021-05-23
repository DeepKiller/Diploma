namespace BDB
{
    public class Program
    {
        static void Main()
        {
            Table test = new Table("test.bdbt");
            string[] Cols = { "one", "two", "id" };
            string[] Data = { "1", "2" };
            test.SetColNames(Cols);
            test.AddRow(Data);
            test.DeleteRow(0);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AddRow(Data);
            test.AuthorName = "Govnok0d";
            test.SaveChanges();
            Table test1 = new Table("test1.bdbt");
            test1.SetColNames(Cols);
            test1.AddRow(Data);
            //test.LoadTableData("test.bdbt");
            test1.SaveChanges();
            test.AddRelation(ref test1);
            test.DeleteRelation(ref test1);
            string[] Data1 = { "a", "b" };
            test.GetRowByID(0).ChangeRow(Data1);
            test.GetRowByID(6).ChangeRow(Data1);
            test.SaveChanges();
            test1.SaveChanges();
            DataBase.MakeBaseFile("test.bdb");
            DataBase.CompresByGlobalPath();
            DataBase.CryptData("Pass");
            DataBase.DeCryptData("Pass");
            DataBase.DecompresByGlobalPath();
            DataBase.DisassembleBaseFile();
        }
    }
}
