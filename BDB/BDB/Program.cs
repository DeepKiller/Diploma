

namespace BDB
{
    class Program
    {
        static void Main()
        {
            Table test = new Table("test.bdbt");
            string[] Cols = { "one", "two", "id" };
            test.SetColNames(Cols);
        }
    }
}
