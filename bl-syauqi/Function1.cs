using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using bl_syauqi.DAL.Models;
using bl_syauqi.DTO;
using bl_syauqi.BLL;
using bl_syauqi.DAL.Repository;
using static bl_syauqi.DAL.Repository.Repositories;

namespace bl_syauqi
{
    public static class Function1
    {
        //[FunctionName("GetAllPerson")]
        //public static IActionResult GetAllPerson(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    ILogger log)
        //{
        //    var query = new SqlQuerySpec("SELECT * FROM c");
        //    var pk = new PartitionKey("Jakarta");
        //    var options = new FeedOptions() { PartitionKey = pk };
        //    var data = documentClient.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri("Course", "Person"), query, options);
        //    return new OkObjectResult(data);
        //}

        //[FunctionName("GetPersonById")]
        //public static IActionResult GetPersonById(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person/{id}")] HttpRequest req,
        //    [CosmosDB(
        //        databaseName: "Course",
        //        collectionName: "Person",
        //        ConnectionStringSetting = "cosmos-db-bl",
        //        Id = "{id}",
        //        PartitionKey = "Jakarta")] Person person1,
        //    ILogger log)
        //{
        //    return new OkObjectResult(person1);
        //}

        //[FunctionName("CreatePersonAsync")]
        //public static async Task<IActionResult> CreatePersonAsync(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Person/create")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    ILogger log)
        //{
        //    var person1 =
        //        JsonConvert.DeserializeObject<Person>(
        //            await new StreamReader(req.Body).ReadToEndAsync());
        //    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Course", "Person");
        //    await documentClient.CreateDocumentAsync(collectionUri, person1);

        //    return new OkObjectResult(person1);
        //}

        //[FunctionName("DeletePersonAsync")]
        //public static async Task<IActionResult> DeletePersonAsync(
        //    [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Person/delete/{id}")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    string id,
        //    ILogger log)
        //{
        //    var person1 =
        //        JsonConvert.DeserializeObject<Person>(
        //            await new StreamReader(req.Body).ReadToEndAsync());
        //    Uri collectionUri = UriFactory.CreateDocumentUri("Course", "Person",id);
        //    await documentClient.DeleteDocumentAsync(collectionUri, new RequestOptions { PartitionKey = new PartitionKey("Jakarta") });

        //    return new OkObjectResult("Data berhasil dihapus");
        //}

        //[FunctionName("PutPersonAsync")]
        //public static async Task<IActionResult> PutPersonAsync(
        //    [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Person")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    ILogger log)
        //{
        //    var person1 =
        //        JsonConvert.DeserializeObject<Person>(
        //            await new StreamReader(req.Body).ReadToEndAsync());
        //    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Course", "Person");
        //    await documentClient.UpsertDocumentAsync(collectionUri, person1);

        //    return new OkObjectResult(person1);
        //}

        //[FunctionName("CreateStudentAsync")]
        //public static async Task<IActionResult> CreateStudentAsync(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Student/create")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    ILogger log)
        //{
        //    var student =
        //        JsonConvert.DeserializeObject<Student>(
        //            await new StreamReader(req.Body).ReadToEndAsync());
        //    var rep = new Repository.Repositories.PersonRepository(documentClient);
        //    var data = await rep.CreateAsync(student);

        //    return new OkObjectResult(data);
        //}

        //[FunctionName("GetAllStudent")]
        //public static async Task<IActionResult> GetAllStudent(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Student")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    ILogger log)
        //{
        //    var rep = new Repository.Repositories.StudentRepository(documentClient);
        //    var data = await rep.GetAsync();
        //    return new OkObjectResult(data);
        //}

        //[FunctionName("DeleteStudentAsync")]
        //public static async Task<IActionResult> DeleteStudentAsync(
        //    [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Student/delete/{id}/{pk}")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    string id,
        //    string pk,
        //    ILogger log)
        //{
        //    var rep = new Repository.Repositories.StudentRepository(documentClient);
        //    try
        //    {
        //        await rep.DeleteAsync(id);
        //        return new OkObjectResult("Data berhasil dihapus");
        //    }
        //    catch (DocumentClientException e)
        //    {
        //        log.LogError(e.Message);
        //        return new OkObjectResult("Data gagal dihapus");
        //    }
        //}

        //[FunctionName("PutStudentAsync")]
        //public static async Task<IActionResult> PutStudentAsync(
        //    [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Student")] HttpRequest req,
        //    [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient documentClient,
        //    ILogger log)
        //{
        //    var student =
        //        JsonConvert.DeserializeObject<Student>(
        //            await new StreamReader(req.Body).ReadToEndAsync());
        //    var rep = new Repository.Repositories.PersonRepository(documentClient);
        //    var data = await rep.UpsertAsync(student.Id, student);

        //    return new OkObjectResult(data);
        //}

        [FunctionName("PostEvGrid")]
        public static async Task<IActionResult> PostEvGrid(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "evgrid")] MessageDTO req,
            [EventGrid(TopicEndpointUri = "eventGridEndPoint", TopicKeySetting = "eventGridEndKey")] IAsyncCollector<EventGridEvent> outputEvents,
            ILogger log)
        {
            // var msg =
            //     JsonConvert.DeserializeObject<MessageDTO>(
            //         await new StreamReader(req.Body).ReadToEndAsync());
            var msg = req;
            var myEvent = new EventGridEvent(msg.id, "subject", msg.data, "evtGridMessage.syauqi", DateTime.UtcNow, "1.0");
            await outputEvents.AddAsync(myEvent);
            return new OkObjectResult(myEvent);
        }
    }
}
