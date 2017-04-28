namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _43 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ComputerHistoryItems", "HistoryComputerOwner_Id", c => c.Int());
            CreateIndex("dbo.ComputerHistoryItems", "HistoryComputerOwner_Id");
            AddForeignKey("dbo.ComputerHistoryItems", "HistoryComputerOwner_Id", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ComputerHistoryItems", "HistoryComputerOwner_Id", "dbo.People");
            DropIndex("dbo.ComputerHistoryItems", new[] { "HistoryComputerOwner_Id" });
            DropColumn("dbo.ComputerHistoryItems", "HistoryComputerOwner_Id");
        }
    }
}
