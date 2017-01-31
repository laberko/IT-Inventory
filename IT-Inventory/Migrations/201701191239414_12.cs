namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _12 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItemAttributes", "ItemType_Id", "dbo.ItemTypes");
            DropIndex("dbo.ItemAttributes", new[] { "ItemType_Id" });
            CreateTable(
                "dbo.ItemTypeItemAttributes",
                c => new
                    {
                        ItemType_Id = c.Int(nullable: false),
                        ItemAttribute_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItemType_Id, t.ItemAttribute_Id })
                .ForeignKey("dbo.ItemTypes", t => t.ItemType_Id, cascadeDelete: true)
                .ForeignKey("dbo.ItemAttributes", t => t.ItemAttribute_Id, cascadeDelete: true)
                .Index(t => t.ItemType_Id)
                .Index(t => t.ItemAttribute_Id);
            
            AlterColumn("dbo.ItemAttributes", "Name", c => c.String(nullable: false));
            DropColumn("dbo.ItemAttributes", "ItemType_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItemAttributes", "ItemType_Id", c => c.Int());
            DropForeignKey("dbo.ItemTypeItemAttributes", "ItemAttribute_Id", "dbo.ItemAttributes");
            DropForeignKey("dbo.ItemTypeItemAttributes", "ItemType_Id", "dbo.ItemTypes");
            DropIndex("dbo.ItemTypeItemAttributes", new[] { "ItemAttribute_Id" });
            DropIndex("dbo.ItemTypeItemAttributes", new[] { "ItemType_Id" });
            AlterColumn("dbo.ItemAttributes", "Name", c => c.String());
            DropTable("dbo.ItemTypeItemAttributes");
            CreateIndex("dbo.ItemAttributes", "ItemType_Id");
            AddForeignKey("dbo.ItemAttributes", "ItemType_Id", "dbo.ItemTypes", "Id");
        }
    }
}
