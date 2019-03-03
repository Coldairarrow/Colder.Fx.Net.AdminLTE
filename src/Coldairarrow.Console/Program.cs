using System;

namespace Coldairarrow.Console1
{
    class Program
    {
        class Test
        {
            public string MyProperty { get;}
        }
        static void Main(string[] args)
        {
            var propperty = typeof(Test).GetProperty("MyProperty");
            Console.WriteLine("完成");
            Console.ReadLine();
        }
    }
}
