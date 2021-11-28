using System;
using System.IO;
using System.Linq;
using Xunit;

namespace GitDb.Tests
{
    public class AllTests : IDisposable
    {
        private readonly IGitDatabase _db;

        public AllTests()
        {
            //init temp folder for database
            var location = Path.GetTempPath();
            location = Path.Combine(location, Guid.NewGuid().ToString());
            Directory.CreateDirectory(location);
            _db = new GitDatabase(location);
        }

        public void Dispose()
        {
            //destroy database on tests end
            _db.DestroyDatabase();
        }

        [Fact]
        public void Test_Main()
        {
            var id = Guid.NewGuid().ToString();
            var person = new Person(id, "Nikola", "Tesla", 86);
            var change1Id = _db.Insert(person.Id, person);
            Assert.NotEmpty(change1Id);
            Assert.NotNull(change1Id);

            var p = _db.Get<Person>(id);
            Assert.Equal(person.Id, p.Id);
            Assert.Equal(person.Name, p.Name);
            Assert.Equal(person.Surname, p.Surname);
            Assert.Equal(person.Age, p.Age);

            //try to reinsert

            try
            {
                //should fail
                _db.Insert(person.Id, person);
                Assert.False(true, "Duplicate id should not be allowed");
            }
            catch
            {
                Assert.True(true);
            }

            try
            {
                //should fail
                _db.Update(person.Id, person);
                Assert.False(true, "Update with no change should not be allowed");
            }
            catch
            {
                Assert.True(true);
            }


            person.Age = int.MaxValue;
            person.Name = "Random string " + Guid.NewGuid();
            person.Surname = "Random string " + Guid.NewGuid();
            person.Age = (int)(DateTime.Now.Ticks % 10000);

            var change2Id = _db.Update(person.Id, person);
            Assert.NotEmpty(change2Id);
            Assert.NotNull(change2Id);

            p = _db.Get<Person>(id);
            Assert.NotNull(p);
            Assert.Equal(person.Id, p.Id);
            Assert.Equal(person.Name, p.Name);
            Assert.Equal(person.Surname, p.Surname);
            Assert.Equal(person.Age, p.Age);

            var change3Id = _db.Delete<Person>(id);
            Assert.NotEmpty(change3Id);
            Assert.NotNull(change3Id);

            //should be null after delete
            p = _db.Get<Person>(id);
            Assert.Null(p);

            //undo deletion
            var change4 = _db.Revert(change3Id);
            Assert.NotEmpty(change4);
            Assert.NotNull(change4);

            p = _db.Get<Person>(id);
            Assert.NotNull(p);
            Assert.Equal(person.Id, p.Id);
            Assert.Equal(person.Name, p.Name);
            Assert.Equal(person.Surname, p.Surname);
            Assert.Equal(person.Age, p.Age);


            //undo undo deletion
            var change5 = _db.Revert(change4);
            Assert.NotEmpty(change5);
            Assert.NotNull(change5);

            //so object should be null again
            p = _db.Get<Person>(id);
            Assert.Null(p);


            var history = _db.GetChanges<Person>(id).ToArray();
            ;
            Assert.Equal(5, history.Length);
            Assert.True(history[0].IsDeleted);
            Assert.True(history[1].IsInserted);
            Assert.True(history[2].IsDeleted);
            Assert.True(history[3].IsUpdated);
            Assert.True(history[4].IsInserted);
            ;
        }
    }
}
