using Contoso_Jobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace Contoso_Jobs.Common
{
    internal class XmlService
    {
        internal static string JobsFile = "jobs.xml";

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public static async Task SaveJobs(IEnumerable<Job> jobs)
        {
            await semaphore.WaitAsync();
            try
            {
                StorageFolder storageFolder;
                storageFolder = ApplicationData.Current.LocalFolder;

                var jobFile = await storageFolder.CreateFileAsync(JobsFile, CreationCollisionOption.ReplaceExisting);

                using (var writeStream = await jobFile.OpenStreamForWriteAsync())
                {
                    new XmlSerializer(jobs.GetType()).Serialize(writeStream, jobs);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public static async Task<List<Job>> LoadJobs()
        {
            List<Job> jobs = new List<Job>();

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            if (await storageFolder.TryGetItemAsync(JobsFile) != null)
            {
                var jobsFile = await storageFolder.CreateFileAsync(JobsFile, CreationCollisionOption.OpenIfExists);

                XmlRootAttribute xRoot = new XmlRootAttribute();
                xRoot.ElementName = "ArrayOfJob";

                var myDeserializer = new XmlSerializer(typeof(List<Job>), xRoot);

                using (var stream = await jobsFile.OpenStreamForReadAsync())
                {
                    try
                    {
                        jobs = (List<Job>)myDeserializer.Deserialize(stream);
                    }
                    catch
                    {
                        // The file was just created so we don't need to do anything

                    }
                }
            }

            return jobs;
        }
    }
}
