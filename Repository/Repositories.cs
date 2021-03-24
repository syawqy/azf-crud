using Microsoft.Azure.Documents.Client;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;
using bl_syauqi.Models;

namespace bl_syauqi.Repository
{
    public class Repositories
    {
        public class PersonRepository : DocumentDBRepository<Person>
        {
            public PersonRepository(DocumentClient client) :
                base("Course", client, partitionProperties: "City")
            { }
        }
        public class StudentRepository : DocumentDBRepository<Student>
        {
            public StudentRepository(DocumentClient client) :
                base("Course", client, partitionProperties: "City")
            { }
        }
    }
}
