namespace Metabase.Web.Models
{
    public class SelectInstancesViewModel
    {
        public string SourceUrl { get; set; }
        public string TargetUrl { get; set; }
        public string SourceToken { get; set; }
        public string TargetToken { get; set; }

        public List<MetabaseInstance> Sources { get; set; }

        public List<MetabaseInstance> Targets { get; set; }
    }
}
