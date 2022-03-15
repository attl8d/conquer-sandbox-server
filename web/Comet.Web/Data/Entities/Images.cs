using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace Comet.Web.Data.Entities
{
    [Table("images")]
    public class Images
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FileDescription { get; set; }
        public virtual string MimeType { get; set; }
        public virtual string StoragePath { get; set; }
        public virtual string StorageName { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual DateTime? DeleteDate { get; set; }

        public virtual ApplicationUser User { get; set; }

        [NotMapped] public string StorageUrl => Path.Combine(StoragePath, StorageName);
    }
}
