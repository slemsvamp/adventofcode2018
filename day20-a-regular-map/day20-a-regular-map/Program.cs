using System;
using System.Drawing;

namespace day20_a_regular_map {
    class Program {
        static void Main(string[] args) {
            Part01.Run();
            Console.WriteLine("--------------------------");
            Part02.Run();
            Console.WriteLine("--------------------------");
            Console.WriteLine("Press any key to exit..");
        }
    }

    [Flags]
    public enum Direction : int {
        North = 1,
        South = 2,
        West = 4,
        East = 8
    };

    public static class PointExtensions {
        public static Point Neighbor(this Point pThis, Direction pDirection) {
            if (pDirection == Direction.North) return new Point(pThis.X, pThis.Y - 1);
            if (pDirection == Direction.West) return new Point(pThis.X - 1, pThis.Y);
            if (pDirection == Direction.East) return new Point(pThis.X + 1, pThis.Y);
            return new Point(pThis.X, pThis.Y + 1);
        }

        public static Point ToPoint(this int pThis, int pWidth) {
            return new Point(pThis % pWidth, pThis / pWidth);
        }

        public static int Index(this Point pThis, int pWidth) {
            return pThis.Y * pWidth + pThis.X;
        }

        public static string ToCoordinateString(this Point pThis) {
            return $"{pThis.X},{pThis.Y}";
        }
    }
}
