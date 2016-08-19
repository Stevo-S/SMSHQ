using Hangfire;
using MessageSender.Jobs;
using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(MessageSender.Startup))]
namespace MessageSender
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();

            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard();
            app.UseHangfireServer();
            ConfigureAuth(app);

            RecurringJob.AddOrUpdate(() => MessageJobs.SendScheduledMessages(), "10 8-20/2 * * *", TimeZoneInfo.Local);
        }
    }
}
