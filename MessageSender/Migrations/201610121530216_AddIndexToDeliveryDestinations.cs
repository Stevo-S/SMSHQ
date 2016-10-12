namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIndexToDeliveryDestinations : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Deliveries", "Destination", name: "IX_Deliveries_Destination");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Deliveries", "IX_Deliveries_Destination");
        }
    }
}
