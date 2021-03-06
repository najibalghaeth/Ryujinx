using ChocolArm64.Decoder;
using ChocolArm64.Memory;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static class AInstEmitMemoryHelper
    {
        private enum Extension
        {
            Zx,
            Sx32,
            Sx64
        }

        public static void EmitReadZxCall(AILEmitterCtx Context, int Size)
        {
            EmitReadCall(Context, Extension.Zx, Size);
        }

        public static void EmitReadSx32Call(AILEmitterCtx Context, int Size)
        {
            EmitReadCall(Context, Extension.Sx32, Size);
        }

        public static void EmitReadSx64Call(AILEmitterCtx Context, int Size)
        {
            EmitReadCall(Context, Extension.Sx64, Size);
        }

        private static void EmitReadCall(AILEmitterCtx Context, Extension Ext, int Size)
        {
            bool IsSimd = GetIsSimd(Context);

            string Name = null;

            if (Size < 0 || Size > (IsSimd ? 4 : 3))
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            if (IsSimd)
            {
                switch (Size)
                {
                    case 0: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadVector8Unchecked)
                        : nameof(AMemory.ReadVector8); break;

                    case 1: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadVector16Unchecked)
                        : nameof(AMemory.ReadVector16); break;

                    case 2: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadVector32Unchecked)
                        : nameof(AMemory.ReadVector32); break;

                    case 3: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadVector64Unchecked)
                        : nameof(AMemory.ReadVector64); break;

                    case 4: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadVector128Unchecked)
                        : nameof(AMemory.ReadVector128); break;
                }
            }
            else
            {
                switch (Size)
                {
                    case 0: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadByteUnchecked)
                        : nameof(AMemory.ReadByte); break;

                    case 1: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadUInt16Unchecked)
                        : nameof(AMemory.ReadUInt16); break;

                    case 2: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadUInt32Unchecked)
                        : nameof(AMemory.ReadUInt32); break;

                    case 3: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.ReadUInt64Unchecked)
                        : nameof(AMemory.ReadUInt64); break;
                }
            }

            Context.EmitCall(typeof(AMemory), Name);

            if (!IsSimd)
            {
                if (Ext == Extension.Sx32 ||
                    Ext == Extension.Sx64)
                {
                    switch (Size)
                    {
                        case 0: Context.Emit(OpCodes.Conv_I1); break;
                        case 1: Context.Emit(OpCodes.Conv_I2); break;
                        case 2: Context.Emit(OpCodes.Conv_I4); break;
                    }
                }

                if (Size < 3)
                {
                    Context.Emit(Ext == Extension.Sx64
                        ? OpCodes.Conv_I8
                        : OpCodes.Conv_U8);
                }
            }
        }

        public static void EmitWriteCall(AILEmitterCtx Context, int Size)
        {
            bool IsSimd = GetIsSimd(Context);

            string Name = null;

            if (Size < 0 || Size > (IsSimd ? 4 : 3))
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            if (Size < 3 && !IsSimd)
            {
                Context.Emit(OpCodes.Conv_I4);
            }

            if (IsSimd)
            {
                switch (Size)
                {
                    case 0: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteVector8Unchecked)
                        : nameof(AMemory.WriteVector8); break;

                    case 1: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteVector16Unchecked)
                        : nameof(AMemory.WriteVector16); break;

                    case 2: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteVector32Unchecked)
                        : nameof(AMemory.WriteVector32); break;

                    case 3: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteVector64Unchecked)
                        : nameof(AMemory.WriteVector64); break;

                    case 4: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteVector128Unchecked)
                        : nameof(AMemory.WriteVector128); break;
                }
            }
            else
            {
                switch (Size)
                {
                    case 0: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteByteUnchecked)
                        : nameof(AMemory.WriteByte); break;

                    case 1: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteUInt16Unchecked)
                        : nameof(AMemory.WriteUInt16); break;

                    case 2: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteUInt32Unchecked)
                        : nameof(AMemory.WriteUInt32); break;

                    case 3: Name = AOptimizations.DisableMemoryChecks
                        ? nameof(AMemory.WriteUInt64Unchecked)
                        : nameof(AMemory.WriteUInt64); break;
                }
            }

            Context.EmitCall(typeof(AMemory), Name);
        }

        private static bool GetIsSimd(AILEmitterCtx Context)
        {
            return Context.CurrOp is IAOpCodeSimd &&
                 !(Context.CurrOp is AOpCodeSimdMemMs ||
                   Context.CurrOp is AOpCodeSimdMemSs);
        }
    }
}