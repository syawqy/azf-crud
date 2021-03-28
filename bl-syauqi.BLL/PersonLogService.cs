using bl_syauqi.DAL.Models;
using Microsoft.Azure.Documents;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace bl_syauqi.BLL
{
    public  class PersonLogService
    {
        private readonly IDocumentDBRepository<PersonLog> _repository;
        public PersonLogService(IDocumentDBRepository<PersonLog> repository)
        {
            if (_repository == null)
            {
                _repository = repository;
            }
        }
        public async Task<Document> CreatePersonLog(PersonLog personlog)
        {
            return await _repository.CreateAsync(personlog);
        }
    }
}
