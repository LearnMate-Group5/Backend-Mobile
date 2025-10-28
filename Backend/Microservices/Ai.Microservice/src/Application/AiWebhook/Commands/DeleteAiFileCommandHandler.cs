using Application.Common.Interfaces;
using MediatR;
using SharedLibrary.Common.ResponseModel;

namespace Application.AiWebhook.Commands;

public class DeleteAiFileCommandHandler : IRequestHandler<DeleteAiFileCommand, Result>
{
    private readonly IAiFileRepository _repository;

    public DeleteAiFileCommandHandler(IAiFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteAiFileCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.FileId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(new Error("AiFile.NotFound", $"File with id '{request.FileId}' was not found."));
        }

        await _repository.DeleteAsync(request.FileId, cancellationToken);
        return Result.Success();
    }
}
