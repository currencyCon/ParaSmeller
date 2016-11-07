using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrencyAnalyzer.Diagnostics;

namespace ConcurrencyChecker.Configuration
{
    public class ConfigurationManager
    {
        public readonly string Filepath = "%appdata%/ConcurrencyChecker";
        public readonly string Filename = "/Checker.config";

        public Configuration LoadConfiguration()
        {   
            try
            {
                /*Configuration configuration = new Configuration();
                string path = Environment.ExpandEnvironmentVariables(Filepath);
                System.IO.StreamReader file = new System.IO.StreamReader(path + Filename);
                configuration.SelectedSmells = Convert(file.ReadLine());
                configuration.MaxDepthAsync = Int32.Parse(file.ReadLine());
                file.Close();*/
                return null;
            }
            catch (Exception)
            {
                return DefaultConfiguration();
            }

        }

        public Configuration DefaultConfiguration()
        {
            Configuration configuration = new Configuration();
            configuration.MaxDepthAsync = 3;
            configuration.SelectedSmells = Enum.GetValues(typeof (Smell)).Cast<string>().ToList();
            return configuration;
        }


        public static List<string> Convert(String str)
        {
            if (str == null) return new List<string>();
            return str.Split(';').ToList();
        }
    }
}
