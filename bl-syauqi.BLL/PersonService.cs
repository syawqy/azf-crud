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
            var pk = new Dictionary<string, string> { { "City", "Bandung" } };
            var data = await _repository.GetAsync(p => true);
            return data;
        }
        public async Task<Person> CreatePerson(Person person)
        {
            return await _repository.CreateAsync(person);
        }
        public async Task<Person> UpdatePerson(Person person)
        {
            return await _repository.UpdateAsync(person.Id,person);
        }
        public async Task<string> DeletePerson(string id, Dictionary<string, string> pk)
        {
            try
            {
                await _repository.DeleteAsync(id, pk);
                return "Data berhasil dihapus";
            }catch
            {
                return "Data tidak ditemukan";
            }
            
        }
    }
}
