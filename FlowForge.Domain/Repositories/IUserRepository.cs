using FlowForge.Domain.Entities;

namespace FlowForge.Domain.Repositories
{
    public interface IUserRepository
    {
        void Add(User user);

        Task<User?> GetByIdAsync(Guid userId);

        void Remove(User user);
    }
}