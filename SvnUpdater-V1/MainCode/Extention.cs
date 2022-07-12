using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SvnUpdater_V1
{

    public static class RichTextBoxExtension
    {
        public static void AppendTextColorfulThread(this RichTextBox rtBox, string text, Color color, bool addNewLine = true)
        {

            if (addNewLine)

            {

                text += Environment.NewLine;

            }

            //rtBox.SelectionStart = rtBox.TextLength;
            //rtBox.SelectionLength = 0;
           // rtBox.SelectionColor = color;
           // rtBox.AppendText(text);
            //rtBox.SelectionColor = rtBox.ForeColor;

        }
        public static void AddNode(this TreeNode Node, string Path,int lv,int fatherlv)
        {
            TreeNode node = new TreeNode();
            node.Text = Manager.Instance.GetPathName(Path);
            node.Name = Node.Name +"/"+ Manager.Instance.GetPathName(Path);
            node.ContextMenuStrip = Form2.Menu;
            node.Tag = lv;
            if (Path.Contains("."))
            {
                NodeSetColor(node, true, lv != 0 || fatherlv == 1 || fatherlv == 2);
                if (fatherlv == 1 || fatherlv == 2) node.Tag = fatherlv;
            }
            else
            {
                NodeSetColor(node, false, lv != 0 || fatherlv == 1 );
                if (fatherlv == 1 ) node.Tag = fatherlv;
            }
            

            Node.Nodes.Add(node);

        }

        public static void NodeSetColor(TreeNode node, bool isfile, bool isactive)
        {
            if (!isfile)
            {
                node.BackColor = Color.White;
            }
            else
            {
                node.BackColor = Color.White;
            }
            if (!isactive)
            {
                node.ForeColor = Color.Silver;
            }
            else
            {
                node.ForeColor = Color.Black;
            }
        }

        public static void RootAddNode(this TreeNode Node, string Path, int layout, int lv)
        {
            TreeNode node = new TreeNode();
            node.Text = Manager.Instance.GetPathName(Path);
            Node.Nodes.Add(node);
            node.ContextMenuStrip = Form2.Menu;

        }

    }

}