namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _50 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SupportFiles", "ClientPath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SupportFiles", "ClientPath");
        }
    }
}
