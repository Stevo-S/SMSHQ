namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MessageSender.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            // Prevent command timeouts during long-running database migration commands
            CommandTimeout = Int32.MaxValue;
        }

        protected override void Seed(MessageSender.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            // Add default subscription welcome messages for pre-existing services
            // without any subscription welcome message
            foreach (var service in context.Services.ToList())
            {
                if (!service.SubscriptionWelcomeMessages.Any())
                {
                    var welcomeMessage = new Models.SubscriptionWelcomeMessage
                    {
                        MessageText = "Welcome to " + service.Name + ".",
                        Service = service,
                        StartTime = "00:00:00",
                        EndTime = "23:59:59",
                        Priority = 1,
                        StartDate = DateTime.Now
                    };
                    context.SubscriptionWelcomeMessages.Add(welcomeMessage);
                }
            }
            
        }
    }
}
