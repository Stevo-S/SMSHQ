namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBatchMessageRecipients : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BatchMessageRecipients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Destination = c.String(),
                        ServiceId = c.String(),
                        Sender = c.String(),
                        Message_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BatchMessages", t => t.Message_Id)
                .Index(t => t.Message_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BatchMessageRecipients", "Message_Id", "dbo.BatchMessages");
            DropIndex("dbo.BatchMessageRecipients", new[] { "Message_Id" });
            DropTable("dbo.BatchMessageRecipients");
        }
    }
}
