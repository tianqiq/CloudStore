using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using QBox.Auth;
using QBox.RS;
using QBox.FileOp;
using QBox.RPC;
using System.IO;
using System.Threading;
using System.Xml;
namespace CloudStore
{
    public partial class Main : Form
    {

        public Main()
        {
            InitializeComponent();
        }

        public String Domain;
        public String Bucket;
        public String LastFileName;

        private void Main_Load(object sender, EventArgs e)
        {
            LoadConfig();
            this.Width = 100;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 130;
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - 60;
        }

        private void LoadConfig()
        {
            if (!File.Exists("./config.xml"))
            {
                new ClientConfig().ShowDialog();
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("./config.xml");
                var stores = doc.SelectSingleNode("//stores");
                var current = stores.Attributes["current"].Value;
                var store = stores.SelectSingleNode(String.Format("./store[@name='{0}']", current));
                var accessKey = store.SelectSingleNode("./accessKey").InnerText.Trim();
                var secretKey = store.SelectSingleNode("./secretKey").InnerText.Trim();
                var domain = store.SelectSingleNode("./domain").InnerText.Trim();
                var bucket = store.SelectSingleNode("./bucket").InnerText.Trim();
                Apply(accessKey, secretKey, domain, bucket);
            }
            catch
            {
                MessageBox.Show("加载配置文件出错！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Environment.Exit(0);
            }
        }

        private void Apply(string access, string secret, string domain, string bucket)
        {
            Config.ACCESS_KEY = access;
            Config.SECRET_KEY = secret;
            Bucket = bucket;
            Domain = domain;
        }

        private void Main_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x0112, 0xF012, 0);
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private void label2_Click(object sender, EventArgs e)
        {
            var c = new CustomUpload();
            if (c.ShowDialog() == DialogResult.OK)
            {
                IniClient();
                var fileInfo = c.getFile();
                ThreadPool.QueueUserWorkItem((o) => { var info = (KeyValuePair<string, string>)o; upfileToServer(info.Key, info.Value); CloseInfo(); }, fileInfo);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            var c = new CustomDel();
            if (c.ShowDialog() == DialogResult.OK)
            {
                IniClient();
                var fileInfo = c.GetFile();
                Run((o) => { delfileToServer(o.ToString()); CloseInfo(); }, fileInfo);
                CloseInfo();
            }
        }

        private void label_DragEnter(object sender, DragEventArgs e)
        {
            IniClient();
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void IniClient()
        {
            conn = new DigestAuthClient();
            rs = new RSService(conn, Bucket);
        }

        private void label_DragDrop(object sender, DragEventArgs e)
        {
            var label = sender as Label;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (label.Text == label2.Text)
            {
                foreach (var item in files)
                {
                    Run((o) => { upfile(o.ToString()); }, item);
                }

            }
            else
            {
                foreach (var item in files)
                {
                    Run((o) => { delfile(o.ToString()); }, item);
                }
            }
        }


        private void Run(WaitCallback call, String path)
        {
            ThreadPool.QueueUserWorkItem(call, path);
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            label1.Cursor = label2.Cursor = Cursors.Hand;
        }


        private Client conn;
        private RSService rs;
        public void upfile(string path)
        {
            if (File.Exists(path))
            {
                upfileToServer(path, Path.GetFileName(path));
                CloseInfo();
            }
            else
            {
                uploadDir(path, Directory.GetParent(path).FullName);
                this.Invoke(new Action<String>(Info), string.Format("全部上传完成!"));
                CloseInfo();
            }
        }

        private void upfileToServer(string path, string key)
        {
            key = key.Replace("\\", "/");
            if (File.Exists(path))
            {
                this.Invoke(new Action<String>(Info), string.Format("正在上传:\r\n{0}", key));
                PutFileRet putFileRet = rs.PutFile(key, null, path, null);
                if (putFileRet.OK)
                {
                    LastFileName = key;
                    this.Invoke(new Action<String>(Info), string.Format("文件上传成功!"));
                }
                else
                {
                    this.Invoke(new Action<String>(Error), string.Format("文件上传失败!"));
                }
            }
        }

        private void CloseInfo()
        {
            this.Invoke(new Action(() =>
            {
                box.Height = 23;
                box.Top = this.Top - box.Height - 2;
                sleep(3);
                box.Hide();
            }));
        }


        public void uploadDir(string path, string rootdir)
        {
            var dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles())
            {
                upfileToServer(file.FullName, file.FullName.Substring(rootdir.Length + 1));
            }
            foreach (var item in dir.GetDirectories())
            {
                uploadDir(item.FullName, rootdir);
            }
        }

        public void delfile(string path)
        {
            var name = Path.GetFileName(path);
            if (File.Exists(path))
            {
                delfileToServer(name);
                CloseInfo();
            }
            else
            {
                delDir(path, Directory.GetParent(path).FullName);
                this.Invoke(new Action<String>(Info), string.Format("全部删除完成!"));
                CloseInfo();
            }
        }

        public void delfileToServer(string name)
        {
            name = name.Replace("\\", "/");
            this.Invoke(new Action<String>(Info), string.Format("正在删除:\r\n{0}", name));
            var putFileRet = rs.Delete(name);
            if (putFileRet.OK)
            {
                this.Invoke(new Action<String>(Info), string.Format("文件删除成功!"));
            }
            else
            {
                if (putFileRet.Exception.ToString().Contains("612"))
                {
                    this.Invoke(new Action<String>(Error), string.Format("文件不存在!"));
                }
                else
                {
                    this.Invoke(new Action<String>(Error), string.Format("文件删除失败!"));
                }
            }
        }


        public void delDir(string path, string rootpath)
        {
            var dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles())
            {
                delfileToServer(file.FullName.Substring(rootpath.Length + 1));
            }
            foreach (var item in dir.GetDirectories())
            {
                delDir(item.FullName, rootpath);
            }
        }

        public void Info(string msg)
        {
            if (box == null)
            {
                box = new Info(msg);
            }
            box.BackColor = Color.Green;
            box.SetState(msg);
            box.Left = this.Left;
            box.Show();
            box.Top = this.Top - box.Height - 2;
        }

        public void sleep(int s)
        {
            for (int i = 0; i < 10 * s; i++)
            {
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
            }
        }


        private Info box;
        public void Error(string msg)
        {
            if (box == null)
            {
                box = new Info(msg);
            }
            box.BackColor = Color.Red;
            box.SetState(msg);
            box.Left = this.Left;
            box.Show();
            box.Top = this.Top - box.Height - 2;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Copy();
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void Copy()
        {
            if (String.IsNullOrEmpty(LastFileName))
            {
                Error("没有链接可以复制!");
                CloseInfo();
                return;
            }

            var url = "";
            if (Domain.StartsWith("http://"))
            {
                url = Domain + "/" + LastFileName;
            }
            else
            {
                url = "http://" + Domain + "/" + LastFileName;
            }

            try
            {
                Clipboard.SetText(url);
                Info("文件链接复制成功!");
                CloseInfo();
            }
            catch
            {
                Error("复制失败!");
                CloseInfo();
            }
        }

        private void 配置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ClientConfig().ShowDialog();
            LoadConfig();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("如果您觉得好用,就分享给您的朋友吧！\r\n\r\n任何关于本软件事情，发送邮件到：\r\n\r\ntianqiq@gmail.com", "关于本软件", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
