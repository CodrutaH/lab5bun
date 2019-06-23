using Lab2.Controllers;
using Lab2.DTOs;
using Lab2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2.Servies
{
    public interface ICommentService
    {
        //IEnumerable<GetCommentsDto> GetComments(string text = "");
        PaginatedList<GetCommentsDto> GetAll(int page, string filterString);
        Comment Create(CommentPostModel task, User addedBy);

        Comment Upsert(int id, Comment comment);

        Comment Delete(int id);

        Comment GetById(int id);
    }
}
