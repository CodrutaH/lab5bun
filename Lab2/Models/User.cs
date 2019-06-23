﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lab2.Models
{

    public enum UserRole
    {
        Regular,
        UserManager,
        Admin,
    }
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        [EnumDataType(typeof(UserRole))]
        public UserRole UserRole { get; set; }
        public DateTime CreatedAt { get;  set; }

        public IEnumerable<Expense> Expenses { get; set; }
    }
}
