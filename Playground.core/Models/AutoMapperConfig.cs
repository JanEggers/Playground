using AutoMapper;

namespace Playground.core.Models
{
    public class AutoMapperConfig
    {
        public static void Initialize() {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Company, CompanyViewModel>()
                    .ForMember(vm => vm.NumberOfSites, conf => conf.MapFrom(ol => ol.Sites.Count));


                cfg.CreateMap<CompanySite, SiteViewModel>()
                    .ForMember(vm => vm.SiteName, conf => conf.MapFrom(ol => ol.Site.Name))
                    .ForMember(vm => vm.CompanyName, conf => conf.MapFrom(ol => ol.Company.Name))
                    .ForMember(vm => vm.Translation, conf => conf.MapFrom(ol => ol.Translation.Text))
                    ;
            });
        }
    }
}
