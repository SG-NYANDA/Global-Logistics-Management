using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public sealed class AgreementFileStore(IWebHostEnvironment environment, IConfiguration configuration)
{
    public const long MaximumFileSize = 10 * 1024 * 1024;

    private string AgreementsDirectory
    {
        get
        {
            var configuredPath = configuration["AgreementStorage:Path"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.Combine(environment.ContentRootPath, "App_Data", "agreements");
            }

            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath);
        }
    }

    public async Task<string> SaveAsync(IFormFile upload, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upload);
        if (upload.Length == 0 || upload.Length > MaximumFileSize)
        {
            throw new InvalidDataException("Agreement files must be between 1 byte and 10 MB.");
        }

        await using var content = new MemoryStream(checked((int)upload.Length));
        await upload.CopyToAsync(content, cancellationToken);
        content.Position = 0;
        PdfAgreementValidator.Validate(Path.GetFileName(upload.FileName), content);
        content.Position = 0;

        Directory.CreateDirectory(AgreementsDirectory);
        var storedFileName = $"{Guid.NewGuid():N}.pdf";
        var destinationPath = Path.Combine(AgreementsDirectory, storedFileName);
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            FileOptions.Asynchronous);
        await content.CopyToAsync(destination, cancellationToken);

        return storedFileName;
    }

    public string? FindPath(string storedFileName)
    {
        if (Path.GetFileName(storedFileName) != storedFileName
            || !string.Equals(Path.GetExtension(storedFileName), ".pdf", StringComparison.OrdinalIgnoreCase)
            || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(storedFileName), "N", out _))
        {
            return null;
        }

        var path = Path.Combine(AgreementsDirectory, storedFileName);
        return File.Exists(path) ? path : null;
    }

    public void Delete(string storedFileName)
    {
        var path = FindPath(storedFileName);
        if (path is not null)
        {
            File.Delete(path);
        }
    }
}