using System;

namespace day12_subterranean_sustainability {
    class Program {
        static void Main(string[] args) {
            Console.WindowWidth = 80;
            Console.BufferWidth = 80;
            Part01.Run();
            Console.WriteLine("---------------");
            Part02.Run();
            Console.WriteLine("---------------");
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }
}
