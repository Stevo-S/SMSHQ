namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddKeywordToSyncOrders : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SyncOrders", "Keyword", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SyncOrders", "Keyword");
        }
    }
}
