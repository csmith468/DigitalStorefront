using API.Database;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.OutputCaching;

namespace API.Services;

public interface ITagService
{
    Task<Result<List<TagDto>>> GetAllTagsAsync();
    Task<List<int>> GetOrCreateTagsAsync(List<string> tagNames);
}

public class TagService : ITagService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private  readonly IMapper _mapper;
    private readonly IOutputCacheStore _outputCacheStore;

    public TagService(IQueryExecutor queryExecutor, ICommandExecutor commandExecutor, IMapper mapper,
        IOutputCacheStore outputCacheStore)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _mapper = mapper;
        _outputCacheStore = outputCacheStore;
    }
    
    public async Task<Result<List<TagDto>>> GetAllTagsAsync()
    {
        var tags = await _queryExecutor.GetAllAsync<Tag>();
        var tagDtos = tags.Select(t => _mapper.Map<TagDto>(t)).ToList();
        return Result<List<TagDto>>.Success(tagDtos);
    }
    
    public async Task<List<int>> GetOrCreateTagsAsync(List<string> tagNames)
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

        var existing = (await _queryExecutor.GetWhereInStrAsync<Tag>("name", normalizedTagNames)).ToList();
        tagIds.AddRange(existing.Select(e => e.TagId));

        var existingNames = existing.Select(e => e.Name).ToHashSet();
        var tagsToAdd = normalizedTagNames.Where(n => !existingNames.Contains(n)).Select(n => new Tag { Name = n }).ToList();

        if (tagsToAdd.Count > 0)
        {
            await _commandExecutor.BulkInsertAsync(tagsToAdd);
            var newlyAddedTagIds = await _queryExecutor.GetWhereInStrAsync<Tag>("name", tagsToAdd.Select(t => t.Name).ToList());
            tagIds.AddRange(newlyAddedTagIds.Select(t => t.TagId));

            await _outputCacheStore.EvictByTagAsync("tags", CancellationToken.None);
        }

        return tagIds.Distinct().ToList();
    }
}