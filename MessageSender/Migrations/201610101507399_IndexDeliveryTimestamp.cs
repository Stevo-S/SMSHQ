namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IndexDeliveryTimestamp : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Deliveries", "TimeStamp", name: "IX_Deliveries_Timestamp");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Deliveries", "IX_Deliveries_Timestamp");
        }
    }
}
