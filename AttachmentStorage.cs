public class AttachmentStorage
{
    public const long MaxFileSize = 10 * 1024 * 1024;

    private readonly string directory;
    private readonly ILogger<AttachmentStorage> logger;

    private static readonly Dictionary<string, string> ContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".webp"] = "image/webp",
            [".pdf"] = "application/pdf",
            [".txt"] = "text/plain",
            [".log"] = "text/plain",
            [".zip"] = "application/zip"
        };

    public AttachmentStorage(
        IWebHostEnvironment environment,
        ILogger<AttachmentStorage> logger)
    {
        directory = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "attachments");

        this.logger = logger;
    }

    public static (string FileName, string ContentType) Validate(
        IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxFileSize)
        {
            throw new TicketOperationException(
                400,
                "Размер файла должен быть от 1 байта до 10 МиБ.");
        }

        var name = Path.GetFileName(
            file.FileName.Replace('\\', '/'));

        if (string.IsNullOrWhiteSpace(name) ||
            name.Length > 255 ||
            name.Any(char.IsControl))
        {
            throw new TicketOperationException(
                400,
                "Некорректное имя файла.");
        }

        var extension = Path.GetExtension(name);

        if (!ContentTypes.TryGetValue(extension, out var contentType))
        {
            throw new TicketOperationException(
                400,
                "Разрешены PNG, JPG, JPEG, WEBP, PDF, TXT, LOG и ZIP.");
        }

        return (name, contentType);
    }

    public async Task SaveAsync(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(directory);

        await using var output = new FileStream(
            GetPath(id),
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81920,
            true);

        await file.CopyToAsync(output, cancellationToken);

        if (output.Length > MaxFileSize)
        {
            throw new TicketOperationException(
                400,
                "Размер файла превышает 10 МиБ.");
        }
    }

    public Stream Open(Guid id)
    {
        try
        {
            return new FileStream(
                GetPath(id),
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
        }
        catch (FileNotFoundException)
        {
            throw new TicketOperationException(
                404,
                "Файл не найден.");
        }
        catch (DirectoryNotFoundException)
        {
            throw new TicketOperationException(
                404,
                "Файл не найден.");
        }
    }

    public void DeleteIfExists(Guid id)
    {
        try
        {
            var path = GetPath(id);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException exception)
        {
            logger.LogWarning(
                exception,
                "Не удалось удалить файл вложения {AttachmentId}",
                id);
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(
                exception,
                "Нет доступа для удаления файла вложения {AttachmentId}",
                id);
        }
    }

    private string GetPath(Guid id)
    {
        return Path.Combine(
            directory,
            id.ToString("N") + ".bin");
    }
}