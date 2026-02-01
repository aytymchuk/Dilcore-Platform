using AutoMapper;
using Dilcore.DocumentDb.MongoDb.Repositories.Abstractions;
using Dilcore.Identity.Core.Abstractions;
using Dilcore.Identity.Domain;
using Dilcore.Identity.Store.Entities;
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

        if (document.IsFailed)
        {
            return document.ToResult<User?>();
        }

        return Result.Ok(_mapper.Map<User?>(document.Value));
    }

    public async Task<Result<User?>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserDocument>.Filter.Eq(u => u.Email, email);
        var document = await _collection.GetAsync(filter, cancellationToken);

        if (document.IsFailed)
        {
            return document.ToResult<User?>();
        }

        return Result.Ok(_mapper.Map<User?>(document.Value));
    }

    public async Task<Result<User>> StoreAsync(User user, CancellationToken cancellationToken = default)
    {
        var document = _mapper.Map<UserDocument>(user);
        var result = await _collection.StoreAsync(document, cancellationToken);

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

        if (result.IsFailed)
        {
            return result.ToResult<bool>();
        }

        return Result.Ok(result.Value);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, long eTag, CancellationToken cancellationToken = default)
    {
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Eq(u => u.Id, id),
            Builders<UserDocument>.Filter.Eq(u => u.ETag, eTag));
        var result = await _collection.DeleteAsync(filter, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToResult<bool>();
        }

        return Result.Ok(result.Value);
    }
}