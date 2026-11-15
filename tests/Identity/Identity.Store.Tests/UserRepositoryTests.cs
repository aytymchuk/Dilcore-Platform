using AutoMapper;
using Dilcore.Identity.Domain;
using Dilcore.Identity.Store.Entities;
using Dilcore.Identity.Store.Repositories;
using Dilcore.MongoDB.Abstractions.Repositories;
using Dilcore.MongoDB.Abstractions.Results;
using Dilcore.Results.Abstractions;
using FluentAssertions;
using FluentResults;
using MongoDB.Driver;
using Moq;

namespace Dilcore.Identity.Store.Tests;

[TestFixture]
public class UserRepositoryTests
{
    private Mock<IGenericRepository<UserDocument>> _collection = null!;
    private Mock<IMapper> _mapper = null!;
    private UserRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _collection = new Mock<IGenericRepository<UserDocument>>();
        _mapper = new Mock<IMapper>();
        _sut = new UserRepository(_collection.Object, _mapper.Object);
    }

    [Test]
    public async Task GetByIdentityIdAsync_WhenDocumentIsMissing_ReturnsNull()
    {
        _collection
            .Setup(x => x.GetAsync(It.IsAny<FilterDefinition<UserDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<UserDocument>(new DocumentNotFoundError()));

        var result = await _sut.GetByIdentityIdAsync("missing");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public async Task StoreAsync_WhenDuplicateKey_ReturnsConflictError()
    {
        var user = new User
        {
            IdentityId = "identity",
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User"
        };
        _mapper
            .Setup(x => x.Map<UserDocument>(user))
            .Returns(new UserDocument
            {
                IdentityId = user.IdentityId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        _collection
            .Setup(x => x.StoreAsync(It.IsAny<UserDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<UserDocument>(new DuplicateKeyError()));

        var result = await _sut.StoreAsync(user);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<ConflictError>();
    }

    [Test]
    public async Task DeleteByIdentityIdAsync_WhenDocumentIsMissing_ReturnsFalse()
    {
        _collection
            .Setup(x => x.DeleteAsync(It.IsAny<FilterDefinition<UserDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<bool>(new DocumentNotFoundError()));

        var result = await _sut.DeleteByIdentityIdAsync("identity", 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}
