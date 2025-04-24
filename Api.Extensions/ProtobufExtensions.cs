using ProtoBuf;

namespace Api.Extensions
{
    public static class ProtobufExtensions
    {
        public static byte[] SerializeProtobuf<T>(this T value) where T : class, new()
        {
            if (value == null)
                return Array.Empty<byte>();

            using var stream = new MemoryStream();

            Serializer.Serialize(stream, value);

            if (!stream.TryGetBuffer(out ArraySegment<byte> buffer))
            {
                buffer = new ArraySegment<byte>(stream.ToArray());
            }

            return buffer.ToArray();
        }

        public static List<string> GetStringProtobuf(this byte[] bytes)
        {
            var reader = ProtoReader.State.Create(bytes, null, null);
            try
            {
                return WriteTree(ref reader);
            }
            finally
            {
                reader.Dispose();
            }
        }

        static List<string> WriteTree(ref ProtoReader.State reader)
        {
            var list = new List<string>();

            while (reader.ReadFieldHeader() > 0)
            {
                switch (reader.WireType)
                {
                    case WireType.Varint:
                        // warning: this appear to be wrong if the 
                        // value was written signed ("zigzag") - to
                        // read zigzag, add: pr.Hint(WireType.SignedVariant);
                        var varint = reader.ReadInt64();
                        list.Add(varint.ToString());
                        break;
                    case WireType.String:
                        // note: "string" here just means "some bytes"; could
                        // be UTF-8, could be a BLOB, could be a "packed array",
                        // or could be sub-object(s); showing UTF-8 for simplicity
                        var str = reader.ReadString();
                        str = str?.Trim();
                        if (!string.IsNullOrWhiteSpace(str))
                            list.Add(str);
                        break;
                    case WireType.Fixed32:
                        // could be an integer, but probably floating point
                        var fixed32 = reader.ReadSingle();
                        list.Add(fixed32.ToString());
                        break;
                    case WireType.Fixed64:
                        // could be an integer, but probably floating point
                        var fixed64 = reader.ReadDouble();
                        list.Add(fixed64.ToString());
                        break;
                    case WireType.StartGroup:
                        // one of 2 sub-object formats
                        var tok = reader.StartSubItem();
                        WriteTree(ref reader);
                        reader.EndSubItem(tok);
                        break;
                    default:
                        reader.SkipField();
                        break;
                }
            }

            return list;
        }

        public static List<byte[]> GetSubTypeProtobuf(this byte[] bytes)
        {
            var reader = ProtoReader.State.Create(bytes, null, null);
            try
            {
                return GetSubTypeProtobuf(ref reader);
            }
            finally
            {
                reader.Dispose();
            }
        }

        static List<byte[]> GetSubTypeProtobuf(ref ProtoReader.State reader)
        {
            var list = new List<byte[]>();

            while (reader.ReadFieldHeader() > 0)
            {
                var bytes = reader.ReadAny<byte[]>();
                list.Add(bytes);
            }

            return list;
        }

    }
}
