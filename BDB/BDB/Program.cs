

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
        }
    }
}
