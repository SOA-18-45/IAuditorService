using Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAuditorService
{
    public interface AuditsRegister
    {
        void RegisterAudit(Audit audit);
        List<Audit> GetAllAudits();
    }

    public class VMAuditsRegister : AuditsRegister
    {
        private List<Audit> audits;

        public VMAuditsRegister()
        {
            audits = new List<Audit>();
        }

        public void RegisterAudit(Audit audit)
        {
            audits.Add(audit);
        }

        public List<Audit> GetAllAudits()
        {
            return audits;
        }
    }

    public class DBAuditsRegister : AuditsRegister
    {

        public void RegisterAudit(Audit audit)
        {
            using (var db = new AuditContext())
            {
                Logger.Log("Registering audit");
                db.Audits.Add(audit);
                db.SaveChanges();
            }
        }

        public List<Audit> GetAllAudits()
        {
            using (var db = new AuditContext())
            {
                var query = from a in db.Audits select a;
                return new List<Audit>(query);
            }
        }
    }

    public class AuditContext : DbContext
    {
        public DbSet<Audit> Audits { get; set; }
    }
}
