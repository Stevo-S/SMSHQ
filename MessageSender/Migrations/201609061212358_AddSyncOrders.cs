namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSyncOrders : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SyncOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 20),
                        UserType = c.Int(nullable: false),
                        ProductId = c.String(maxLength: 32),
                        ServiceId = c.String(maxLength: 32),
                        ServicesList = c.String(maxLength: 1024),
                        UpdateType = c.Int(nullable: false),
                        UpdateDescription = c.String(maxLength: 32),
                        UpdateTime = c.DateTime(nullable: false),
                        EffectiveTime = c.DateTime(nullable: false),
                        ExpiryTime = c.DateTime(nullable: false),
                        TransactionId = c.String(maxLength: 64),
                        OrderKey = c.String(maxLength: 64),
                        MDSPSUBEXPMODE = c.Int(nullable: false),
                        ObjectType = c.Int(nullable: false),
                        RentSuccess = c.Boolean(nullable: false),
                        TraceUniqueId = c.String(maxLength: 64),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SyncOrders");
        }
    }
}
