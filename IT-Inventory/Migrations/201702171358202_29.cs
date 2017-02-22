namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _29 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SupportRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        Urgency = c.Int(nullable: false),
                        Category = c.Int(nullable: false),
                        From_Id = c.Int(),
                        To_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.People", t => t.From_Id)
                .ForeignKey("dbo.People", t => t.To_Id)
                .Index(t => t.From_Id)
                .Index(t => t.To_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SupportRequests", "To_Id", "dbo.People");
            DropForeignKey("dbo.SupportRequests", "From_Id", "dbo.People");
            DropIndex("dbo.SupportRequests", new[] { "To_Id" });
            DropIndex("dbo.SupportRequests", new[] { "From_Id" });
            DropTable("dbo.SupportRequests");
        }
    }
}
