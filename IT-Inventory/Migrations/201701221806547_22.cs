namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _22 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.People", "History_Id", c => c.Int());
            CreateIndex("dbo.People", "History_Id");
            AddForeignKey("dbo.People", "History_Id", "dbo.Histories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.People", "History_Id", "dbo.Histories");
            DropIndex("dbo.People", new[] { "History_Id" });
            DropColumn("dbo.People", "History_Id");
        }
    }
}
