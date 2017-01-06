using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MessageSender.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<ExpressMessage> ExpressMessages { get; set; }
        public DbSet<ScheduledMessage> ScheduledMessages { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<BatchMessage> BatchMessages { get; set; }
        public DbSet<BatchMessageRecipient> BatchMessageRecipients { get; set; }
        public DbSet<WebRequest> WebRequests { get; set; }
        public DbSet<WebResponse> WebResponses { get; set; }
        public DbSet<SyncOrder> SyncOrders { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<ShortCode> ShortCodes { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<SubscriptionWelcomeMessage> SubscriptionWelcomeMessages { get; set; }
        public DbSet<OutboundMessage> OutboundMessages { get; set; }
    }
}