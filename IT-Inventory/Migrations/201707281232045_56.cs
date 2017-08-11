namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _56 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Computers", "LastReportDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Computers", "LastReportDate");
        }
    }
}
