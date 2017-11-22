namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _77 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.People", "Office_Id", c => c.Int());
            CreateIndex("dbo.People", "Office_Id");
            AddForeignKey("dbo.People", "Office_Id", "dbo.Offices", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.People", "Office_Id", "dbo.Offices");
            DropIndex("dbo.People", new[] { "Office_Id" });
            DropColumn("dbo.People", "Office_Id");
        }
    }
}
