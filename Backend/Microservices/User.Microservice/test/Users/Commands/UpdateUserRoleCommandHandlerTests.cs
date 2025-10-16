using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Users.Commands;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace test.Users.Commands
{
    public class UpdateUserRoleCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;

        public UpdateUserRoleCommandHandlerTests()
        {
            _userRepositoryMock = new();
            _roleRepositoryMock = new();
            _userRoleRepositoryMock = new();
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_RoleIsNotSupported()
        {
            var handler = new UpdateUserRoleCommandHandler(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object);

            var command = new UpdateUserRoleCommand(Guid.NewGuid(), "Manager");

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            _userRepositoryMock.VerifyNoOtherCalls();
            _roleRepositoryMock.VerifyNoOtherCalls();
            _userRoleRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_When_UserDoesNotExist()
        {
            var userId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException());

            var handler = new UpdateUserRoleCommandHandler(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object);

            var command = new UpdateUserRoleCommand(userId, "Admin");

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_Should_AssignRole_When_UserExists()
        {
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var user = new User { UserId = userId };
            var role = new Role { RoleId = roleId, RoleName = "Admin" };

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _roleRepositoryMock
                .Setup(repo => repo.GetByNameAsync("Admin", It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _userRoleRepositoryMock
                .Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<UserRole>());

            _userRoleRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateUserRoleCommandHandler(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _userRoleRepositoryMock.Object);

            var command = new UpdateUserRoleCommand(userId, "Admin");

            var result = await handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _userRoleRepositoryMock.Verify(repo => repo.DeleteRange(It.IsAny<IEnumerable<UserRole>>()), Times.Never);
            _userRoleRepositoryMock.Verify(repo => repo.AddAsync(
                It.Is<UserRole>(ur => ur.UserId == userId && ur.RoleId == roleId),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
