using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SvnUpdater_V1
{
    public  class Manager
    {
        public static Manager Instance { get; private set; }

        public List<ItemDef> m_Items = new List<ItemDef>();

        public Dictionary<string, List<ItemDef>> Items = new Dictionary<string, List<ItemDef>>();

        public const string URL = " https://svn-nshm.leihuo.netease.com/svn/nshm/trunk ";

       public string CurPath = System.IO.Directory.GetCurrentDirectory();

        /// <summary>
        /// 路径配置SvnList.json的第一个
        /// </summary>
        public string SvnPath
        {
            get
            {
                if (!Items["SvnPath"][0].Path.Equals("None"))
                {
                    return Items["SvnPath"][0].Path + "/svn.exe";
                }
                else
                {
                    return CurPath + "/svn/svn.exe";
                }
            }
        }

        public string NowClass;

        public bool Debug = false;
        string Pan
        {
            get
            {
                string _pan = CurPath[0].ToString() + CurPath[1];
                return _pan;
            }
        }
        public RichTextBox label1;
        public TextBox textBox1;
        public ComboBox ComboBox;
        public ComboBox ComboBox2;


        const int MaxTree = 100;
        public string Comd {
            get {
                string key = ComboBox.SelectedItem.ToString();
                switch (key)
                {
                    case "所有内容":
                        return "infinity";
                        break;
                    case "文件":
                        return "files";
                        break;
                    case "只有文件夹":
                        return "empty";
                        break;
                    case "黑名单（待测试）":
                        return "exclude";
                        break;
                    default:
                        return "empty";
                        break;
                }
            }
        }
        public void Init()
        {
            if(Instance == null)
            Instance = this;

            for (int i = 0; i < MaxTree; i++)
            {
                Nodes.Add(new Dictionary<string, int>());
            }

        }

        public void Start()
        {
            //把itemdef加载
            //m_Items.Add(new ItemDef() { ClassUse = "策划", Path = "dev/client", Kind = 1});
            //m_Items.Add(new ItemDef() { ClassUse = "策划", Path = "dev/design/data/UI", Kind = 1 });
            // m_Items.Add(new ItemDef() { ClassUse = "策划", Path = "dev/design/data", Kind = 1 });
            // m_Items.Add(new ItemDef() { ClassUse = "策划", Path = "dev/client", Kind = 0 });
            //m_Items.Add(new ItemDef() { ClassUse = "策划", Path = "dev/server_data",Kind = 1 });
            //m_Items.Add(new ItemDef() { ClassUse = "策划", Path = "dev" });
            //Items

            LoadItems();


            //ComboBox.Items.AddRange(new object[] {
            //"所有内容",
           // "文件",
           // "只有文件夹",
           // "黑名单（待测试）"});
            //ComboBox.SelectedIndex = 0;
        }



        public void LoadItems()
        {
            string js = Log.GetJsonFile(CurPath + "\\Json\\SvnList.json");
            if (js != null)
            {
                m_Items = JsonConvert.DeserializeObject<List<ItemDef>>(js);
            }
            if (m_Items == null) m_Items.Add(new ItemDef() { ClassUse = "没有json", Path = "", Kind = 3 });

            foreach (var item in m_Items)
            {
                if (!Items.ContainsKey(item.ClassUse))
                {
                    Items.Add(item.ClassUse, new List<ItemDef>());
                }
                Items[item.ClassUse].Add(item);
            }
            ComboBox2.Items.Clear();
            var keys = Items.Keys.ToArray<string>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (!keys[i].Equals("SvnPath"))
                ComboBox2.Items.Add(keys[i]);
            }
            ComboBox2.SelectedIndex = 0;

        }

        public Dictionary<string, int> m_pathempty = new Dictionary<string, int>();

        public bool CheckPathLv(ItemDef.spit def)
        {
            if (Nodes[def.Lv].ContainsKey(def.realpath))
            {
                //如果已存在path，但空级更高，则取低空级
                if (Nodes[def.Lv][def.realpath] > (int)def.type)
                {
                    Nodes[def.Lv][def.realpath] = (int)def.type;
                    return true;
                }
                return false;
            }
            else
            {
                Nodes[def.Lv].Add(def.realpath, (int)def.type);
                return true;
            }
        }

        public void Save(string ex = "")
        {
            string jsonString = JsonConvert.SerializeObject(Nodes, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            Log.WriteJsonFile(CurPath + "\\Json\\OldTree"+ ex + ".json", jsonString);
        }

        public List<Dictionary<string, int>> Load()
        {
            string js = Log.GetJsonFile(CurPath + "\\Json\\OldTree.json");
            List<Dictionary<string, int>> OldTree = JsonConvert.DeserializeObject<List<Dictionary<string, int>>>(js);

            //Manager.Instance.Save("_u");
            List<Dictionary<string, int>> ExTree = new List<Dictionary<string, int>>();
            for (int i = 0; i < MaxTree; i++)
            {
                ExTree.Add(new Dictionary<string, int>());
            }
            if(OldTree != null)
            {
                //新比旧多的
                for (int i = 0; i < MaxTree; i++)
                {
                    foreach (var item in Nodes[i])
                    {
                        if (!OldTree[i].ContainsKey(item.Key))
                        {
                            ExTree[i].Add(item.Key, item.Value);
                        }
                        else
                        {
                            if (OldTree[i][item.Key] > item.Value || OldTree[i][item.Key] == 0)
                            {
                                ExTree[i].Add(item.Key, item.Value);
                            }
                            else if(OldTree[i][item.Key] < item.Value)
                            {
                                var ls =GetLSFiles(item.Key);
                                foreach (var wj in ls)
                                {
                                    if (wj.Contains("/"))
                                    {
                                        var wjj = wj.Remove(wj.Length-1, 1);
                                        if (!OldTree[i + 1].ContainsKey(item.Key + "/" + wjj))
                                        {
                                            ExTree[i].Add(item.Key + "/" + wjj, 0);
                                        }
                                    }
                                    else
                                    {
                                        ExTree[i].Add(item.Key + "/" + wj, 0);
                                    }
                                }
                            }
                        }
                    }
                }
                //新比旧少的

                for (int i = 0; i < MaxTree; i++)
                {
                    foreach (var item in OldTree[i])
                    {
                        if (!Nodes[i].ContainsKey(item.Key))
                        {
                            if (System.IO.Directory.Exists(CurPath + "\\nshm" + item.Key))
                            {
                                ExTree[i].Add(item.Key, 0);
                            }
                        }
                    }
                }
            }

            return ExTree;
        }

        public List<Dictionary<string, int>> Nodes = new List<Dictionary<string, int>>();

        public class FatherNode
        {
            public int Layout;

            public string Name;

            public int lv;
        }

        public List<string> GetLSFiles(string realpath)
        {
            List<string> lsFiles = new List<string>();

            // commnd = Pan + " & cd " + CurPath + "\\svn & ";
            //commnd += "svn ls " + "https://svn-nshm.leihuo.netease.com/svn/nshm/trunk" + realpath; commnd += " & ";


            var process = new Process();
            process.StartInfo.FileName = SvnPath;
            process.StartInfo.Arguments = "ls " + "https://svn-nshm.leihuo.netease.com/svn/nshm/trunk" + realpath; 
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    lsFiles.Add(e.Data );
                    //outString.Add(e.Data);
                }
            };
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    //errString.Add(e.Data);
                }
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Close();

            return lsFiles;
        }

        public string[] GetPathSpit(string Path)
        {
            var path = Path.Replace('\\', '/');
            string[] strings = path.Split('/');

            return strings;
        }

        public string GetFatherPath(string Path)
        {
            string[] strings = GetPathSpit(Path);
            string realpath = "";

            for (int i = 0; i < strings.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(strings[i]))
                {
                    realpath += "/";
                    realpath += strings[i];
                }
            }

            return realpath;
        }
        public string GetPathName(string Path)
        {
            string[] strings = GetPathSpit(Path);
            string realpath = "";
            if(strings[strings.Length - 1] != "")
            {
                realpath = strings[strings.Length - 1];
            }
            else
            {
                realpath = strings[strings.Length - 2];
            }

            return realpath;
        }

        public Color GetLvColor(int i)
        {
            Color color = new Color();
            switch (i)
            {
                case 0:
                    color = Color.FromArgb(255, 255, 192);
                    break;
                case 1:
                    color = Color.FromArgb(192, 255, 192);
                    break;
                case 2:
                    color = Color.FromArgb(192, 255, 255);
                    break;
                case 3:
                    color = Color.Silver;
                    break;
                default:
                    color = Color.FromArgb(255, 128, 128);
                    break;
            }
            return color;
        }

        public int GetColorLv(Color color)
        {


            return 0;
        }

        public FatherNode GetFarFN(int lay,string path)
        {
            var fn = new FatherNode();
            fn.Name = null;

            List<ItemDef.spit> spits = new List<ItemDef.spit>();

            path = path.Replace('\\', '/');
            string[] strings = path.Split('/');
            string realpath = "";
            int lv = 0;

            for (int i = 0; i < strings.Length - 1; i++)
            {
                if (!string.IsNullOrEmpty(strings[i]))
                {
                    realpath += "/";
                    realpath += strings[i];
                    spits.Add(new ItemDef.spit(realpath, lv, ItemDef.PathType.Only));
                    lv++;
                }
            }

            realpath += "/";
            realpath += strings[strings.Length - 1];
            spits.Add(new ItemDef.spit(realpath, lv, ItemDef.PathType.Only));


            for (int i = 0; i < spits.Count; i++)
            {
                if (Nodes[i].ContainsKey(spits[i].realpath))
                {
                    if (Nodes[i][spits[i].realpath] == 1)
                    {
                        fn.Name = spits[i].realpath;
                        fn.Layout = i;
                        fn.lv = spits[i].Lv;
                        return fn;
                    }
                }
            }

            return fn;
        }
    }
}
