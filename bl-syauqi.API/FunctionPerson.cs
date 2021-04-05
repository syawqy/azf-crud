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
    // TODO: dibiasakan kerja rapih, d folder2kan class berdasarkan objective
    public static class FunctionPerson
    {
        [FunctionName("GetAllPerson")]
        public static async Task<IActionResult> GetAllPerson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person")] HttpRequest req,
            ILogger log)
        {
            var rep = new PersonRepository();

            // TODO: saat startup dibuat DI untuk cosmosclient, supaya singleton.
            //       klo di-define pada saat pembuatan repo, d takutkan object akan d buat berkali2 which is akan membuat performance drop.
            var personService = new PersonService(new PersonRepository());
            var data = await personService.GetPerson();
            
            return new OkObjectResult(data);
        }

        // TODO: swagger untuk response ditambahkan utk semua AZF. 
        // utk http 200, badrequest, notfound --> cek dokumentasi "Implementasi Swagger UI pada Azure Function v3 dengan Menggunakan Swashbuckle"
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
