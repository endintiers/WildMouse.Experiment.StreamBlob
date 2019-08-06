using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

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
            //_containerSASUrl = "https://fmgsyddevstacdsdmpfiles.blob.core.windows.net/testfiles?st=2019-08-01T07%3A33%3A00Z&se=2055-08-07T07%3A33%3A00Z&sp=rl&sv=2018-03-28&sr=c&sig=EmXM8Ya0YCegUKq2r4SEbLeC%2BK3Yszcph%2Fzeg5VVYVI%3D";
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
