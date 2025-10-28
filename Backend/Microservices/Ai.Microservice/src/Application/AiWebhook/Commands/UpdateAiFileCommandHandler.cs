using Application.AiWebhook.Queries;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Commands;

public class UpdateAiFileCommandHandler
    : IRequestHandler<UpdateAiFileCommand, Result<AiFileDto>>
{
    private readonly IAiFileRepository _repository;

    public UpdateAiFileCommandHandler(IAiFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AiFileDto>> Handle(
        UpdateAiFileCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.FileId, cancellationToken);

        if (existing is null)
        {
            return Result.Failure<AiFileDto>(
                new Error("AiFile.NotFound", $"File with id '{request.FileId}' was not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            existing.FileName = request.FileName.Trim();
        }

        if (request.OcrContent is not null)
        {
            existing.OcrContent = request.OcrContent;
        }

        if (request.TranslatedContent is not null)
        {
            existing.TranslatedContent = request.TranslatedContent;
        }

        if (request.CurrentContent is not null)
        {
            existing.CurrentContent = request.CurrentContent;
        }

        await _repository.UpdateAsync(existing, cancellationToken);

        var dto = new AiFileDto(
            existing.FileId,
            existing.FileName,
            existing.UserId,
            existing.CreatedDate,
            existing.Status);

        return Result.Success(dto);
    }
}
