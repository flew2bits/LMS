using MARC;

namespace LMS;

public static class MarcFieldTags
{
    public const string Isbn = "010";
    public const string Title = "245";
    public const string TitleRemainder = "245.b";
    public const string JcpsBarCode = "852.p";
}

public static class MarcFields
{
    private static string? GetSubfieldData(Record marcRecord, string field, char subfield) =>
        (marcRecord[field] as DataField)?[subfield]?.Data;

    private static string? GetSubfieldData(Record marcRecord, string fullField)
    {
        var parts = fullField.Split('.');
        return parts.Length == 1
            ? GetSubfieldData(marcRecord, parts[0], 'a')
            : GetSubfieldData(marcRecord, parts[0], parts[1][0]);
    }

    public static string? ISBN(this Record marcRecord) => GetSubfieldData(marcRecord, MarcFieldTags.Isbn);
    public static string? Title(this Record marcRecord) => GetSubfieldData(marcRecord, MarcFieldTags.Title);

    public static string FullTitle(this Record marcRecord) =>
        $"{GetSubfieldData(marcRecord, MarcFieldTags.Title)}{GetSubfieldData(marcRecord, MarcFieldTags.TitleRemainder) ?? ""}";
    public static string? JcpsBarCode(this Record marcRecord) => GetSubfieldData(marcRecord, MarcFieldTags.JcpsBarCode);
}