namespace IT_Inventory.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _14 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ItemAttributeValues", name: "Item_Id", newName: "ParentItem_Id");
            RenameIndex(table: "dbo.ItemAttributeValues", name: "IX_Item_Id", newName: "IX_ParentItem_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ItemAttributeValues", name: "IX_ParentItem_Id", newName: "IX_Item_Id");
            RenameColumn(table: "dbo.ItemAttributeValues", name: "ParentItem_Id", newName: "Item_Id");
        }
    }
}
