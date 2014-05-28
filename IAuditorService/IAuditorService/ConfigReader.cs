using log4net;
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
        private static readonly ILog log = LogManager.GetLogger(typeof(ConfigReader));

        private static void Initialize()
        {
            log.Info("ConfigReader: Initializing...");
            string configFile = System.IO.File.ReadAllText(readerPath);
            configFile.Replace(" ", String.Empty);
            configFile.Replace("\r", String.Empty);
            configFile.Replace("\t", String.Empty);
            parameters = configFile.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            log.Info("ConfigReader: Initialized!");
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
            log.Error("ConfigReader: requested parameter ("+name+") not found in the config file!");
            return string.Empty;
        }
    }
}
