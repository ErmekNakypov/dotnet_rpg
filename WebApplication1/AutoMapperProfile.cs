using AutoMapper;
using WebApplication1.Dtos.Character;
using WebApplication1.Dtos.Dto;
using WebApplication1.Dtos.Weapon;
using WebApplication1.Models;

namespace WebApplication1;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Character, GetCharacterDto>();
        CreateMap<AddCharacterDto, Character>();
        CreateMap<Weapon,GetWeaponDto>();
        CreateMap<Skill, GetSkillDto>();
    }
}