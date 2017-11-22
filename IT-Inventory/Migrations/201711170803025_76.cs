namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _76 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.People", "LevelDep1", c => c.Int(nullable: false));
            AddColumn("dbo.People", "LevelDep2", c => c.Int(nullable: false));
            DropColumn("dbo.People", "Level");
        }
        
        public override void Down()
        {
            AddColumn("dbo.People", "Level", c => c.Int(nullable: false));
            DropColumn("dbo.People", "LevelDep2");
            DropColumn("dbo.People", "LevelDep1");
        }
    }
}
