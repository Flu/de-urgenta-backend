﻿using System;
using System.Threading.Tasks;
using DeUrgenta.Certifications.Api.Queries;
using DeUrgenta.Certifications.Api.Validators;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain.Api;
using DeUrgenta.Tests.Helpers;
using DeUrgenta.Tests.Helpers.Builders;
using FluentAssertions;
using Xunit;

namespace DeUrgenta.Certifications.Api.Tests.Validators
{
    [Collection(TestsConstants.DbCollectionName)]
    public class GetCertificationsValidatorShould
    {
        private readonly DeUrgentaContext _dbContext;
        public GetCertificationsValidatorShould(DatabaseFixture fixture)
        {
            _dbContext = fixture.Context;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("my-weird-sub")]
        public async Task Invalidate_request_when_no_user_found_by_sub(string sub)
        {
            // Arrange
            var sut = new GetCertificationsValidator(_dbContext);

            // Act
            var isValid = await sut.IsValidAsync(new GetCertifications(sub));

            // Assert
            isValid.Should().BeOfType<GenericValidationError>();
        }

        [Fact]
        public async Task Validate_when_user_was_found_by_sub()
        {
            var sut = new GetCertificationsValidator(_dbContext);

            // Arrange
            var userSub = Guid.NewGuid().ToString();
            var user = new UserBuilder().WithSub(userSub).Build();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var isValid = await sut.IsValidAsync(new GetCertifications(userSub));

            // Assert
            isValid.Should().BeOfType<ValidationPassed>();
        }
    }
}
