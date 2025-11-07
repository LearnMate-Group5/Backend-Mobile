using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
