namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Histories", "Item", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Histories", "Item");
        }
    }
}
