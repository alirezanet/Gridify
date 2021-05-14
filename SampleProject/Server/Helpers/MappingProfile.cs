using AutoMapper;
using SampleProject.DataTransferObjects;
using SampleProject.Entites;

namespace SampleProject.Helpers
{
   public class MappingProfile : Profile
   {
      public MappingProfile()
      {
         CreateMap<Person, PersonDto>()
            .ForMember(x => x.Address, opt => opt.MapFrom(x => x.Contact.Address))
            .ForMember(x => x.PhoneNumber, opt => opt.MapFrom(x => x.Contact.PhoneNumber.ToString()));
      }
   }
}
