namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubscriptionWelcomeMessages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SubscriptionWelcomeMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MessageText = c.String(maxLength: 1024),
                        StartTime = c.String(maxLength: 8),
                        EndTime = c.String(maxLength: 8),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        Priority = c.Int(nullable: false),
                        ServiceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Services", t => t.ServiceId, cascadeDelete: true)
                .Index(t => t.ServiceId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SubscriptionWelcomeMessages", "ServiceId", "dbo.Services");
            DropIndex("dbo.SubscriptionWelcomeMessages", new[] { "ServiceId" });
            DropTable("dbo.SubscriptionWelcomeMessages");
        }
    }
}
