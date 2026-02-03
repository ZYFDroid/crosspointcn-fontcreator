using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using XTEinkToolkit.Controls;
using XTEinkTools;

namespace XTEinkToolkit
{

    public partial class FrmMain : Form
    {

        private PrivateFontCollection privateFont;

        public FrmMain()
        {
            InitializeComponent();
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }
        }

        bool fontSelected = false;

        private static Size XTScreenSize = new Size(480, 800);

        private static Size SwapDirection(Size s)
        {
            return new Size(s.Height, s.Width);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //if (!EULADialog.ShowDialog(this, FrmMainCodeString.dlgEULAContent, FrmMainCodeString.dlgEULATitle, "eula_v1"))
            //{
            //    Application.Exit();
            //    return;
            //}

            previewSurface.ScaleMode = XTEinkToolkit.Controls.CanvasControl.RenderScaleMode.PreferCenter;
            previewSurface.CanvasSize = new System.Drawing.Size(480, 800);
            chkTraditionalChinese.Checked = FrmMainCodeString.boolShowTCPreview.Contains("true");
            DoPreview();
        }

        private void btnSelectFont_Click(object sender, EventArgs e)
        {
            //if (!AutoConfirmDialog.ShowDialog(this, FrmMainCodeString.dlgConfirmSelectSystemFont, FrmMainCodeString.dlgConfirmSelectFontTitle, FrmMainCodeString.dlgConfirmSelectFontNeverAsk, "flagAllowFontAccess"))
            //{
            //    return;
            //}

            fontDialog.Font = lblFontSource.Font;

            if (fontDialog.ShowDialog(this) == DialogResult.OK)
            {
                lblFontSource.Font = fontDialog.Font;
                lblFontSource.Text = fontDialog.Font.Name + "\r\n" + FrmMainCodeString.abcFontPreviewText;
                numFontSizePt.ValueChanged -= numFontSizePt_ValueChanged;
                numFontSizePt.Value = (decimal)fontDialog.Font.Size;
                numFontSizePt.ValueChanged += numFontSizePt_ValueChanged;
                btnDoGeneration.Enabled = true;
            }
            DoPreview();
        }


        private void btnChooseFontFile_Click(object sender, EventArgs e)
        {
            //if (!AutoConfirmDialog.ShowDialog(this, FrmMainCodeString.dlgConfirmSelectFontFile, FrmMainCodeString.dlgConfirmSelectFontTitle, FrmMainCodeString.dlgConfirmSelectFontNeverAsk, "flagAllowFontFileAccess"))
            //{
            //    return;
            //}
            if (DlgSelectCustomFont.ShowSelectDialog(this, out var pfc, out var fnt))
            {
                lblFontSource.Font = fnt;
                privateFont?.Dispose();
                privateFont = pfc;

                lblFontSource.Text = lblFontSource.Font.Name + "\r\n" + FrmMainCodeString.abcFontPreviewText;

                numFontSizePt.ValueChanged -= numFontSizePt_ValueChanged;
                numFontSizePt.Value = (decimal)lblFontSource.Font.Size;
                numFontSizePt.ValueChanged += numFontSizePt_ValueChanged;
                btnDoGeneration.Enabled = true;
                DoPreview();
            }
        }

