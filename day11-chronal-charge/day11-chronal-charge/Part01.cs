using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace day11_chronal_charge {
    class Part01 {
        class FuelCell {
            public int RackId { get; set; }
            public long PowerLevel { get; set; }
            public Point Position { get; set; }
        }

        class FuelCellAggregate {
            public long PowerLevel { get; set; }
        }

        delegate void PerFuelCellDelegate(FuelCell pCell, int pX, int pY);
        delegate void PerFuelCellAggregateDelegate(FuelCellAggregate pCell, int pX, int pY);

        static Size FuelCellGridSize;

        public static void Run() {
            int input = 8141;
            int testInput = 8;
            //input = testInput;

            FuelCellGridSize = new Size(300, 300);

            var fuelCells = new FuelCell[FuelCellGridSize.Width, FuelCellGridSize.Height];
            var fuelCellAggregates = new FuelCellAggregate[FuelCellGridSize.Width, FuelCellGridSize.Height];

            var perFuelCell = new Action<PerFuelCellDelegate>(dlgt => {
                for (int x = 1; x <= FuelCellGridSize.Width; x++) {
                    for (int y = 1; y <= FuelCellGridSize.Height; y++) {
                        dlgt?.Invoke(fuelCells[x - 1, y - 1], x, y);
                    }
                }
            });

            var perFuelCellAggregate = new Action<PerFuelCellAggregateDelegate>(dlgt => {
                for (int x = 0; x < FuelCellGridSize.Width; x++) {
                    for (int y = 0; y < FuelCellGridSize.Height; y++) {
                        dlgt?.Invoke(fuelCellAggregates[x, y], x, y);
                    }
                }
            });

            perFuelCell((cell, x, y) => {
                fuelCells[x-1, y-1] = new FuelCell();
            });

            perFuelCell((cell, x, y) => {
                cell.Position = new Point(x, y);
                cell.RackId = x + 10;
                var powerLevel = CalculatePowerLevel(cell.RackId, y, input);
                cell.PowerLevel = powerLevel;
            });

            perFuelCellAggregate((cell, x, y) => {
                fuelCellAggregates[x, y] = new FuelCellAggregate();
            });

            for (int x = 0; x < FuelCellGridSize.Width - 2; x++) {
                for (int y = 0; y < FuelCellGridSize.Height - 2; y++) {
                    for (int sx = 0; sx < 3; sx++) {
                        for (int sy = 0; sy < 3; sy++) {
                            fuelCellAggregates[x + 1, y + 1].PowerLevel = fuelCellAggregates[x + 1, y + 1].PowerLevel + fuelCells[x + sx, y + sy].PowerLevel;
                        }
                    }
                }
            }

            long largestPowerCellCenter = long.MinValue;
            var largestPowerCellTopLeft = Point.Empty;

            perFuelCellAggregate((cell, x, y) => {
                if (cell.PowerLevel > largestPowerCellCenter) {
                    largestPowerCellCenter = cell.PowerLevel;
                    largestPowerCellTopLeft = new Point(x - 1, y - 1);
                }
            });

            Console.WriteLine($"Point X={largestPowerCellTopLeft.X+1}, Y={largestPowerCellTopLeft.Y+1}");
        }

        public static int CalculatePowerLevel(int pRackId, int pY, int pInput) {
            long step1 = (pRackId * pY + pInput) * pRackId;
            int step2 = (int)Math.Floor(step1 / 100f);
            int step3 = (step2 % 10) - 5;
            return step3;
        }
    }
}
