using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Searcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "検索するディレクトリを選択してください";
            folderBrowserDialog1.ShowNewFolderButton = false;
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == "")
            {
                MessageBox.Show("検索する文字列を入力してください");
                return;
            }
            if(textBox2.Text == "" || !Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("検索するディレクトリを正しく指定してください");
                return;
            }
            search(textBox1.Text, textBox2.Text);
        }

        private async void search(string target, string directory)
        {
            var files = Directory.GetFiles(directory);

            richTextBox1.Text = "";
            button1.Enabled = false;
            button2.Enabled = false;
            Text = "Searching...";
            progressBar1.Maximum = files.Length;
            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            label1.Text = "0/" + files.Length;
            label1.Text = string.Format("{0}/{1}", progressBar1.Value, files.Length);

            foreach(string file in files)
            {
                bool find = false;
                await Task.Run(() =>
                {
                    string data = File.ReadAllText(file);
                    find = data.IndexOf(target) >= 0;
                });
                if (find)
                    richTextBox1.Text += string.Format("Find: {0}\n", file);
                progressBar1.Increment(1);
                label1.Text = string.Format("{0}/{1}", progressBar1.Value, files.Length);
            }

            button1.Enabled = true;
            button2.Enabled = true;
            Text = "Done!!";
        }
    }
}
