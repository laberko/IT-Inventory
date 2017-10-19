namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _72 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Computers", "MonitorFixed", c => c.String());
            DropColumn("dbo.Computers", "CpuFixed");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Computers", "CpuFixed", c => c.String());
            DropColumn("dbo.Computers", "MonitorFixed");
        }
    }
}
