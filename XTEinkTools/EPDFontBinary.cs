using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XTEinkTools
{
    public class EPDFontBinary
    {
        private class GlyphInfo
        {
            public int CharCode;
            public EPDFontBitmap Bitmap;
            public byte[] Data; // 打包后的二进制数据
            public int DataOffset; // 在 Bitmap 数据块中的偏移量
        }

        private readonly Dictionary<int, GlyphInfo> _glyphs = new Dictionary<int, GlyphInfo>();
        private readonly int _height;
        private readonly bool _is2Bit;

        public int Height { get { return _height; } }

        public bool Is2Bit { get { return _is2Bit; } }

        public EPDFontBinary(int height, bool is2Bit)
        {
            _height = height;
            _is2Bit = is2Bit;
        }

        /// <summary>
        /// 添加一个新的bmp。
        /// </summary>
        public void AddBitmap(int charCode, EPDFontBitmap bmp)
        {
            // 验证输入参数
            if (bmp.Height != _height)
            {
                throw new ArgumentException($"Bitmap height ({bmp.Height}) does not match font height ({_height}).");
            }
            if (bmp.Is2Bit != _is2Bit)
            {
                throw new ArgumentException($"Bitmap 2-bit mode ({bmp.Is2Bit}) does not match font mode ({_is2Bit}).");
            }

            // 预计算打包后的数据，用于后续统计大小和写入
            byte[] packedData = bmp.GetPackedBytes();

            _glyphs[charCode] = new GlyphInfo
            {
                CharCode = charCode,
                Bitmap = bmp,
                Data = packedData
            };
        }

        /// <summary>
        /// 获取所有需要渲染的字体码点。
        /// </summary>
        public static IEnumerable<int> GetAllRequiredCharPoints()
        {
            foreach (var item in intervals)
            {
                for (int i = item.Item1; i <= item.Item2; i++)
                {
                    yield return i;
                }
            }
        }
        static (int, int)[] intervals = new (int, int)[]{
    // *** Basic Latin // ***
    // 大一桶，管你什么符号统统往里面塞
    (0x0000, 0xFFFF),
    (0x20000, 0x2A6DF),
    // Extension C–F (扩展C-F区汉字)
    (0x2A700, 0x2EBEF),
    // Extension G (扩展G区汉字)
    (0x30000, 0x3134F),
    
};
        /// <summary>
        /// 将字体文件保存到指定的文件流中。
        /// </summary>
        public void SaveTo(Stream outputStream)
        {
            // 1. 准备数据
            // 对码点进行排序，确保生成的文件中字形是有序的
            var sortedCodes = _glyphs.Keys.OrderBy(c => c).ToList();

            // 合并区间
            var intervals = MergeIntervals(sortedCodes);

            // 2. 计算各部分大小和偏移量
            // 文件头: 48 字节
            int headerSize = 48;
            // 区间表: 每个区间 12 字节 (3 * uint32)
            int intervalsSize = intervals.Count * 12;
            // 字形属性表: 每个字形 13 字节
            int glyphsSize = sortedCodes.Count * 13;
            // 位图数据块
            int bitmapsSize = sortedCodes.Sum(c => _glyphs[c].Data.Length);

            // 计算偏移量
            int offsetIntervals = headerSize;
            int offsetGlyphs = offsetIntervals + intervalsSize;
            int offsetBitmaps = offsetGlyphs + glyphsSize;
            int fileSize = offsetBitmaps + bitmapsSize;

            // 3. 开始写入二进制流
            using (var writer = new BinaryWriter(outputStream))
            {
                // --- 写入文件头 (48 bytes) ---
                writer.Write(System.Text.Encoding.ASCII.GetBytes("EPDF")); // 0x00: Magic
                writer.Write((uint)intervals.Count);                         // 0x04: IntervalCount
                writer.Write((uint)fileSize);                                // 0x08: FileSize
                writer.Write((uint)_height);                                // 0x0C: Height
                writer.Write((uint)sortedCodes.Count);                       // 0x10: GlyphCount
                writer.Write((int)0);                                        // 0x14: Ascender (设为0)
                writer.Write((int)0);                                        // 0x18: Reserved
                writer.Write((int)0);                                        // 0x1C: Descender (设为0)
                writer.Write((uint)(_is2Bit ? 1 : 0));                       // 0x20: Is2Bit
                writer.Write((uint)offsetIntervals);                         // 0x24: OffsetIntervals
                writer.Write((uint)offsetGlyphs);                            // 0x28: OffsetGlyphs
                writer.Write((uint)offsetBitmaps);                           // 0x2C: OffsetBitmaps

                // --- 写入区间表 ---
                // IndexOffset 指向该区间第一个字符在 Glyph 表中的索引
                int currentGlyphIndex = 0;
                foreach (var interval in intervals)
                {
                    writer.Write((uint)interval.Start);       // Start
                    writer.Write((uint)interval.End);         // End
                    writer.Write((uint)currentGlyphIndex);   // IndexOffset (相对于Glyph表的下标)

                    // 更新索引，指向下一个区间的起始位置
                    currentGlyphIndex += (interval.End - interval.Start + 1);
                }

                // --- 写入字形属性表并计算位图偏移 ---
                int currentBitmapOffset = 0;
                foreach (var code in sortedCodes)
                {
                    var info = _glyphs[code];
                    info.DataOffset = currentBitmapOffset; // 记录偏移供写入位图时使用，这里其实也可以直接写

                    int width = info.Bitmap.Width;
                    int height = info.Bitmap.Height;
                    int dataLength = info.Data.Length;

                    // 结构: width, height, advance_x, left, padding, top, padding, data_length, data_offset
                    writer.Write((byte)width);          // 0x00: width
                    writer.Write((byte)height);         // 0x01: height
                    writer.Write((byte)width);          // 0x02: advance_x (简化逻辑：取width)
                    writer.Write((sbyte)0);             // 0x03: left (简化逻辑：取0)
                    writer.Write((byte)0);              // 0x04: padding
                    writer.Write((sbyte)0);             // 0x05: top (简化逻辑：取0)
                    writer.Write((byte)0);              // 0x06: padding
                    writer.Write((ushort)dataLength);   // 0x07: data_length
                    writer.Write((uint)currentBitmapOffset); // 0x09: data_offset

                    currentBitmapOffset += dataLength;
                }

                // --- 写入位图数据块 ---
                foreach (var code in sortedCodes)
                {
                    writer.Write(_glyphs[code].Data);
                }
            }
        }

        // 辅助方法：合并连续的 Unicode 区间
        private List<(int Start, int End)> MergeIntervals(List<int> sortedCodes)
        {
            var result = new List<(int, int)>();
            if (sortedCodes.Count == 0) return result;

            int start = sortedCodes[0];
            int prev = sortedCodes[0];

            for (int i = 1; i < sortedCodes.Count; i++)
            {
                int curr = sortedCodes[i];
                // 如果当前码点和上一个码点是连续的，则扩展当前区间
                if (curr == prev + 1)
                {
                    prev = curr;
                }
                else
                {
                    // 否则，结束上一个区间，开始新区间
                    result.Add((start, prev));
                    start = curr;
                    prev = curr;
                }
            }
            // 添加最后一个区间
            result.Add((start, prev));

            return result;
        }

        public class EPDFontBitmap
        {
            private readonly byte[,] _pixels; // 存储像素数据
            public int Height { get; }
            public int Width { get; }
            public bool Is2Bit { get; }

            public EPDFontBitmap(int height, int width, bool is2Bit)
            {
                Height = height;
                Width = width;
                Is2Bit = is2Bit;
                _pixels = new byte[width, height];
            }

            public void setPixel(int x, int y, int value)
            {
                if (x >= 0 && x < Width && y >= 0 && y < Height)
                {
                    _pixels[x, y] = (byte)value;
                }
            }

            // 旧的代码库里抄过来的辅助方法
            private readonly byte[] bitMask = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x8, 0x4, 0x2, 0x1 };

            private byte SetBitAt(byte b, int p, bool value)
            {
                if (value)
                {
                    return (byte)(b | bitMask[p]);
                }
                else
                {
                    return (byte)(b & (~bitMask[p]));
                }
            }

            // 将二维像素数据打包成 EPDFont 需要的一维字节数组
            public byte[] GetPackedBytes()
            {
                if (Is2Bit)
                {
                    return Pack2Bit();
                }
                else
                {
                    return Pack1Bit();
                }
            }

            private byte[] Pack1Bit()
            {
                // 计算总字节数：不需要按行对齐，而是总像素数向上取整
                int totalPixels = Width * Height;
                int totalBytes = (totalPixels + 7) / 8;
                byte[] result = new byte[totalBytes];

                int currentByteIndex = 0;
                byte currentByte = 0;
                int bitIndex = 0; // 0-7, 对应 bitMask 的索引

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        bool isBlack = _pixels[x, y] > 0;

                        // 使用位掩码设置当前位
                        if (isBlack)
                        {
                            currentByte = (byte)(currentByte | bitMask[bitIndex]);
                        }
                        else
                        {
                            currentByte = (byte)(currentByte & (~bitMask[bitIndex]));
                        }

                        bitIndex++;

                        // 只有当攒够8位时才写入字节并重置，而不是在每行结束时
                        if (bitIndex == 8)
                        {
                            result[currentByteIndex++] = currentByte;
                            currentByte = 0;
                            bitIndex = 0;
                        }
                    }
                    // 注意：这里不再处理行尾未填满的情况，也不重置 currentByte 和 bitIndex
                }

                // 处理最后剩余的不足8位的数据
                if (bitIndex != 0)
                {
                    result[currentByteIndex] = currentByte;
                }

                return result;
            }

            private byte[] Pack2Bit()
            {
                // 计算总字节数：每4个像素(2bit * 4)一个字节，不需要按行对齐
                int totalPixels = Width * Height;
                int totalBytes = (totalPixels + 3) / 4;
                byte[] result = new byte[totalBytes];

                int currentByteIndex = 0;
                byte currentByte = 0;
                int pixelCount = 0; // 0-3, 表示当前字节已存入的像素数

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        byte val = (byte)(_pixels[x, y] & 0x03);

                        // 将像素移位放入当前字节的高位
                        // pixelCount 0 -> shift 6 (bits 7-6)
                        // pixelCount 1 -> shift 4 (bits 5-4)
                        // ...
                        currentByte |= (byte)(val << (6 - pixelCount * 2));

                        pixelCount++;

                        // 只有当攒够4个像素时才写入字节并重置
                        if (pixelCount == 4)
                        {
                            result[currentByteIndex++] = currentByte;
                            currentByte = 0;
                            pixelCount = 0;
                        }
                    }
                    // 注意：这里不再处理行尾未填满的情况，也不重置
                }

                // 处理最后剩余的不足4个像素的数据
                if (pixelCount != 0)
                {
                    result[currentByteIndex] = currentByte;
                }

                return result;
            }

        }
    }
}
