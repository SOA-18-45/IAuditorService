using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAuditorService
{
    public static class Logger
    {
        public static void Log(string str)
        {
            Console.WriteLine(str);
        }

        public static void LogError(string str)
        {
            Log("[ERROR] " + str);
        }
    }
}
