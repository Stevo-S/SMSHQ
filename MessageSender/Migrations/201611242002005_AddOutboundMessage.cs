namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOutboundMessage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OutboundMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Destination = c.String(maxLength: 20),
                        Text = c.String(),
                        Sender = c.String(maxLength: 16),
                        ServiceId = c.String(maxLength: 50),
                        LinkId = c.String(maxLength: 100),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OutboundMessages");
        }
    }
}
