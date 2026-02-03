using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XTEinkTools
{
    /// <summary>
    /// 字形渲染器
    /// </summary>
    public class XTEinkFontRenderer : IDisposable
    {

        public enum AntiAltasMode
        {
            System1BitGridFit,
            System1Bit,
            SystemAntiAltasGridFit,
            SystemAntiAltas,
        }

        public AntiAltasMode AAMode { get; set; } = AntiAltasMode.System1BitGridFit;

        public int LightThrehold { get; set; } = 128;
        public int LineSpacingPx { get; set; } = 0;
        public int CharSpacingPx { get; set; } = 0;
        public bool IsVerticalFont { get; set; } = false;
        public Font Font { get; set; } = SystemFonts.DefaultFont;
        public bool IsOldLineAlignment { get; set; }
        public bool RenderBorder { get; set; }

        private Bitmap _tempRenderSurface;
        private Graphics _tempGraphics;

        public delegate void RenderMethod(int x, int y, bool pixel);

        public Size GetFontRenderSize(char chr = '坐')
        {
            if(chr == ' ') { chr = 'l'; }
            using (Bitmap bmp = new Bitmap(32, 32))
            {
                bmp.SetResolution(96, 96);
                using(Graphics g = Graphics.FromImage(bmp))
                {
                    StringFormat strfmt = new StringFormat(StringFormat.GenericTypographic);
                    // strfmt.FormatFlags |= StringFormatFlags.FitBlackBox | StringFormatFlags.NoClip;
                    SizeF sf = g.MeasureString(chr.ToString(), this.Font,999,strfmt); 
                    //if (IsVerticalFont)
                    //{
                    //    sf = new SizeF(sf.Height, sf.Width);
                    //}
                    Size s = new Size((int)Math.Round(sf.Width) + this.CharSpacingPx, (int)Math.Round(sf.Height) + this.LineSpacingPx);
                    if(s.Height < 5)
                    {
                        s.Height = 5;
                        // LineSpacingPx = (int)(sf.Height - 5);
                    }
                    if (s.Width < 5)
                    {
                        s.Width = 5;
                        // CharSpacingPx = (int)(sf.Width - 5);
                    }
                    return s;
                }
            }

        }

        StringFormat _format = new StringFormat(StringFormat.GenericTypographic)
        {
        };

        private void ensureRenderSurfaceSize(int width,int height)
        {

            if (this._tempRenderSurface != null && this._tempGraphics != null) {
                if (this._tempRenderSurface.Width == width && this._tempRenderSurface.Height == height)
                {
                    return;
                }
            }
            
            try
            {
                this._tempGraphics?.Dispose();
            }
            catch { }
            try
            {
                this._tempRenderSurface?.Dispose();
            }catch { }

            this._tempRenderSurface = new Bitmap(width, height);
            this._tempRenderSurface.SetResolution(96, 96);
            this._tempGraphics = Graphics.FromImage(this._tempRenderSurface);
            this._tempGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            this._tempGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;
            this._tempGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
        }

        private void syncSettings()
        {
            if (this._tempGraphics != null) {
                TextRenderingHint targetHint = TextRenderingHint.SingleBitPerPixelGridFit;
                switch (AAMode)
                {
                    case AntiAltasMode.System1BitGridFit:
                        targetHint = TextRenderingHint.SingleBitPerPixelGridFit; break;
                    case AntiAltasMode.System1Bit:
                        targetHint = TextRenderingHint.SingleBitPerPixel; break;
                    case AntiAltasMode.SystemAntiAltasGridFit:
                        targetHint = TextRenderingHint.AntiAliasGridFit; break;
                    case AntiAltasMode.SystemAntiAltas:
                        targetHint = TextRenderingHint.AntiAlias; break;
                }
                if(this._tempGraphics.TextRenderingHint != targetHint)
                {
                    this._tempGraphics.TextRenderingHint = targetHint;
                }
                if (IsVerticalFont)
                {
                    _format.FormatFlags |= StringFormatFlags.DirectionVertical;
                }
            }
            
        }

        public void RenderFont(int charCodePoint,EPDFontBinary renderer, int height, int width)
        {
            // 

            char chr = (char)charCodePoint;
            ensureRenderSurfaceSize(width, height);
            var bitmapSize = this.GetFontRenderSize(chr);
            syncSettings();

            this._tempGraphics.Clear(Color.Black);


            this._tempGraphics.ResetTransform();
            if (RenderBorder)
            {
                this._tempGraphics.DrawRectangle(Pens.White, 0, 0, bitmapSize.Width - 1, bitmapSize.Height - 1);
            }
            if (IsVerticalFont)
            {
                this._tempGraphics.TranslateTransform(0, bitmapSize.Height);
                this._tempGraphics.RotateTransform(-90);
            }

            bool shouldSetLineAlignmentToCenter = true;


            if (IsOldLineAlignment)
            {
                shouldSetLineAlignmentToCenter = LineSpacingPx < 0;
            }
            if (shouldSetLineAlignmentToCenter)
            {
                if (IsVerticalFont)
                {

                    this._tempGraphics.TranslateTransform(LineSpacingPx / 2, 0);
                }
                else
                {

                    this._tempGraphics.TranslateTransform(0, LineSpacingPx / 2);
                }
            }
            if (CharSpacingPx != 0 && charCodePoint > 255)
            {
                if (IsVerticalFont)
                {

                    this._tempGraphics.TranslateTransform(0, CharSpacingPx / 2);
                }
                else
                {

                    this._tempGraphics.TranslateTransform(CharSpacingPx / 2,0);
                }
            }

            this._tempGraphics.DrawString(chr.ToString(), this.Font, Brushes.White, 0,0, _format);
            
            renderer.LoadFromBitmap(charCodePoint, _tempRenderSurface, 0, 0,bitmapSize.Width,height, this.LightThrehold,renderer.Is2Bit);
        }

        public void RenderFont(int charCodePoint, XTEinkFontBinary renderer)
        {
            ensureRenderSurfaceSize(renderer.Width, renderer.Height);
            syncSettings();
            char chr = (char)charCodePoint;
            this._tempGraphics.Clear(Color.Black);

            this._tempGraphics.ResetTransform();
            if (RenderBorder)
            {
                this._tempGraphics.DrawRectangle(Pens.White, 0, 0, renderer.Width - 1, renderer.Height - 1);
            }
            if (IsVerticalFont)
            {
                this._tempGraphics.TranslateTransform(0, renderer.Height);
                this._tempGraphics.RotateTransform(-90);
            }

            bool shouldSetLineAlignmentToCenter = true;


            if (IsOldLineAlignment)
            {
                shouldSetLineAlignmentToCenter = LineSpacingPx < 0;
            }
            if (shouldSetLineAlignmentToCenter)
            {
                if (IsVerticalFont)
                {

                    this._tempGraphics.TranslateTransform(LineSpacingPx / 2, 0);
                }
                else
                {

                    this._tempGraphics.TranslateTransform(0, LineSpacingPx / 2);
                }
            }
            if (CharSpacingPx != 0 && charCodePoint > 255)
            {
                if (IsVerticalFont)
                {

                    this._tempGraphics.TranslateTransform(0, CharSpacingPx / 2);
                }
                else
                {

                    this._tempGraphics.TranslateTransform(CharSpacingPx / 2, 0);
                }
            }

            this._tempGraphics.DrawString(chr.ToString(), this.Font, Brushes.White, 0, 0, _format);

            renderer.LoadFromBitmap(charCodePoint, _tempRenderSurface, 0, 0, this.LightThrehold);
        }

        void IDisposable.Dispose()
        {
            try
            {
                this._tempGraphics?.Dispose();
            }
            catch { }
            try
            {
                this._tempRenderSurface?.Dispose();
            }
            catch { }

        }
    }
}
