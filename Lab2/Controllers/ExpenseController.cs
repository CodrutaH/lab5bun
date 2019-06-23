using Lab2.DTOs;
using Lab2.Models;
using Lab2.Servies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private IExpenseService expenseService;
        private IUserService usersService;
        public ExpenseController(IExpenseService service, IUserService usersService)
        {
            this.expenseService = service;
            this.usersService = usersService;
        }
        /// <summary>
        /// Gets all the expenses.
        /// </summary>
        /// <param name="from">Optional, filter by start date</param>
        /// <param name="to">Optional, filter by end date</param>
        /// <param name="type">Optional, filter by type of expense</param>
        /// /// <param name="page">Optional, filter by type of expense</param>
        /// <returns>A list of Expense Objects</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
       // [Authorize(Roles = "Admin,Regular")]
        [HttpGet]
        public PaginatedList<GetExpenseDto> GetAll([FromQuery]DateTime? from, [FromQuery]DateTime? to, [FromQuery]TypeEnum? type, [FromQuery]int page = 1)
        {
            page = Math.Max(page, 1);
            return expenseService.GetAll(page, from, to, type);
        }

        /// <summary>
        /// Gets an expense by a given id.
        /// </summary>
        /// <param name="id">Get expense by id</param>
        /// <returns>An Expense Object</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[Authorize(Roles = "Admin,Regular")]
        [HttpGet("{id}")]
        public IActionResult GetExpense(int id)
        {
            var existing = expenseService.GetById(id);

            if (existing == null)
            {
                return NotFound();
            }

            return Ok(existing);
        }

        /// <summary>
        /// Add an expense.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /expense
        ///     {
        ///         "description": "expense1",
        ///         "sum": 100,
        ///         "location": "location1",
        ///         "date": "2019-05-14T20:30:00",
        ///         "currency": "RON",
        ///         "type": "Groceries",
        ///         "comments": [
        ///    	        {
        ///    		        "text": "super",
        ///    		        "important": true
        ///
        ///             },
        ///    	        {
        ///    		        "text": "wow",
        ///    		        "important": false
        ///    	        }	
        ///         ]
        ///     }	
        ///</remarks>
        /// <param name="expenseDto">The expense to add.</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Regular")]
        [HttpPost]
        public void Post([FromBody] PostExpenseDto expenseDto)
        {
            User addedBy = usersService.GetCurrentUser(HttpContext);
            expenseService.Create(expenseDto,addedBy);
        }

        /// <summary>
        /// Updates an expense and if it doesn't exist, it creates one.
        /// </summary>
        /// <param name="id">The id of an expense</param>
        /// <param name="expense">The expense to update</param>
        /// <returns>The updated expense.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Regular")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Expense expense)
        {
            var result = expenseService.Upsert(id, expense);
            return Ok(result);
        }

        /// <summary>
        /// Delete an expanse by a given id
        /// </summary>
        /// <param name="id">The id of an expense</param>
        /// <returns>The deleted expense.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Regular")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(int id)
        {
            var result = expenseService.Delete(id);

            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        
    }
}
