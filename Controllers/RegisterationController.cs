using Aspose.Pdf;
using Aspose.Pdf.Drawing;
using FastMember;
using FluentFTP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UploadFTPFilesCore.Models;
using UploadFTPFilesCore.ViewModels;

namespace UploadFTPFilesCore.Controllers
{
    public class RegisterationController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public RegisterationController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        /// <summary>
        /// For displaying list of data
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var samples = await _context.sampleForms.OrderByDescending(m => m.Title).ToListAsync();
            return View(samples);
        }
        /// <summary>
        /// for getting create form
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var viewModel = new RFormViewModel
            {
            };

            return View("RegisterForm",viewModel);
        }
        /// <summary>
        /// for getting upload ftp form
        /// </summary>
        /// <returns></returns>
        public IActionResult UploadFTP()
        {
            var viewModel = new SampleForm
            {
            };

           return View("UploadFTP", viewModel);

        }
        public bool ftpTransfer(byte[] buffer, string fileName)
        {
            
            try
            {
                string ftpAddress = "192.168.1.3";
                string username = "ftp-user";
                string password = "123";

                WebRequest request = WebRequest.Create("ftp://" + ftpAddress + "/" + fileName);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(username, password);
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(buffer, 0, buffer.Length);
                //string s = Convert.ToBase64String(fileBytes);

                reqStream.Close();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// action for UploadFTP files to ftp server 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFTP(SampleForm data)
        {

            if (Request.Form.Files.Count() > 0)
            {
                var file = Request.Form.Files[0];

                string fileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
                string extension = System.IO.Path.GetExtension(file.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    this.ftpTransfer(fileBytes, fileName);
                    // act on the Base64 data
                }
            }
            var files = Request.Form.Files;

            if (!files.Any())
            {
                ModelState.AddModelError("Photo", "Please select an image!");
                return View("UploadFTP", data);
            }

            var photo = files.FirstOrDefault();

            using var dataStream = new MemoryStream();

            await photo.CopyToAsync(dataStream);

            var frms = new SampleForm
            {
                Title = data.Title,
                Description = data.Description,
                Photo = dataStream.ToArray()
            };

            _context.sampleForms.Add(frms);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        /// <summary>
        /// action for crating form data and save it to database
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("RegisterForm", model);
            }

            var files = Request.Form.Files;

            if (!files.Any())
            {
                ModelState.AddModelError("Photo", "Please select an image!");
                return View("RegisterForm", model);
            }

            var photo = files.FirstOrDefault();

            using var dataStream = new MemoryStream();

            await photo.CopyToAsync(dataStream);

            var frms = new SampleForm
            {
                Title = model.Title,
                Description = model.Description,
                Photo = dataStream.ToArray()
            };

            _context.sampleForms.Add(frms);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// to save pdf file
        /// </summary>
        /// <returns></returns>
        public async  Task< IActionResult> ExportPDF()
        {
            //It is not the best choice for using 
            //the first line in the page always:
            //Evaluation Only. Created with Aspose.PDF. Copyright 2002-2021 Aspose Pty Ltd.
            #region exportPDF using Aposepdf package

            var doc = new Document
            {
                PageInfo = new PageInfo { Margin = new MarginInfo(28, 28, 28, 40) }
            };
            var pdfPage = doc.Pages.Add();
            Table table = new Table
            {
                ColumnWidths = "60% 10% 20% 25%",
                DefaultCellPadding = new MarginInfo(10, 5, 10, 5),
                Border = new BorderInfo(BorderSide.All, .5f, Color.Black),
                DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Color.Black),

            };
            List<SampleForm> data = await _context.sampleForms.OrderBy(m => m.Id).ToListAsync();
            foreach (var item in data)
            {
                //// instantiate Image object

                //Aspose.Pdf.Image image = new Aspose.Pdf.Image();

                //image.IsInNewPage = true;

                //// specify Image file path

                //image.File = item.Photo.ToString();

                //// add image to paragraphs collection of first page of document

                //pdfPage.Paragraphs.Add(image);
            }
            DataTable dt = new DataTable();
            using (var reader = ObjectReader.Create(data))
            {
                dt.Load(reader);
            }
            table.ImportDataTable(dt, true, 0, 0);
            doc.Pages[1].Paragraphs.Add(table);

            using (var streamOut = new MemoryStream())
            {
                doc.Save(streamOut);
                return new FileContentResult(streamOut.ToArray(), "application/pdf")
                {
                    FileDownloadName = "Report.pdf"
                };
            }
            #endregion

            #region export pdf using rotativa package
            //return new ViewAsPdf("Index");
            #endregion
        }
        /// <summary>
        /// action for getting edit view 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var frm = await _context.sampleForms.FindAsync(id);

            if (frm == null)
                return NotFound();

            var viewModel = new RFormViewModel
            {
                Id = frm.Id,
                Title = frm.Title,
                Description = frm.Description,
                Photo = frm.Photo,
            };

            return View("RegisterForm", viewModel);
        }
        public async Task<IActionResult> EditFTPForm(int? id)
        {
            if (id == null)
                return BadRequest();

            var frm = await _context.sampleForms.FindAsync(id);

            if (frm == null)
                return NotFound();

            var viewModel = new SampleForm
            {
                Id = frm.Id,
                Title = frm.Title,
                Description = frm.Description,
                Photo = frm.Photo,
            };

            return View("UploadFTP", viewModel);
        }
        /// <summary>
        /// action for implementing edit
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("RegisterForm", model);
            }

            var form = await _context.sampleForms.FindAsync(model.Id);

            if (form == null)
                return NotFound();

            var files = Request.Form.Files;

            if (files.Any())
            {
                var image = files.FirstOrDefault();

                using var dataStream = new MemoryStream();

                await image.CopyToAsync(dataStream);

                model.Photo = dataStream.ToArray();
            }
            form.Photo = model.Photo;
            form.Title = model.Title;
            form.Description = model.Description;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        /// <summary>
        /// action for deleting item from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var form = await _context.sampleForms.FindAsync(id);

            if (form == null)
                return NotFound();

            _context.sampleForms.Remove(form);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }
    }
}
