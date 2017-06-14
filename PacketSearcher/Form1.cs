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
using System.Text.RegularExpressions;

namespace PacketSearcher
{
    public partial class Form1 : Form
    {
        PacketCollection pkColl = new PacketCollection();

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (pkColl.exists(textBox1.Text))
                {
                    Packet pk = pkColl.get(textBox1.Text);
                    string instr = "";
                    foreach (string line in pk.instr)
                        instr += line + "\n";
                    richTextBox1.Text = instr.Substring(0, instr.Length - 1);
                }
                else
                {
                    MessageBox.Show("パケットが見つかりませんでした。");
                }
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            openFileDialog1.Title = "逆アセのリザルトファイルを選択してください";
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                this.Text = string.Format("解析中...( {0} )", Path.GetFileName(openFileDialog1.FileName));
                await Task.Run(() =>
                {
                    using (StreamReader reader = new StreamReader(openFileDialog1.FileName))
                    {
                        long linesCnt = 0;
                        while (reader.Peek() != -1)
                        {
                            linesCnt++;
                            string line = reader.ReadLine().TrimEnd();
                            if (line.IndexOf("SharedConstants::NetworkProtocolVersion") != -1)
                            {
                                Console.WriteLine("Line {0}: Reading...", linesCnt);
                                Console.WriteLine(" Detecting protocol version...");
                                linesCnt++;
                                string next = reader.ReadLine().Trim();
                                Match line_protocol = Regex.Match(next, "[0-9a-f]+:[ \t]+([0-9a-f]+)");
                                if (line_protocol.Success)
                                {
                                    Console.WriteLine("Detected {0}", Convert.ToUInt32(line_protocol.Groups[1].Value, 16));
                                }
                            }
                            else if (Regex.IsMatch(line, @"(\w+Packet)::getId\(\) const\>"))
                            {
                                Match line_packet_getid = Regex.Match(line, @"(\w+Packet)::getId\(\)");
                                string name = line_packet_getid.Groups[1].Value;

                                Console.WriteLine("Line {0}: Reading...", linesCnt);
                                Console.WriteLine(" Detecting packet-to-ID declaration...");
                                linesCnt++;
                                string next = reader.ReadLine().Trim();
                                Match line_id = Regex.Match(next, "[0-9a-f]{4}[ \t]+movs[ \t]+r0, #([0-9]+)");
                                if (line_id.Success)
                                {
                                    uint id = Convert.ToUInt32(line_id.Groups[1].Value, 10);
                                    Console.WriteLine(" Detected {0}", id);
                                    pkColl.get(name).id = id;
                                }
                            }
                            else if (Regex.IsMatch(line, @"^[0-9a-f]+ \<(\w+Packet)::read\((RakNet::BitStream\*|BinaryStream\&)\)\>:"))
                            {
                                Match line_instr_start = Regex.Match(line, @"^[0-9a-f]+ <(\w+Packet)::read\((RakNet::BitStream\*|BinaryStream\&)\)>:");
                                Console.WriteLine("Line {0}: Reading...", linesCnt);
                                string pkName = line_instr_start.Groups[1].Value;
                                Packet pk = pkColl.get(pkName);
                                pk.startAnalyze();
                                while (reader.Peek() != -1)
                                {
                                    linesCnt++;
                                    Console.CursorLeft = 0;
                                    Console.Write("Line {0}: Reading {1} packet structure...", linesCnt, pkName);
                                    line = reader.ReadLine().TrimEnd();
                                    //if (line.Length - 2 != line.TrimStart(' ').Length)
                                    //    break;
                                    if (line == "")
                                        break;
                                    pk.analyze(line);
                                }
                                Console.WriteLine();
                                pk.stopAnalyze();
                                
                            }
                        }
                    }
                });
                this.Text = string.Format("解析完了！！ ( {0} )", Path.GetFileName(openFileDialog1.FileName));
                this.button1.Enabled = true;
                this.button2.Enabled = true;
            }
            else
            {
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string result = "";
            foreach (Packet pk in pkColl.packets)
                result += string.Format("{0,3}: {1}\n", pk.id, pk.name);
            richTextBox1.Text = result.Substring(0, result.Length - 1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "パケットデータを保存するディレクトリ選んでくだせえ";
            if (folderBrowserDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                foreach (Packet pk in pkColl.packets)
                {
                    string filename = string.Format("{0}_0x{1}.pk", pk.name, pk.id.ToString("x2"));
                    File.WriteAllLines(folderBrowserDialog1.SelectedPath + "\\" + filename, pk.instr.ToArray());
                }
            }
            MessageBox.Show("保存しました！！");
        }
    }

    public class Packet
    {
        public uint id;
        public string name;
        public List<string> instr = new List<string>();

        public Packet(string name)
        {
            this.name = name;
        }

        public void startAnalyze()
        {
            this.instr = new List<string>();
        }

        public void analyze(string line)
        {
            this.instr.Add(line);
        }

        public void stopAnalyze()
        {

        }
    }

    public class PacketCollection
    {
        public List<Packet> packets = new List<Packet>();

        public List<Packet> getPackets()
        {
            return this.packets;
        }

        public Packet get(string name)
        {
            if (!packets.Exists(d => d.name.Equals(name)))
                this.packets.Add(new Packet(name));
            return this.packets.Find(d => d.name.Equals(name));
        }

        public bool exists(string name)
        {
            return packets.Exists(d => d.name.Equals(name));
        }
    }
}
