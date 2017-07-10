using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Social.Job
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<SchedulerBootstrap>(s =>
                {
                    s.ConstructUsing(() => new SchedulerBootstrap());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());

                });
                x.RunAsLocalSystem();
                x.SetDescription("Comm100 Social Media Job.");
                x.SetDisplayName("Comm100 Social Media Job.");
                x.SetServiceName("Comm100.Social.Job");
                x.StartAutomaticallyDelayed();
                x.EnableServiceRecovery(action => action.RestartService(1));
            });
        }
    }
}
