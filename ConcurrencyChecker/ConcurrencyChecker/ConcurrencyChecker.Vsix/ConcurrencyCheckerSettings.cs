using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace ConcurrencyChecker.Vsix
{
    [Guid("43991993-21b1-4585-99d9-0ab9c65a8411")]
    public class ConcurrencyCheckerSettings : DialogPage
    {
        public readonly string Filepath = "%appdata%/ConcurrencyChecker";
        public readonly string Filename = "/Checker.config";

        public List<Smell> Smells => Enum.GetValues(typeof(Smell)).Cast<Smell>().ToList();
        private int maxDepthAsync = 3;

        public string SelectedSmellsStr
        {
            get { return Convert(SelectedSmells); }
            set { SelectedSmells = Convert(value); }
        }
        
        public List<string> SelectedSmells { get; set; }

        public int MaxDepthAsync
        {
            get { return maxDepthAsync; }
            set { maxDepthAsync = value; }
        }

        protected override IWin32Window Window
        {
            get
            {
                ConcurrencyCheckerSettingsUi page = new ConcurrencyCheckerSettingsUi();
                page.optionsPage = this;
                page.Initialize();
                return page;
            }
        }
        
        public static string Convert(List<String> list)
        {
            if (list == null) return string.Empty;
            return string.Join(";", list);
        }

        public override void SaveSettingsToStorage()
        {
            string path = Environment.ExpandEnvironmentVariables(Filepath);
            Directory.CreateDirectory(path);
            System.IO.StreamWriter file = new System.IO.StreamWriter(path+Filename);
            file.WriteLine(SelectedSmellsStr+"\n"+maxDepthAsync);
            file.Close();
        }

        public override void LoadSettingsFromStorage()
        {
            try
            {
                string path = Environment.ExpandEnvironmentVariables(Filepath);
                System.IO.StreamReader file = new System.IO.StreamReader(path+Filename);
                SelectedSmellsStr = file.ReadLine();
                maxDepthAsync = Int32.Parse(file.ReadLine());
                file.Close();
            }
            catch (Exception e)
            {
                
            }
        }

        public static List<string> Convert(String str)
        {
            if (str == null) return new List<string>();
            return str.Split(';').ToList();
        }
    }
}
