using System;

namespace day13_mine_cart_madness {
    class Program {
        static void Main(string[] args) {
            Part01.Run();
            Console.WriteLine("---------------");
            Part02.Run(false);
            Console.WriteLine("---------------");
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }
}
