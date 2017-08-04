using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Playground.core.Models
{
    public static class AutoMapperConfig
    {
        public static IServiceCollection AddMappings( this IServiceCollection services ) {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Company, CompanyViewModel>()
                    .ForMember(vm => vm.NumberOfSites, conf => conf.MapFrom(ol => ol.Sites.Count));

                cfg.CreateMap<CompanyViewModel, CompanyViewModel>();


                cfg.CreateMap<CompanySite, SiteViewModel>()
                    .ForMember(vm => vm.SiteName, conf => conf.MapFrom(ol => ol.Site.Name))
                    .ForMember(vm => vm.CompanyName, conf => conf.MapFrom(ol => ol.Company.Name))
                    .ForMember(vm => vm.Translation, conf => conf.MapFrom(ol => ol.Translation.Text))
                    ;
            });

            services.AddSingleton<IConfigurationProvider>(config);

            return services;
        }
    }
}
