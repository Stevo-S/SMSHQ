// <auto-generated />
namespace MessageSender.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.3-40302")]
    public sealed partial class MoveSenderDetailsFromRecipientToBatchMessage : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(MoveSenderDetailsFromRecipientToBatchMessage));
        
        string IMigrationMetadata.Id
        {
            get { return "201608171456300_MoveSenderDetailsFromRecipientToBatchMessage"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
