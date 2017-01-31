namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _4 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Histories", name: "Cartridge_Id", newName: "Item_Id");
            RenameIndex(table: "dbo.Histories", name: "IX_Cartridge_Id", newName: "IX_Item_Id");
            DropColumn("dbo.Histories", "Item");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Histories", "Item", c => c.String());
            RenameIndex(table: "dbo.Histories", name: "IX_Item_Id", newName: "IX_Cartridge_Id");
            RenameColumn(table: "dbo.Histories", name: "Item_Id", newName: "Cartridge_Id");
        }
    }
}
