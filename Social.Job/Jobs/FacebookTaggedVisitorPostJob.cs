using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace Social.Job.Jobs
{
    public class FacebookTaggedVisitorPostJob : JobBase, ITransient
    {
        protected override Task ExecuteJob(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
