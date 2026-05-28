using Baxture.Api.Dtos;
using Baxture.Api.Models;

namespace Baxture.Api.Services;

public interface IUserExportService
{
    ExportResult Export(IReadOnlyCollection<User> users, string format);
}
