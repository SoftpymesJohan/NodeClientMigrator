using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeClientMigrator.EntityDB
{
    class LogDB
    {
        public virtual int LogId { get; set; }
        public virtual string DataBases { get; set; }
        public virtual bool InitialProcess { get; set; }
        public virtual bool Processing { get; set; }
        public virtual bool Finish { get; set; }
    }
}
