﻿using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain.Api;
using DeUrgenta.Group.Api.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeUrgenta.Group.Api.CommandHandlers
{
    public class DeleteGroupHandler : IRequestHandler<DeleteGroup, Result<Unit, ValidationResult>>
    {
        private readonly IValidateRequest<DeleteGroup> _validator;
        private readonly DeUrgentaContext _context;

        public DeleteGroupHandler(IValidateRequest<DeleteGroup> validator, DeUrgentaContext context)
        {
            _validator = validator;
            _context = context;
        }

        public async Task<Result<Unit, ValidationResult>> Handle(DeleteGroup request, CancellationToken ct)
        {
            var validationResult = await _validator.IsValidAsync(request, ct);
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            var user = await _context.Users.FirstAsync(u => u.Sub == request.UserSub, ct);
            var group = await _context.Groups
                .Include(x=>x.Backpack)
                .FirstAsync(g => g.AdminId == user.Id, ct);

            _context.Backpacks.Remove(group.Backpack);
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}