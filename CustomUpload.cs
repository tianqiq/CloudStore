using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CloudStore
{
    public partial class CustomUpload : Form
    {
        public CustomUpload()
        {
            InitializeComponent();
        }


        private string path;

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog1.FileName;
                textBox1.Text = openFileDialog1.FileName;
                button1.Enabled = true;
                textBox2.Text = openFileDialog1.SafeFileName;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private void CustomUpload_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x0112, 0xF012, 0);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

            button2.Enabled = textBox2.Text.Trim() != "";
        }


        public KeyValuePair<string, String> getFile()
        {
            return new KeyValuePair<string, string>(path, textBox2.Text.Trim());
        }


    }
}
