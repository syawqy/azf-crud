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
using System.Collections.Generic;
using bl_syauqi.DAL.Models;
using static bl_syauqi.DAL.Repository.Repositories;
using bl_syauqi.BLL;
using AzureFunctions.Extensions.Swashbuckle;
using AzureFunctions.Extensions.Swashbuckle.Attribute;

namespace bl_syauqi
{
    public static class FunctionPerson
    {
        [FunctionName("GetAllPerson")]
        public static async Task<IActionResult> GetAllPerson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person")] HttpRequest req,
            ILogger log)
        {
            var personService = new PersonService(new PersonRepository());
            var data = await personService.GetPerson();
            var rep = new PersonRepository();
            var data2 = await rep.GetAsync();
            var data3 = await rep.GetAsync(p => true);
            var data4 = await rep.GetAsync(sqlQuery: "select * from c");
            return new OkObjectResult(data2);
        }

        [FunctionName("GetPersonById")]
        public static async Task<IActionResult> GetPersonById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository());
            var data = await personService.GetPersonById(id, new Dictionary<string, string> { { "City", "Bandung" } });
            return new OkObjectResult(data);
        }

        [FunctionName("DeletePersonAsync")]
        public static async Task<IActionResult> DeletePersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Person")] Person person,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository());
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
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Person")]
            [RequestBodyType(typeof(Person), "person request")] Person person,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository());
            var data = await personService.CreatePerson(person);

            return new OkObjectResult(data);
        }

        [FunctionName("PutPersonAsync")]
        public static async Task<IActionResult> PutPersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Person")]
            [RequestBodyType(typeof(Person), "person request")] Person person,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository());
            var data = await personService.UpdatePerson(person);

            return new OkObjectResult(data);
        }
    }
}
