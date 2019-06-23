using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab2.DTOs;
using Lab2.Models;
using Lab2.Servies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Controllers
{
    [Authorize]
    //[Route("api/[controller]/[action]")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]LoginPostDto login)
        {
            var user = _userService.Authenticate(login.Username, login.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registerModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpPost]
        public IActionResult Register([FromBody]RegisterUserPostDto registerModel)
        {
            var user = _userService.Register(registerModel);
            if (user == null)
            {
                return BadRequest(new { ErrorMessage = "Username already exists." });
            }
            return Ok(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Regular, UserManager")]
        // GET: api/Users/5
        [HttpGet("{id}", Name = "GetUser")]
        public IActionResult GetUser(int id)
        {
            var existing = _userService.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            return Ok(existing);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userNew"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin,UserManager")]
        [HttpPost]
        public IActionResult Post([FromBody] PostUserDto userNew)
        {

            var user = _userService.Create(userNew);
            if (user == null)
            {
                return BadRequest(user);
            }
            return Ok();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userNew"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,UserManager")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] PostUserDto userNew)
        {
            //User addedBy = _userService.GetCurrentUser(HttpContext);
           // var result = _userService.Upsert(id, userNew, addedBy);
            User currentLogedUser = _userService.GetCurrentUser(HttpContext);
            var regDate = currentLogedUser.CreatedAt;
            var currentDate = DateTime.Now;
            var minDate = currentDate.Subtract(regDate).Days / (365 / 12);

            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser == null)
                {
                    return NotFound();
                }

            }

            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser.UserRole == UserRole.Admin)
                {
                    return Forbid();
                }


            }

            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser.UserRole == UserRole.UserManager && minDate <= 6)

                    return Forbid();
            }

            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser.UserRole == UserRole.UserManager && minDate >= 6)
                {
                    var result1 = _userService.Upsert(id, userNew);
                    return Ok(result1);
                }

            }

            var result = _userService.Upsert(id, userNew);
            return Ok(result); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,UserManager")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            //User addedBy = _userService.GetCurrentUser(HttpContext);
           // var result = _userService.Delete(id, addedBy);

            User currentLogedUser = _userService.GetCurrentUser(HttpContext);
            var regDate = currentLogedUser.CreatedAt;
            var currentDate = DateTime.Now;
            var minDate = currentDate.Subtract(regDate).Days / (365 / 12);

            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser.UserRole == UserRole.Admin)
                {
                    return Forbid();
                }

            }

            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser.UserRole == UserRole.UserManager && minDate <= 6)

                    return Forbid();
            }
            if (currentLogedUser.UserRole == UserRole.UserManager)
            {
                User getUser = _userService.GetById(id);
                if (getUser.UserRole == UserRole.UserManager && minDate >= 6)
                {
                    var result1 = _userService.Delete(id);
                    return Ok(result1);
                }



            }

            var result = _userService.Delete(id);
            if (result == null)
            {
                return NotFound("User with the given id not fount !");
               
            }


            return Ok(result);
        }

        
    }
    }
