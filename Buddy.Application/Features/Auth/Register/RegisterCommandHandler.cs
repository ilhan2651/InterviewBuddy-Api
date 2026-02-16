using BCrypt.Net;
using Buddy.Application.Common.Interfaces;
using Buddy.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.Auth.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = new Buddy.Domain.Entities.User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _unitOfWork.GetRepository<Buddy.Domain.Entities.User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RegisterResponse
            {
                Success = true,
                Message = "User registered successfully.",
                UserId = user.Id
            };
        }
    }
}
