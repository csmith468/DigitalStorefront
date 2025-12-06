using API.Database;
using API.Models;
using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services;

public interface ITagService
{
    Task<Result<List<TagDto>>> GetAllTagsAsync(CancellationToken ct = default);
    Task<List<int>> GetOrCreateTagsAsync(List<string> tagNames, CancellationToken ct = default);
}

public class TagService : ITagService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IMapper _mapper;
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly IMemoryCache _cache;

    private const string TagsCacheKey = "tags:all";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);

    public TagService(IQueryExecutor queryExecutor, ICommandExecutor commandExecutor, IMapper mapper,
        IOutputCacheStore outputCacheStore, IMemoryCache cache)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _mapper = mapper;
        _outputCacheStore = outputCacheStore;
        _cache = cache;
    }
    
    public async Task<Result<List<TagDto>>> GetAllTagsAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(TagsCacheKey, out List<TagDto>? cached))
            return Result<List<TagDto>>.Success(cached!);
        
        var tags = await _queryExecutor.GetAllAsync<Tag>(ct);
        var tagDtos = tags.Select(t => _mapper.Map<TagDto>(t)).ToList();

        _cache.Set(TagsCacheKey, tagDtos, _cacheDuration);
        return Result<List<TagDto>>.Success(tagDtos);
    }
    
    public async Task<List<int>> GetOrCreateTagsAsync(List<string> tagNames, CancellationToken ct = default)
    {
        if (tagNames.Count == 0) return [];
        
        var normalizedTagNames = new List<string>();
        var tagIds = new List<int>();

        foreach (var tagName in tagNames)
        {
            var words = tagName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrWhiteSpace(t));
            normalizedTagNames.AddRange(words);
        }

        var existing = (await _queryExecutor.GetWhereInStrAsync<Tag>("name", normalizedTagNames, ct)).ToList();
        tagIds.AddRange(existing.Select(e => e.TagId));

        var existingNames = existing.Select(e => e.Name).ToHashSet();
        var tagsToAdd = normalizedTagNames.Where(n => !existingNames.Contains(n)).Select(n => new Tag { Name = n }).ToList();

        if (tagsToAdd.Count > 0)
        {
            await _commandExecutor.BulkInsertAsync(tagsToAdd, ct);
            var newlyAddedTagIds =
                await _queryExecutor.GetWhereInStrAsync<Tag>("name", tagsToAdd.Select(t => t.Name).ToList(), ct);
            tagIds.AddRange(newlyAddedTagIds.Select(t => t.TagId));

            _cache.Remove(TagsCacheKey);
            await _outputCacheStore.EvictByTagAsync("tags", CancellationToken.None);
        }

        return tagIds.Distinct().ToList();
    }
}