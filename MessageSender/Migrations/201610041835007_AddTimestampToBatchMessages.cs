namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTimestampToBatchMessages : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BatchMessages", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BatchMessages", "CreatedAt");
        }
    }
}
