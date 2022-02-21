using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UploadFTPFilesCore.Models
{
    public class SampleForm
    {
        public int Id { get; set; }

        [Required, MaxLength(250)]
        public string Title { get; set; }

        [Required, MaxLength(2500)]
        public string Description { get; set; }

        [Required]
        public byte[] Photo { get; set; }
    }
}
