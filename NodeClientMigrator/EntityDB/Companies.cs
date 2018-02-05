using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeClientMigrator.EntityDB
{
    class Companies
    {
        public virtual int CompanyId { get; set; }
        public virtual string Name { get; set; }
        public virtual string IdentificationNumber { get; set; }
        public virtual int CompanyDbId { get; set; }
        public virtual int LogId { get; set; }
    }
}
