using BudgetAppBackend.Application.Contracts;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.DeleteAccount
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly IAuthRepository _authRepository;
        public DeleteAccountCommandHandler(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = _authRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            await _authRepository.DeleteUserAsync(user, cancellationToken);
            return true;
        }
    }
}
