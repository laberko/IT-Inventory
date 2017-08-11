namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _54 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SupportRequests", "HardwareReplaced");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SupportRequests", "HardwareReplaced", c => c.String());
        }
    }
}
