using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XTEinkToolkit.Controls
{
    public partial class DlgSelectCustomFont : Form
    {
        public DlgSelectCustomFont()
        {
            InitializeComponent();
        }

        int selectedIndex = -1;

        public static bool ShowSelectDialog(Form owner,out PrivateFontCollection privateFontCollection,out Font font)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = (FrmMainCodeString.abcOpenFontTypeName).Trim() + "|*.ttf";
            ofd.Multiselect = true;
            ofd.Title = FrmMainCodeString.abcOpenFontDialogTitle;
            var dialogResult = ofd.ShowDialog(owner);
            if (dialogResult != DialogResult.OK) {
                privateFontCollection = null;
                font = null;
                return false;
            }
            var fileNames = ofd.FileNames;
            PrivateFontCollection pfc = new PrivateFontCollection();
            foreach (var item in fileNames)
            {
                pfc.AddFontFile(item);
            }
            var families = pfc.Families;
            if(families == null || families.Length == 0)
            {
                privateFontCollection = null;
                font = null;
                pfc.Dispose();
                MessageBox.Show(owner,FrmMainCodeString.abcOpenFontNoFamily,"",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return false;
            }

            DlgSelectCustomFont dlg = new DlgSelectCustomFont();
            dlg.availableFamilies = families;
            dlg.fontFamilyNames = families.Select(f => f.Name).ToArray();
            dlg.tblFontFamilyList.Items.Clear();
            dlg.tblFontFamilyList.Items.AddRange(dlg.fontFamilyNames);
            dlg.tblFontFamilyList.SelectedIndex = 0;
            if(dlg.ShowDialog() != DialogResult.OK)
            {
                privateFontCollection = null;
                font = null;
                return false;
            }

            privateFontCollection = pfc;
            font = new Font(dlg.availableFamilies[dlg.selectedIndex], 24, FontStyle.Regular);
            return true;
        }


        public FontFamily[] availableFamilies;
        public string[] fontFamilyNames;

        private void tblFontFamilyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndex = tblFontFamilyList.SelectedIndex;
            lblFontPreview.Font = new Font(availableFamilies[selectedIndex], 21, FontStyle.Regular);
            btnOK.Enabled = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if(selectedIndex != -1)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
