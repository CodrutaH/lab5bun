using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab2.DTOs;
using Lab2.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Servies
{
    public class ExpenseService : IExpenseService
    {
        private ExpensesDbContext context;

        public ExpenseService(ExpensesDbContext context)
        {
            this.context = context;
        }

        public PaginatedList<GetExpenseDto> GetAll(int page, DateTime? from = null, DateTime? to = null, TypeEnum? type = null)
        {
            IQueryable<Expense> result = context
                .Expenses
                .OrderBy(e => e.Date)
                .Include(expense => expense.Comments);

            PaginatedList<GetExpenseDto> paginatedResult = new PaginatedList<GetExpenseDto>();
            paginatedResult.CurrentPage = page;

            //if (from == null && to == null && type == null)
            //{
            //    return result.Select(expense => GetExpenseDto.DtoFromModel(expense));
            //}

            if (from != null)
            {
                result = result.Where(expense => expense.Date >= from);
            }

            if (to != null)
            {
                result = result.Where(expense => expense.Date <= to);
            }

            if (type != null)
            {
                result = result.Where(expense => expense.Type == type);
            }

            paginatedResult.NumberOfPages = (result.Count() - 1) / PaginatedList<GetExpenseDto>.EntriesPerPage + 1;
            result = result
                .Skip((page - 1) * PaginatedList<GetExpenseDto>.EntriesPerPage)
                .Take(PaginatedList<GetExpenseDto>.EntriesPerPage);
            paginatedResult.Entries = result.Select(expense => GetExpenseDto.DtoFromModel(expense)).ToList();

            return paginatedResult;
        }

        public Expense GetById(int id)
        {
            return context.Expenses.Include(ex => ex.Comments).FirstOrDefault(ex => ex.Id == id);
        }

        public Expense Create(PostExpenseDto expenseDto, User addedBy)
        {
            // TODO: how to store the user that added the expense as a field in Expense?
            Expense toAdd = PostExpenseDto.ModelFromDto(expenseDto);
            toAdd.Owner = addedBy;
            context.Expenses.Add(toAdd);
            context.SaveChanges();
            return toAdd;
        }

        public Expense Upsert(int id, Expense expense)
        {
            var existing = context.Expenses.AsNoTracking().FirstOrDefault(ex => ex.Id == id);

            if (existing == null)
            {
                context.Expenses.Add(expense);
                context.SaveChanges();
                return expense;
            }

            expense.Id = id;
            context.Expenses.Update(expense);
            context.SaveChanges();
            return expense;
        }

        public Expense Delete(int id)
        {
            var existing = context.Expenses
                .Include(ex => ex.Comments)
                .FirstOrDefault(expense => expense.Id == id);

            if (existing == null)
            {
                return null;
            }

            context.Expenses.Remove(existing);
            context.SaveChanges();
            return existing;
        }
        
    }
}
