using bl_syauqi.DAL.Models;
using Moq;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace bl_syauqi.BLL.TEST
{
    public class PersonServiceTest
    {
        [Theory]
        [InlineData("1")]
        [InlineData("4")]
        public async void GetPersonById_ResultFound(string id)
        {
            // arrange
            var repo = new Mock<IDocumentDBRepository<Person>>();

            IEnumerable<Person> persons = new List<Person>
            {
                {new Person() { Id = "1", FirstName = "abcd", LastName = "Mnnnn"} },
                {new Person() { Id = "2", FirstName = "xyz0", LastName = "oooee" } }
            };
            var personData = persons.Where(o => o.Id == id).FirstOrDefault();
            repo.Setup(c => c.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()
            )).Returns(
                Task.FromResult<Person>(personData)
            );
            var svc = new PersonService(repo.Object);

            // act
            var act = await svc.GetPersonById("", null);

            // assert
            Assert.Equal(personData, act);
        }
    }
}
