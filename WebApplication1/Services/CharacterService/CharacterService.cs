using System.Security.Claims;
using WebApplication1.Dtos.Character;
using WebApplication1.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos;

namespace WebApplication1.Services.CharacterService;

public class CharacterService : ICharacterService
{
    private readonly IMapper _mapper;
    private readonly DataContext _dataContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CharacterService(IMapper mapper, DataContext dataContext, IHttpContextAccessor httpContextAccessor)
    {
        _mapper = mapper;
        _dataContext = dataContext;
        _httpContextAccessor = httpContextAccessor;
    }
    
    private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
        .FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
    {
        var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
        var dbCharacters = await _dataContext.Characters
            .Include(c => c.Weapon)
            .Include(c => c.Skills)
            .Where(c => c.User!.Id == GetUserId()).ToListAsync();
        serviceResponse.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
        return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
    {
        var serviceResponse = new ServiceResponse<GetCharacterDto>();
        var dbCharacter = await _dataContext.Characters
            .Include(c => c.Weapon)
            .Include(c => c.Skills)
            .FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
        serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);   
        return serviceResponse;
    }

    public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
    {
        var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
        var character = _mapper.Map<Character>(newCharacter);
        character.User = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
        _dataContext.Add(character);
        await _dataContext.SaveChangesAsync();
        serviceResponse.Data = 
           await _dataContext.Characters
               .Where(c => c.User!.Id == GetUserId())
               .Select(c => _mapper.Map<GetCharacterDto>(c))
               .ToListAsync();
        return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
    {
        var serviceResponse = new ServiceResponse<GetCharacterDto>();
        try
        {
            var character =  
               await _dataContext.Characters
                   .Include(c => c.User)   
                   .FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);
            if (character == null || character.User!.Id != GetUserId())
            {
                throw new Exception($"Character with id {updatedCharacter.Id} not found");
            }
            
            character.Name = updatedCharacter.Name;
            character.HitPoints = updatedCharacter.HitPoints;
            character.Strength = updatedCharacter.Strength;
            character.Defense = updatedCharacter.Defense;
            character.Intelligence = updatedCharacter.Intelligence;
            character.Class = updatedCharacter.Class;

            await _dataContext.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        return serviceResponse;
    }

    [HttpDelete]
    public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
    {
        var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
        try
        {
            var character = await _dataContext.Characters
                .FirstAsync(c => c.Id == id && c.User!.Id == GetUserId());
            if (character == null)
            {
                throw new Exception($"Character with id {id} not found");
            }
            _dataContext.Characters.Remove(character);
            await _dataContext.SaveChangesAsync();
            serviceResponse.Data = await _dataContext.Characters
                .Where(c => c.User!.Id == GetUserId())   
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        return serviceResponse;
    }
    
    public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
    {
        var response = new ServiceResponse<GetCharacterDto>();
        try
        {
            var character = await _dataContext.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId &&
                                          c.User!.Id == GetUserId());

            if (character is null)
            {
                response.Success = false;
                response.Message = "Character not found.";
                return response;
            }

            var skill = await _dataContext.Skills
                .FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);
            if (skill is null)
            {
                response.Success = false;
                response.Message = "Skill not found.";
                return response;
            }

            character.Skills!.Add(skill);
            await _dataContext.SaveChangesAsync();
            response.Data = _mapper.Map<GetCharacterDto>(character);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
        }

        return response;
    }
}