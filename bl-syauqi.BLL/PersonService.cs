using bl_syauqi.DAL.Models;
using Microsoft.Azure.Documents;
using Nexus.Base.CosmosDBRepository;
using System.Collections.Generic;
using System.Threading.Tasks;
using static bl_syauqi.DAL.Repository.Repositories;

namespace bl_syauqi.BLL
{
    public class PersonService
    {
        private readonly IDocumentDBRepository<Person> _repository;
        public PersonService(IDocumentDBRepository<Person> repository)
        {
            if (_repository == null)
            {
                _repository = repository;
            }
        }

        public async Task<Person> GetPersonById(string id, Dictionary<string,string> pk)
        {
            return await _repository.GetByIdAsync(id, pk);
        }
        public async Task<PageResult<Person>> GetPerson()
        {
            var data = await _repository.GetAsync(p => true);
            return data;
        }
        public async Task<Document> CreatePerson(Person person)
        {
            return await _repository.CreateAsync(person);
        }
        public async Task<Document> UpdatePerson(Person person)
        {
            return await _repository.UpdateAsync(person.Id,person);
        }
        public async void DeletePerson(string id, Dictionary<string, string> pk)
        {
            await _repository.DeleteAsync(id,pk);
        }
    }
}
