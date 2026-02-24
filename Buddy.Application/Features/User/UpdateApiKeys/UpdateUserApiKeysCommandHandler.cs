using Buddy.Application.Common.Interfaces;
using Buddy.Application.Services;
using Buddy.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Buddy.Application.Features.User.UpdateApiKeys
{
    public class UpdateUserApiKeysCommandHandler : IRequestHandler<UpdateUserApiKeysCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEncryptionService _encryptionService;

        public UpdateUserApiKeysCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IEncryptionService encryptionService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _encryptionService = encryptionService;
        }

        public async Task<bool> Handle(UpdateUserApiKeysCommand request, CancellationToken cancellationToken)
        {
            var currentUserIntId = _currentUserService.UserId;
            if (!currentUserIntId.HasValue)
            {
                throw new UnauthorizedAccessException("Giriş yapan kullanıcı bulunamadı.");
            }
            var userId = currentUserIntId.Value;

            var user = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.ApiKeys)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
            }

            if (user.ApiKeys == null)
            {
                user.ApiKeys = new UserApiKey { UserId = userId };
                await _unitOfWork.UserApiKeys.AddAsync(user.ApiKeys);
            }

            // Encrypt and save keys
            user.ApiKeys.SimliApiKey = _encryptionService.Encrypt(request.SimliApiKey);
            user.ApiKeys.ElevenLabsApiKey = _encryptionService.Encrypt(request.ElevenLabsApiKey);

            _unitOfWork.UserApiKeys.Update(user.ApiKeys);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
