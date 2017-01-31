namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.People", "Department_Id", "dbo.Departments");
            DropIndex("dbo.People", new[] { "Department_Id" });
            DropColumn("dbo.People", "Department_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.People", "Department_Id", c => c.Int());
            CreateIndex("dbo.People", "Department_Id");
            AddForeignKey("dbo.People", "Department_Id", "dbo.Departments", "Id");
        }
    }
}
