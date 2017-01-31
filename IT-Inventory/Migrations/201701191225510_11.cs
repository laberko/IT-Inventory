namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _11 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItemAttributes", "Name", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItemAttributes", "Name", c => c.String(nullable: false));
        }
    }
}
