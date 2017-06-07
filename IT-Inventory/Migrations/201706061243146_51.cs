namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _51 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SupportFiles", "ClientPath");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SupportFiles", "ClientPath", c => c.String());
        }
    }
}
