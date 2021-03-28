using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using bl_syauqi.BLL;
using bl_syauqi.DAL.Repository;
using System.Collections.Generic;
using bl_syauqi.DAL.Models;
using static bl_syauqi.DAL.Repository.Repositories;

namespace bl_syauqi
{
    public static class FunctionPerson
    {
        [FunctionName("GetAllPerson")]
        public static async Task<IActionResult> GetAllPerson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient client,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(client));
            var data = await personService.GetPerson();
            return new OkObjectResult(data);
        }

        [FunctionName("GetPersonById")]
        public static async Task<IActionResult> GetPersonById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person/{id}")] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient client,
            string id,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(client));
            var data = await personService.GetPersonById(id, new Dictionary<string, string> { { "City", "Bandung" } });
            return new OkObjectResult(data);
        }

        [FunctionName("DeletePersonAsync")]
        public static async Task<IActionResult> DeletePersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Person")] Person person,
            [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient client,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(client));
            try
            {
                var pk = new Dictionary<string, string>();
                pk.Add("City", person.City);
                personService.DeletePerson(person.Id,pk);
                return new OkObjectResult("Data berhasil dihapus");
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new OkObjectResult("Data gagal dihapus");
            }

        }

        [FunctionName("CreatePersonAsync")]
        public static async Task<IActionResult> CreatePersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Person")] Person person,
            [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient client,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(client));
            var data = await personService.CreatePerson(person);

            return new OkObjectResult(data);
        }

        [FunctionName("PutPersonAsync")]
        public static async Task<IActionResult> PutPersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Person")] Person person,
            [CosmosDB(ConnectionStringSetting = "cosmos-db-bl")] DocumentClient client,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(client));
            var data = await personService.UpdatePerson(person);

            return new OkObjectResult(data);
        }
    }
}
