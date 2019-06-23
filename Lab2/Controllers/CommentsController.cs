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
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private ICommentService commentService;
        private readonly IUserService userService;

        public CommentsController(ICommentService service, IUserService userService)
        {
            this.commentService = service;
            this.userService = userService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public PaginatedList<GetCommentsDto> GetComments([FromQuery]string filterString, [FromQuery]int page = 1)
        {
            page = Math.Max(page, 1);
            return commentService.GetAll(page, filterString); ;
        }
   

[HttpGet("{id}")]
[Authorize(Roles = "Admin,Regular")]
public IActionResult Get(int id)
{
    var found = commentService.GetById(id);
    if (found == null)
    {
        return NotFound();
    }

    return Ok(found);
}

[HttpPost]
[Authorize(Roles = "Admin,Regular")]
[ProducesResponseType(201)]
[ProducesResponseType(400)]
public void Post([FromBody] CommentPostModel comment)
{
    User addedBy = userService.GetCurrentUser(HttpContext);
    commentService.Create(comment, addedBy);
}

[Authorize(Roles = "Admin,Regular")]
[HttpPut("{id}")]
public IActionResult Put(int id, [FromBody] Comment comment)
{
    var result = commentService.Upsert(id, comment);
    return Ok(result);
}

[ProducesResponseType(200)]
[ProducesResponseType(404)]
[HttpDelete("{id}")]
[Authorize(Roles = "Admin,Regular")]
public IActionResult Delete(int id)
{
    var result = commentService.Delete(id);
    if (result == null)
    {
        return NotFound();
    }

    return Ok(result);
}


   }
}
