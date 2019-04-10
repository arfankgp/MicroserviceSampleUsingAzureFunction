using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace BookingService
{
    public static class BookingService
    {
        [FunctionName("BookingService")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var e = JsonConvert.DeserializeObject<Booking>(requestBody as string);
            string sRow = e.Email + e.CustomerName ;
            BookingEntity bookingEntity = new BookingEntity("booking", sRow);

            bookingEntity.CustomerName = e.CustomerName;
            bookingEntity.Email = e.Email;
            bookingEntity.Phone = e.Phone;
            bookingEntity.BookingStatus = e.BookingStatus;
            bookingEntity.BookingDate = DateTime.Now.ToString();        
            // Connect to the Storage account.
            var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
            var storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccountName, storageAccountKey), false);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("BookingTable");

            await table.CreateIfNotExistsAsync();

            TableOperation insertOperation = TableOperation.Insert(bookingEntity);

            await table.ExecuteAsync(insertOperation);

            return (ActionResult)new OkObjectResult($"Successfully Stored");
            //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    public class Booking
    {
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BookingStatus { get; set; }
        public string BookingDate { get; set; }
    }

    public class BookingEntity : TableEntity
    {
        public BookingEntity(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }

        public BookingEntity() { }

        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BookingStatus { get; set; }
        public string BookingDate { get; set; }
    }
}
