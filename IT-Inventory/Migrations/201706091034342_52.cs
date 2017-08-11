namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _52 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Computers", "Monitor", c => c.String());
            AddColumn("dbo.Computers", "MonitorInvent", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Computers", "MonitorInvent");
            DropColumn("dbo.Computers", "Monitor");
        }
    }
}
