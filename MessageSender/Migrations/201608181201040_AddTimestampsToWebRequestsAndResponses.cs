namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTimestampsToWebRequestsAndResponses : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WebRequests", "Timestamp", c => c.DateTime(nullable: false));
            AddColumn("dbo.WebResponses", "Timestamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WebResponses", "Timestamp");
            DropColumn("dbo.WebRequests", "Timestamp");
        }
    }
}
