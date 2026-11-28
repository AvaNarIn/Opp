public class AttachmentDownload
{
    public Stream Content { get; set; } = Stream.Null;

    public string ContentType { get; set; } = "";

    public string FileName { get; set; } = "";
}