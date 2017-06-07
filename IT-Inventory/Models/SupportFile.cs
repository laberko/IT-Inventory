using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_Inventory.Models
{
    public class SupportFile
    {
        [Key]
        public int Id { get; set; }

        public string Path { get; set; }

        [NotMapped]
        public bool IsImage
        {
            get
            {
                var extension = System.IO.Path.GetExtension(Path);
                return extension == ".jpg"
                       || extension == ".jpeg"
                       || extension == ".gif"
                       || extension == ".png"
                       || extension == ".tiff";
            }
        }

        [NotMapped]
        public string FileName => System.IO.Path.GetFileName(Path);
    }
}