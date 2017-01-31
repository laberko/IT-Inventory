namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _24 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Histories", "Person_Id", "dbo.People");
            DropForeignKey("dbo.People", "History_Id", "dbo.Histories");
            DropIndex("dbo.Histories", new[] { "Person_Id" });
            DropIndex("dbo.People", new[] { "History_Id" });
            DropColumn("dbo.Histories", "Person_Id");
            DropColumn("dbo.People", "History_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.People", "History_Id", c => c.Int());
            AddColumn("dbo.Histories", "Person_Id", c => c.Int());
            CreateIndex("dbo.People", "History_Id");
            CreateIndex("dbo.Histories", "Person_Id");
            AddForeignKey("dbo.People", "History_Id", "dbo.Histories", "Id");
            AddForeignKey("dbo.Histories", "Person_Id", "dbo.People", "Id");
        }
    }
}
