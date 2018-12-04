using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace day04_repose_record {
    class Part02 {
        enum State {
            ShiftBegin, Awake, Asleep
        }

        class Guard {
            public int Id { get; private set; }
            public Dictionary<DateTime, State> States { get; set; }
            public int Asleep { get; set; }
            public Dictionary<DateTime, int[]> Sleepheats { get; set; }
            public int[] AggregatedSleepheat { get; set; }
            public int MaxSleptMinute { get; set; }
            public int MaxSleptMinuteTimes { get; set; }

            public Guard(int id) {
                Id = id;
                States = new Dictionary<DateTime, State>();
                Sleepheats = new Dictionary<DateTime, int[]>();
            }
        }

        struct Entry {
            public DateTime Date { get; set; }
            public string Message { get; set; }
        }

        static Regex historyRegex;
        static Regex guardRegex;
        static Dictionary<int, Guard> guards;
        static List<string> orderedHistory;
        static List<Entry> entries;

        public static void Run() {
            var lines = File.ReadLines("input.txt");
            orderedHistory = new List<string>();
            entries = new List<Entry>();
            guards = new Dictionary<int, Guard>();

            historyRegex = new Regex(@"\[(?<year>\d*)-(?<month>\d*)-(?<day>\d*) (?<hour>\w*):(?<minute>\w*)\] (?<message>.*)");
            guardRegex = new Regex(@"Guard #(?<id>\d*).*");

            int guardId = -1;
            var state = State.Awake;
            foreach (var line in lines) {
                var historyMatches = historyRegex.Match(line);
                var dateTime = DateTime.Parse($"{historyMatches.Groups["year"].Value}-{historyMatches.Groups["month"].Value}-{historyMatches.Groups["day"].Value} {historyMatches.Groups["hour"].Value}:{historyMatches.Groups["minute"].Value}");
                entries.Add(new Entry {
                    Date = dateTime,
                    Message = historyMatches.Groups["message"].Value
                });
            }

            foreach (var entry in entries.OrderBy(e => e.Date)) {
                string message = entry.Message;

                if (message.StartsWith("Guard")) {
                    var guardMatches = guardRegex.Match(message);
                    guardId = int.Parse(guardMatches.Groups["id"].Value);
                    state = State.ShiftBegin;
                } else {
                    switch (message) {
                        case "falls asleep":
                            state = State.Asleep;
                            break;
                        default:
                        case "wakes up":
                            state = State.Awake;
                            break;
                    }

                }

                if (!guards.ContainsKey(guardId)) {
                    guards.Add(guardId, new Guard(guardId));
                }

                guards[guardId].States.Add(entry.Date, state);
            }

            foreach (var guard in guards.Values) {
                int minutesAsleep = 0;
                DateTime fellAsleep = DateTime.MinValue;
                var day = DateTime.MinValue;

                foreach (var record in guard.States) {
                    if (record.Value == State.ShiftBegin) {
                        day = record.Key;
                        guard.Sleepheats.Add(day, new int[60]);
                        fellAsleep = DateTime.MinValue;
                    } else if (record.Value == State.Asleep) {
                        fellAsleep = record.Key;
                    }

                    if (fellAsleep != DateTime.MinValue && record.Value == State.Awake) {
                        minutesAsleep += record.Key.Minute - fellAsleep.Minute;

                        for (int sh = fellAsleep.Minute; sh < record.Key.Minute; sh++) {
                            guard.Sleepheats[day][sh] = guard.Sleepheats[day][sh] + 1;
                        }
                    }
                }

                guard.Asleep = minutesAsleep;
                guard.AggregatedSleepheat = new int[60];
            }

            foreach (var guard in guards.Values) {
                int index = 0;
                int maxValue = 0;

                foreach (var sh in guard.Sleepheats) {
                    for (int m = 0; m < 60; m++) {
                        int newValue = guard.AggregatedSleepheat[m] + sh.Value[m];
                        guard.AggregatedSleepheat[m] = newValue;

                        if (newValue > maxValue) {
                            maxValue = newValue;
                            index = m;
                        }
                    }
                }

                guard.MaxSleptMinute = index;
                guard.MaxSleptMinuteTimes = guard.AggregatedSleepheat[index];
            }

            var guardWithOneMinuteMostSlept = guards.Values.OrderByDescending(g => g.MaxSleptMinuteTimes).First();

            var result = guardWithOneMinuteMostSlept.Id * guardWithOneMinuteMostSlept.MaxSleptMinute;

            Console.WriteLine($"Guard With One Minute Most Slept: {guardWithOneMinuteMostSlept.Id}, Max Minute Index: {guardWithOneMinuteMostSlept.MaxSleptMinute}");
            Console.WriteLine($"Result: {result}");

            Console.ReadKey(true);
        }
    }
}
