namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Histories", "Recieved", c => c.Boolean(nullable: false));
            AddColumn("dbo.Histories", "Person_Id", c => c.Int());
            CreateIndex("dbo.Histories", "Person_Id");
            AddForeignKey("dbo.Histories", "Person_Id", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Histories", "Person_Id", "dbo.People");
            DropIndex("dbo.Histories", new[] { "Person_Id" });
            DropColumn("dbo.Histories", "Person_Id");
            DropColumn("dbo.Histories", "Recieved");
        }
    }
}
