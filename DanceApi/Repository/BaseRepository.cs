using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DanceApi.Data;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Repository
{
    public abstract class BaseRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<T> CreateAsync(T entity, string userId)
        {
            entity.CreatedById = userId;
            entity.CreatedDate = DateTime.UtcNow;
    
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();


            return await _context.Set<T>()
                .Include(e => e.CreatedBy)  
                .FirstOrDefaultAsync(e => e.Id == entity.Id);
        }

        public async Task<bool> UpdateAsync(T entity, string userId)
        {
            entity.UpdatedById = userId;
            entity.UpdatedDate = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SoftDeleteAsync(T entity, string userId)
        {
            entity.IsDeleted = true;
            entity.DeletedById = userId;
            entity.DeletedDate = DateTime.UtcNow;
            _context.Set<T>().Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>()
                .Include(e => e.CreatedBy) 
                .Include(e => e.UpdatedBy) 
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }
        
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>()
                .Include(e => e.CreatedBy)  
                .Include(e => e.UpdatedBy)  
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }
    }
}