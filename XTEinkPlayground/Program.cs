using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XTEinkTools;

namespace XTEinkPlayground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DrawGlyph("霞鹜文楷.epdfont", '.');
            Console.ReadLine();
        }
        /// <summary>
        /// 从二进制字体文件中读取并绘制指定字符码的字形。
        /// </summary>
        /// <param name="filePath">.epdfont 文件路径</param>
        /// <param name="charCode">要绘制的 Unicode 字符码</param>
        public static void DrawGlyph(string filePath, int charCode)
        {
            Console.WriteLine(charCode);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"文件未找到: {filePath}");
                return;
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                // --- 1. 读取并验证文件头 (48 bytes) ---
                byte[] magic = br.ReadBytes(4);
                string magicStr = System.Text.Encoding.ASCII.GetString(magic);
                if (magicStr != "EPDF")
                {
                    Console.WriteLine("无效的文件格式 (Magic mismatch)");
                    return;
                }

                uint intervalCount = br.ReadUInt32();       // 4
                /* uint fileSize = */
                br.ReadUInt32();       // 8
                /* uint height = */
                br.ReadUInt32();         // 12
                uint glyphCount = br.ReadUInt32();          // 16
                /* int ascender = */
                br.ReadInt32();         // 20
                br.ReadInt32();                              // 24 (Reserved)
                /* int descender = */
                br.ReadInt32();        // 28
                uint is2BitFlag = br.ReadUInt32();           // 32 (1 for 2-bit, 0 for 1-bit)
                bool is2Bit = (is2BitFlag == 1);

                uint offsetIntervals = br.ReadUInt32();     // 36
                uint offsetGlyphs = br.ReadUInt32();         // 40
                uint offsetBitmaps = br.ReadUInt32();       // 44

                // --- 2. 查找字符所在的区间以获取字形索引 ---
                int glyphIndex = -1;
                fs.Seek(offsetIntervals, SeekOrigin.Begin);

                for (uint i = 0; i < intervalCount; i++)
                {
                    uint start = br.ReadUInt32();
                    uint end = br.ReadUInt32();
                    uint indexOffset = br.ReadUInt32();

                    if (charCode >= start && charCode <= end)
                    {
                        // 计算字形在 Glyphs 数组中的索引
                        glyphIndex = (int)(indexOffset + (charCode - start));
                        break;
                    }
                }

                if (glyphIndex == -1)
                {
                    Console.WriteLine($"字符码 U+{charCode:X4} 未在字体区间中找到。");
                    return;
                }

                if (glyphIndex >= glyphCount)
                {
                    Console.WriteLine($"计算出的字形索引 {glyphIndex} 超出范围。");
                    return;
                }

                // --- 3. 读取字形属性 (13 bytes per glyph) ---
                fs.Seek((long)(offsetGlyphs + (ulong)glyphIndex * 13), SeekOrigin.Begin);

                byte width = br.ReadByte();        // 1
                byte height = br.ReadByte();       // 1
                byte advanceX = br.ReadByte();     // 1
                sbyte left = br.ReadSByte();       // 1
                br.ReadByte();                     // 1 (padding)
                sbyte top = br.ReadSByte();        // 1
                br.ReadByte();                     // 1 (padding)
                ushort dataLength = br.ReadUInt16(); // 2
                uint dataOffset = br.ReadUInt32();   // 4

                // --- 4. 读取位图数据 ---
                fs.Seek(offsetBitmaps + dataOffset, SeekOrigin.Begin);
                byte[] bitmapData = br.ReadBytes(dataLength);

                // --- 5. 控制台模拟绘制 ---
                // 为了处理 top (Y轴偏移)，我们可能在上方打印空行，
                // 但为了简单直观，这里主要绘制字形位图本身。

                // 我们可以简单的根据 top 属性打印一些空格或空行来表示位置，但这里聚焦于字形内容。

                Console.WriteLine($"绘制字符: U+{charCode:X4} (索引: {glyphIndex})");
                Console.WriteLine($"尺寸: {width}x{height}, 偏移: ({left}, {top}), 格式: {(is2Bit ? "2-bit" : "1-bit")}");
                Console.WriteLine(new string('-', width + 2));

                for (int y = 0; y < height; y++)
                {
                    Console.Write("|"); // 左边框
                    for (int x = 0; x < width; x++)
                    {
                        int pixelValue = GetPixelValue(bitmapData, width, x, y, is2Bit);
                        Console.Write(GetCharForPixel(pixelValue, is2Bit));
                    }
                    Console.WriteLine("|"); // 右边框
                }
                Console.WriteLine(new string('-', width + 2));
            }
        }

        /// <summary>
        /// 根据位图数据获取指定坐标的像素值。
        /// </summary>
        private static int GetPixelValue(byte[] bitmap, int width, int x, int y, bool is2Bit)
        {
            int totalPixelIndex = y * width + x;

            if (is2Bit)
            {
                // 2-bit 格式: 每个像素 2 bits，一个 byte 包含 4 个像素
                // Python 逻辑: px = px << 2; ... pixels2b.append(px);
                // 这意味着高位在前。Byte: [P0(7-6)][P1(5-4)][P2(3-2)][P3(1-0)]
                int byteIndex = totalPixelIndex / 4;
                int bitOffset = 6 - ((totalPixelIndex % 4) * 2); // 6, 4, 2, 0

                if (byteIndex >= bitmap.Length) return 0;
                return (bitmap[byteIndex] >> bitOffset) & 0x03;
            }
            else
            {
                // 1-bit 格式: 每个像素 1 bit，一个 byte 包含 8 个像素
                // Python 逻辑: px = px << 1; ...
                // 这意味着高位在前。Byte: [P0...P7]
                int byteIndex = totalPixelIndex / 8;
                int bitOffset = 7 - (totalPixelIndex % 8);

                if (byteIndex >= bitmap.Length) return 0;
                return (bitmap[byteIndex] >> bitOffset) & 0x01;
            }
        }

        /// <summary>
        /// 将像素值映射为控制台字符。
        /// </summary>
        private static string GetCharForPixel(int value, bool is2Bit)
        {
            if (is2Bit)
            {
                // 2-bit 灰度模拟: 0=空, 1=░, 2=▒, 3=█
                switch (value)
                {
                    case 0: return "  ";
                    case 1: return "11";
                    case 2: return "88";
                    case 3: return "██";
                    default: return "??";
                }
            }
            else
            {
                // 1-bit 单色: 0=空, 1=@
                return value == 1 ? "██" : "  ";
            }
        }
    }




}
