using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeClientMigrator.EntityDB
{
    class Process
    {
        public virtual int ProcessId { get; set; }
        public virtual bool InitialProcess { get; set; }
        public virtual bool Processing { get; set; }
        public virtual bool Finish { get; set; }
        public virtual DateTime DateInitialProcess { get; set; }
        public virtual DateTime DateProcessing { get; set; }
        public virtual DateTime DateFinish { get; set; }
    }
}
