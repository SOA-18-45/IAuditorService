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
            Console.WriteLine("AuditorService starting up...");
            AuditorService service= new AuditorService();
            ServiceHost sh = new ServiceHost(service, new Uri[] { new Uri("net.tcp://127.0.0.1:54321") });
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            sh.AddServiceEndpoint(typeof(IAudtitorService), binding, "net.tcp://127.0.0.1:54321/IAuditorService");
            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }
            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
            sh.Open();
            Console.WriteLine("AuditorService is up!");
            Console.ReadLine();
        }
    }

    [ServiceContract]
    public interface IAudtitorService
    {
        //typy IEnumerable będą dodane później
        [OperationContract]
        IEnumerable<int> getClients();
        [OperationContract]
        IEnumerable<int> getAccounts();

        //i tak dalej, i tak dalej
    }

    private class AuditorService : IAudtitorService
    {
        IEnumerable<int> getClients()
        {
            return null;
        }
        IEnumerable<int> getAccounts()
        {
            return null;
        }
    }
}
