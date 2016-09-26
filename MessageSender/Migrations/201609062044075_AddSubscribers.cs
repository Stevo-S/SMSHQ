namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubscribers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Subscribers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PhoneNumber = c.String(maxLength: 20),
                        ServiceId = c.String(maxLength: 16),
                        isActive = c.Boolean(nullable: false),
                        FirstSubscriptionDate = c.DateTime(nullable: false),
                        LastSubscriptionDate = c.DateTime(nullable: false),
                        LastUnsubscriptionDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Subscribers");
        }
    }
}
