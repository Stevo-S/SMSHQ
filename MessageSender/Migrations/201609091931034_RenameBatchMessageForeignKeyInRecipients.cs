namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameBatchMessageForeignKeyInRecipients : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BatchMessageRecipients", "Message_Id", "dbo.BatchMessages");
            DropIndex("dbo.BatchMessageRecipients", new[] { "Message_Id" });
            RenameColumn(table: "dbo.BatchMessageRecipients", name: "Message_Id", newName: "MessageId");
            AlterColumn("dbo.BatchMessageRecipients", "MessageId", c => c.Int(nullable: false));
            CreateIndex("dbo.BatchMessageRecipients", "MessageId");
            AddForeignKey("dbo.BatchMessageRecipients", "MessageId", "dbo.BatchMessages", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BatchMessageRecipients", "MessageId", "dbo.BatchMessages");
            DropIndex("dbo.BatchMessageRecipients", new[] { "MessageId" });
            AlterColumn("dbo.BatchMessageRecipients", "MessageId", c => c.Int());
            RenameColumn(table: "dbo.BatchMessageRecipients", name: "MessageId", newName: "Message_Id");
            CreateIndex("dbo.BatchMessageRecipients", "Message_Id");
            AddForeignKey("dbo.BatchMessageRecipients", "Message_Id", "dbo.BatchMessages", "Id");
        }
    }
}
