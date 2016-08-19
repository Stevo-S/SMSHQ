namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTimestampToBatchMessageRecipients : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BatchMessageRecipients", "Timestamp", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BatchMessageRecipients", "Timestamp");
        }
    }
}
