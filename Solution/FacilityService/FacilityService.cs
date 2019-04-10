
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FacilityService
{
    public static class FacilityService
    {
        [FunctionName("FacilityService")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            var e = JsonConvert.DeserializeObject<Facility>(requestBody);
            e.FacilityId = Guid.NewGuid();
            string sRow = Convert.ToString(e.FacilityId);
            FacilityEntity facility = new FacilityEntity("facility", sRow);

            facility.RoomNumber = e.RoomNumber;
            facility.AirConditioning = e.AirConditioning;
            facility.SwimmingPool = e.SwimmingPool;
            facility.WifiAvailibility = e.WifiAvailibility;
            facility.Date = DateTime.Now;
            facility.FacilityId = e.FacilityId;

            // Connect to the Storage account.
            var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
            var storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccountName, storageAccountKey), false);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("FacilityTable");

            await table.CreateIfNotExistsAsync();

            TableOperation insertOperation = TableOperation.Insert(facility);

            await table.ExecuteAsync(insertOperation);
            return (ActionResult)new OkObjectResult($"Facility Amenities Created");
        }

    }
    public class FacilityEntity : TableEntity
    {
        public FacilityEntity(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }

        public FacilityEntity() { }

        public Guid FacilityId { get; set; }
        public string RoomNumber { get; set; }
        public string AirConditioning { get; set; }
        public string SwimmingPool { get; set; }
        public string WifiAvailibility { get; set; }
        public DateTime Date { get; set; }
    }

    public class Facility
    {
        public Guid FacilityId { get; set; }
        public string RoomNumber { get; set; }
        public string AirConditioning { get; set; }
        public string SwimmingPool { get; set; }
        public string WifiAvailibility { get; set; }


    }


}