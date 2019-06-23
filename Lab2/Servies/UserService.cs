using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lab2.DTOs;
using Lab2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lab2.Servies
{
    public class UserService : IUserService
    {
        private ExpensesDbContext context;
        private readonly AppSettings appSettings;

        public UserService(ExpensesDbContext context, IOptions<AppSettings> appSettings)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }



        public GetUserDto Authenticate(string username, string password)
        {
            var user = context.Users
                .SingleOrDefault(x => x.Username == username &&
                                        x.Password == ComputeSha256Hash(password));

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
                    new Claim(ClaimTypes.Role,user.UserRole.ToString()),
                    new Claim(ClaimTypes.UserData, user.CreatedAt.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var result = new GetUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Token = tokenHandler.WriteToken(token)
            };

            return result;
        }

        private string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            // TODO: also use salt
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public GetUserDto Register(RegisterUserPostDto registerInfo)
        {
            User existing = context.Users.FirstOrDefault(u => u.Username == registerInfo.Username);
            if (existing != null)
            {
                return null;
            }

            context.Users.Add(new User
            {
                FullName = registerInfo.FullName,
                Email = registerInfo.Email,
                Username = registerInfo.Username,
                Password = ComputeSha256Hash(registerInfo.Password),
                UserRole = UserRole.Regular,
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
            return Authenticate(registerInfo.Username, registerInfo.Password);
        }

        public User GetCurrentUser(HttpContext httpContext)
        {
            string username = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            //string accountType = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod).Value;
            //return _context.Users.FirstOrDefault(u => u.Username == username && u.AccountType.ToString() == accountType);
            return context.Users.FirstOrDefault(u => u.Username == username);
        }


        public IEnumerable<GetUserDto> GetAll()
        {
            // return users without passwords
            return context.Users.Select(user => new GetUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                UserRole = user.UserRole,
                Token = null
            });
        }


        public User GetById(int id)
        {
            return context.Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Id == id);
        }

        public User Create(PostUserDto user)
        {
            User toAdd = PostUserDto.ToUser(user);

            context.Users.Add(toAdd);
            context.SaveChanges();
            return toAdd;

        }

        public User Upsert(int id, PostUserDto user, User addedBy)
        {
            var existing = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
            if (existing == null)
            {
                User toAdd = PostUserDto.ToUser(user);
                user.Password = ComputeSha256Hash(user.Password);
                context.Users.Add(toAdd);
                context.SaveChanges();
                return toAdd;
            }

            User toUpdate = PostUserDto.ToUser(user);
            toUpdate.Password = existing.Password;
            toUpdate.CreatedAt = existing.CreatedAt;
            toUpdate.Id = id;

            if (user.UserRole.Equals("Admin") && !addedBy.UserRole.Equals(UserRole.Admin))
            {
                return null;
            }
            else if ((existing.UserRole.Equals(UserRole.Regular) && 
                addedBy.UserRole.Equals(UserRole.UserManager)) ||

                (existing.UserRole.Equals(UserRole.UserManager) && 
                addedBy.UserRole.Equals(UserRole.UserManager) &&
                addedBy.CreatedAt.AddMonths(6) <= DateTime.Now))
            {
                context.Users.Update(toUpdate);
                context.SaveChanges();
                return toUpdate;
            }
            else if (addedBy.UserRole.Equals(UserRole.Admin))
            {
                context.Users.Update(toUpdate);
                context.SaveChanges();
                return toUpdate;
            }


            return null;
        }
        public User Upsert(int id, PostUserDto user)
        {
            var existing = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
            if (existing == null)
            {
                User toAdd = PostUserDto.ToUser(user);
                user.Password = ComputeSha256Hash(user.Password);
                context.Users.Add(toAdd);
                context.SaveChanges();
                return toAdd;
            }

            User toUpdate = PostUserDto.ToUser(user);
            toUpdate.Password = existing.Password;
            toUpdate.CreatedAt = existing.CreatedAt;
            toUpdate.Id = id;

            context.Users.Update(toUpdate);
            context.SaveChanges();
            return toUpdate;
        }

        public User Delete(int id, User addedBy)
        {

            User existing = context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);

            if (existing == null)

            {
                return null;
                }

                if (existing.UserRole.Equals("Admin") && !addedBy.UserRole.Equals(UserRole.Admin))
                {
                    return null;
                }
                else if ((existing.UserRole.Equals(UserRole.Regular) && addedBy.UserRole.Equals(UserRole.UserManager)) ||
                    (existing.UserRole.Equals(UserRole.UserManager) && addedBy.UserRole.Equals(UserRole.UserManager) && addedBy.CreatedAt.AddMonths(6) <= DateTime.Now))
                {
                    context.Comments.RemoveRange(context.Comments.Where(u => u.Owner.Id == existing.Id));
                    context.SaveChanges();
                    context.Expenses.RemoveRange(context.Expenses.Where(u => u.Owner.Id == existing.Id));
                    context.SaveChanges();

                    context.Users.Remove(existing);
                    context.SaveChanges();
                    return existing;
                }
                else if (addedBy.UserRole.Equals(UserRole.Admin))
                {
                    context.Comments.RemoveRange(context.Comments.Where(u => u.Owner.Id == existing.Id));
                    context.SaveChanges();
                    context.Expenses.RemoveRange(context.Expenses.Where(u => u.Owner.Id == existing.Id));
                    context.SaveChanges();

                    context.Users.Remove(existing);
                    context.SaveChanges();
                    return existing;
                }
                return null;
               
            }

        public GetUserDto Delete(int id)
        {
            var existing = context.Users.FirstOrDefault(u => u.Id == id);
            if (existing == null)
            {
                return null;
            }

            context.Users.Remove(existing);
            context.SaveChanges();

            return GetUserDto.FromUser(existing);
        }

        
    }
}
