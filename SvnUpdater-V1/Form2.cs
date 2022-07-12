using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SvnUpdater_V1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            Menu = this.contextMenuStrip1;
            treeView1.ImageList = imageList1;
            treeView1.ImageIndex = 2;
            treeView1.SelectedImageIndex = 1;

            var list = Manager.Instance.GetLSFiles("");
            foreach (var file in list)
            {
                var name = Manager.Instance.GetPathName(file);
                System.Windows.Forms.TreeNode treeNodetest = new System.Windows.Forms.TreeNode();
                treeNodetest.Text = name;
                treeNodetest.Name = "/"+ name;
                treeNodetest.Tag = 0;
                if (Manager.Instance.Nodes[0].ContainsKey(treeNodetest.Name))
                {
                    treeNodetest.Tag = Manager.Instance.Nodes[0][treeNodetest.Name];
                }
                if (name.Contains("."))
                {

                }
                else
                {
                    NodeLoad(treeNodetest);
                    RichTextBoxExtension.NodeSetColor(treeNodetest, false, true);

                }


                treeView1.Nodes.Add(treeNodetest);

            }
            imageList1.ImageSize = new Size(16, 16);



            treeView1.BeforeExpand += treeBeforeExpand;
            //treeView1.MouseDoubleClick += treeView1_MouseDouble;
            treeView1.MouseDown += treeView1_MouseDown;
        }
        private void treeBeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            for (int i = 0; i < e.Node.Nodes.Count; i++)
            {
                NodeLoad(e.Node.Nodes[i]);
            }
        }
        private void treeView1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)//判断你点的是不是右键
            {
                Point ClickPoint = new Point(e.X, e.Y);
                TreeNode CurrentNode = treeView1.GetNodeAt(ClickPoint);
                if (CurrentNode != null)//判断你点的是不是一个节点
                {
                    CurrentNode.ContextMenuStrip = contextMenuStrip1;
                    var name = treeView1.SelectedNode.Text.ToString();//存储节点的文本
                    treeView1.SelectedNode = CurrentNode;//选中这个节点
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip menu = (ContextMenuStrip)sender;

            TreeNode node = treeView1.SelectedNode;
            menu.Items[0].Text = node.Name;
        }

        public void NodeLoad(TreeNode node)
        {
            if (node.Text.Contains(".")) return;
            if (node.Nodes.Count == 0)
            {
                node.ImageIndex = 0;
                node.SelectedImageIndex = 0;

                int lv = (int)node.Tag;
                var list = Manager.Instance.GetLSFiles(node.Name);
                foreach (var item in list)
                {
                    var name = node.Name + "/" + item;
                   node.AddNode(name, 0, lv);

                }
                foreach (var item in Manager.Instance.Nodes[node.Level + 1])
                {
                    bool find = false;
                    for (int i = 0; i < node.Nodes.Count; i++)
                    {
                        // var name1 = name.Substring(0, name.Length - 1);
                        if (node.Nodes[i].Name == item.Key)
                        {
                            RichTextBoxExtension.NodeSetColor(node.Nodes[i], false,true);
                            node.Nodes[i].Tag = item.Value;
                            //node.Nodes[i].BackColor = Manager.Instance.GetLvColor(item.Value);
                            find = true;
                        }
                    }
                    if (!find && node.Name == Manager.Instance.GetFatherPath(item.Key))
                    {
                        node.AddNode(item.Key, -1, lv);
                    }
                }

                if (Manager.Instance.Debug) node.Text += "_" + node.Tag;

            }
        }
        public void NodeClear(TreeNode node)
        {
            node.Nodes.Clear();
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;


            SetNodeLv(node, 0);
            // TreeNode node = treeView1.GetNodeAt(e.);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

            TreeNode node = treeView1.SelectedNode;

            SetNodeLv(node, 1);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            SetNodeLv(node, 2);
        }

        private void SetNodeLv(TreeNode node,int lv)
        {
            //
            node.Tag = lv;
            node.ForeColor = Color.Black;


            var def = new ItemDef();
            def.ClassUse = Manager.Instance.NowClass;
            def.Path = node.Name;
            def.Kind = lv;
            bool find = false;

            foreach (var item in Manager.Instance.m_Items)
            {
                if (item.Path.Equals(def.Path) && item.ClassUse.Equals(def.ClassUse))
                {
                    item.Kind = lv;
                    find = true;
                }
            }
            if (!find)
            {
                Manager.Instance.m_Items.Add(def);
            }

            NodeClear(node);
            NodeLoad(node);

            string jsonString = JsonConvert.SerializeObject(Manager.Instance.m_Items, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            Log.WriteJsonFile(Manager.Instance.CurPath + "\\Json\\SvnList.json", jsonString);
        }

        private void sdToolStripMenuItem_Click(object sender, EventArgs e)
        {

            TreeNode node = treeView1.SelectedNode;
            node.Tag = 0;
            node.ForeColor = Color.Silver;


            var def = new ItemDef();
            def.ClassUse = Manager.Instance.NowClass;
            def.Path = node.Name;
            for (int i = 0; i < Manager.Instance.m_Items.Count; i++)
            {
                var item = Manager.Instance.m_Items[i];
                if (item.Path.Equals(def.Path) && item.ClassUse.Equals(def.ClassUse))
                {
                    Manager.Instance.m_Items.Remove(item);
                    continue;
                }
            }

            NodeClear(node);
            NodeLoad(node);

            string jsonString = JsonConvert.SerializeObject(Manager.Instance.m_Items, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            Log.WriteJsonFile(Manager.Instance.CurPath + "\\Json\\SvnList.json", jsonString);
        }
    }
}
