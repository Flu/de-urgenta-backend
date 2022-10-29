﻿using System.Threading;
using System.Threading.Tasks;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain.Api;
using DeUrgenta.Group.Api.Commands;
using Microsoft.EntityFrameworkCore;

namespace DeUrgenta.Group.Api.Validators
{
    public class RemoveFromGroupValidator : IValidateRequest<RemoveFromGroup>
    {
        private readonly DeUrgentaContext _context;

        public RemoveFromGroupValidator(DeUrgentaContext context)
        {
            _context = context;
        }

        public async Task<ValidationResult> IsValidAsync(RemoveFromGroup request, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Sub == request.UserSub, ct);
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user == null || targetUser == null)
            {
                return ValidationResult.GenericValidationError;
            }

            if (user.Id == request.UserId)
            {
                return new LocalizableValidationError("cannot-remove-user", "cannot-remove-yourself-message");
            }

            var isPartOfTheGroup = await _context.UsersToGroups.AnyAsync(utg => utg.UserId == user.Id
                && utg.GroupId == request.GroupId,
                ct);
            if (!isPartOfTheGroup)
            {
                return ValidationResult.GenericValidationError;
            }

            var isAdmin = await _context.Groups.AnyAsync(g => g.Admin.Id == user.Id, ct);
            if (!isAdmin)
            {
                return new LocalizableValidationError("cannot-remove-user", "only-group-admin-can-remove-users-message");
            }

            var requestedUserIsInGroup = await _context
                .UsersToGroups
                .AnyAsync(utg => utg.Group.Id == request.GroupId && utg.User.Id == request.UserId, ct);

            if (!requestedUserIsInGroup)
            {
                return ValidationResult.GenericValidationError;
            }

            return ValidationResult.Ok;
        }
    }
}