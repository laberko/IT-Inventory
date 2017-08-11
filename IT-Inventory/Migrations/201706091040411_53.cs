namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _53 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComputerHistoryItems", "HistoryMonitor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ComputerHistoryItems", "HistoryMonitor");
        }
    }
}
