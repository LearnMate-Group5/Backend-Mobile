using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Queries;

internal sealed class GetAiFileByIdQueryHandler
    : IRequestHandler<GetAiFileByIdQuery, Result<AiFileDetailDto>>
{
    private readonly IAiFileRepository _repository;

    public GetAiFileByIdQueryHandler(IAiFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AiFileDetailDto>> Handle(
        GetAiFileByIdQuery request,
        CancellationToken cancellationToken)
    {
        var file = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (file is null)
        {
            return Result.Failure<AiFileDetailDto>(
                new Error("AiFile.NotFound", $"File with id '{request.FileId}' was not found."));
        }

        var dto = new AiFileDetailDto(
            file.FileId,
            file.FileName,
            file.OcrContent,
            file.TranslatedContent,
            file.UserId,
            file.CreatedDate,
            file.Status,
            file.CurrentContent);

        return Result.Success(dto);
    }
}
