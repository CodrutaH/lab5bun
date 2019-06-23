using Lab2.DTOs;
using Lab2.Models;
using Lab2.Servies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Linq;

namespace Tests
{
    public class UserServiceTests
    {
        private IOptions<AppSettings> config;

        [SetUp]
        public void Setup()
        {
            config = Options.Create(new AppSettings
            {
                Secret = "dsadhjcghduihdfhdifd8ih"
            });
        }

        /// <summary>
        /// TODO: AAA - Arrange, Act, Assert
        /// </summary>
        [Test]
        public void ValidRegisterShouldCreateNewUser()
        {
            var options = new DbContextOptionsBuilder<ExpensesDbContext>()
               .UseInMemoryDatabase(databaseName: nameof(ValidRegisterShouldCreateNewUser))// "ValidRegisterShouldCreateNewUser")
               .Options;

            using (var context = new ExpensesDbContext(options))
            {
                var usersService = new UserService(context, config);
                var added = new RegisterUserPostDto
                {
                    FullName = "fdsfsdfs",
                    Username = "test_username",
                    Email = "a@a.b",
                    Password = "1234567",
                };
                var result = usersService.Register(added);

                Assert.IsNotNull(result);
                Assert.AreEqual(added.Username, result.Username);
            }
        }

        [Test]
        public void AuthenticateShouldLoginAUser()
        {
            var options = new DbContextOptionsBuilder<ExpensesDbContext>()
              .UseInMemoryDatabase(databaseName: nameof(AuthenticateShouldLoginAUser))
              .Options;

            using (var context = new ExpensesDbContext(options))
            {
                var usersService = new UserService(context, config);
                var added = new RegisterUserPostDto

                {
                    FullName = "codruta",
                    Username = "ioana",
                    Email = "maria@gmail.com",
                    Password = "1234567"
                };
                var result = usersService.Register(added);
                var authenticated = new LoginPostDto
                {
                    Username = "ioana",
                    Password = "1234567"
                };
                var authresult = usersService.Authenticate(added.Username, added.Password);

                Assert.IsNotNull(authresult);
                Assert.AreEqual(1, authresult.Id);
                Assert.AreEqual(authenticated.Username, authresult.Username);
            }
        }
        [Test]
        public void GetAllShouldReturnAllUser()
        {
            var options = new DbContextOptionsBuilder<ExpensesDbContext>()
              .UseInMemoryDatabase(databaseName: nameof(GetAllShouldReturnAllUser))
              .Options;

            using (var context = new ExpensesDbContext(options))
            {
                var usersService = new UserService(context, config);
                var added1 = new RegisterUserPostDto

                {
                    FullName = "codruta1",
                    
                    Email = "maria@gmail.com",
                    Username = "ioana",
                    Password = "1234567"
                };
                var added2 = new RegisterUserPostDto

                {
                    FullName = "codruta3",
                    
                    Email = "maria@gmail.com",
                    Username = "ioana",

                    Password = "1234567"
                };

                usersService.Register(added1);
                usersService.Register(added2);

                int numberOfElements = usersService.GetAll().Count();

                Assert.NotZero(numberOfElements);
                

            }
        }

    }
}