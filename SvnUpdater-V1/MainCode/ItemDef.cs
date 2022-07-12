using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SvnUpdater_V1
{
    public  class ItemDef
    {

        public string ClassUse;

        public string Path;

        /// <summary>
        /// 0All,1File,2Only
        /// </summary>
        public int Kind = 0;
        public enum PathType
        {
            //黑名单
            Black,
            //所有文件与文件夹
            All,
            //只有文件
            File,
            //只有文件夹
            Only
        }

        public static string GetType(int i)
        {
            string type = " empty ";
            switch (i)
            {
                case 1:
                    type = " infinity ";
                    break;
                case 2:
                    type = " files ";
                    break;
                case 3:
                    type = " empty ";
                    break;
                case 0:
                    type = " exclude ";
                    break;
                default:
                    break;
            }
            return type;
        }

        public class spit
        {
            public spit(string path,int lv,PathType type)
            {
                Lv = lv;
                realpath = path;
                this.type = type;   
            }
            public int Lv = 0; 

            public PathType type = PathType.All;

            public string realpath;
        }

        public static List<spit> MySpit(ItemDef def, PathType type = PathType.All)
        {
            List<spit> spits = new List<spit>();

            var path = def.Path.Replace('\\', '/');
            string[] strings = path.Split('/');
            string realpath = "";
            int lv = 0;

            for (int i = 0; i < strings.Length-1; i++) 
            {
                if (!string.IsNullOrEmpty(strings[i]))
                {
                    realpath += "/";
                    realpath += strings[i];
                    spits.Add(new spit(realpath,lv, PathType.Only));
                    lv++;
                }
            }

            realpath += "/";
            realpath += strings[strings.Length-1];
            spits.Add(new spit(realpath,lv, type));

            return spits;
        }

        
    }

    
}
