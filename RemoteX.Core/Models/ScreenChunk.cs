using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    public static class ScreenChunk
    {
        //Gói UDP cho Screen: 17 byte Header + payload JPEG một phần
        public const byte Type = 0xCC;
        public const int HeaderSize = 17;

        public static bool IsChunk(ReadOnlySpan<byte> data) 
            => data.Length >= HeaderSize && data[0] == Type;

        public static void WriteHeader(
            Span<byte> dst,
            uint frameID, ushort idx, ushort cnt,
            uint timestampMs, ushort flags, ushort crc16 = 0)
        {
            dst[0] = Type;
            BinaryPrimitives.WriteUInt32LittleEndian(dst.Slice(1, 4), frameID); //Id khung
            BinaryPrimitives.WriteUInt16LittleEndian(dst.Slice(5, 2), idx); //STT chunk
            BinaryPrimitives.WriteUInt16LittleEndian(dst.Slice(7, 2), cnt); //Tong so chunk
            BinaryPrimitives.WriteUInt32LittleEndian(dst.Slice(9, 4), timestampMs); //Moc thoi gian (drop tre)
            BinaryPrimitives.WriteUInt16LittleEndian(dst.Slice(13, 2), flags); //bit 0 co the dung lam "direction"
            BinaryPrimitives.WriteUInt16LittleEndian(dst.Slice(15, 2), crc16); //De 0, sau muon them checksum thi dung
        }

        public static bool TryReadHeader(ReadOnlySpan<byte> src, 
            out uint frameId, out ushort idx, out ushort cnt, out uint tsMs, 
            out ushort flags, out ushort crc16)
        {
            frameId = 0; idx = 0; cnt = 0; tsMs = 0; flags = 0; crc16 = 0;
            if (src.Length < HeaderSize || src[0] != Type) return false;
            frameId = BinaryPrimitives.ReadUInt32LittleEndian(src.Slice(1, 4));
            idx = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(5, 2));
            cnt = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(7, 2));
            tsMs = BinaryPrimitives.ReadUInt32LittleEndian(src.Slice(9, 4));
            flags = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(13, 2));
            crc16 = BinaryPrimitives.ReadUInt16LittleEndian(src.Slice(15, 2));

            return true;
        }
    }
}