namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProductIdToSubscribers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Subscribers", "ProductId", c => c.String(maxLength: 16));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Subscribers", "ProductId");
        }
    }
}
