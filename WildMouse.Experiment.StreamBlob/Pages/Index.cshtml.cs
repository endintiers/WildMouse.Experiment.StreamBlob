using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace WildMouse.Experiment.StreamBlob
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private string _containerSASUrl;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
            _containerSASUrl = configuration["ContainerSASUrl"];
        }

        public List<string> BlobFileNames { get; set; }
        public string SelectedFileName { get; set; }

        public void OnGet()
        {
            BlobFileNames = GetFileNames(_containerSASUrl);
        }

        public async Task<IActionResult> OnPostAsync(string SelectedFileName)
        {
            var container = new CloudBlobContainer(new Uri(_containerSASUrl));
            var blob = container.GetBlobReference(SelectedFileName);
            var stream = await blob.OpenReadAsync();
            return File(stream, blob.Properties.ContentType, SelectedFileName);
        }

        public List<string> GetFileNames(string containerSASUrl)
        {
            var container = new CloudBlobContainer(new Uri(containerSASUrl));
            var fileNames = new List<string>();
            var context = new OperationContext();
            var options = new BlobRequestOptions();
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.All,
                    null, blobContinuationToken, options, context).Result;
                blobContinuationToken = results.ContinuationToken;
                foreach (var item in results.Results)
                {
                    fileNames.Add(Path.GetFileName(HttpUtility.UrlDecode(item.Uri.AbsoluteUri)));
                }
            } while (blobContinuationToken != null);
            return fileNames;
        }
    }
}
