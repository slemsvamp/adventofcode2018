using System;
using System.IO;

namespace day03_no_matter_how_you_slice_it {
    class Program {
        static void Main(string[] args) {
            Part01.Run();
            Console.WriteLine("----------------");
            Part02.Run();
            Console.WriteLine("----------------");
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }
}
