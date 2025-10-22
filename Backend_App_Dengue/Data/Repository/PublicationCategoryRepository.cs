using Backend_App_Dengue.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Data.Repository
{
    public class PublicationCategoryRepository
    {
        private readonly AppDbContext _context;

        public PublicationCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PublicationCategory>> GetAllAsync()
        {
            return await _context.Set<PublicationCategory>()
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<PublicationCategory?> GetByIdAsync(int id)
        {
            return await _context.Set<PublicationCategory>()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<PublicationCategory> AddAsync(PublicationCategory category)
        {
            _context.Set<PublicationCategory>().Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> UpdateAsync(PublicationCategory category)
        {
            _context.Set<PublicationCategory>().Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category == null) return false;

            _context.Set<PublicationCategory>().Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
