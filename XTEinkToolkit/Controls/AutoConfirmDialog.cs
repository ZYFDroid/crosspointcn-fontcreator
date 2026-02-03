using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XTEinkToolkit.Controls
{
    public static class AutoConfirmDialog
    {
        public static bool ShowDialog(IWin32Window owner, string dialogContent, string dialogTitle, string autoConfirmLabel, string autoConfirmFlagKey)
        {
            // 检查标志文件是否存在
            string flagFilePath = GetFlagFilePath(autoConfirmFlagKey);
            if (File.Exists(flagFilePath))
            {
                return true; // 自动确认
            }

            // 创建对话框窗体
            using (var form = new Form())
            {
                form.Text = dialogTitle;
                form.AutoScaleMode = AutoScaleMode.None;
                form.Font = SystemFonts.MessageBoxFont;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.Width = 420;
                form.ShowInTaskbar = false;
                form.Height = 240;
                form.Padding = new Padding(10);

                // 内容标签
                var contentLabel = new Label
                {
                    Text = dialogContent,
                    Dock = DockStyle.Fill,
                    Height = 60,
                    Padding = new Padding(10, 10, 10, 0),
                    TextAlign = System.Drawing.ContentAlignment.TopLeft
                };

                // 不再提示复选框
                var checkBox = new CheckBox
                {
                    Text = autoConfirmLabel,
                    Dock = DockStyle.Bottom,
                    Height = 30,
                    Padding = new Padding(10, 0, 0, 0),
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                };

                // 确定按钮
                var okButton = new Button
                {
                    Text = FrmMainCodeString.abcOK,
                    DialogResult = DialogResult.OK,
                    Width = 80,
                    Height = 30
                };

                // 取消按钮
                var cancelButton = new Button
                {
                    Text = FrmMainCodeString.abcCancel,
                    DialogResult = DialogResult.Cancel,
                    Width = 80,
                    Height = 30
                };

                // 按钮面板
                var buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    FlowDirection = FlowDirection.RightToLeft,
                    Height = 45,
                    Padding = new Padding(0, 10, 0, 0)
                };
                buttonPanel.Controls.Add(cancelButton);
                buttonPanel.Controls.Add(okButton);

                // 添加控件到窗体
                form.Controls.Add(contentLabel);
                form.Controls.Add(checkBox);
                form.Controls.Add(buttonPanel);

                // 设置按钮事件
                okButton.Click += (sender, e) =>
                {
                    if (checkBox.Checked)
                    {
                        CreateFlagFile(autoConfirmFlagKey);
                    }
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };

                // 显示对话框
                var result = form.ShowDialog(owner);
                return result == DialogResult.OK;
            }
        }

        public static void ClearFlags(string autoConfirmFlagKey)
        {
            string flagFilePath = GetFlagFilePath(autoConfirmFlagKey);
            if (File.Exists(flagFilePath))
            {
                File.Delete(flagFilePath);
            }
        }

        public static void ClearAllFlags()
        {
            string flagsDirectory = GetFlagsDirectory();
            if (Directory.Exists(flagsDirectory))
            {
                Directory.Delete(flagsDirectory, true);
            }
        }

        private static string GetFlagsDirectory()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            return Path.Combine(localAppData, assemblyName, "ac_flags");
        }

        private static string GetFlagFilePath(string autoConfirmFlagKey)
        {
            string flagsDirectory = GetFlagsDirectory();
            return Path.Combine(flagsDirectory, $"{autoConfirmFlagKey}.ok");
        }

        private static void CreateFlagFile(string autoConfirmFlagKey)
        {
            string flagFilePath = GetFlagFilePath(autoConfirmFlagKey);
            string directory = Path.GetDirectoryName(flagFilePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(flagFilePath, string.Empty);
        }
    }


    public static class EULADialog
    {
        public static bool ShowDialog(IWin32Window owner, string eula, string title, string eulaFlagKey)
        {
            // 检查标志文件是否存在
            string flagFilePath = GetFlagFilePath(eulaFlagKey);
            if (File.Exists(flagFilePath))
            {
                return true; // 用户已同意，直接返回
            }

            // 创建对话框窗体
            using (var form = new Form())
            {
                form.Text = title;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.AutoScaleMode = AutoScaleMode.None;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ShowInTaskbar = true;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new System.Drawing.Size(600, 400);
                form.Font = new System.Drawing.Font(SystemFonts.MessageBoxFont.FontFamily, 11F, System.Drawing.FontStyle.Regular);

                // 创建EULA文本框
                var textBox = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.White,
                    Text = eula,
                    Font = new System.Drawing.Font(SystemFonts.MessageBoxFont.FontFamily, 11F, System.Drawing.FontStyle.Regular)
                };

                // 创建按钮面板
                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 50,
                    BackColor = System.Drawing.SystemColors.Control
                };

                // 创建同意按钮
                var agreeButton = new Button
                {
                    Text = FrmMainCodeString.abcAccept,
                    DialogResult = DialogResult.OK,
                    Width = 100,
                    Height = 30,
                    Top = 10,
                    Left = 350
                };
                agreeButton.Click += (sender, e) =>
                {
                    // 创建标志文件
                    Directory.CreateDirectory(Path.GetDirectoryName(flagFilePath));
                    File.WriteAllText(flagFilePath, string.Empty);
                };

                // 创建取消按钮
                var cancelButton = new Button
                {
                    Text = FrmMainCodeString.abcCancel,
                    DialogResult = DialogResult.Cancel,
                    Width = 100,
                    Height = 30,
                    Top = 10,
                    Left = 470
                };
                
                // 添加控件到窗体
                buttonPanel.Controls.Add(agreeButton);
                buttonPanel.Controls.Add(cancelButton);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonPanel);

                // 设置按钮为默认和取消按钮
                form.AcceptButton = agreeButton;
                form.CancelButton = cancelButton;

                form.Shown += (sender, e) =>
                {
                    textBox.SelectionLength = 0;
                };

                // 显示对话框并返回结果
                return form.ShowDialog(owner) == DialogResult.OK;
            }
        }

        public static void ClearFlags(string eulaFlagKey)
        {
            string flagFilePath = GetFlagFilePath(eulaFlagKey);
            if (File.Exists(flagFilePath))
            {
                File.Delete(flagFilePath);
            }
        }

        private static string GetFlagsDirectory()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            return Path.Combine(localAppData, assemblyName, "eula_flags");
        }

        private static string GetFlagFilePath(string autoConfirmFlagKey)
        {
            string flagsDirectory = GetFlagsDirectory();
            return Path.Combine(flagsDirectory, $"{autoConfirmFlagKey}.ok");
        }


        public static void ClearAllFlags()
        {
            string flagsDirectory = GetFlagsDirectory();
            if (Directory.Exists(flagsDirectory))
            {
                Directory.Delete(flagsDirectory, true);
            }
        }


    }


}
