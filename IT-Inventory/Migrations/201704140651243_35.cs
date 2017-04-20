namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _35 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.People", "Email", c => c.String());
            AddColumn("dbo.People", "PhoneNumber", c => c.String());
            AddColumn("dbo.People", "Dep_Id", c => c.Int());
            CreateIndex("dbo.People", "Dep_Id");
            AddForeignKey("dbo.People", "Dep_Id", "dbo.Departments", "Id");
            DropColumn("dbo.People", "IsItUser");
        }
        
        public override void Down()
        {
            AddColumn("dbo.People", "IsItUser", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.People", "Dep_Id", "dbo.Departments");
            DropIndex("dbo.People", new[] { "Dep_Id" });
            DropColumn("dbo.People", "Dep_Id");
            DropColumn("dbo.People", "PhoneNumber");
            DropColumn("dbo.People", "Email");
        }
    }
}
