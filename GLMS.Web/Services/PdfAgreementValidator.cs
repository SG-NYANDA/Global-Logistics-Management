namespace GLMS.Web.Services;

public static class PdfAgreementValidator
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    public static void Validate(string fileName, Stream content)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Only PDF agreements are allowed.");
        }

        ArgumentNullException.ThrowIfNull(content);
        var originalPosition = content.CanSeek ? content.Position : (long?)null;
        Span<byte> signature = stackalloc byte[PdfSignature.Length];
        try
        {
            content.ReadExactly(signature);
        }
        catch (EndOfStreamException exception)
        {
            throw new InvalidDataException("The agreement is not a valid PDF file.", exception);
        }
        finally
        {
            if (originalPosition.HasValue)
            {
                content.Position = originalPosition.Value;
            }
        }

        if (!signature.SequenceEqual(PdfSignature))
        {
            throw new InvalidDataException("The agreement is not a valid PDF file.");
        }
    }
}