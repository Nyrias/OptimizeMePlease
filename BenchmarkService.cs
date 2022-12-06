using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using Microsoft.EntityFrameworkCore;
using OptimizeMePlease.Context;

namespace OptimizeMePlease
{
    [MemoryDiagnoser]
    [HideColumns(Column.Job, Column.RatioSD, Column.StdDev, Column.AllocRatio)]
    [Config(typeof(Config))]
    public class BenchmarkService
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                SummaryStyle = SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend);
            }
        }

        /// <summary>
        /// Get top 2 Authors (FirstName, LastName, UserName, Email, Age, Country) 
        /// from country Serbia aged 27, with the highest BooksCount
        /// and all his/her books (Book Name/Title and Publishment Year) published before 1900
        /// </summary>
        /// <returns></returns>
        [Benchmark(Baseline = true)]
        public async Task<List<AuthorDTO>> GetAuthors()
        {
            await using var dbContext = new AppDbContext();

            var authors = (await dbContext.Authors
                    .Include(x => x.User)
                    .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .Include(x => x.Books)
                    .ThenInclude(x => x.Publisher)
                    .ToListAsync())
                .Select(x => new AuthorDTO
                {
                    UserCreated = x.User.Created,
                    UserEmailConfirmed = x.User.EmailConfirmed,
                    UserFirstName = x.User.FirstName,
                    UserLastActivity = x.User.LastActivity,
                    UserLastName = x.User.LastName,
                    UserEmail = x.User.Email,
                    UserName = x.User.UserName,
                    UserId = x.User.Id,
                    RoleId = x.User.UserRoles.FirstOrDefault(y => y.UserId == x.UserId)!.RoleId,
                    BooksCount = x.BooksCount,
                    AllBooks = x.Books.Select(y => new BookDto
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Published = y.Published,
                        ISBN = y.ISBN,
                        PublisherName = y.Publisher.Name
                    }).ToList(),
                    AuthorAge = x.Age,
                    AuthorCountry = x.Country,
                    AuthorNickName = x.NickName,
                    Id = x.Id
                })
                .ToList()
                .Where(x => x.AuthorCountry == "Serbia" && x.AuthorAge == 27)
                .ToList();

            var orderedAuthors = authors.OrderByDescending(x => x.BooksCount).ToList().Take(2).ToList();

            List<AuthorDTO> finalAuthors = new List<AuthorDTO>();
            foreach (var author in orderedAuthors)
            {
                List<BookDto> books = new List<BookDto>();

                var allBooks = author.AllBooks;

                foreach (var book in allBooks)
                {
                    if (book.Published.Year < 1900)
                    {
                        book.PublishedYear = book.Published.Year;
                        books.Add(book);
                    }
                }

                author.AllBooks = books;
                finalAuthors.Add(author);
            }

            return finalAuthors;
        }

        [Benchmark]
        public async Task<List<AuthorDTO_Optimized>> GetAuthors_Optimized()
        {
            await using var dbContext = new AppDbContext();
            var authors = await dbContext.Authors
                .Where(x => x.Country == "Serbia" && x.Age == 27)
                .OrderByDescending(x => x.BooksCount)
                .Select(x => new AuthorDTO_Optimized
                {
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Email = x.User.Email,
                    UserName = x.User.UserName,
                    Books = x.Books.Select(y => new BookDTO_Optimized
                    {
                        Title = y.Name,
                        PublishedYear = y.Published.Year
                    }),
                    Age = x.Age,
                    Country = x.Country,
                })
                .Take(2)
                .ToListAsync();

            foreach (var author in authors)
                author.Books = author.Books.Where(b => b.PublishedYear < 1900).ToList();

            return authors;
        }

        [Benchmark]
        public async Task<List<AuthorDTO_OptimizedStruct>> GetAuthors_Optimized_Struct()
        {
            await using var dbContext = new AppDbContext();
            var authors = await dbContext.Authors
                .Where(x => x.Country == "Serbia" && x.Age == 27)
                .OrderByDescending(x => x.BooksCount)
                .Select(x => new AuthorDTO_OptimizedStruct
                {
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Email = x.User.Email,
                    UserName = x.User.UserName,
                    Books = x.Books.Select(y => new BookDTO_OptimizedStruct
                    {
                        Title = y.Name,
                        PublishedYear = y.Published.Year
                    }),
                    Age = x.Age,
                    Country = x.Country,
                })
                .Take(2)
                .ToListAsync();

            var authorCount = authors.Count;
            for (var i = 0; i < authorCount; i++)
            {
                var author = authors[i];
                author.Books = author.Books.Where(b => b.PublishedYear < 1900).ToList();
            }

            return authors;
        }
    }
}