        private void btnPreview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DoPreviewDirect();
        }

        private void numFontSizePt_ValueChanged(object sender, EventArgs e)
        {
            float newSize = (float)numFontSizePt.Value;
            Font oldFont = lblFontSource.Font;
            lblFontSource.Font = new Font(oldFont.FontFamily, newSize, oldFont.Style);
            oldFont.Dispose();
            DoPreview();
        }
        private void numFontGamma_ValueChanged(object sender, EventArgs e)
        {
            DoPreview();
        }

        private void numLineSpacing_ValueChanged(object sender, EventArgs e)
        {

            DoPreview();
        }


        private void chkRenderGridFit_CheckedChanged(object sender, EventArgs e)
        {
            numFontGamma.Enabled = chkRenderAntiAltas.Checked;
            DoPreview();
        }


        void DoPreview()
        {
            debounceCd = 18;
            debounceTimer.Enabled = true;
        }

        private int debounceCd = 0;
        private void debounceTimer_Tick(object sender, EventArgs e)
        {
            debounceCd--;
            if (debounceCd < 0)
            {
                debounceTimer.Enabled = false;
                DoPreviewDirect();
            }
        }
        string previewStringSC = Properties.Resources.previewTexts;
        string previewStringTC = Properties.Resources.previewTestTC;
        void DoPreviewDirect()
        {
            debounceTimer.Enabled = false;
            btnPreview.Enabled = false;
            btnPreview.Text = FrmMainCodeString.abcRenderingPreview;
            Application.DoEvents();
            using (XTEinkFontRenderer renderer = new XTEinkFontRenderer())
            {
                ConfigureRenderer(renderer);
                Size fontRenderSize = renderer.GetFontRenderSize('坐');

                var screenSize = XTScreenSize;
                var rotatedScreenSize = chkLandspace.Checked ? SwapDirection(screenSize) : screenSize;

                var previewSize = rotatedScreenSize;
                if (chkVerticalFont.Checked)
                {
                    previewSize = SwapDirection(previewSize);
                }

                previewSurface.CanvasSize = previewSize; ;
                XTEinkFontBinary fontBinary = new XTEinkFontBinary(fontRenderSize.Width, fontRenderSize.Height);
                var g = previewSurface.GetGraphics();
                g.ResetTransform();
                if (chkVerticalFont.Checked)
                {
                    g.TranslateTransform(previewSize.Width, 0);
                    g.RotateTransform(90);
                }

                string previewString = chkTraditionalChinese.Checked ? previewStringTC : previewStringSC;
                if (chkShowENCharacter.Checked)
                {
                    previewString = FrmMainCodeString.abcPreviewEN;
                }
                var size = Utility.RenderPreview(previewString, fontBinary, renderer, g, rotatedScreenSize, chkShowBorder.Checked);
                lblPreviewMessage.Text = string.Format(FrmMainCodeString.abcPreviewParameters, size.Height, size.Width, size.Height * size.Width, fontRenderSize.Width, fontRenderSize.Height).Trim();
                previewSurface.Commit();
            }
            GC.Collect();
            btnPreview.Enabled = true;
            btnPreview.Text = FrmMainCodeString.abcBtnPreviewText;
        }

        private void ConfigureRenderer(XTEinkFontRenderer renderer)
        {
            renderer.Font = lblFontSource.Font;
            renderer.LineSpacingPx = (int)numLineSpacing.Value;
            renderer.LightThrehold = numFontGamma.Value;
            renderer.IsVerticalFont = chkVerticalFont.Checked;
            renderer.IsOldLineAlignment = chkOldLineAlignment.Checked;
            renderer.RenderBorder = chkShowBorderInBinaryFont.Checked;
            XTEinkFontRenderer.AntiAltasMode[] aaModesEnum = new XTEinkFontRenderer.AntiAltasMode[] {
                    XTEinkFontRenderer.AntiAltasMode.System1Bit, // 0x0
                    XTEinkFontRenderer.AntiAltasMode.System1BitGridFit, // 0x1
                    XTEinkFontRenderer.AntiAltasMode.SystemAntiAltas, // 0x2
                    XTEinkFontRenderer.AntiAltasMode.SystemAntiAltasGridFit //0x3
                };
            var whichAAMode = (chkRenderAntiAltas.Checked ? 2 : 0) + (chkRenderGridFit.Checked ? 1 : 0);
            renderer.CharSpacingPx = (int)numCharSpacing.Value;
            renderer.AAMode = aaModesEnum[whichAAMode];
        }

        private string GetRenderTargetSize()
        {
            using (XTEinkFontRenderer renderer = new XTEinkFontRenderer())
            {

                ConfigureRenderer(renderer);
                Size fontRenderSize = renderer.GetFontRenderSize();
                return fontRenderSize.Width + "×" + fontRenderSize.Height;
            }
        }


        /// <summary>
        /// 检查程序是否运行在阅星曈SD卡目录下或者已经插入阅星曈的SD卡
        /// </summary>
        /// <returns>如果是则返回字体文件夹路径，否则返回null</returns>
        private string GetXTSDCardPath()
        {
            return null;
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                string rootPath = System.IO.Path.GetPathRoot(appPath);

                // 检查程序所在驱动器是否存在XTCache文件夹
                string xtCachePath = System.IO.Path.Combine(rootPath, "XTCache");
                if (System.IO.Directory.Exists(xtCachePath))
                {
                    var fontPath = Path.Combine(rootPath, "Fonts");

                    return fontPath;
                }

                // 检查可移动磁盘根目录是否存在XTCache文件夹
                foreach (var drive in System.IO.DriveInfo.GetDrives())
                {
                    if (drive.DriveType == System.IO.DriveType.Removable && drive.IsReady)
                    {
                        xtCachePath = System.IO.Path.Combine(drive.Name, "XTCache");
                        if (System.IO.Directory.Exists(xtCachePath))
                        {
                            var fontPath = Path.Combine(drive.Name, "Fonts");

                            return fontPath;
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        private void requireUpdatePreview(object sender, EventArgs e)
        {
            DoPreview();
        }

        private void btnAdvancedOptions_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            mnuAdvancedOptions.Show(btnAdvancedOptions, 0, btnAdvancedOptions.Height);
        }

        private void btnDoGeneration_Click(object sender, EventArgs e)
        {
            //if (!EULADialog.ShowDialog(this, FrmMainCodeString.dlgEULA2Content, FrmMainCodeString.dlgEULA2Title, "fonteula_v1"))
            //{
            //    return;
            //}

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = (FrmMainCodeString.abcSaveDialogTypeName.Trim()) + "|*.epdfont";

            string fontName = lblFontSource.Font.Name;
            fontName = Regex.Replace(fontName, "×", "x");
            string targetSize = GetRenderTargetSize();
            string fontSize = lblFontSource.Font.SizeInPoints.ToString("F2").TrimEnd('0').TrimEnd('.'); // 移除尾随的0
            string suggestedFileName = $"{fontName}.epdfont";

            sfd.FileName = suggestedFileName;

            var xtsdPath = GetXTSDCardPath();
            sfd.Title = FrmMainCodeString.dlgSaveFileDialogTitle;

            if (xtsdPath != null && AutoConfirmDialog.ShowDialog(this, FrmMainCodeString.dlgXTSDExistsDialogText + "\r\n" + xtsdPath, FrmMainCodeString.dlgXTSDExistsDialogTitle, FrmMainCodeString.dlgXTSDExistsDialogCheckBox, "save_to_xtsd"))
            {
                if (xtsdPath != null)
                {
                    if (!Directory.Exists(xtsdPath))
                    {
                        Directory.CreateDirectory(xtsdPath);
                    }
                    sfd.InitialDirectory = xtsdPath;
                    sfd.Title += FrmMainCodeString.dlgSaveFileDialogTitleExtra;
                }
            }
            else
            {
                xtsdPath = null;
            }
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            
            string savePath = sfd.FileName;
            btnDoGeneration.Enabled = false;
            EssentialDialogs.ProgressDialog.RunWork(this, (ps) =>
            {
                var renderingMsg = FrmMainCodeString.abcRenderingFont;
                ps.SetMessage(renderingMsg);
                using (XTEinkFontRenderer renderer = new XTEinkFontRenderer())
                {
                    bool is2Bit = false;
                    Invoke(new Action(() =>
                    {
                        is2Bit = chkExport2BitFont.Checked;
                        ConfigureRenderer(renderer);
                    }));
                    Size fontRenderSize = renderer.GetFontRenderSize();

                    EPDFontBinary fontBinary = new EPDFontBinary(fontRenderSize.Height, is2Bit);
                    
                    var allChars = EPDFontBinary.GetAllRequiredCharPoints().ToList();
                    var maxCharRange = allChars.Count;
                    for (int i = 0; i < allChars.Count; i++)
                    {
                        try
                        {
                            ps.SetProgress(i, allChars.Count);
                            ps.SetMessage($"{renderingMsg}({i}/{maxCharRange})");
                            renderer.RenderFont(allChars[i], fontBinary,fontRenderSize.Height,fontRenderSize.Width);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    using (var stream = File.Create(savePath))
                    {

                        ps.SetMessage($"正在保存最终文件...");
                        fontBinary.SaveTo(stream);
                    }
                }

            }, (err) =>
            {
                btnDoGeneration.Enabled = true;
                if (err != null)
                {
                    MessageBox.Show(this, FrmMainCodeString.abcRenderingError + "：\r\n" + err.GetType().FullName + ": " + err.Message, FrmMainCodeString.abcRenderingError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, FrmMainCodeString.abcSuccessDialogMsg, FrmMainCodeString.abcSuccessDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
        }

        private void chkExport2BitFont_CheckedChanged(object sender, EventArgs e)
        {
            chkRenderAntiAltas.Enabled = !chkExport2BitFont.Checked;
            numFontGamma.Enabled = !chkExport2BitFont.Checked;
            chkRenderAntiAltas.Checked = true;
            numFontGamma.Value = 127;
        }
    }
}
