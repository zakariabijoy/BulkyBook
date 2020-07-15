using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository:IRepositoryAsync<Category>
    {
        Task UpdateAsync(Category category);

    }
}
