using System.Collections.Generic;
using System.Linq;
using DevRating.Domain.Fake;
using Xunit;

namespace DevRating.Domain.Test
{
    public sealed class DefaultInsertWorkParamsTest
    {
        [Fact]
        public void InsertsWorkWithoutUsedRating()
        {
            var works = new List<Work>();
            var author = new FakeAuthor("email");

            new DefaultInsertWorkParams()
                .InsertionResult(
                    new FakeInsertWorkOperation(
                        works,
                        new List<Author> {author},
                        new List<Rating>()
                    ),
                    author
                );

            Assert.Single(works);
        }

        [Fact]
        public void InsertsWorkWithUsedRating()
        {
            var author = new FakeAuthor("email");
            var work = new FakeWork(10, author);
            var rating = new FakeRating(1500, work, author);
            var works = new List<Work> {work};
            
            new DefaultInsertWorkParams()
                .InsertionResult(
                    new FakeInsertWorkOperation(
                        works,
                        new List<Author> {author},
                        new List<Rating> {rating}
                    ),
                    author,
                    rating
                );

            Assert.True(works.Last().HasUsedRating());
        }
    }
}