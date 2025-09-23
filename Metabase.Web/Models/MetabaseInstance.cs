namespace Metabase.Web.Models
{
    public class MetabaseInstance
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class MetabaseConfig
    {
        public List<MetabaseInstance> Sources { get; set; }
        public List<MetabaseInstance> Targets { get; set; }
    }
}

