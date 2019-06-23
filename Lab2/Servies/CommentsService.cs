using Lab2.Controllers;
using Lab2.DTOs;
using Lab2.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2.Servies
{
    public class CommentsService : ICommentService
    {

        private ExpensesDbContext context;

        public CommentsService(ExpensesDbContext context)
        {
            this.context = context;
        }

        //public IEnumerable<GetCommentsDto> GetComments(string text)
        //{
        //    IQueryable<GetCommentsDto> result = context.Comments.Select(x => new Comment
        //    {
        //        Id = x.Id,
        //        Text = x.Text,
        //        Important = x.Important,
        //        ExpenseId = (from Expense in context.Expenses
        //                     where Expense.Id == x.ExpenseId
        //                     select Expense.Id).FirstOrDefault()
        //    });
        //    //var result = context.Comments.Select(x 

        //    if (text != null)
        //    {
        //        result = result.Where(comment => comment.Text.Contains(text));
        //    }

        //    return result.Select(comment => GetCommentsDto.DtoFromModel(comment));
        //}

        public PaginatedList<GetCommentsDto> GetAll(int page, string filterString)
        {
            IQueryable<Comment> result = context
                .Comments
                .Where(c => string.IsNullOrEmpty(filterString) || c.Text.Contains(filterString))
                .OrderBy(c => c.Id)
                .Include(c => c.Expense);
            var paginatedResult = new PaginatedList<GetCommentsDto>();
            paginatedResult.CurrentPage = page;

            paginatedResult.NumberOfPages = (result.Count() - 1) / PaginatedList<GetCommentsDto>.EntriesPerPage + 1;
            result = result
                .Skip((page - 1) * PaginatedList<GetCommentsDto>.EntriesPerPage)
                .Take(PaginatedList<GetCommentsDto>.EntriesPerPage);
            paginatedResult.Entries = result.Select(c => GetCommentsDto.DtoFromModel(c)).ToList();

            return paginatedResult;
        }


        public IEnumerable<GetCommentsDto> GetAll(String filter)
        {
            IQueryable<Expense> result = context.Expenses.Include(c => c.Comments);

            List<GetCommentsDto> resultComments = new List<GetCommentsDto>();
            List<GetCommentsDto> resultCommentsAll = new List<GetCommentsDto>();

            foreach (Expense expense in result)
            {
                expense.Comments.ForEach(c =>
                {
                    if (c.Text == null || filter == null)
                    {
                        GetCommentsDto comment = new GetCommentsDto
                        {
                            Id = c.Id,
                            Important = c.Important,
                            Text = c.Text,
                            ExpenseId = expense.Id

                        };
                        resultCommentsAll.Add(comment);
                    }
                    else if (c.Text.Contains(filter))
                    {
                        GetCommentsDto comment = new GetCommentsDto
                        {
                            Id = c.Id,
                            Important = c.Important,
                            Text = c.Text,
                            ExpenseId = expense.Id

                        };
                        resultComments.Add(comment);

                    }
                });
            }
            if (filter == null)
            {
                return resultCommentsAll;
            }
            return resultComments;
        }
        public Comment Create(CommentPostModel comment, User addedBy)
        {
            Comment commentAdd = CommentPostModel.ToComment(comment);
            commentAdd.Owner = addedBy;
            context.Comments.Add(commentAdd);
            context.SaveChanges();
            return commentAdd;
        }

        public Comment Delete(int id)
        {
            var existing = context.Comments.FirstOrDefault(comment => comment.Id == id);
            if (existing == null)
            {
                return null;
            }
            context.Comments.Remove(existing);
            context.SaveChanges();
            return existing;
        }

        public Comment GetById(int id)
        {
            return context.Comments.FirstOrDefault(c => c.Id == id);
        }

        public Comment Upsert(int id, Comment comment)
        {
            var existing = context.Comments.AsNoTracking().FirstOrDefault(c => c.Id == id);
            if (existing == null)
            {
                context.Comments.Add(comment);
                context.SaveChanges();
                return comment;

            }

            comment.Id = id;
            context.Comments.Update(comment);
            context.SaveChanges();
            return comment;

        }
    }
}
