using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Users.Commands;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using MassTransit;
using Moq;
using SharedLibrary.Abstractions.UnitOfWork;
using SharedLibrary.Common.ResponseModel;
using SharedLibrary.Authentication;
using SharedLibrary.Contracts.UserCreating;

namespace test.Users.Commands
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;

        public RegisterUserCommandHandlerTests()
        {
            _userRepositoryMock = new();
            _roleRepositoryMock = new();
            _userRoleRepositoryMock = new();
            _mapperMock = new();
            _unitOfWorkMock = new();
            _passwordHasherMock = new();
            _publishEndpointMock = new();
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessResult_When_UserNotExist()
        {
            var command = new RegisterUserCommand("test_user", "test_user_email", "test_password");

            _userRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<User>());
            _userRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _roleRepositoryMock
                .Setup(repo => repo.GetByNameAsync("User", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role?)null);
            _roleRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _userRoleRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper => mapper.Map<User>(It.IsAny<RegisterUserCommand>()))
                .Returns((RegisterUserCommand cmd) => new User { UserId = Guid.NewGuid(), Name = cmd.Name, Email = cmd.Email });

            _passwordHasherMock
                .Setup(hasher => hasher.HashPassword(It.IsAny<string>()))
                .Returns("hashed_password");

            _publishEndpointMock
                .Setup(endpoint => endpoint.Publish(It.IsAny<UserCreatingSagaStart>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new RegisterUserCommandHandler(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _mapperMock.Object,
                _unitOfWorkMock.Object,
                _passwordHasherMock.Object,
                _publishEndpointMock.Object);

            Result result = await handler.Handle(command, default);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
