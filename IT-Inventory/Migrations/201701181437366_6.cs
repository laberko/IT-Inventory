namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _6 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Cartridges", newName: "Items");
            CreateTable(
                "dbo.ItemAttributes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Item_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Items", t => t.Item_Id)
                .Index(t => t.Item_Id);
            
            CreateTable(
                "dbo.ItemTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItemAttributeValues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        Attribute_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItemAttributes", t => t.Attribute_Id)
                .Index(t => t.Attribute_Id);
            
            AddColumn("dbo.Items", "ItemType_Id", c => c.Int());
            CreateIndex("dbo.Items", "ItemType_Id");
            AddForeignKey("dbo.Items", "ItemType_Id", "dbo.ItemTypes", "Id");
            DropColumn("dbo.Items", "Color");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Items", "Color", c => c.String());
            DropForeignKey("dbo.ItemAttributeValues", "Attribute_Id", "dbo.ItemAttributes");
            DropForeignKey("dbo.Items", "ItemType_Id", "dbo.ItemTypes");
            DropForeignKey("dbo.ItemAttributes", "Item_Id", "dbo.Items");
            DropIndex("dbo.ItemAttributeValues", new[] { "Attribute_Id" });
            DropIndex("dbo.ItemAttributes", new[] { "Item_Id" });
            DropIndex("dbo.Items", new[] { "ItemType_Id" });
            DropColumn("dbo.Items", "ItemType_Id");
            DropTable("dbo.ItemAttributeValues");
            DropTable("dbo.ItemTypes");
            DropTable("dbo.ItemAttributes");
            RenameTable(name: "dbo.Items", newName: "Cartridges");
        }
    }
}
