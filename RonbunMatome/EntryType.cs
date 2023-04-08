namespace RonbunMatome
{
    public enum EntryType
    {
        Article,
        InProceedings,
        Misc,
        PhdThesis,
        TechReport
    }

    public class EntryTypeConverter
    {
        public static EntryType Convert(string value)
        {
            if (value == "article")
            {
                return EntryType.Article;
            }

            if (value == "inproceedings")
            {
                return EntryType.InProceedings;
            }

            return EntryType.Misc;
        }

        public static EntryType Convert(EntryType value)
        {
            return value;
        }
    }
}
