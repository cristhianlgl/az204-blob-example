using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

Console.WriteLine("Azure Blob Storage exercise\n");

// Run the examples asynchronously, wait for the results before proceeding
ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Press enter to exit the sample application.");
Console.ReadLine();

static async Task ProcessAsync()
{
    //Create a unique name for the container
    string containerName = "wtblob" + Guid.NewGuid().ToString();
    string fileName =  "wtfile" + Guid.NewGuid().ToString() + ".txt";
    string localFilePath = await CreateFileAsync(fileName);

    var blobServiceClient = ConnectionBlobServiceClient();
    var containerClient = await CreateContainerClientAsync(blobServiceClient, containerName);
    var blobClient = await UploadBlobAsync(containerClient, fileName, localFilePath);  

    await GetBlobsAsync(containerClient); 
    var downloadFilePath = await DownloadBlobAsync(blobClient, localFilePath);
    await DeleteContainer(containerClient, localFilePath, downloadFilePath);

}

static BlobServiceClient ConnectionBlobServiceClient () {
    // Copy the connection string from the portal in the variable below.
    string storageConnectionString = "";
    // Create a client that can authenticate with a connection string
    BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
    return blobServiceClient;
}

static async Task<BlobContainerClient> CreateContainerClientAsync (BlobServiceClient blobServiceClient, string containerName){
        // Create the container and return a container client object
    BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
    Console.WriteLine("A container named '" + containerName + "' has been created. " +
        "\nTake a minute and verify in the portal." + 
        "\nNext a file will be created and uploaded to the container.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
    return containerClient;
}

static async Task<string>  CreateFileAsync (string fileName) {
     // Create a local file in the ./data/ directory for uploading and downloading
    string localPath = "./data/";    
    string localFilePath = Path.Combine(localPath, fileName);

    // Write text to the file
    await File.WriteAllTextAsync(localFilePath, "Hello, World!");
    return localFilePath;
}

static async Task<BlobClient>  UploadBlobAsync (BlobContainerClient containerClient, string fileName, string localFilePath) {
    // Get a reference to the blob
    BlobClient blobClient = containerClient.GetBlobClient(fileName);

    Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

    // Open the file and upload its data
    using (FileStream uploadFileStream = File.OpenRead(localFilePath))
        {
            await blobClient.UploadAsync(uploadFileStream);
            uploadFileStream.Close();
        }

    Console.WriteLine("\nThe file was uploaded. We'll verify by listing" + 
            " the blobs next.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
    return blobClient;
}

static async Task GetBlobsAsync(BlobContainerClient containerClient) {
     // List blobs in the container
    Console.WriteLine("Listing blobs...");
    await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
    {
        Console.WriteLine("\t" + blobItem.Name);
    }

    Console.WriteLine("\nYou can also verify by looking inside the " + 
            "container in the portal." +
            "\nNext the blob will be downloaded with an altered file name.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
}

static async Task<string> DownloadBlobAsync(BlobClient blobClient, string localFilePath) {
        // Download the blob to a local file
    // Append the string "DOWNLOADED" before the .txt extension 
    string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

    Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);

    // Download the blob's contents and save it to a file
    BlobDownloadInfo download = await blobClient.DownloadAsync();

    using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
    {
        await download.Content.CopyToAsync(downloadFileStream);
    }
    Console.WriteLine("\nLocate the local file in the data directory created earlier to verify it was downloaded.");
    Console.WriteLine("The next step is to delete the container and local files.");
    Console.WriteLine("Press 'Enter' to continue.");
    Console.ReadLine();
    return downloadFilePath;
}

static async Task DeleteContainer (BlobContainerClient containerClient, string localFilePath, string downloadFilePath) {
    // Delete the container and clean up local files created
    Console.WriteLine("\n\nDeleting blob container...");
    await containerClient.DeleteAsync();

    Console.WriteLine("Deleting the local source and downloaded files...");
    File.Delete(localFilePath);
    File.Delete(downloadFilePath);

    Console.WriteLine("Finished cleaning up.");
}