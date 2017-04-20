namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _37 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SupportRequests", "Person_Id", "dbo.People");
            AddForeignKey("dbo.SupportRequests", "Person_Id", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SupportRequests", "Person_Id", "dbo.People");
            AddForeignKey("dbo.SupportRequests", "Person_Id", "dbo.People", "Id", cascadeDelete: true);
        }
    }
}
