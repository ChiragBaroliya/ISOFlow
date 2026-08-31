using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface ICapaRepository
{
    Task<List<CAPA>> GetAllCapasAsync();
    Task<PagedResponse<CAPA>> GetPagedCapasAsync(PagedRequestDto request);
    Task<CAPA?> GetCapaByIdAsync(string id);
    Task<CAPA> CreateCapaAsync(CAPA capa);
    Task<CAPA?> UpdateCapaAsync(CAPA capa);
    Task<bool> DeleteCapaAsync(string id);
    Task<bool> AddActionItemAsync(string capaId, CapaActionItem item);
    Task<bool> ToggleActionItemAsync(string capaId, string actionItemId);
}
