using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Domain.UserAggregate;
using BudgetAppBackend.Domain.UserAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BudgetAppBackend.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

       
        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RegisterAsync(User addNewUser, CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(addNewUser, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task DeleteUserAsync(Task<User?> user, CancellationToken cancellationToken)
        {
            _context.Users.Remove(user.Result);
            return _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<User?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
}
