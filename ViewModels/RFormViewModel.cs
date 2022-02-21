using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace UploadFTPFilesCore.ViewModels
{
    public class RFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(250)]
        public string Title { get; set; }

        [Required, StringLength(2500)]
        public string Description { get; set; }
        [Display(Name = "Select Image ..")]
        public byte[] Photo { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
