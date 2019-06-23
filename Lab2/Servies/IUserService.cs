using Lab2.DTOs;
using Lab2.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2.Servies
{
    public interface IUserService
    {
        GetUserDto Authenticate(string username, string password);

        GetUserDto Register(RegisterUserPostDto registerInfo);

        IEnumerable<GetUserDto> GetAll();
        User GetCurrentUser(HttpContext httpContext);

        User GetById(int id);
        User Create(PostUserDto user);
        User Upsert(int id, PostUserDto userPostModel, User addedBy);
        GetUserDto Delete(int id);
        User Delete(int id, User addedBy);
        User Upsert(int id, PostUserDto userNew);
    }

}
