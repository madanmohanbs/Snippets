using Dce.Web.Portal.Data.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Dce.Web.Portal.Data.Repository.Infrastructure;
using Dce.Web.Portal.Utilities;

namespace Dce.Web.Portal.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Download")]
    public class DownloadController : CoreController
    {
        public DownloadController(ILogger logger, IRepositoryManager manager) : base(logger, manager)
        {
        }
        [AllowAnonymous]
        [Route("GetBlob")]
        [HttpGet]
        public HttpResponseMessage GetBlob(string file)
        {
            try
            {
                
                string filename = "";
                filename = GetPdfBlobUrl(file);
                //filename = @"\8FEB33F6-5C6D-4925-8EEB-7D6EBBC5A419\2017\06\30\77bcf1c8-1348-4800-b31c-6d7c9bd0b124\03526011-012b-4a9f-882f-8c8135f8120e.pdf.pdf";
                filename = filename.StartsWith("\\") ? filename.Substring(1) : filename;
                // Retrieve storage account from connection string.
                //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

                var accountName = ConfigurationManager.AppSettings["Blob:account:name"];
                var accountKey = ConfigurationManager.AppSettings["Blob:account:key"];
                var containerName = ConfigurationManager.AppSettings["Blob:file:container"];

                CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), false);

                // Create the blob client.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve reference to a previously created container.
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Retrieve reference to a blob named "file".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);

                var memoryStream = new MemoryStream();
                // Save blob contents to memory.
                blockBlob.DownloadToStream(memoryStream);


                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                memoryStream.Seek(0, SeekOrigin.Begin);
                result.Content = new StreamContent(memoryStream);
                //result.Content = new ByteArrayContent(memoryStream.ToArray());
                memoryStream.Flush();
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); //application/octet-stream,application/pdf, text/html
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") //attachment,inline
                {
                    FileName = file+".pdf"
                };

                return result;
                //return File(memoryStream.ToArray(), blockBlob.Properties.ContentType);
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
            return null;
        }

        public string Get()
        {
            return DateTime.Now.ToString();
        }
        private string GetPdfBlobUrl(string InvoiceId)
        {
            string imagePdfUrl = "";
            Guid dbDocumentId = Guid.Parse(InvoiceId);

            string blobURL = manager.InvoiceRepository.GetBlobUrlByInvoiceId(dbDocumentId);
            if (blobURL == null)
            {
                throw new ApiException(HttpStatusCode.NotFound, "Invoice with Id '" + Convert.ToString(dbDocumentId) + "' not found");
            }
            else
            {
                imagePdfUrl = blobURL;
            }

            return imagePdfUrl;
        }
    }
}
