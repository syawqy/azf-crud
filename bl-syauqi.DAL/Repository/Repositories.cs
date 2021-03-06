using Microsoft.Azure.Documents.Client;
using Nexus.Base.CosmosDBRepository;
using System;
using bl_syauqi.DAL.Models;
using Microsoft.Azure.Cosmos;

namespace bl_syauqi.DAL.Repository
{
    public class Repositories
    {
        private static readonly string _eventGridEndPoint = Environment.GetEnvironmentVariable("eventGridEndPoint");
        private static readonly string _eventGridKey = Environment.GetEnvironmentVariable("eventGridEndKey");
        private static readonly string _dbKey = Environment.GetEnvironmentVariable("dbKey");
        private static readonly string _dbEndPoint = Environment.GetEnvironmentVariable("dbEndPoint");
        public class PersonRepository : DocumentDBRepository<Person>
        {
            public PersonRepository(CosmosClient client) :
                base(databaseId: "Course", client, partitionProperties: "City")
            { }
        }
        public class VideoRepository : DocumentDBRepository<ResourceVideo>
        {
            public VideoRepository(CosmosClient client) :
                base(databaseId: "Course", client)
            { }
        }
        public class StudentRepository : DocumentDBRepository<Student>
        {
            public StudentRepository(CosmosClient client) :
                base("Course", client, partitionProperties: "City", eventGridEndPoint: _eventGridEndPoint, eventGridKey: _eventGridKey)
            { }
        }
        public class PersonLogRepository : DocumentDBRepository<PersonLog>
        {
            public PersonLogRepository() :
                base(databaseId: "Course", endPoint:_dbEndPoint, key: _dbKey, partitionProperties: "City")
            { }
        }
    }
}
