namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _57 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "Location_Id", c => c.Int());
            CreateIndex("dbo.Items", "Location_Id");
            AddForeignKey("dbo.Items", "Location_Id", "dbo.Offices", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Items", "Location_Id", "dbo.Offices");
            DropIndex("dbo.Items", new[] { "Location_Id" });
            DropColumn("dbo.Items", "Location_Id");
        }
    }
}
