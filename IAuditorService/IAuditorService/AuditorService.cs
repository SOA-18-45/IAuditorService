using Contracts;
using log4net;
using log4net.Config;
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
        private AuditsRegister audRegister;
        private static readonly ILog log = LogManager.GetLogger(typeof(AuditorService));

        private void CreateAuditsRegister()
        {
            //audRegister = new VMAuditsRegister();
            audRegister = new DBAuditsRegister();
            //DBTest();
        }

        private bool RegisterService()
        {
            log.Info("Registering service in ServiceRepository...");
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IServiceRepository> cf = new ChannelFactory<IServiceRepository>(binding, ConfigReader.GetParameter("ServiceRepoAddress"));
            try
            {
                serviceRepository = cf.CreateChannel();
                serviceRepository.registerService(ConfigReader.GetParameter("ServiceName"), ConfigReader.GetParameter("ServiceAddress"));
            }
            catch
            {
                log.Error("Couldn't connect to service repository. Check configuration and retry. ");
                return false;
            }
            var timer = new System.Threading.Timer(e => Ping(), null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            log.Info("Successfully registered service in ServiceRepository!");
            return true;
        }

        private void Ping()
        {
            serviceRepository.isAlive(ConfigReader.GetParameter("ServiceName"));
            log.Debug("Ping...");
        }

        private void UnregisterService()
        {
            log.Info("Unregistering service from ServiceRepository...");
            serviceRepository.unregisterService(ConfigReader.GetParameter("ServiceName"));
            sh.Close();
            log.Info("Service unregistered!");
        }

        public static void StartService()
        {
            BasicConfigurator.Configure();
            log.Info("AuditorService starting up...");
            AuditorService service = new AuditorService();
            if (!service.CreateServiceHost())
            {
                Console.ReadKey();
                return;
            }
            service.CreateAuditsRegister();
            log.Info("AuditorService is up!");
            if (!service.RegisterService())
            {
                Console.ReadKey();
                return;
            }

            Console.ReadKey();
            service.UnregisterService();
        }

        private bool CreateServiceHost()
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
            try
            {
                sh.Open();
            }
            catch
            {
                log.Error("Couldn't open service host. Check configuration and retry. ");
                return false;
            }
            return true;
        }

        private void RegisterAudit(AuditType type)
        {
            Audit audit = new Audit();
            audit.AuditDate = DateTime.Now.ToString();
            audit.Id = Guid.NewGuid();
            audit.Type = type;
            audRegister.RegisterAudit(audit);
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
            log.Info("Connecting to account repository...");
            string accountRepoAddress = serviceRepository.getServiceAddress("IAccountRepository");
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            ChannelFactory<IAccountRepository> cf = new ChannelFactory<IAccountRepository>(binding, accountRepoAddress);
            IAccountRepository accountRepo = cf.CreateChannel();
            return accountRepo;
        }

        public List<Audit> GetAllAuditsData()
        {
            return audRegister.GetAllAudits();
        }

        private void DBTest()
        {
            RegisterAudit(AuditType.AccountByTypeCount);
            RegisterAudit(AuditType.AccountByTypeCount);
            RegisterAudit(AuditType.Accounts);
            RegisterAudit(AuditType.ClientCount);
            RegisterAudit(AuditType.AccountByTypeCount);
            RegisterAudit(AuditType.ClientCount);
            RegisterAudit(AuditType.AccountsByType);
            List<Audit> audits = GetAllAuditsData();
            foreach (Audit audit in audits)
            {
                log.Info(audit.Id+"   "+audit.AuditDate + "   " + audit.Type);
            }
        }
    }
}
