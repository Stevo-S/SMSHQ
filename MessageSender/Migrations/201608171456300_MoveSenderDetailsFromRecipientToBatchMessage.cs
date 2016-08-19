namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveSenderDetailsFromRecipientToBatchMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BatchMessages", "ServiceId", c => c.String());
            AddColumn("dbo.BatchMessages", "Sender", c => c.String());
            DropColumn("dbo.BatchMessageRecipients", "ServiceId");
            DropColumn("dbo.BatchMessageRecipients", "Sender");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BatchMessageRecipients", "Sender", c => c.String());
            AddColumn("dbo.BatchMessageRecipients", "ServiceId", c => c.String());
            DropColumn("dbo.BatchMessages", "Sender");
            DropColumn("dbo.BatchMessages", "ServiceId");
        }
    }
}
