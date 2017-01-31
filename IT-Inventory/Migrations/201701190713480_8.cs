namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItemAttributes", "IsNumber", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItemAttributeValues", "IsNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItemAttributeValues", "IsNumber", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItemAttributes", "IsNumber");
        }
    }
}
