namespace MessageSender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeLastUnsubcriptionDateNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Subscribers", "LastUnsubscriptionDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Subscribers", "LastUnsubscriptionDate", c => c.DateTime(nullable: false));
        }
    }
}
