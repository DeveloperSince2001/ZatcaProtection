using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZatcaProtection
{
    public partial class Introfrm : Form
    {
        public Introfrm()
        {
            InitializeComponent();
            label12.Text = "Mobile : +201032289410";
            label13.Text = "Copyright ©. All rights reserved. Developed by Amir Said";
         


        }



        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you need to close ?", "Zatca Integration (Trail Version)", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }



        private void LicenseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void Introfrm_Load(object sender, EventArgs e)
        {
        }

        private void ProtectionKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmKeyGenerator _mFrmKeyGenerator = new FrmKeyGenerator();
            _mFrmKeyGenerator.Show();

        }
    }
}
