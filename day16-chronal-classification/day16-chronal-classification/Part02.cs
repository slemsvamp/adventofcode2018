using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day16_chronal_classification {
    class Part02 {
        class Instruction {
            public int OpCode { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }
        class OpcodeResult {
            public int[] Before { get; set; }
            public int[] After { get; set; }
            public Instruction Instruction { get; set; }
        }
        static int[] registers;

        static List<OpcodeResult> opcodeResults;
        static Dictionary<int, HashSet<Opcode>> opcodeCandidates;
        static Dictionary<Opcode, HashSet<int>> discardedCandidates;
        static List<Instruction> data;
        static Dictionary<int, Opcode> opcodeRules;
        static Dictionary<Opcode, Dictionary<int, int>> opcodeStatistics;
        struct Stats {
            public Opcode Opcode { get; set; }
            public int Instruction { get; set; }
            public bool Success { get; set; }
        }

        static List<Stats> stats;

        enum Opcode {
            addr, addi,
            mulr, muli,
            banr, bani,
            borr, bori,
            setr, seti,
            gtir, gtri, gtrr,
            eqir, eqri, eqrr
        }

        public static void Run() {
            Initialize("input.txt");
            FindCandidates();
            SolveSetup();

            foreach (var d in data) {
                RunOpcode(opcodeRules[d.OpCode], d, ref registers);
            }

            Console.WriteLine("Answer: " + registers[0]);
        }

        private static void GenerateStatistics() {
            var opcodes = new HashSet<Opcode>();
            foreach (var oc in opcodeCandidates) {
                foreach (var c in oc.Value) {
                    if (!opcodes.Contains(c)) {
                        opcodes.Add(c);
                    }
                }
            }

            foreach (var opcode in opcodes) {
                foreach (var candidate in opcodeResults) {
                    if (CandidateOpcode(candidate, opcode, false)) {
                        if (!opcodeStatistics.ContainsKey(opcode)) {
                            opcodeStatistics.Add(opcode, new Dictionary<int, int>());
                        }
                        if (!opcodeStatistics[opcode].ContainsKey(candidate.Instruction.OpCode)) {
                            opcodeStatistics[opcode].Add(candidate.Instruction.OpCode, 0);
                        }
                        opcodeStatistics[opcode][candidate.Instruction.OpCode] += 1;
                    }
                }
            }
        }

        static void SolveSetup() {
            int opCodeCandidateCount = 0;

            foreach (var rule in opcodeRules) {
                foreach (var falseCandidate in opcodeCandidates) {
                    if (falseCandidate.Value.Contains(rule.Value)) {
                        falseCandidate.Value.Remove(rule.Value);
                    }
                }
            }

            do {
                opCodeCandidateCount = opcodeCandidates.Count;
                var candidatesKeys = opcodeCandidates.Where(oc => oc.Value.Count == 1).Select(oc => oc.Key).ToList();
                foreach (var candidateKey in candidatesKeys) {
                    if (!opcodeCandidates.ContainsKey(candidateKey) || opcodeCandidates[candidateKey].Count == 0)
                        continue;
                    var opcode = opcodeCandidates[candidateKey].Single();
                    opcodeRules.Add(candidateKey, opcode);
                    opcodeCandidates.Remove(candidateKey);
                    foreach (var falseCandidate in opcodeCandidates) {
                        if (falseCandidate.Value.Contains(opcode)) {
                            falseCandidate.Value.Remove(opcode);
                        }
                    }
                }

            } while (opcodeCandidates.Count != opCodeCandidateCount);
        }

        static void RunOpcode(Opcode pOpcode, Instruction pInstruction, ref int[] pRegisters) {
            switch (pOpcode) {
                case Opcode.addr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] + pRegisters[pInstruction.B];
                    break;
                case Opcode.addi:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] + pInstruction.B;
                    break;
                case Opcode.mulr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] * pRegisters[pInstruction.B];
                    break;
                case Opcode.muli:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] * pInstruction.B;
                    break;
                case Opcode.banr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] & pRegisters[pInstruction.B];
                    break;
                case Opcode.bani:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] & pInstruction.B;
                    break;
                case Opcode.borr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] | pRegisters[pInstruction.B];
                    break;
                case Opcode.bori:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] | pInstruction.B;
                    break;
                case Opcode.setr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A];
                    break;
                case Opcode.seti:
                    pRegisters[pInstruction.C] = pInstruction.A;
                    break;
                case Opcode.gtir:
                    pRegisters[pInstruction.C] = pInstruction.A > pRegisters[pInstruction.B] ? 1 : 0;
                    break;
                case Opcode.gtri:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] > pInstruction.B ? 1 : 0;
                    break;
                case Opcode.gtrr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] > pRegisters[pInstruction.B] ? 1 : 0;
                    break;
                case Opcode.eqir:
                    pRegisters[pInstruction.C] = pInstruction.A == pRegisters[pInstruction.B] ? 1 : 0;
                    break;
                case Opcode.eqri:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] == pInstruction.B ? 1 : 0;
                    break;
                case Opcode.eqrr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A] == pRegisters[pInstruction.B] ? 1 : 0;
                    break;
            }
        }

        static void FindCandidates() {
            foreach (var candidate in opcodeResults) {
                var opcodesBehavedLike = new List<Opcode>();
                foreach (var opcode in Enum.GetValues(typeof(Opcode)).Cast<Opcode>()) {
                    if (CandidateOpcode(candidate, opcode)) {
                        stats.Add(new Stats { Opcode = opcode, Instruction = candidate.Instruction.OpCode, Success = true });
                        if (!opcodeStatistics.ContainsKey(opcode)) {
                            opcodeStatistics.Add(opcode, new Dictionary<int, int>());
                        }
                        if (!opcodeStatistics[opcode].ContainsKey(candidate.Instruction.OpCode)) {
                            opcodeStatistics[opcode].Add(candidate.Instruction.OpCode, 0);
                        }
                        opcodeStatistics[opcode][candidate.Instruction.OpCode] += 1;
                        opcodesBehavedLike.Add(opcode);
                    } else {
                        stats.Add(new Stats { Opcode = opcode, Instruction = candidate.Instruction.OpCode, Success = false });
                        if (opcodeCandidates.ContainsKey(candidate.Instruction.OpCode) && opcodeCandidates[candidate.Instruction.OpCode].Contains(opcode)) {
                            // proven itself to not be able to be this
                            opcodeCandidates[candidate.Instruction.OpCode].Remove(opcode);
                            if (!discardedCandidates.ContainsKey(opcode)) {
                                discardedCandidates.Add(opcode, new HashSet<int>());
                            }
                            discardedCandidates[opcode].Add(candidate.Instruction.OpCode);
                        }
                    }
                }
            }

            opcodeCandidates = new Dictionary<int, HashSet<Opcode>>(opcodeCandidates.Where(oc => oc.Value.Count > 0));
        }

        static bool CandidateOpcode(OpcodeResult pResult, Opcode pOpcode, bool pRegister = true) {
            var instruction = pResult.Instruction;
            var fakeRegister = new int[4];
            pResult.Before.CopyTo(fakeRegister, 0);
            RunOpcode(pOpcode, instruction, ref fakeRegister);
            if (ArrayEq(fakeRegister, pResult.After)) {
                // If we're in register mode, we haven't already added the instruction and it's not in our discarded possibility list.
                if (!opcodeCandidates.ContainsKey(instruction.OpCode)) {
                    opcodeCandidates.Add(instruction.OpCode, new HashSet<Opcode>());
                }
                if (pRegister && !opcodeCandidates[instruction.OpCode].Contains(pOpcode) && !(discardedCandidates.ContainsKey(pOpcode) && discardedCandidates[pOpcode].Contains(instruction.OpCode))) {
                    opcodeCandidates[instruction.OpCode].Add(pOpcode);
                }
                return true;
            }
            return false;
        }

        static void Initialize(string pFile) {
            var lines = File.ReadAllLines(pFile);
            registers = new int[4];
            data = new List<Instruction>();
            stats = new List<Stats>();
            opcodeCandidates = new Dictionary<int, HashSet<Opcode>>();
            discardedCandidates = new Dictionary<Opcode, HashSet<int>>();
            opcodeStatistics = new Dictionary<Opcode, Dictionary<int, int>>();
            opcodeResults = new List<OpcodeResult>();
            opcodeRules = new Dictionary<int, Opcode>();
            bool endOfInstructions = false;
            var registerFromFile = new List<Instruction>();
            for (var l = 0; l < lines.Length; l++) {
                if (!endOfInstructions && lines[l] == lines[l + 1]) {
                    l += 2; endOfInstructions = true; continue;
                }
                if (!endOfInstructions) {
                    opcodeResults.Add(
                        new OpcodeResult {
                            Before = lines[l].Substring(9, 10).Split(new string[] { ", " }, StringSplitOptions.None).Select(n => int.Parse(n)).ToArray(),
                            After = lines[l + 2].Substring(9, 10).Split(new string[] { ", " }, StringSplitOptions.None).Select(n => int.Parse(n)).ToArray(),
                            Instruction = ArrayToInstruction(lines[l + 1].Split(new string[] { " " }, StringSplitOptions.None).Select(n => int.Parse(n)).ToArray())
                        }
                    );
                    l += 3;
                } else {
                    data.Add(ArrayToInstruction(lines[l].Split(new string[] { " " }, StringSplitOptions.None).Select(n => int.Parse(n)).ToArray()));
                }
            }
        }

        static Instruction ArrayToInstruction(int[] pArray) {
            return new Instruction { OpCode = pArray[0], A = pArray[1], B = pArray[2], C = pArray[3] };
        }

        static bool ArrayEq(int[] pA, int[] pB) {
            return pA.Length == pB.Length && pA[0] == pB[0] && pA[1] == pB[1] && pA[2] == pB[2] && pA[3] == pB[3];
        }
    }
}
