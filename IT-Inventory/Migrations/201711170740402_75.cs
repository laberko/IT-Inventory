namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _75 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.People", "Level", c => c.Int(nullable: false));
            AddColumn("dbo.Departments", "Level", c => c.Int(nullable: false));
            AddColumn("dbo.SubDepartments", "Level", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SubDepartments", "Level");
            DropColumn("dbo.Departments", "Level");
            DropColumn("dbo.People", "Level");
        }
    }
}
