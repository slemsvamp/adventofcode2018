using System;
using System.Drawing;

namespace day17_reservoir_research {
    class Program {
        static void Main(string[] args) {
            Console.SetWindowSize(220, 60);
            Console.SetBufferSize(220, 60);
            Console.CursorVisible = false;
            Part01And02.Run();
            Console.WriteLine("-------------------");
            Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }

    public static class PointExtensions {
        public static Point Up(this Point pThis) {
            return new Point(pThis.X, pThis.Y - 1);
        }

        public static Point Down(this Point pThis) {
            return new Point(pThis.X, pThis.Y + 1);
        }

        public static Point Left(this Point pThis) {
            return new Point(pThis.X - 1, pThis.Y);
        }

        public static Point Right(this Point pThis) {
            return new Point(pThis.X + 1, pThis.Y);
        }
    }
}
