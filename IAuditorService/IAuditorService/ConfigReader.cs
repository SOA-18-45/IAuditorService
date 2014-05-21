using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAuditorService
{
    public static class ConfigReader
    {
        private const string readerPath = @"..\..\config.txt";
        private static string[] parameters = null;

        private static void Initialize()
        {
            Logger.Log("ConfigReader: Initializing...");
            string configFile = System.IO.File.ReadAllText(readerPath);
            configFile.Replace(" ", String.Empty);
            configFile.Replace("\r", String.Empty);
            parameters = configFile.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Logger.Log("ConfigReader: Initialized!");
        }

        public static string GetParameter(string name)
        {
            if (parameters == null)
                Initialize();
            foreach (string str in parameters)
            {
                if (str.Substring(0, name.Length).Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return str.Substring(name.Length + 1);
                }
            }
            Logger.LogError("ConfigReader: requested parameter ("+name+") not found in the config file!");
            return string.Empty;
        }
    }
}
