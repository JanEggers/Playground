namespace Playground.core.Models
{
    public class CompanySite
    {
        static int count = 0;

        public CompanySite()
        {
            count++;
        }

        public Company Company { get; set; }

        public Site Site { get; set; }
    }

    public class SiteViewModel
    {
        public string CompanyName { get; set; }

        public string SiteName { get; set; }
    }
}
