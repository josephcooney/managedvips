Here is the C# code equivalent to the provided C code:

```csharp
using System;

public enum LzwResult {
    LZW_OK,
    LZW_NO_MEM,
    LZW_BAD_PARAM,
    LZW_BAD_ICODE,
    LZW_NO_DATA,
    LZW_OK_EOD,
    LZW_BAD_CODE,
    LZW_EOI_CODE,
    LZW_NO_COLOUR
}

public struct LzwTableEntry {
    public byte Value;
    public byte First;
    public ushort Count;
    public ushort Extends;
}

public class LzwReadContext {
    public byte[] Data { get; set; }
    public int DataLen { get; set; }
    public int DataSbNext { get; set; }

    public byte[] SbData { get; set; }
    public int SbBit { get; set; }
    public int SbBitCount { get; set; }
}

public class LzwContext {
    public LzwReadContext Input { get; set; }

    public ushort PrevCode;
    public ushort PrevCodeFirst;
    public ushort PrevCodeCount;

    public byte InitialCodeSize;
    public byte CodeSize;
    public ushort CodeMax;

    public ushort ClearCode;
    public ushort EoiCode;

    public ushort TableSize;
    public ushort OutputCode;
    public ushort OutputLeft;

    public bool HasTransparency;
    public byte TransparencyIdx;
    public uint[] ColourMap;

    public LzwTableEntry[] Table;
}

public class LzwWriter {
    public delegate int WriteFn(LzwContext ctx, byte[] outputData, int outputLength, int outputUsed, ushort code, ushort left);

    public static int WriteFnImpl(LzwContext ctx, byte[] outputData, int outputLength, int outputUsed, ushort code, ushort left) {
        LzwTableEntry[] table = ctx.Table;
        uint space = (uint)(outputLength - outputUsed);
        uint count = (uint)left;

        if (count > space) {
            left = (ushort)(count - space);
            count = space;
        } else {
            left = 0;
        }

        ctx.OutputCode = code;
        ctx.OutputLeft = left;

        for (int i = (int)left; i != 0; i--) {
            LzwTableEntry entry = table[code];
            code = entry.Extends;
        }

        byte[] outputPos = outputData;
        outputPos += count;
        for (int i = (int)count; i != 0; i--) {
            LzwTableEntry entry = table[code];
            *--outputPos = entry.Value;
            code = entry.Extends;
        }

        return (int)count;
    }
}

public class LzwContextCreator {
    public static LzwResult Create(LzwContext[] ctx) {
        if (ctx == null || ctx.Length == 0) {
            return LzwResult.LZW_NO_MEM;
        }

        LzwContext c = new LzwContext();
        ctx[0] = c;

        return LzwResult.LZW_OK;
    }
}

public class LzwContextDestroyer {
    public static void Destroy(LzwContext[] ctx) {
        if (ctx != null && ctx.Length > 0) {
            ctx[0].Input.Data = null;
            ctx[0].Table = null;
            ctx[0] = default(LzwContext);
        }
    }
}

public class LzwBlockAdvancer {
    public static LzwResult Advance(LzwReadContext ctx) {
        if (ctx == null || ctx.DataLen <= 0) {
            return LzwResult.LZW_NO_DATA;
        }

        int blockSize = ctx.Data[ctx.DataSbNext];
        if ((ctx.DataSbNext + blockSize) >= ctx.DataLen) {
            return LzwResult.LZW_NO_DATA;
        }

        ctx.SbBit = 0;
        ctx.SbBitCount = (int)(blockSize * 8);

        if (blockSize == 0) {
            ctx.DataSbNext += 1;
            return LzwResult.LZW_OK_EOD;
        }

        byte[] sbData = new byte[ctx.Data.Length];
        Array.Copy(ctx.Data, ctx.DataSbNext + 1, sbData, 0, blockSize);
        ctx.SbData = sbData;
        ctx.DataSbNext += (blockSize + 1);

        return LzwResult.LZW_OK;
    }
}

public class LzwCodeReader {
    public static LzwResult ReadCode(LzwReadContext ctx, byte codeSize, ref ushort codeOut) {
        if (ctx == null || ctx.SbBitCount <= 0) {
            return LzwResult.LZW_NO_DATA;
        }

        uint currentBit = (uint)(ctx.SbBit & 7);
        uint code = 0;

        if ((ctx.SbBit + 24) <= ctx.SbBitCount) {
            byte[] sbData = ctx.SbData;
            code |= sbData[ctx.SbBit >> 3] << 0;
            code |= sbData[(ctx.SbBit >> 3) + 1] << 8;
            code |= sbData[(ctx.SbBit >> 3) + 2] << 16;
            ctx.SbBit += (int)codeSize;
        } else {
            byte advance = ((currentBit + codeSize) >> 3);
            byte[] sbData = ctx.SbData;

            uint bitsRemaining0 = (uint)(codeSize < (8 - currentBit)) ? codeSize : (8 - currentBit);
            uint bitsRemaining1 = (uint)(codeSize - bitsRemaining0);
            byte[] bitsUsed = new byte[3];
            bitsUsed[0] = (byte)bitsRemaining0;
            bitsUsed[1] = (byte)(bitsRemaining1 < 8 ? bitsRemaining1 : 8);
            bitsUsed[2] = (byte)(bitsRemaining1 - 8);

            while (true) {
                LzwResult res = Advance(ctx);
                if (res != LzwResult.LZW_OK) {
                    return res;
                }

                for (int i = 0; i <= advance; i++) {
                    code |= sbData[ctx.SbBit >> 3] << ((i * 8) + currentBit);
                    ctx.SbBit += bitsUsed[i];
                }

                if (advance > 2) {
                    break;
                }
            }
        }

        codeOut = (ushort)((code >> currentBit) & ((1 << codeSize) - 1));
        return LzwResult.LZW_OK;
    }
}

public class LzwClearCodeHandler {
    public static LzwResult HandleClear(LzwContext ctx, ref ushort codeOut) {
        uint16_t code;

        // Reset table building context
        ctx.CodeSize = ctx.InitialCodeSize;
        ctx.CodeMax = (ushort)((1 << ctx.InitialCodeSize) - 1);
        ctx.TableSize = ctx.EoiCode + 1;

        do {
            LzwResult res = ReadCode(ctx.Input, ctx.CodeSize, ref code);
            if (res != LzwResult.LZW_OK) {
                return res;
            }
        } while (code == ctx.ClearCode);

        // The initial code must be from the initial table.
        if (code > ctx.ClearCode) {
            return LzwResult.LZW_BAD_ICODE;
        }

        codeOut = code;
        return LzwResult.LZW_OK;
    }
}

public class LzwDecoderInitializer {
    public static LzwResult Init(LzwContext ctx, byte minimumCodeSize, byte[] input_data, int input_length, int input_pos) {
        if (minimumCodeSize >= 12) {
            return LzwResult.LZW_BAD_ICODE;
        }

        // Initialise the input reading context
        ctx.Input.Data = input_data;
        ctx.Input.DataLen = input_length;
        ctx.Input.DataSbNext = input_pos;

        ctx.Input.SbBit = 0;
        ctx.Input.SbBitCount = 0;

        // Initialise the table building context
        ctx.InitialCodeSize = minimumCodeSize + 1;

        ctx.ClearCode = (ushort)((1 << minimumCodeSize) + 0);
        ctx.EoiCode = (ushort)((1 << minimumCodeSize) + 1);

        ctx.OutputLeft = 0;

        // Initialise the standard table entries
        LzwTableEntry[] table = new LzwTableEntry[ctx.ClearCode];
        for (int i = 0; i < ctx.ClearCode; i++) {
            table[i].Value = (byte)i;
            table[i].First = (byte)i;
            table[i].Count = 1;
            table[i].Extends = 0;
        }

        LzwResult res = HandleClear(ctx, ref code);
        if (res != LZW_OK) {
            return res;
        }

        // Store details of this code as "previous code" to the context.
        ctx.PrevCodeFirst = table[code].First;
        ctx.PrevCodeCount = table[code].Count;
        ctx.PrevCode = code;

        // Add code to context for immediate output.
        ctx.OutputCode = code;
        ctx.OutputLeft = 1;

        ctx.HasTransparency = false;
        ctx.TransparencyIdx = 0;
        ctx.ColourMap = null;

        return LZW_OK;
    }
}

public class LzwDecoderInitializerWithColourTable {
    public static LzwResult Init(LzwContext ctx, byte minimumCodeSize, uint transparency_idx, uint[] colour_table, byte[] input_data, int input_length, int input_pos) {
        if (colour_table == null) {
            return LZW_BAD_PARAM;
        }

        LzwResult res = Init(ctx, minimumCodeSize, input_data, input_length, input_pos);
        if (res != LZW_OK) {
            return res;
        }

        ctx.HasTransparency = transparency_idx <= 0xFF;
        ctx.TransparencyIdx = transparency_idx;
        ctx.ColourMap = colour_table;

        return LZW_OK;
    }
}

public class LzwTableAdder {
    public static void AddEntry(LzwContext ctx, ushort code) {
        LzwTableEntry[] table = ctx.Table;
        LzwTableEntry entry = new LzwTableEntry();
        entry.Value = (byte)code;
        entry.First = (byte)ctx.PrevCodeFirst;
        entry.Count = (ushort)(ctx.PrevCodeCount + 1);
        entry.Extends = ctx.PrevCode;

        table[ctx.TableSize] = entry;
        ctx.TableSize++;
    }
}

public class LzwDecoder {
    public static LzwResult Decode(LzwContext ctx, byte[] output_data, ref int output_written) {
        if (output_data == null || output_data.Length <= 0) {
            return LZW_NO_DATA;
        }

        while (true) {
            LzwResult res = ReadCode(ctx.Input, ctx.CodeSize, ref ctx.PrevCode);
            if (res != LZW_OK) {
                return res;
            }

            // Handle the new code
            if (ctx.PrevCode == ctx.EoiCode) {
                // Got End of Information code
                return LZW_EOI_CODE;

            } else if (ctx.PrevCode > ctx.TableSize) {
                // Code is invalid
                return LZW_BAD_CODE;

            } else if (ctx.TableSize < 4096) {
                AddEntry(ctx, ctx.PrevCode);

                // Ensure code size is increased, if needed.
                if (ctx.TableSize == ctx.CodeMax && ctx.CodeSize < 12) {
                    ctx.CodeSize++;
                    ctx.CodeMax = (ushort)((1 << ctx.CodeSize) - 1);
                }
            }

            output_written += LzwWriter.WriteFnImpl(ctx, output_data, output_data.Length, output_written, ctx.PrevCode, ctx.Table[ctx.PrevCode].Count);

            // Store details of this code as "previous code" to the context.
            ctx.PrevCodeFirst = ctx.Table[ctx.PrevCode].First;
            ctx.PrevCodeCount = ctx.Table[ctx.PrevCode].Count;
            ctx.PrevCode = ctx.PrevCode;

            if (output_written == output_data.Length) {
                break;
            }
        }

        return LZW_OK;
    }
}

public class LzwDecoderWithColourTable {
    public static LzwResult Decode(LzwContext ctx, uint[] output_data, int output_length, ref int output_written) {
        if (output_data == null || output_data.Length <= 0) {
            return LZW_NO_DATA;
        }

        while (true) {
            LzwResult res = ReadCode(ctx.Input, ctx.CodeSize, ref ctx.PrevCode);
            if (res != LZW_OK) {
                return res;
            }

            // Handle the new code
            if (ctx.PrevCode == ctx.EoiCode) {
                // Got End of Information code
                return LZW_EOI_CODE;

            } else if (ctx.PrevCode > ctx.TableSize) {
                // Code is invalid
                return LZW_BAD_CODE;

            } else if (ctx.TableSize < 4096) {
                AddEntry(ctx, ctx.PrevCode);

                // Ensure code size is increased, if needed.
                if (ctx.TableSize == ctx.CodeMax && ctx.CodeSize < 12) {
                    ctx.CodeSize++;
                    ctx.CodeMax = (ushort)((1 << ctx.CodeSize) - 1);
                }
            }

            output_written += LzwWriter.WriteFnImpl(ctx, output_data, output_length, output_written, ctx.PrevCode, ctx.Table[ctx.PrevCode].Count);

            // Store details of this code as "previous code" to the context.
            ctx.PrevCodeFirst = ctx.Table[ctx.PrevCode].First;
            ctx.PrevCodeCount = ctx.Table[ctx.PrevCode].Count;
            ctx.PrevCode = ctx.PrevCode;

            if (output_written == output_length) {
                break;
            }
        }

        return LZW_OK;
    }
}
```