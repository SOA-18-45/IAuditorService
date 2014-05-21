using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace IAuditorService
{
    class Program
    {
        static void Main(string[] args)
        {
            AuditorService.StartService();
        }
    }
}
