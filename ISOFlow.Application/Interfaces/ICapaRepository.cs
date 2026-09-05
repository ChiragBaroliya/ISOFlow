using ISOFlow.Application.DTOs;
using ISOFlow.Domain.Entities;

namespace ISOFlow.Application.Interfaces;

public interface ICapaRepository
{
    Task<List<CAPA>> GetAllCapasAsync(int? organizationId);
    Task<PagedResponse<CAPA>> GetPagedCapasAsync(PagedRequestDto request, int? organizationId);
    Task<CAPA?> GetCapaByIdAsync(string id, int? organizationId);
    Task<CAPA> CreateCapaAsync(CAPA capa);
    Task<CAPA?> UpdateCapaAsync(CAPA capa, int organizationId);
    Task<bool> DeleteCapaAsync(string id, int organizationId);
    Task<bool> AddActionItemAsync(string capaId, CapaActionItem item, int organizationId);
    Task<bool> ToggleActionItemAsync(string capaId, string actionItemId, int organizationId);
}
