
using Newtonsoft.Json;
using System.Reflection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
namespace SvnUpdater_V1
{
    public partial class SVN导入工具 : Form
    {
        string CurPath = System.IO.Directory.GetCurrentDirectory();

        string Pan
        {
            get
            {
                string _pan = CurPath[0].ToString() + CurPath[1];
                return _pan;
            }
        }

        Manager MainMgr;

        private List<string> Commonds = new List<string>();
        public SVN导入工具()
        {
            InitializeComponent();

            MainMgr = new Manager();
            MainMgr.Init();
            Bind(MainMgr);
            MainMgr.Start();



        }
        public void SetDepth(int lv,ref List<string> comd)
        {
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in Manager.Instance.Nodes[i])
                {
                    if (item.Value == lv )
                    {
                        string type = ItemDef.GetType(item.Value);
                        comd.Add("update  --set-depth " + type + CurPath + "\\nshm" + item.Key);  
                    }
                }
            }
        }
        public void GetTree(ItemDef s)
        {
            var spits = ItemDef.MySpit(s, (ItemDef.PathType)s.Kind);

            foreach (var item in spits)
            {
                Manager.Instance.CheckPathLv(item);
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //comboBox1.Enabled = false;
            //textBox1.Enabled = false;
            //button4.Enabled = false;
            //button5.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认重新导入？", "此操作不可逆", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string k = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string[] s = k.Split("\\");

                string[] so, se;
                List<string> commnd = new List<string>();
                commnd.Add("checkout " + Manager.URL + CurPath + "\\nshm --depth empty "); 



                foreach (var item in Manager.Instance.Items[comboBox2.Text])
                {
                    GetTree(item);
                }
                CheckBlack();

                SetDepth(3, ref commnd);
                SetDepth(2, ref commnd);
                SetDepth(1, ref commnd);
                SetDepth(0, ref commnd);

                Commonds = commnd;
                Start();

                MainMgr.Save();

            }


        }

        private void CheckBlack()
        {
            List<KeyValuePair<string, int>> ls_ex = new List<KeyValuePair<string, int>>();
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in Manager.Instance.Nodes[i])
                {
                    if (item.Value == 0)
                    {
                        ls_ex.Add(new KeyValuePair<string, int>(item.Key, i));

                    }
                }
            }
            foreach (var item in ls_ex)
            {
                //找到最上级的infinity点
                var fn = MainMgr.GetFarFN(item.Value, item.Key);
                if (fn.Name == null) continue;
                //递归设置新树
                var fp = MainMgr.GetFatherPath(item.Key);
                var nowNode = item.Key;
                int layout = item.Value;

                while (!nowNode.Equals(fn.Name))
                {
                    var fs = MainMgr.GetLSFiles(fp);
                    List<string> files = new List<string>();
                    foreach (var dd in fs)
                    {
                        string ddd = fp + "/" + dd;
                        if (dd.Contains("/") && !ddd.Equals(nowNode + "/")) {
                            ddd = ddd.Remove(ddd.Length - 1, 1);
                            files.Add(ddd);
                        } 
                    }
                    //把父亲设成files级
                    int skjdksd = MainMgr.Nodes[layout - 1][fp];
                    MainMgr.Nodes[layout - 1][fp] = 2;
                    //把files中的节点加入树
                    foreach (var exitem in files)
                    {
                        if (!MainMgr.Nodes[layout].ContainsKey(exitem))
                        {
                            MainMgr.Nodes[layout].Add(exitem, 1);
                        }
                        else
                        {
                            if (MainMgr.Nodes[layout][exitem] > 1)
                            {
                                MainMgr.Nodes[layout][exitem] = 1;
                            }
                        }
                    }

                    layout--;
                    nowNode = fp;
                    fp = MainMgr.GetFatherPath(fp);

                }
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            //Log.ShowSvn(mt.stout, mt.sterr);
            if (MessageBox.Show("确认删除？", "此删除不可恢复", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string[] so, se;
                List<string> commnd = new List<string>();
                commnd.Add("update  --set-depth " + " empty " + CurPath + "\\nshm ");
                //Manager.RunCmd(commnd, out so, out se);
                Commonds = commnd;
                Start();
                for (int i = 0; i < MainMgr.Nodes.Count; i++)
                {
                    MainMgr.Nodes[i] = new Dictionary<string, int>();
                }
                MainMgr.Save();
                //Log.ShowSvn(so, se);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            MainMgr.LoadItems();
            string[] so, se;
            List<string> commnd = new List<string>();

            foreach (var item in Manager.Instance.Items[comboBox2.Text])
            {
                // Checkout(item, ref commnd);
                GetTree(item);
            }

            CheckBlack();


            List<Dictionary<string, int>>  et = MainMgr.Load();
            
            for (int i = 0; i < 100; i++)
            {
                foreach (var item in et[i])
                {
                    string type = ItemDef.GetType(item.Value);

                    commnd.Add("update  --set-depth " + type + CurPath + "\\nshm" + item.Key);

                }
            }

            commnd.Add("update  " + CurPath + "\\nshm" );

            //Manager.RunCmd(commnd, out so, out se);

            Commonds = commnd;
            Start();

            MainMgr.Save();

            // Log.ShowSvn(so, se);
        }

        /// <summary>
        /// 自定义命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string[] so, se;
            List<string> commnd = new List<string>();
            commnd.Add("update  --set-depth " + MainMgr.Comd + " " + CurPath + "\\nshm\\" + MainMgr.textBox1.Text);

            Commonds = commnd;
            Start();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string[] so, se;
            List<string> commnd = new List<string>();
            commnd.Add("info "+ CurPath + "\\nshm\\" + MainMgr.textBox1.Text);// + " \" ^ Depth:\"";


            Commonds = commnd;
            Start();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            label1.Text = "";
        }

        public void OpenDebug()
        {
            //comboBox1.Enabled = true;
            //textBox1.Enabled = true;
            //button4.Enabled = true;
            //button5.Enabled = true;
        }

        public void SwitchUsing()
        {
            button1.Enabled = !button1.Enabled;
            button2.Enabled = !button2.Enabled;
            button3.Enabled = !button3.Enabled;
        }

        Thread MyThread;
        Process process;
        public void Start()
        {
            label1.Text = "";
            SwitchUsing();
            MyThread = new Thread(new ThreadStart(RunCmd));
            MyThread.Start();
        }
        public void Stop()
        {
            process.Close();
            //MyThread.Interrupt();
            //Application.Exit();
        }
        public void RunCmd()
        {
            for (int i = 0; i < Commonds.Count; i++)
            {
                Invoke(new Action(() => {
                    label1.SelectionStart = label1.TextLength;
                    label1.SelectionLength = 0;
                    label1.SelectionColor = Color.Blue;
                    label1.AppendText(Commonds[i] + "\n");
                    label1.SelectionColor = label1.ForeColor;

                }));


                process = new Process();
                process.StartInfo.FileName = Manager.Instance.SvnPath;// Path.Combine(CurPath+"/svn", "svn.exe");
                process.StartInfo.Arguments = Commonds[i];
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                    {
                        Invoke(new Action(() => {
                            label1.SelectionStart = label1.TextLength;
                            label1.SelectionLength = 0;
                            label1.SelectionColor = Color.Black;
                            label1.AppendText(e.Data + "\n");
                            label1.SelectionColor = label1.ForeColor;

                        }));
                    }
                };
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                    {
                        Invoke(new Action(() => {
                            label1.SelectionStart = label1.TextLength;
                            label1.SelectionLength = 0;
                            label1.SelectionColor = Color.Red;
                            if(e.Data.Contains("was not found") && Commonds[i].Contains("exclude"))
                            {
                                label1.AppendText("exclude success!" + "\n");
                            }
                            else
                            {
                                label1.AppendText(e.Data + "\n");
                            }
                            label1.SelectionColor = label1.ForeColor;

                        }));
                    }
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.StandardInput.WriteLine("exit");
                process.WaitForExit();
                int exitCode = process.ExitCode;
                process.Close();
            }

            Invoke(new Action(() => button1.Enabled = !button1.Enabled));
            Invoke(new Action(() => button2.Enabled = !button2.Enabled));
            Invoke(new Action(() => button3.Enabled = !button3.Enabled));
        }

        private void label3_Click(object sender, EventArgs e)
        {
            OpenDebug();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            MainMgr.LoadItems();

            List<string> commnd = new List<string>();
            Manager.Instance.NowClass = comboBox2.Text;
            foreach (var item in Manager.Instance.Nodes)
            {
                item.Clear();
            }
            foreach (var item in Manager.Instance.Items[comboBox2.Text])
            {
                GetTree(item);
            }
            CheckBlack();


            new Form2().ShowDialog();
        }

        private void button_Break_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认打断？这意味着要删库并重新导入", "此操作不可逆", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                process?.Close();
            }
        }
    }
}