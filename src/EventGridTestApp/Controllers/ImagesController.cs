using EventGridTestApp.Helpers;
using EventGridTestApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventGridTestApp.Controllers {
    
    [Route("api/[controller]")]
    public class ImagesController : Controller {

        private readonly AzureStorageConfig storageConfig = null;

        public ImagesController(IOptions<AzureStorageConfig> config) {
            storageConfig = config.Value;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files) {

            bool isUploaded = false;

            try {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload.");
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)
                    return BadRequest("Sorry, can't retrieve your azure storage detail from appsettings.js, make sure that you add azure storage details there.");
                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                foreach (var formFile in files) {
                    if (StorageHelper.IsImage(formFile)) {
                        if (formFile.Length > 0) {
                            using (Stream stream = formFile.OpenReadStream()) {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                            }
                        }
                    } else {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded) {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();
                } else
                    return BadRequest("Looks like the image couldn't upload to the storage.");
            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails() {

            try {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == strring.Empty)
                    return BadRequest("Sorry, can't retrieve your azure storage detail from appsettings.js, make sure that you add azure storage details there.");
                if (storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your thumbnail container in the azure blob storage");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);
                return new ObjectResult(thumbnailUrls);
            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }
    }
}