namespace Metabase.Web.Models
{
    public class MetabaseInstance
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
    }

    public class MetabaseConfig
    {
        public List<MetabaseInstance> Sources { get; set; }
        public List<MetabaseInstance> Targets { get; set; }
    }
}

