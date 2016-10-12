namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IndexDeliveryStatus : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Deliveries", "DeliveryStatus");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Deliveries", new[] { "DeliveryStatus" });
        }
    }
}
