using AutoMapper;
using BudgetAppBackend.Application.Contracts;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using BudgetAppBackend.Domain.UserAggregate;
using MediatR;

namespace BudgetAppBackend.Application.Features.Authentication.Registration
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResult>
    {
        public readonly IMapper _mapper;
        private readonly IAuthRepository? _authRepository;

        public RegisterUserCommandHandler(IMapper mapper, IAuthRepository? authRepository)
        {
            _mapper = mapper;
            _authRepository = authRepository;
        }

        public async Task<AuthResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            
            var currentUser = _mapper.Map<User>(request.AddUser);

            var newUser = User.CreateNewUser(
                currentUser.FirstName,
                currentUser.LastName,
                currentUser.Email,
               request.AddUser.Password
            );

            var verificationCode = User.GenerateVerificationToken();
            newUser.SetEmailVerificationCode(verificationCode, DateTime.UtcNow.AddHours(1), newUser.FirstName, newUser.LastName, newUser.Email);
            await _authRepository!.RegisterAsync(newUser, cancellationToken);

            var authResult = new AuthResult { Success = true, Message = "Thank you for registering! A verification email has been sent to your email address." };
            return authResult;
         
        }

    }
}
