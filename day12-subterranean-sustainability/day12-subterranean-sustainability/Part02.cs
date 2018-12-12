using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace day12_subterranean_sustainability {
    class Part02 {
        public static void Run() {
            var lines = File.ReadAllLines("input.txt");
            var bit = new Func<string, int[]>(pl => {
                byte plb = 0; var plc = new List<int>();
                for (int plx = 0; plx <= pl.Length - 5; plx++) {
                    plc.Add(pl.Substring(plx, 5).Select(pld => { plb++; return pld == '#' ? 1 << (plb - 1) : 0; }).Sum()); plb = 0;
                }
                return plc.ToArray();
            });
            var ini = new { s = 0, a = bit("...." + lines[0].Substring(15) + "....") };
            var rules = lines.Where(l => l.IndexOf("=>") >= 0).Select(l => l.Split(new string[] { " => " }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            var filters = rules.Select(r => new { c = r[1][0] == '#', f = bit(r[0]) });
            string genl = "";
            var gen = new int[ini.a.Length];
            ini.a.CopyTo(gen, 0);
            var sums = new Queue<int>();
            for (long g = 0; g < 50000000000; g++) {
                genl = "..";
                for (int p = 0; p < gen.Length + 3; p++) {
                    bool hit = false;
                    foreach (var filter in filters) {
                        if (p < gen.Length && filter.f[0] == gen[p] && filter.c) {
                            genl += "#"; hit = true;
                        }
                    }
                    if (!hit) genl += ".";
                }
                gen = bit(genl);
                int n = -5, sum = genl.Select(gl => { n++; return gl == '#' ? n : 0; }).Sum();
                if (g > 10) sums.Dequeue();
                sums.Enqueue(sum);
                if (g > 10) {
                    var df = new List<int>(); var sm = sums.ToArray();
                    for (int sdf = 0; sdf < 10; sdf++) df.Add(sm[10 - sdf] - sm[9 - sdf]);
                    if (df.Distinct().Count() == 1) {
                        Console.WriteLine(sm[9] + (50000000000 - g) * df[0]);
                        break;
                    }
                }
            }
        }
    }
}
