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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class AuditorService : IAudtitorService
    {
        private IServiceRepository serviceRepository;
        private ServiceHost sh;
        private AuditsRegister AudRegister;

        private void CreateAuditsRegister()
        {
            AudRegister = new VMAuditsRegister();
        }

        private void RegisterService()
        {
            Logger.Log("Registering service in ServiceRepository...");
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, ConfigReader.GetParameter("ServiceRepoAddress"));
            serviceRepository = cf.CreateChannel();
            serviceRepository.registerService(ConfigReader.GetParameter("ServiceName"), ConfigReader.GetParameter("ServiceAddress"));
            var timer = new System.Threading.Timer(e => Ping(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Logger.Log("Successfully registered service in ServiceRepository!");
        }

        private void Ping()
        {
            serviceRepository.isAlive(ConfigReader.GetParameter("ServiceName"));
            Logger.Log("Ping...");
        }

        private void UnregisterService()
        {
            Logger.Log("Unregistering service from ServiceRepository...");
            serviceRepository.unregisterService(ConfigReader.GetParameter("ServiceName"));
            sh.Close();
            Logger.Log("Service unregistered!");
        }

        public static void StartService()
        {
            Logger.Log("AuditorService starting up...");
            AuditorService service = new AuditorService();
            service.CreateServiceHost();
            service.CreateAuditsRegister();
            Logger.Log("AuditorService is up!");
            service.RegisterService();

            Console.ReadLine();
            service.UnregisterService();
        }

        private void CreateServiceHost()
        {
            sh = new ServiceHost(this, new Uri[] { new Uri(ConfigReader.GetParameter("ServiceAddress")) });
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            sh.AddServiceEndpoint(typeof(IAudtitorService), binding, ConfigReader.GetParameter("ServiceAddress"));
            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }
            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
            sh.Open();
        }

        private void RegisterAudit(AuditType type)
        {
            Audit audit = new Audit();
            audit.AuditDate = DateTime.Now.ToString();
            audit.Id = new Guid();
            audit.Type = type;
            AudRegister.RegisterAudit(audit);
        }

        public int GetAccountCount()
        {
            RegisterAudit(AuditType.AccountCount);
            IAccountRepository accountRepo = connectToAccountRepository();
            return accountRepo.GetAllAccounts().Count;
        }

        public List<BasicAccountInfo> GetAccounts()
        {
            RegisterAudit(AuditType.Accounts);
            IAccountRepository accountRepo = connectToAccountRepository();
            List<BasicAccountInfo> basicAccounts = new List<BasicAccountInfo>();
            List<AccountDetails> accounts = accountRepo.GetAllAccounts();
            foreach (AccountDetails account in accounts)
            {
                BasicAccountInfo basicInfo = new BasicAccountInfo();
                basicInfo.AccountNumber = account.AccountNumber;
                basicInfo.Money = account.Money;
                basicAccounts.Add(basicInfo);
            }
            return basicAccounts;
        }

        public int GetClientCount()
        {
            RegisterAudit(AuditType.ClientCount);
            string clientRepoAddress = serviceRepository.getServiceAddress("IClientRepository");
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IClientRepository> cf = new ChannelFactory<IClientRepository>(binding, clientRepoAddress);
            IClientRepository clientRepo = cf.CreateChannel();
            
            throw new NotImplementedException();
        }

        public int GetAccountCountByType(string type)
        {
            RegisterAudit(AuditType.AccountByTypeCount);
            IAccountRepository accountRepo = connectToAccountRepository();
            return accountRepo.GetAllAccounts().Count(e => e.Type.Equals(type));
        }

        public List<BasicAccountInfo> GetAccountsByType(string type)
        {
            RegisterAudit(AuditType.AccountsByType);
            IAccountRepository accountRepo = connectToAccountRepository();
            List<BasicAccountInfo> basicAccounts = new List<BasicAccountInfo>();
            List<AccountDetails> accounts = new List<AccountDetails>(accountRepo.GetAllAccounts().Where(e => e.Type.Equals(type)));
            foreach (AccountDetails account in accounts)
            {
                BasicAccountInfo basicInfo = new BasicAccountInfo();
                basicInfo.AccountNumber = account.AccountNumber;
                basicInfo.Money = account.Money;
                basicAccounts.Add(basicInfo);
            }
            return basicAccounts;
        }

        private IAccountRepository connectToAccountRepository()
        {
            string accountRepoAddress = serviceRepository.getServiceAddress("IAccountRepository");
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IAccountRepository> cf = new ChannelFactory<IAccountRepository>(binding, accountRepoAddress);
            IAccountRepository accountRepo = cf.CreateChannel();
            return accountRepo;
        }

        public List<Audit> GetAllAuditsData()
        {
            return AudRegister.GetAllAudits();
        }
    }
}
