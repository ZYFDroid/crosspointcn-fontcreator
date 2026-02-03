using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XTEinkTools
{
    public static class Utility
    {
        /// <summary>
        /// 在控制台打印指定字形的像素呈现
        /// </summary>
        /// <param name="font">字体</param>
        /// <param name="charCode">码点</param>
        public static void PrintCharInConsole(this XTEinkFontBinary font, int charCode)
        {
            for (int y = 0; y < font.Height; y++)
            {
                Console.Write("[");
                for (int x = 0; x < font.Width; x++)
                {
                    if (font.GetPixel(charCode, x, y))
                    {
                        Console.Write("██");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.Write("]");
                Console.WriteLine();
            }
            
        }

        public static void LoadFromBitmap(this XTEinkFontBinary font, int charCode, Bitmap bmp, int sx = 0, int sy = 0, int threhold = 128) {
            for (int y = 0; y < font.Height; y++)
            {
                for (int x = 0; x < font.Width; x++)
                {
                    bool isOn = bmp.GetPixel(x + sx, y + sy).G > threhold;
                    font.SetPixel(charCode, x, y,isOn);
                }
            }
        }
        public static void LoadFromBitmap(this EPDFontBinary font, int charCode, Bitmap bmp, int sx, int sy, int sw,int sh, int threhold = 128, bool is2bit = true)
        {
            sw = Math.Min(sw, bmp.Width);
            EPDFontBinary.EPDFontBitmap fontBitmap = new EPDFontBinary.EPDFontBitmap(sh, sw, is2bit);
            if (!is2bit)
            {
                for (int y = 0; y < sh; y++)
                {
                    for (int x = 0; x < sw; x++)
                    {
                        bool isOn = bmp.GetPixel(x + sx, y + sy).G > threhold;
                        fontBitmap.setPixel(x, y, isOn ? 1 : 0);
                    }
                }
            }
            else {
                for (int y = 0; y < sh; y++)
                {
                    for (int x = 0; x < sw; x++)
                    {
                        int isOn = bmp.GetPixel(x + sx, y + sy).G / 64;
                        fontBitmap.setPixel(x, y, isOn);
                    }
                }
            }
            font.AddBitmap(charCode,fontBitmap);
                
        }


        public static void RenderToCanvas(this XTEinkFontBinary font, int charCode, Graphics dest, int sx, int sy, bool showBorder = false)
        {
            for (int y = 0; y < font.Height; y++)
            {
                for (int x = 0; x < font.Width; x++)
                {
                    bool isOn = font.GetPixel(charCode,x,y);
                    if (isOn)
                    {
                        dest.FillRectangle(Brushes.Black, sx + x, sy + y, 1, 1);

                    }
                }
            }
            if (showBorder) { dest.DrawRectangle(Pens.Green, sx, sy, font.Width - 1, font.Height - 1); }
        }


        public static Size RenderPreview(String texts,XTEinkFontBinary fontBinary,XTEinkFontRenderer renderer,Graphics target,Size screenSize,bool showBorder)
        {
            /*
            嗯，用户给我整了个大活，要编写一个模拟阅星曈字体渲染的程序，让我一步步来看看怎样编写程序。

            首先，我应该观察阅星曈是怎样渲染字体的。以下内容是根据观察进行的猜测，阅星曈会先根据需要的字体文件，判断能渲染多少行字体。
            这里就先将屏幕高度除以字体的高度，然后向下取整作为行数。
            然后看看每一行的距离为多少个像素。
            为了避免行不均匀的情况，需要让screen.Height / lineCount，计算出行高，
            抹掉小数后作为实际行高，再乘回去，并将屏幕的实际高度减去乘回去的结果，
            作为屏幕上下边距填充，这样就实现了行高的计算。

            然后，对于每一行，按照中文的字体宽度，计算出每一行最多能显示多少个字符。然后拿出行高的老套路。
            说起来，我应该将这些东西抽象成一个内部方法 int calcPosition(int outerWidth,int innerWidth, int count,int index),
            这个在计算宽高方面都会有帮助。因为用户给出的文本是王尔德童话《少年王》的一段文字，其中不包含英文，因此无需考虑英文渲染，只需要考虑中文渲染即可。
            
            在每行渲染时，每次拉取指定数量个字符，然后将字符渲染到计算好的位置上，并继续渲染下一个字符。
            如果该行字符数量不足，根据对阅星曈真机的观察结果，这一行的文字将会紧凑显示。但这里即使不紧凑显示，也不影响断行，为了简化这里就不还原这个行为。

            最后，要考虑边距问题，这里假设边距为4，然后把screen的size减一下，可能还需要对坐标进行变换。具体的宽度先写死，然后让用户自己调试。
            总结一下，代码如下：
             */

            target.Clear(Color.FromArgb(255,253,250));

            int screenPadding = 4;
            Size surface = new Size(screenSize.Width - screenPadding * 2, screenSize.Height - screenPadding * 2);

            int calcPosition(int totalWidth,int unitWidth,int count,int index)
            {
                // |--------------------------------------------------------------------|
                //   |------|   |------|
                // 应该吧
                var segSize = totalWidth / count;
                var halfSize = segSize / 2;
                var halfBegin = halfSize - unitWidth / 2;
                var offset = index* segSize;
                return halfBegin + offset;
            }

            var totalLines = surface.Height / fontBinary.Height;
            var pxPerLine = surface.Height / totalLines;
            var textAreaHeight = pxPerLine * totalLines;
            var extraSpace = surface.Height - textAreaHeight;

            var outerHeight = textAreaHeight;
            var screenPaddingTop = screenPadding + extraSpace / 2;
            // 不管了，高度算是计算好了。
            // 然后开始计算宽度

            var totalCharacter = surface.Width / fontBinary.Width;
            var pxPerChar = surface.Width / totalCharacter;
            var textAreaWidth = pxPerChar * totalCharacter;
            var extraWidthSpace = surface.Width - textAreaWidth;

            var outerWidth = textAreaWidth;
            var screenPaddingLeft = screenPadding + extraWidthSpace / 2;
            // 不管了，宽度也算是计算好了。
            
            Point getCharacterPosition(int cx,int cy)
            {
                var px = calcPosition(outerWidth, fontBinary.Width, totalCharacter, cx);
                var py = calcPosition(outerHeight, fontBinary.Height, totalLines, cy);
                return new Point(px, py);
            }

            void drawChar(char chr,int cx,int cy)
            {
                var pt = getCharacterPosition(cx,cy);
                renderer.RenderFont((int)chr, fontBinary);
                fontBinary.RenderToCanvas((int)chr,target,pt.X + screenPaddingLeft,pt.Y + screenPaddingTop,showBorder);
            }

            int currentLine = 0;
            int currentChar = 0;
            string[] lines = texts.Split(new char[] {'\r','\n'},StringSplitOptions.RemoveEmptyEntries);

            var returnVal = new Size(totalCharacter,totalLines);

            foreach (var oneLine in lines)
            {
                bool hasNoMoreCharButSwitchedLine = false;
                foreach (var oneChar in oneLine)
                {
                    hasNoMoreCharButSwitchedLine = false;
                    drawChar(oneChar,currentChar,currentLine);
                    currentChar++;
                    if(currentChar >= totalCharacter)
                    {
                        currentChar = 0;
                        currentLine++;
                        hasNoMoreCharButSwitchedLine = true;
                    }
                    if(currentLine >= totalLines)
                    {
                        return returnVal;
                    }
                }
                if (!hasNoMoreCharButSwitchedLine)
                {

                    currentLine++;
                }
                currentChar = 0;
                if (currentLine >= totalLines)
                {
                    return returnVal;
                }
            }

            return returnVal;
        }

    }
}
