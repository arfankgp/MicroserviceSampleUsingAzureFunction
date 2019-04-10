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

namespace UserService
{
    public static class UserService
    {
        [FunctionName("UserService")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var e = JsonConvert.DeserializeObject<User>(requestBody as string);
            string sRow = e.Email + e.LastName;
            UserEntity userEntity = new UserEntity("user", sRow);

            userEntity.FirstName = e.FirstName;
            userEntity.LastName = e.LastName;
            userEntity.Email = e.Email;

            // Connect to the Storage account.
            var storageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
            var storageAccountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
            CloudStorageAccount storageAccount = new CloudStorageAccount(new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(storageAccountName, storageAccountKey), false);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference("UserTable");

            await table.CreateIfNotExistsAsync();

            TableOperation insertOperation = TableOperation.Insert(userEntity);

            await table.ExecuteAsync(insertOperation);

            return (ActionResult)new OkObjectResult($"Successfully Stored");
            //: new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class UserEntity : TableEntity
    {
        public UserEntity(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }

        public UserEntity() { }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
