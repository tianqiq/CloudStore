using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace CloudStore
{
    public partial class ClientConfig : Form
    {
        public ClientConfig()
        {
            InitializeComponent();
        }

        public void LoadConfig()
        {
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
                textBox1.Text = accessKey;
                textBox2.Text = secretKey;
                textBox3.Text = bucket;
                textBox4.Text = domain;
            }
            catch 
            {

            }
        }


        string template = @"<config>
<stores current='def'>
		<store name='def'>
			<accessKey>{0}</accessKey>
			<secretKey>{1}</secretKey>
            <bucket>{2}</bucket>
			<domain>{3}</domain>
		</store>
	</stores>
</config>";

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.InnerXml = string.Format(template, textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text);
                doc.Save("./config.xml");
                MessageBox.Show("保存成功!      ", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
        }

        private void ClientConfig_Load(object sender, EventArgs e)
        {
            LoadConfig();
        }
    }
}
