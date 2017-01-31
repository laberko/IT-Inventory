namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _28 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Printers", "Office_Id", "dbo.Offices");
            DropIndex("dbo.Printers", new[] { "Office_Id" });
            DropColumn("dbo.Printers", "Office_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Printers", "Office_Id", c => c.Int());
            CreateIndex("dbo.Printers", "Office_Id");
            AddForeignKey("dbo.Printers", "Office_Id", "dbo.Offices", "Id");
        }
    }
}
