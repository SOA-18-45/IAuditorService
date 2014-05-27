using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAuditorService
{
    public interface AuditsRegister
    {
        public void RegisterAudit(Audit audit);
        public List<Audit> GetAllAudits();
    }

    public class VMAuditsRegister : AuditsRegister
    {
        private List<Audit> audits;

        public VMAuditRegister()
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
}
