using WebApplication1.Dtos.Character;
using WebApplication1.Dtos.Weapon;
using WebApplication1.Models;

namespace WebApplication1.Services.WeaponService;

public interface IWeaponService
{
    Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon);
}