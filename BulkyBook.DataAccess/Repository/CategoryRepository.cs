using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository :  RepositoryAsync<Category>,ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public async Task UpdateAsync(Category category)
        {
            var objFromDb = await _db.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
            if (objFromDb != null)
            {
                objFromDb.Name = category.Name;

            }
       
        }
    }
}
