using System;
using System.Drawing;

namespace day15_beverage_bandits {
    class Program {
        static void Main(string[] args) {
            Console.SetWindowSize(90, 40);
            Console.SetBufferSize(90, 40);
            Part01.Run();
            //Console.WriteLine("----------------");
            //Part02.Run();
            //Console.WriteLine("----------------");
            //Console.WriteLine("Press any key to exit..");
            Console.ReadKey(true);
        }
    }

    public enum Direction { Up, Down, Left, Right };

    public static class PointExtensions {
        public static Point Neighbor(this Point pThis, Direction pDirection) {
            if (pDirection == Direction.Up) return new Point(pThis.X, pThis.Y - 1);
            if (pDirection == Direction.Left) return new Point(pThis.X - 1, pThis.Y);
            if (pDirection == Direction.Right) return new Point(pThis.X + 1, pThis.Y);
            return new Point(pThis.X, pThis.Y + 1);
        }

        public static Point ToPoint(this int pThis, int pWidth) {
            return new Point(pThis % pWidth, pThis / pWidth);
        }

        public static int Index(this Point pThis, int pWidth) {
            return pThis.Y * pWidth + pThis.X;
        }
    }
}
