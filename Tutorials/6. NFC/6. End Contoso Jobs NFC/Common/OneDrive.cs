using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using System.Diagnostics;
using Windows.Storage;
using System.IO;

namespace Contoso_Jobs.Common
{
    public class OneDrive
    {
        private OneDriveClient oneDriveClient;

        public async Task Init()
        {
            try
            {
                string clientId = "<put your client id here>";

                MsaAuthenticationProvider msaAuthProvider =
                    new MsaAuthenticationProvider(clientId,
                    "https://login.live.com/oauth20_desktop.srf",
                    new string[] { "onedrive.readwrite", "wl.signin" },
                    new CredentialVault(clientId));
                await msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
                oneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0",
                    msaAuthProvider);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }

        public async Task<bool> DownloadFileFromOneDrive()
        {
            bool downloaded = false;

            try
            {
                if (oneDriveClient == null)
                {
                    await Init();
                }

                Item item = await oneDriveClient
                                 .Drive
                                 .Root
                                 .ItemWithPath("documents/ContosoJobs.xml")
                                 .Request()
                                 .GetAsync();

                Stream contentStream = await oneDriveClient
                                          .Drive
                                          .Items[item.Id]
                                          .Content
                                          .Request()
                                          .GetAsync();

                //write the stream to our local storage for this session
                StorageFolder storageFolder;
                storageFolder = ApplicationData.Current.LocalFolder;

                StorageFile jobFile = await storageFolder.CreateFileAsync(XmlService.JobsFile,
                    CreationCollisionOption.ReplaceExisting);

                using (var writeStream = await jobFile.OpenStreamForWriteAsync())
                {
                    int count = 0;
                    do
                    {
                        var buffer = new byte[1024];
                        count = contentStream.Read(buffer, 0, 1024);
                        await writeStream.WriteAsync(buffer, 0, count);
                    }
                    while (contentStream.CanRead && count > 0);

                    writeStream.Flush();
                    downloaded = true;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return downloaded;
        }

        public async Task UploadFileToOneDrive()
        {
            try
            {
                if (oneDriveClient == null)
                {
                    await Init();
                }

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

                if (await storageFolder.TryGetItemAsync(XmlService.JobsFile) != null)
                {
                    var jobFile = await storageFolder.CreateFileAsync(XmlService.JobsFile,
                        CreationCollisionOption.OpenIfExists);

                    using (Stream readStream = await jobFile.OpenStreamForReadAsync())
                    {
                        var uploadedItem = await oneDriveClient
                                             .Drive
                                             .Root
                                             .ItemWithPath("documents/ContosoJobs.xml")
                                             .Content
                                             .Request()
                                             .PutAsync<Item>(readStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


    }
}
