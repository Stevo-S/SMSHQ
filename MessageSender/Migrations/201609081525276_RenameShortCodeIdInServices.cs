namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameShortCodeIdInServices : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Services", "ShortCode_Id", "dbo.ShortCodes");
            DropIndex("dbo.Services", new[] { "ShortCode_Id" });
            RenameColumn(table: "dbo.Services", name: "ShortCode_Id", newName: "ShortCodeId");
            AlterColumn("dbo.Services", "ShortCodeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Services", "ShortCodeId");
            AddForeignKey("dbo.Services", "ShortCodeId", "dbo.ShortCodes", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Services", "ShortCodeId", "dbo.ShortCodes");
            DropIndex("dbo.Services", new[] { "ShortCodeId" });
            AlterColumn("dbo.Services", "ShortCodeId", c => c.Int());
            RenameColumn(table: "dbo.Services", name: "ShortCodeId", newName: "ShortCode_Id");
            CreateIndex("dbo.Services", "ShortCode_Id");
            AddForeignKey("dbo.Services", "ShortCode_Id", "dbo.ShortCodes", "Id");
        }
    }
}
