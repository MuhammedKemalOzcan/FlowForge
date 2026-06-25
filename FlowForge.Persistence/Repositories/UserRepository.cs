using FlowForge.Domain.Entities;
using FlowForge.Domain.Repositories;
using FlowForge.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace FlowForge.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FlowForgeAPIDbContext _context;

        public UserRepository(FlowForgeAPIDbContext context)
        {
            _context = context;
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public void Remove(User user)
        {
            _context.Users.Remove(user);
        }
    }
}