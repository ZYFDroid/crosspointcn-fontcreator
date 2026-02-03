using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EssentialDialogs
{
    public partial class ProgressDialog : Form
    {

		#region UI
		
		// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.animTimer = new System.Windows.Forms.Timer(this.components);
            this.tblProgressContainer = new System.Windows.Forms.TableLayoutPanel();
            this.lblMsg = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel114514 = new System.Windows.Forms.Panel();
            this.tblProgressBar = new System.Windows.Forms.Panel();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tblProgressContainer.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel114514.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(600, 2);
            this.label1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Location = new System.Drawing.Point(0, 154);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(600, 2);
            this.label2.TabIndex = 1;
            // 
            // animTimer
            // 
            this.animTimer.Enabled = true;
            this.animTimer.Interval = 1;
            this.animTimer.Tick += new System.EventHandler(this.animTimer_Tick);
            // 
            // tblProgressContainer
            // 
            this.tblProgressContainer.ColumnCount = 3;
            this.tblProgressContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00331F));
            this.tblProgressContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tblProgressContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.99669F));
            this.tblProgressContainer.Controls.Add(this.lblMsg, 0, 0);
            this.tblProgressContainer.Controls.Add(this.panel1, 1, 1);
            this.tblProgressContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblProgressContainer.Location = new System.Drawing.Point(0, 6);
            this.tblProgressContainer.Name = "tblProgressContainer";
            this.tblProgressContainer.RowCount = 2;
            this.tblProgressContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 52.05479F));
            this.tblProgressContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 47.94521F));
            this.tblProgressContainer.Size = new System.Drawing.Size(600, 148);
            this.tblProgressContainer.TabIndex = 2;
            // 
            // lblMsg
            // 
            this.tblProgressContainer.SetColumnSpan(this.lblMsg, 3);
            this.lblMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMsg.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMsg.ForeColor = System.Drawing.Color.White;
            this.lblMsg.Location = new System.Drawing.Point(3, 0);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.lblMsg.Size = new System.Drawing.Size(594, 77);
            this.lblMsg.TabIndex = 1;
            this.lblMsg.Text = "请稍后...";
            this.lblMsg.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.panel114514);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(153, 80);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(2);
            this.panel1.Size = new System.Drawing.Size(294, 25);
            this.panel1.TabIndex = 2;
            // 
            // panel114514
            // 
            this.panel114514.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel114514.Controls.Add(this.tblProgressBar);
            this.panel114514.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel114514.Location = new System.Drawing.Point(2, 2);
            this.panel114514.Name = "panel114514";
            this.panel114514.Padding = new System.Windows.Forms.Padding(2);
            this.panel114514.Size = new System.Drawing.Size(290, 21);
            this.panel114514.TabIndex = 0;
            // 
            // tblProgressBar
            // 
            this.tblProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblProgressBar.Location = new System.Drawing.Point(2, 2);
            this.tblProgressBar.Name = "tblProgressBar";
            this.tblProgressBar.Size = new System.Drawing.Size(286, 17);
            this.tblProgressBar.TabIndex = 0;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // ProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(600, 160);
            this.Controls.Add(this.tblProgressContainer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ProgressDialog";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.Text = "ProgressDialog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProgressDialog_FormClosed);
            this.Load += new System.EventHandler(this.ProgressDialog_Load);
            this.tblProgressContainer.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel114514.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer animTimer;
        private System.Windows.Forms.TableLayoutPanel tblProgressContainer;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel114514;
        private System.Windows.Forms.Panel tblProgressBar;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
		
		#endregion

        Form parentForm = null;

        [DllImport("USER32.DLL")]
        public static extern int GetSystemMenu(int hwnd, int bRevert);
        [DllImport("USER32.DLL")]
        public static extern int EnableMenuItem(int hMenu, int nPosition, int wFlags);

        private const int MF_GRAYED = 0x1;
        private const int MF_ENABLED = 0x0;

        const int SC_CLOSE = 0xF060; 


        private ProgressDialog(Form parentForm,Action<ProgressStateAccessor> work)
        {
            InitializeComponent();
            ShowInTaskbar = false;
            this.parentForm = parentForm;
            this.work = work;
        }

        Graphics progressBarGraphics = null;
        Bitmap bufferedProgressBarImage = null;
        Graphics g = null;
        Brush whiteBrush = Brushes.White;
        Pen intermedatePen = new Pen(Brushes.White, 5f);

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            SyncWindowPos();
            parentForm.SizeChanged += ParentForm_SizeChanged;
            parentForm.Move += ParentForm_Move;
            progressBarGraphics = Graphics.FromHwnd(tblProgressBar.Handle);
            progressBarGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            bufferedProgressBarImage = new Bitmap(tblProgressBar.Width, tblProgressBar.Height);
            g = Graphics.FromImage(bufferedProgressBarImage);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            animTimer.Start();
            backgroundWorker1.RunWorkerAsync();
        }

        private void ParentForm_Move(object sender, EventArgs e)
        {
            SyncWindowPos();
        }

        private void ParentForm_SizeChanged(object sender, EventArgs e)
        {
            SyncWindowPos();
        }

        private void SyncWindowPos()
        {
            if(parentForm.WindowState == FormWindowState.Minimized)
            {
                this.Visible = false;
            }
            else
            {
                this.Visible = true;
            }
            this.Width = parentForm.ClientSize.Width;
            this.Left = parentForm.PointToScreen(Point.Empty).X;
            this.Top = parentForm.PointToScreen(Point.Empty).Y + parentForm.ClientSize.Height / 2 - this.Height / 2;
            
        }

        private bool isIndetermine = true;
        private int begin = 0;
        private int end = 100;
        private int current = 0;
        private int total = 1;
        private string text = "请稍后...";
        private float t = 0;



        private void animTimer_Tick(object sender, EventArgs e)
        {
            bool isIndetermine = true;
            int begin = 0;
            int end = 100;
            int current = 0;
            int total = 1;
            string text = "请稍后...";

            lock (this)
            {
                isIndetermine = this.isIndetermine;
                begin = this.begin;
                end = this.end;
                current = this.current;
                total = this.total;
                text = this.text;

            }

            g.Clear(this.BackColor);

            if (isIndetermine)
            {
                t += 0.4f;
                for (float i = 0; i < tblProgressBar.Width + 15; i += 10)
                {
                    g.DrawLine(intermedatePen, i - t, -3, i - 8f - t, 20);
                }
                if (t > 10)
                {
                    t -= 10;
                }
            }
            else
            {
                float progressFactor = clamp(current / (total == 0f ? 1f : total));

                float baseBegin = begin / 100f;
                float baseLen = (end - begin) / 100f;
                float actualWidth = baseBegin + baseLen * progressFactor;

                float width = actualWidth * (float)tblProgressBar.Width;
                g.FillRectangle(whiteBrush, 0f, 0f, width, 20f);
            }
            lblMsg.Text = text;

            progressBarGraphics.DrawImage(bufferedProgressBarImage, 0, 0, bufferedProgressBarImage.Width, bufferedProgressBarImage.Height);
        }

        private float clamp(float f) { if (f > 1) { return 1; } if (f < 0) { return 0; } return f; }

        public class ProgressStateAccessor
        {
            private ProgressDialog _accessor;

            public ProgressStateAccessor(ProgressDialog accessor)
            {
                _accessor = accessor;

            }

            public bool isIndetermine = true;
            public int begin = 0;
            public int end = 100;
            public int current = 0;
            public int total = 1;
            public string text = "请稍后...";

            public void syncProgress()
            {
                lock (_accessor)
                {
                    _accessor.isIndetermine = isIndetermine;
                    _accessor.begin = begin;
                    _accessor.end = end;
                    _accessor.current = current;
                    _accessor.total = total;
                    _accessor.text = text;
                }
            }

            public void SetSegement(int begin, int end)
            {
                this.begin = begin;
                this.end = end;
                this.current = 0;
                this.isIndetermine = false;
                syncProgress();
            }

            public void SetMessage(string msg)
            {
                this.text = msg;
                syncProgress();
            }

            public void SetProgress(int current,int total)
            {
                this.current = current;
                this.total = total;
                this.isIndetermine = false;
                syncProgress();
            }

            public void SetIndetermine(bool indetermine)
            {
                this.isIndetermine = indetermine;
                syncProgress();
            }
        }

        private Action<ProgressStateAccessor> work;
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                work(new ProgressStateAccessor(this));
            }catch(Exception ex)
            {
                e.Result = ex;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private Exception err = null;

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            err = e.Result as Exception;
            animTimer.Enabled = false;
            Close();
        }
        private void ProgressDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            g.Dispose();
            progressBarGraphics.Dispose();
            bufferedProgressBarImage.Dispose();
            parentForm.Move -= ParentForm_Move;
            parentForm.SizeChanged -= ParentForm_SizeChanged;
        }

        private void DisableCloseButton(int handle)
        {
            int hMenu = GetSystemMenu(handle, 0);
            EnableMenuItem(hMenu, SC_CLOSE, MF_GRAYED);
        }

        private void EnableCloseButton(int handle)
        {
            int hMenu = GetSystemMenu(handle, 0);
            EnableMenuItem(hMenu, SC_CLOSE, MF_ENABLED);
        }

        public static void RunWork(Form attachForm,Action<ProgressStateAccessor> work,Action<Exception> callback = null)
        {
            ProgressDialog form = new ProgressDialog(attachForm,work);
            
            form.Show(attachForm);

            form.DisableCloseButton(attachForm.Handle.ToInt32());
            
                form.FormClosed += delegate
                {
                    form.EnableCloseButton(attachForm.Handle.ToInt32()) ;   
                    if(callback != null)
                    {
                        attachForm.BeginInvoke(callback,(form.err));
                        callback = null;
                    }
                    
                };
            
        }

        
    }
}
