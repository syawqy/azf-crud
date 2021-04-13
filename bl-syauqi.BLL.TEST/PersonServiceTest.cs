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
        // TODO: Buat class utk mengelompokan methods
        public class GetPersonById
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
                var act = await svc.GetPersonById(id, null);

                // assert
                Assert.Equal(personData, act);
            }

            [Theory]
            [InlineData("5")]
            [InlineData("4")]
            public async void GetPersonById_ResultNotFound(string id)
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
                    It.Is<string>(i => persons.Any(a => a.Id == i)),
                    It.IsAny<Dictionary<string, string>>()
                )).Returns(
                    Task.FromResult<Person>(personData)
                );
                var svc = new PersonService(repo.Object);

                // act
                var act = await svc.GetPersonById(id, null);

                // assert
                Assert.Null(act);
            }
        }

        // TODO: Buat test untuk methods yang lain
        public class CreatePerson
        {
            [Fact]
            public async void CreateNormal_Success()
            {
                // arrange
                var repo = new Mock<IDocumentDBRepository<Person>>();

                Person personnew = new Person()
                {
                    Id = "123",
                    FirstName = "tes name",
                    LastName = "last name"
                };

                repo.Setup(c => c.CreateAsync(
                    It.IsAny<Person>(),
                    It.IsAny<EventGridOptions>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )).Returns(
                    Task.FromResult<Person>(personnew)
                );
                var svc = new PersonService(repo.Object);

                // act
                var act = await svc.CreatePerson(personnew);

                // assert
                Assert.Contains("- new",act.FirstName);
                //Assert.Equal(personnew.FirstName, act.FirstName);
                //Assert.Equal(personnew.LastName, act.LastName);
            }
        }
        public class PutPerson
        {
            [Fact]
            public async void PutPerson_Success()
            {
                // arrange
                var repo = new Mock<IDocumentDBRepository<Person>>();

                Person personnew = new Person()
                {
                    Id = "123",
                    FirstName = "new name",
                    LastName = "last name 2"
                };

                repo.Setup(c => c.UpdateAsync(
                    It.IsAny<string>(),
                    It.IsAny<Person>(),
                    It.IsAny<EventGridOptions>(),
                    It.IsAny<string>()
                )).Returns(
                    Task.FromResult<Person>(personnew)
                );
                var svc = new PersonService(repo.Object);

                // act
                var act = await svc.UpdatePerson(personnew);

                // assert
                Assert.Contains("- edit", personnew.FirstName);
                //Assert.Equal(personnew.FirstName, act.FirstName);
                //Assert.Equal(personnew.LastName, act.LastName);
            }
        }
        public class DeletePerson
        {
            [Fact]
            public async void DeletePerson_Success()
            {
                // arrange
                var repo = new Mock<IDocumentDBRepository<Person>>();

                Person personnew = new Person()
                {
                    Id = "123",
                    FirstName = "new name",
                    LastName = "last name 2"
                };
                var pk = new Dictionary<string, string>();
                pk.Add("City", personnew.City);

                repo.Setup(c => c.DeleteAsync(
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string,string>>(),
                    It.IsAny<EventGridOptions>()
                )).Returns(
                    Task.FromResult<string>("Data berhasil dihapus")
                );
                var svc = new PersonService(repo.Object);

                // act
                var act = await svc.DeletePerson(personnew.Id,pk);

                // assert
                Assert.Equal("Data berhasil dihapus", act);
            }
        }
    }
}
