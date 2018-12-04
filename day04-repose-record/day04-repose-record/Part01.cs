﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace day04_repose_record {
    class Part01 {
        enum State {
            ShiftBegin, Awake, Asleep
        }

        class Guard {
            public int Id { get; private set; }
            public Dictionary<DateTime, State> States { get; set; }
            public int Asleep { get; set; }
            public int[] Sleepheat { get; set; }

            public Guard(int id) {
                Id = id;
                States = new Dictionary<DateTime, State>();
                Sleepheat = new int[60];
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
                foreach (var record in guard.States) {
                    if (record.Value == State.ShiftBegin) {
                        fellAsleep = DateTime.MinValue;
                    } else if (record.Value == State.Asleep) {
                        fellAsleep = record.Key;
                    }

                    if (fellAsleep != DateTime.MinValue && record.Value == State.Awake) {
                        minutesAsleep += record.Key.Minute - fellAsleep.Minute;

                        for (int sh = fellAsleep.Minute; sh < record.Key.Minute; sh++) {
                            guard.Sleepheat[sh] = guard.Sleepheat[sh] + 1;
                        }
                    }
                }
                guard.Asleep = minutesAsleep;
            }

            var mostAsleepGuard = guards.Values.OrderByDescending(g => g.Asleep).First();

            int highIndex = 0;
            int value = 0;
            for (int shIndex = 0; shIndex < 60; shIndex++) {
                if (mostAsleepGuard.Sleepheat[shIndex] > value) {
                    highIndex = shIndex;
                    value = mostAsleepGuard.Sleepheat[shIndex];
                }
            }

            var result = mostAsleepGuard.Id * highIndex;

            Console.WriteLine($"Most asleep guard: #{mostAsleepGuard.Id}, {mostAsleepGuard.Asleep} minute(s), index: {highIndex}");

            Console.WriteLine($"Result: {result}");

            Console.ReadKey(true);
        }
    }
}
