using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace day16_chronal_classification {
    class Part01 {
        class Instruction {
            public int OpCode { get; set; }
            public byte A { get; set; }
            public byte B { get; set; }
            public byte C { get; set; }
        }
        class OpcodeResult {
            public byte[] Before { get; set; }
            public byte[] After { get; set; }
            public Instruction Instruction { get; set; }
        }
        static byte[] registers;

        static List<OpcodeResult> opcodeResults;
        static Dictionary<int, HashSet<Opcode>> opcodeCandidates;
        static Dictionary<Opcode, HashSet<int>> discardedCandidates;
        static List<Instruction> data;
        static Dictionary<int, Opcode> opcodeRules;

        static int samplesBehavedLikeThreeOrMore;

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

            Console.WriteLine("Three or More: " + samplesBehavedLikeThreeOrMore);
        }

        static void RunOpcode(Opcode pOpcode, Instruction pInstruction, ref byte[] pRegisters) {
            // TIL: There's no byte addition operator for bytes, so I have to explicitly cast them.
            switch (pOpcode) {
                case Opcode.addr:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] + pRegisters[pInstruction.B]);
                    break;
                case Opcode.addi:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] + pInstruction.B);
                    break;
                case Opcode.mulr:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] * pRegisters[pInstruction.B]);
                    break;
                case Opcode.muli:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] * pInstruction.B);
                    break;
                case Opcode.banr:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] & pRegisters[pInstruction.B]);
                    break;
                case Opcode.bani:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] & pInstruction.B);
                    break;
                case Opcode.borr:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] | pRegisters[pInstruction.B]);
                    break;
                case Opcode.bori:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] | pInstruction.B);
                    break;
                case Opcode.setr:
                    pRegisters[pInstruction.C] = pRegisters[pInstruction.A];
                    break;
                case Opcode.seti:
                    pRegisters[pInstruction.C] = pInstruction.A;
                    break;
                case Opcode.gtir:
                    pRegisters[pInstruction.C] = (byte)(pInstruction.A > pRegisters[pInstruction.B] ? 1 : 0);
                    break;
                case Opcode.gtri:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] > pInstruction.B ? 1 : 0);
                    break;
                case Opcode.gtrr:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] > pRegisters[pInstruction.B] ? 1 : 0);
                    break;
                case Opcode.eqir:
                    pRegisters[pInstruction.C] = (byte)(pInstruction.A == pRegisters[pInstruction.B] ? 1 : 0);
                    break;
                case Opcode.eqri:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] == pInstruction.B ? 1 : 0);
                    break;
                case Opcode.eqrr:
                    pRegisters[pInstruction.C] = (byte)(pRegisters[pInstruction.A] == pRegisters[pInstruction.B] ? 1 : 0);
                    break;
            }
        }

        static void FindCandidates() {
            foreach (var candidate in opcodeResults) {
                var opcodesBehavedLike = new List<Opcode>();
                foreach (var opcode in Enum.GetValues(typeof(Opcode)).Cast<Opcode>()) {
                    if (CandidateOpcode(candidate, opcode)) {
                        opcodesBehavedLike.Add(opcode);
                    }
                }

                if (opcodesBehavedLike.Count >= 3) {
                    samplesBehavedLikeThreeOrMore++;
                }
            }

            opcodeCandidates = new Dictionary<int, HashSet<Opcode>>(opcodeCandidates.Where(oc => oc.Value.Count > 0));
        }

        static bool CandidateOpcode(OpcodeResult pResult, Opcode pOpcode, bool pRegister = true) {
            var instruction = pResult.Instruction;
            if (!opcodeCandidates.ContainsKey(instruction.OpCode)) {
                opcodeCandidates.Add(instruction.OpCode, new HashSet<Opcode>());
            }
            var fakeRegister = pResult.Before;
            RunOpcode(pOpcode, instruction, ref fakeRegister);
            if (ArrayEq(fakeRegister, pResult.After)) {
                // If we're in register mode, we haven't already added the instruction and it's not in our discarded possibility list.
                if (pRegister && !opcodeCandidates[instruction.OpCode].Contains(pOpcode) && !(discardedCandidates.ContainsKey(pOpcode) && discardedCandidates[pOpcode].Contains(instruction.OpCode))) {
                    opcodeCandidates[instruction.OpCode].Add(pOpcode);
                }
                return true;
            }
            return false;
        }

        static void Initialize(string pFile) {
            var lines = File.ReadAllLines(pFile);
            samplesBehavedLikeThreeOrMore = 0;
            registers = new byte[4];
            data = new List<Instruction>();
            opcodeCandidates = new Dictionary<int, HashSet<Opcode>>();
            discardedCandidates = new Dictionary<Opcode, HashSet<int>>();
            opcodeResults = new List<OpcodeResult>();
            bool endOfInstructions = false;
            var registerFromFile = new List<Instruction>();
            for (var l = 0; l < lines.Length; l++) {
                if (!endOfInstructions && lines[l] == lines[l + 1]) {
                    l += 2; endOfInstructions = true; continue;
                }
                if (!endOfInstructions) {
                    opcodeResults.Add(
                        new OpcodeResult {
                            Before = lines[l].Substring(9, 10).Split(new string[] { ", " }, StringSplitOptions.None).Select(n => byte.Parse(n)).ToArray(),
                            After = lines[l+2].Substring(9, 10).Split(new string[] { ", " }, StringSplitOptions.None).Select(n => byte.Parse(n)).ToArray(),
                            Instruction = ArrayToInstruction(lines[l+1].Split(new string[] { " " }, StringSplitOptions.None).Select(n => byte.Parse(n)).ToArray())
                        }
                    );
                    l += 3;
                } else {
                    data.Add(ArrayToInstruction(lines[l].Split(new string[] { " " }, StringSplitOptions.None).Select(n => byte.Parse(n)).ToArray()));
                }
            }
        }

        static Instruction ArrayToInstruction(byte[] pArray) {
            return new Instruction { OpCode = pArray[0], A = pArray[1], B = pArray[2], C = pArray[3] };
        }

        static bool ArrayEq(byte[] pA, byte[] pB) {
            return pA.Length == pB.Length && pA[0] == pB[0] && pA[1] == pB[1] && pA[2] == pB[2] && pA[3] == pB[3];
        }
    }
}
