using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Infrastructure.MockData;

namespace ISOFlow.Infrastructure.Repositories;

public class CapaRepository : ICapaRepository
{
    public Task<List<CAPA>> GetAllCapasAsync() => Task.FromResult(MockStore.Capas);

    public Task<CAPA?> GetCapaByIdAsync(string id) =>
        Task.FromResult(MockStore.Capas.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase) || c.Code.Equals(id, StringComparison.OrdinalIgnoreCase)));

    public Task<CAPA> CreateCapaAsync(CAPA capa)
    {
        if (string.IsNullOrWhiteSpace(capa.Id))
        {
            capa.Id = MockStore.NextId("CAPA", MockStore.Capas.Count);
        }
        if (string.IsNullOrWhiteSpace(capa.Code))
        {
            capa.Code = capa.Id;
        }
        MockStore.Capas.Add(capa);
        return Task.FromResult(capa);
    }

    public Task<CAPA?> UpdateCapaAsync(CAPA capa)
    {
        var existing = MockStore.Capas.FirstOrDefault(c => c.Id.Equals(capa.Id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Title = capa.Title;
            existing.FindingId = capa.FindingId;
            existing.RootCause = capa.RootCause;
            existing.CorrectiveAction = capa.CorrectiveAction;
            existing.Owner = capa.Owner;
            existing.DueDate = capa.DueDate;
            existing.Status = capa.Status;
            existing.EffectivenessVerification = capa.EffectivenessVerification;
        }
        return Task.FromResult(existing);
    }

    public Task<bool> DeleteCapaAsync(string id)
    {
        var existing = MockStore.Capas.FirstOrDefault(c => c.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            MockStore.Capas.Remove(existing);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> AddActionItemAsync(string capaId, CapaActionItem item)
    {
        var capa = MockStore.Capas.FirstOrDefault(c => c.Id.Equals(capaId, StringComparison.OrdinalIgnoreCase));
        if (capa != null)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                item.Id = MockStore.NextId("ACT", capa.ActionItems.Count);
            }
            item.CapaId = capa.Id;
            capa.ActionItems.Add(item);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> ToggleActionItemAsync(string capaId, string actionItemId)
    {
        var capa = MockStore.Capas.FirstOrDefault(c => c.Id.Equals(capaId, StringComparison.OrdinalIgnoreCase));
        if (capa != null)
        {
            var item = capa.ActionItems.FirstOrDefault(a => a.Id.Equals(actionItemId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
                return Task.FromResult(true);
            }
        }
        return Task.FromResult(false);
    }
}
