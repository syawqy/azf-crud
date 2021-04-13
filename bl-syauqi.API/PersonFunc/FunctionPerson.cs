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
using Microsoft.Azure.Cosmos;
using System.Net;
using Nexus.Base.CosmosDBRepository;

namespace bl_syauqi
{
    // TODO: dibiasakan kerja rapih, d folder2kan class berdasarkan objective
    public class FunctionPerson
    {
        private readonly CosmosClient _cosmosClient;
        public FunctionPerson(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
        }

        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageResult<Person>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("GetAllPerson")]
        public async Task<IActionResult> GetAllPerson(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person")] HttpRequest req,
            ILogger log)
        {
            // TODO: saat startup dibuat DI untuk cosmosclient, supaya singleton.
            //       klo di-define pada saat pembuatan repo, d takutkan object akan d buat berkali2 which is akan membuat performance drop.
            var personService = new PersonService(new PersonRepository(_cosmosClient));
            var data = await personService.GetPerson();
            
            return new OkObjectResult(data);
        }

        // TODO: swagger untuk response ditambahkan utk semua AZF. 
        // utk http 200, badrequest, notfound --> cek dokumentasi "Implementasi Swagger UI pada Azure Function v3 dengan Menggunakan Swashbuckle"
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Person))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("GetPersonById")]
        public async Task<IActionResult> GetPersonById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Person/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(_cosmosClient));
            var data = await personService.GetPersonById(id, new Dictionary<string, string> { { "City", "Bandung" } });
            if(data == null)
            {
                return new NotFoundObjectResult("Data tidak ditemukan");
            }
            return new OkObjectResult(data);
        }
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Person))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("DeletePersonAsync")]
        public async Task<IActionResult> DeletePersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Person")] Person person,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(_cosmosClient));
            try
            {
                var pk = new Dictionary<string, string>();
                pk.Add("City", person.City);
                var result = await personService.DeletePerson(person.Id,pk);
                if(result.Contains("Data tidak ditemukan"))
                {
                    return new NotFoundObjectResult(result);
                }
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                return new OkObjectResult("Data gagal dihapus");
            }

        }
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Person))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("CreatePersonAsync")]
        public async Task<IActionResult> CreatePersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Person")]
            [RequestBodyType(typeof(Person), "person data")] Person person,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(_cosmosClient));
            var data = await personService.CreatePerson(person);

            return new OkObjectResult(data);
        }
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Person))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("PutPersonAsync")]
        public async Task<IActionResult> PutPersonAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "Person")]
            [RequestBodyType(typeof(Person), "person data")] Person person,
            ILogger log)
        {
            PersonService personService = new PersonService(new PersonRepository(_cosmosClient));
            var dataPerson = await personService.GetPersonById(person.Id, new Dictionary<string, string> { { "City", person.City } });
            if (dataPerson == null)
            {
                return new NotFoundObjectResult("Data tidak ditemukan");
            }
            var data = await personService.UpdatePerson(person);

            return new OkObjectResult(data);
        }
    }
}
