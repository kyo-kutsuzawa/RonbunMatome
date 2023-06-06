namespace RonbunMatome
{
    public enum EntryType
    {
        Article,
        Book,
        InBook,
        InProceedings,
        MastersThesis,
        Misc,
        PhdThesis,
        TechReport
    }

    public class EntryTypeConverter
    {
        public static string Convert(EntryType value)
        {
            return value switch
            {
                EntryType.Article => "article",
                EntryType.Book => "book",
                EntryType.InBook => "inbook",
                EntryType.InProceedings => "inproceedings",
                EntryType.MastersThesis => "mastersthesis",
                EntryType.Misc => "misc",
                EntryType.PhdThesis => "phdthesis",
                EntryType.TechReport => "techreport",
                _ => "misc",
            };
        }

        public static EntryType ConvertBack(string value)
        {
            return value switch
            {
                "article" => EntryType.Article,
                "book" => EntryType.Book,
                "inbook" => EntryType.InBook,
                "inproceedings" => EntryType.InProceedings,
                "mastersthesis" => EntryType.MastersThesis,
                "misc" => EntryType.Misc,
                "phdthesis" => EntryType.PhdThesis,
                "techreport" => EntryType.TechReport,
                _ => EntryType.Misc,
            };
        }
    }
}
