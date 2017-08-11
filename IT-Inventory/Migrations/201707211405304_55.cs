namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _55 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SupportRequests", "HardwareId", c => c.Int(nullable: false));
            AddColumn("dbo.SupportRequests", "HardwareQuantity", c => c.Int(nullable: false));
            DropColumn("dbo.SupportRequests", "HardwareInstalled");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SupportRequests", "HardwareInstalled", c => c.String());
            DropColumn("dbo.SupportRequests", "HardwareQuantity");
            DropColumn("dbo.SupportRequests", "HardwareId");
        }
    }
}
