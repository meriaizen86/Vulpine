using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Vulpine.SpirV
{
    public class SpirV
    {
        public struct Instruction
        {
            int[] Words;
            public OpCodeID OpCode
            {
                get
                {
                    return (OpCodeID)(Words[0] & 0x0000ffff);
                }
            }

            public int Length
            {
                get
                {
                    return Words.Length;
                }
            }
            public int this[int ind]
            {
                get
                {
                    return Words[ind];
                }
                set
                {
                    Words[ind] = value;
                }
            }

            public Instruction(BinaryReader br)
            {
                var firstWord = br.ReadInt32();
                Words = new int[firstWord >> 16];
                Words[0] = firstWord;
                for (var i = 1; i < Words.Length; i++)
                    Words[i] = br.ReadInt32();
            }

            public void Write(BinaryWriter bw)
            {
                foreach (var word in Words)
                    bw.Write(word);
            }

            public string String(int ind)
            {
                unsafe
                {
                    fixed (int* start = Words)
                    {
                        var str = Encoding.UTF8.GetString((byte*)(start + ind), (Words.Length - ind) * 4);
                        for (var i = 0; i < str.Length; i++)
                            if (str[i] == '\0')
                                return new string(str.Take(i).ToArray());
                        return str;
                    }
                }
            }

            public string String(int ind, out int nextInd)
            {
                unsafe
                {
                    fixed (int* start = Words)
                    {
                        var str = Encoding.UTF8.GetString((byte*)(start + ind), (Words.Length - ind) * 4);
                        for (var i = 0; i < str.Length; i++)
                        {
                            if (str[i] == '\0')
                            {
                                var trimmed = new string(str.Take(i).ToArray());
                                nextInd = (trimmed.Length + 1) / 4 + 1;
                                return trimmed;
                            }
                        }
                        nextInd = (str.Length + 1) / 4 + 1;
                        return str;
                    }
                }
            }

            public string ToStringFormatted()
            {
                string operandString;

                switch (OpCode)
                {
                    case OpCodeID.Capability:
                        operandString = ((Capability)Words[1]).ToString();
                        break;
                    case OpCodeID.Extension:
                        operandString = $"{ String(1) }\"";
                        break;
                    case OpCodeID.ExtInstImport:
                        operandString = $"{ Words[1] } \"{ String(2) }\"";
                        break;
                    case OpCodeID.EntryPoint:
                        operandString = $"{ Words[1] } { Words[2] } \"{ String(3) }\" { Words[4] } { Words[5] }" +
                            $" { Words[6] } { Words[7] }";
                        break;
                    case OpCodeID.Name:
                        operandString = $"{ Words[1] } \"{ String(2) }\"";
                        break;
                    case OpCodeID.MemberName:
                        operandString = $"{ Words[1] } { Words[2] } \"{ String(3) }\"";
                        break;
                    default:
                        operandString = string.Join(" ", Words.Skip(1));
                        break;
                }

                return $"{OpCode} { operandString }";
            }
        }

        const uint SpirVMagic = 0x07230203;
        const byte WriteVersionMajor = 1;
        const byte WriteVersionMinor = 0;
        const uint ThisGeneratorMagic = 0x666;

        public byte VersionMajor;
        public byte VersionMinor;
        public uint GeneratorMagic;
        public int Bound;
        public List<Instruction> Instructions;
        public List<Capability> Capabilities { get; private set; } = new List<Capability>();
        public List<string> Extensions { get; private set; } = new List<string>();
        public Dictionary<int, string> ExtInstImports { get; private set; } = new Dictionary<int, string>();
        public MemoryModel MemoryModel { get; private set; }
        public List<EntryPoint> EntryPoints { get; private set; } = new List<EntryPoint>();
        public Dictionary<int, ExecutionMode> ExecutionModes { get; private set; } = new Dictionary<int, ExecutionMode>();
        public Dictionary<int, string> Names { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, Dictionary<int, string>> MemberNames { get; private set; } = new Dictionary<int, Dictionary<int, string>>();

        public SpirV(Stream stream)
        {
            using (var br = new BinaryReader(stream))
            {
                // Word 0; file magic
                var magic = br.ReadUInt32();
                if (magic != SpirVMagic)
                    throw new InvalidDataException("File magic was incorrect");

                // Word 1; version number
                br.ReadByte();
                VersionMinor = br.ReadByte();
                VersionMajor = br.ReadByte();
                br.ReadByte();

                // Word 2; generator magic
                GeneratorMagic = br.ReadUInt32();

                // Word 3; bound IDs where 0 < ID < this
                Bound = br.ReadInt32();

                // Word 4; optional instruction schema
                br.ReadInt32();

                // Word 5 and beyond; instructions
                Instructions = new List<Instruction>();
                while (stream.Position < stream.Length)
                {
                    var inst = new Instruction(br);
                    string sA;
                    int iA;
                    List<int> lA;
                    switch (inst.OpCode)
                    {
                        case OpCodeID.Capability:
                            Capabilities.Add((Capability)inst[1]);
                            break;
                        case OpCodeID.Extension:
                            Extensions.Add(inst.String(1));
                            break;
                        case OpCodeID.ExtInstImport:
                            ExtInstImports.Add(inst[1], inst.String(2));
                            break;
                        case OpCodeID.MemoryModel:
                            MemoryModel = (MemoryModel)inst[1];
                            break;
                        case OpCodeID.EntryPoint:
                            sA = inst.String(3, out iA);
                            lA = new List<int>();
                            for (var i = iA; i < inst.Length; i++)
                                lA.Add(inst[i]);
                            EntryPoints.Add(new EntryPoint((ExecutionModel)inst[1], inst[2], sA, lA));
                            break;
                        case OpCodeID.ExecutionMode:
                            ExecutionModes.Add(inst[1], (ExecutionMode)inst[2]);
                            break;
                        case OpCodeID.Name:
                            Names.Add(inst[1], inst.String(2));
                            MemberNames.Add(inst[1], new Dictionary<int, string>());
                            break;
                        case OpCodeID.MemberName:
                            MemberNames[inst[1]].Add(inst[2], inst.String(3));
                            break;
                        default:
                            Instructions.Add(new Instruction(br));
                            break;
                    }
                }
            }
        }

        public void Write(Stream stream)
        {
            using (var bw = new BinaryWriter(stream))
            {
                // Word 0; file magic
                bw.Write(SpirVMagic);

                // Word 1; version number
                bw.Write((byte)0);
                bw.Write(WriteVersionMinor);
                bw.Write(WriteVersionMajor);
                bw.Write((byte)0);

                // Word 2; generator magic
                bw.Write(ThisGeneratorMagic);

                // Word 3; bound IDs where 0 < ID < this
                bw.Write(Bound);

                // Word 4; optional instruction schema
                bw.Write(0);

                // Word 5 and beyond; instructions
                foreach (var inst in Instructions)
                    inst.Write(bw);
            }
        }

        public string ToStringFormatted()
        {
            var stringBuilder = new StringBuilder();
            foreach (var inst in Instructions)
                stringBuilder.Append($"\t\t{ inst.ToStringFormatted() }\n");
            var instructionString = stringBuilder.ToString();
            stringBuilder.Clear();
            
            return "Spir-V Source [\n" +
                $"\tVersion: 0.{ VersionMajor }.{ VersionMinor }.0\n" +
                $"\tGenerator: { GeneratorMagic }\n" +
                $"\tBound: { Bound }\n" +
                $"\tCapabilities: { string.Join(", ", Capabilities ) }\n" +
                $"\tExtensions: { string.Join(", ", Extensions) }\n" +
                $"\tExtInstImports: { ExtInstImports.ToStringFormatted() }\n" +
                $"\tMemoryModel: { MemoryModel }\n" +
                $"\tEntryPoints: { string.Join(", ", EntryPoints) }\n" +
                $"\tExecutionModes: { ExecutionModes.ToStringFormatted() }\n" +
                $"\tNames: { Names.ToStringFormatted() }\n" +
                $"\tMemberNames: { MemberNames.ToStringFormatted() }\n" +
                $"\tInstructions:\n" +
                $"{instructionString}]";
        }
    }
}
