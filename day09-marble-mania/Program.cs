using System;

namespace day09_marble_mania {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("I am keeping the slow version, just to show how");
            Console.WriteLine("part 2 had me realize what a bad approach I had.");
            Part01.Run();
            Console.WriteLine("-------------");
            Part02.Run();
            Console.WriteLine("-------------");
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }
}
