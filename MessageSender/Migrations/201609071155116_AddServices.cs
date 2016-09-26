namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddServices : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Services",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 32),
                        ServiceId = c.String(maxLength: 32),
                        SmsNotificationCorrelator = c.String(maxLength: 16),
                        SmsNotificationCriteria = c.String(maxLength: 16),
                        ShortCode_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ShortCodes", t => t.ShortCode_Id)
                .Index(t => t.ServiceId, unique: true)
                .Index(t => t.ShortCode_Id);
            
            AlterColumn("dbo.ShortCodes", "Code", c => c.String(maxLength: 32));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Services", "ShortCode_Id", "dbo.ShortCodes");
            DropIndex("dbo.Services", new[] { "ShortCode_Id" });
            DropIndex("dbo.Services", new[] { "ServiceId" });
            AlterColumn("dbo.ShortCodes", "Code", c => c.String());
            DropTable("dbo.Services");
        }
    }
}
