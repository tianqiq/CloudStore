using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CloudStore
{
    public partial class Info : Form
    {

        public string Msg { get; set; }

        public Info(string msg)
        {
            Msg = msg;
            InitializeComponent();
        }

        private void info_Load(object sender, EventArgs e)
        {
        }

        public void SetState(string msg)
        {
            label1.Text = msg;
        }

        new public void Show()
        {
            //var size = label1.GetPreferredSize(label1.Size);
            var drawSize = label1.CreateGraphics().MeasureString(label1.Text, label1.Font);
            this.Width = (int)drawSize.Width + 10;
            label1.Height = (int)drawSize.Height;
            this.Height = 33;
            base.Show();
        }
    }
}
