using System;
using System.Collections.Generic;
using System.Linq;
using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Queries;

public class GetAiFilesQueryHandler
    : IRequestHandler<GetAiFilesQuery, Result<IReadOnlyList<AiFileDto>>>
{
    private readonly IAiFileRepository _repository;

    public GetAiFilesQueryHandler(IAiFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AiFileDto>>> Handle(
        GetAiFilesQuery request,
        CancellationToken cancellationToken)
    {
        var files = await _repository.GetAsync(
            request.IncludeAll,
            request.UserId,
            cancellationToken);

        IEnumerable<Domain.Entities.AiFile> filteredFiles = request.IncludeAll
            ? files
            : files.Where(file =>
                file.UserId.Equals(request.UserId, StringComparison.OrdinalIgnoreCase));

        var response = filteredFiles
            .Select(file => new AiFileDto(
                file.FileId,
                file.FileName,
                file.UserId,
                file.CreatedDate,
                file.Status))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<AiFileDto>>(response);
    }
}
