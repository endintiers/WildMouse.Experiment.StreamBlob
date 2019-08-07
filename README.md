# WildMouse.Experiment.StreamBlob
Stream Blob from Store via Razor Web App

Working example of responding async to a file request with a blob from Azure Storage.
The point is that it doesn't copy the (maybe very large) file to memory.

        public async Task<IActionResult> OnPostAsync(string SelectedFileName)
        {
            var container = new CloudBlobContainer(new Uri(_containerSASUrl));
            var blob = container.GetBlobReference(SelectedFileName);
            var stream = await blob.OpenReadAsync();
            return File(stream, blob.Properties.ContentType, SelectedFileName);
        }
