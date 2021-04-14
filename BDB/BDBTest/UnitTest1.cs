using Microsoft.VisualStudio.TestTools.UnitTesting;
using BDB;

namespace BDBTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Table test = new Table("test.bdbt");
            string[] Cols = { "one", "two", "id" };
            string[] Data = { "1", "2" };
            test.SetColNames(Cols);
            test.AddRow(Data);
            test.SaveChanges();
            Table test1 = new Table("test1.bdbt");
            test1.SetColNames(Cols);
            test1.AddRow(Data);
            test.LoadTableData("test.bdbt");
            test1.SaveChanges();
            DataBase.MakeBaseFile("test.bdb");
            DataBase.CompressByGlobalPath();
            DataBase.CryptData("Pass");
            DataBase.DeCryptData("Pass");
            DataBase.DecompressByGlobalPath();
        }
    }
}
