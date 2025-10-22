using Backend_App_Dengue.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend_App_Dengue.Data.Repository
{
    public class PublicationTagRepository
    {
        private readonly AppDbContext _context;

        public PublicationTagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PublicationTag>> GetAllAsync()
        {
            return await _context.Set<PublicationTag>()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<PublicationTag?> GetByIdAsync(int id)
        {
            return await _context.Set<PublicationTag>()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<PublicationTag> AddAsync(PublicationTag tag)
        {
            _context.Set<PublicationTag>().Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> UpdateAsync(PublicationTag tag)
        {
            _context.Set<PublicationTag>().Update(tag);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tag = await GetByIdAsync(id);
            if (tag == null) return false;

            _context.Set<PublicationTag>().Remove(tag);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<PublicationTag>> SearchAsync(string query)
        {
            return await _context.Set<PublicationTag>()
                .Where(t => t.Name.Contains(query))
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}
