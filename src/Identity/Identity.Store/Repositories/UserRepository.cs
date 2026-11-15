using AutoMapper;
using Dilcore.Identity.Core.Abstractions;
using Dilcore.Identity.Domain;
using Dilcore.Identity.Store.Entities;
using Dilcore.MongoDB.Abstractions.Repositories;
using Dilcore.MongoDB.Abstractions.Results;
using Dilcore.Results.Abstractions;
using FluentResults;
using MongoDB.Driver;

namespace Dilcore.Identity.Store.Repositories;

/// <summary>
/// MongoDB implementation of user repository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IGenericRepository<UserDocument> _collection;
    private readonly IMapper _mapper;

    public UserRepository(IGenericRepository<UserDocument> collection, IMapper mapper)
    {
        _collection = collection;
        _mapper = mapper;
    }

    public async Task<Result<User?>> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserDocument>.Filter.Eq(u => u.IdentityId, identityId);
        var document = await _collection.GetAsync(filter, cancellationToken);

        return MapOptional(document);
    }

    public async Task<Result<User?>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var filter = Builders<UserDocument>.Filter.Eq(u => u.Email, normalizedEmail);
        var document = await _collection.GetAsync(filter, cancellationToken);

        return MapOptional(document);
    }

    public async Task<Result<User>> StoreAsync(User user, CancellationToken cancellationToken = default)
    {
        var document = _mapper.Map<UserDocument>(user);
        var result = await _collection.StoreAsync(document, cancellationToken);

        if (result.HasError<DuplicateKeyError>())
        {
            return Result.Fail<User>(new ConflictError("A user with this email already exists."));
        }

        if (result.IsFailed)
        {
            return result.ToResult<User>();
        }

        return Result.Ok(_mapper.Map<User>(result.Value));
    }

    public async Task<Result<bool>> DeleteByIdentityIdAsync(string identityId, long eTag, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(u => u.IdentityId, identityId),
            Builders<UserDocument>.Filter.Eq(u => u.ETag, eTag));
        var result = await _collection.DeleteAsync(filter, cancellationToken);

        return MapDeleted(result);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, long eTag, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(u => u.Id, id),
            Builders<UserDocument>.Filter.Eq(u => u.ETag, eTag));
        var result = await _collection.DeleteAsync(filter, cancellationToken);

        return MapDeleted(result);
    }

    private Result<User?> MapOptional(Result<UserDocument> result)
    {
        if (result.IsSuccess)
        {
            return Result.Ok(_mapper.Map<User?>(result.Value));
        }

        if (result.HasError<DocumentNotFoundError>())
        {
            return Result.Ok<User?>(null);
        }

        return result.ToResult<User?>();
    }

    private static Result<bool> MapDeleted(Result<bool> result)
    {
        if (result.IsSuccess)
        {
            return result;
        }

        if (result.HasError<DocumentNotFoundError>())
        {
            return Result.Ok(false);
        }

        return result;
    }
}
