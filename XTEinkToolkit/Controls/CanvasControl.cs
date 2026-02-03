using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XTEinkToolkit.Controls
{
    /// <summary>
    /// 提供一个具有固定画布尺寸的用户控件，允许开发者通过Graphics对象进行绘制操作。<br />
    /// 支持双缓冲技术，绘制完成后通过Commit方法将内容按比例缩放并居中显示在控件上，<br />
    /// 未填充区域显示控件的BackColor。
    /// </summary>
    /// <remarks>智谱清言GLM-4.5<br />
    /// 提示词：WinForm，写一个用户控件，这个控件允许开发者设置好一个特定的尺寸（CanvasSize)，<br />
    /// 然后在这个尺寸下，开发者可以获取一个对应的Graphics对象，可以在上面绘制各种东西，<br />
    /// 然后当调用Commit方法时，则将Graphics中绘制显示的内容更新到屏幕（控件）上。<br />
    /// 需要有双缓冲功能，且Graphics所依附的Bitmap在显示在屏幕上时，需要居中按比例缩放到<br />
    /// 填充符合控件的实际大小，未填充的部分则显示BackColor
    /// </remarks>
    public class CanvasControl : UserControl
    {
        private Size _canvasSize = new Size(100, 100);
        private Bitmap _canvasBitmap;
        private Graphics _canvasGraphics;
        private bool _needsCommit = true;

        public CanvasControl()
        {
            // 启用双缓冲
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            // 初始化画布
            InitializeCanvas();
        }

        [Category("Appearance")]
        [Description("设置内部画布的尺寸")]
        public Size CanvasSize
        {
            get => _canvasSize;
            set
            {
                if (_canvasSize != value)
                {
                    _canvasSize = value;
                    InitializeCanvas();
                    Invalidate();
                }
            }
        }

        public enum RenderScaleMode
        {
            Center,Zoom,PreferCenter
        }

        private RenderScaleMode _scaleMode = RenderScaleMode.Zoom;

        [Category("Appearance")]
        [Description("设置内部画布的尺寸")]
        public RenderScaleMode ScaleMode
        {
            get => _scaleMode;
            set
            {
                if (_scaleMode != value)
                {
                    _scaleMode = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// 获取用于绘制的Graphics对象
        /// </summary>
        public Graphics GetGraphics()
        {
            if (_canvasGraphics == null)
            {
                InitializeCanvas();
            }
            return _canvasGraphics;
        }

        /// <summary>
        /// 提交绘制内容到屏幕显示
        /// </summary>
        public void Commit()
        {
            _needsCommit = false;
            Invalidate();
            Update(); // 强制立即重绘
        }

        private void InitializeCanvas()
        {
            // 释放旧资源
            _canvasGraphics?.Dispose();
            _canvasBitmap?.Dispose();

            // 创建新画布
            _canvasBitmap = new Bitmap(_canvasSize.Width, _canvasSize.Height);
            _canvasGraphics = Graphics.FromImage(_canvasBitmap);

            // 设置高质量渲染
            _canvasGraphics.SmoothingMode = SmoothingMode.HighSpeed;
            _canvasGraphics.InterpolationMode = InterpolationMode.Low;
            _canvasGraphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            // 初始清除为透明
            _canvasGraphics.Clear(Color.Transparent);

            _needsCommit = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 清除控件背景
            e.Graphics.Clear(BackColor);

            if (_canvasBitmap == null) return;

            // 计算缩放比例和位置
            float scale = 1;

            bool needZoom = _scaleMode == RenderScaleMode.Zoom;
            if (_scaleMode == RenderScaleMode.PreferCenter)
            {
                if(this.ClientSize.Width < _canvasSize.Width || this.ClientSize.Height < _canvasSize.Height)
                {
                    needZoom = true;
                }
            }

            if (needZoom)
            {
                scale = Math.Min(
                (float)ClientSize.Width / _canvasSize.Width,
                (float)ClientSize.Height / _canvasSize.Height);
            }
            int scaledWidth = (int)(_canvasSize.Width * scale);
            int scaledHeight = (int)(_canvasSize.Height * scale);

            int x = (ClientSize.Width - scaledWidth) / 2;
            int y = (ClientSize.Height - scaledHeight) / 2;

            // 绘制缩放后的图像
            e.Graphics.DrawImage(
                _canvasBitmap,
                new Rectangle(x, y, scaledWidth, scaledHeight),
                new Rectangle(0, 0, _canvasSize.Width, _canvasSize.Height),
                GraphicsUnit.Pixel);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate(); // 大小改变时重绘
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _canvasGraphics?.Dispose();
                _canvasBitmap?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
