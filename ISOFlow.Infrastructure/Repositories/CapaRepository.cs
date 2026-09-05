using Dapper;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using ISOFlow.Domain.Enums;
using ISOFlow.Infrastructure.Data;

namespace ISOFlow.Infrastructure.Repositories;

public class CapaRepository : BaseRepository, ICapaRepository
{
    public CapaRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

    public Task<List<CAPA>> GetAllCapasAsync(int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedListAsync("SELECT * FROM sp_capas_get_all(@p_organization_id)", r => new CAPA
        {
            Id = r.id.ToString(),
            OrganizationId = (int)r.organization_id,
            Code = (string)r.code,
            FindingId = r.finding_id != null ? r.finding_id.ToString() : string.Empty,
            Title = (string)r.title,
            RootCause = (string)r.root_cause ?? string.Empty,
            CorrectiveAction = (string)r.corrective_action ?? string.Empty,
            Owner = (string)r.owner,
            DueDate = (DateTime)r.due_date,
            Status = (CapaStatus)(int)r.status,
            EffectivenessVerification = (string)r.effectiveness_verification ?? string.Empty
        }, parameters);
    }

    public Task<PagedResponse<CAPA>> GetPagedCapasAsync(PagedRequestDto request, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_capas_get_paged(@p_page_number, @p_page_size, @p_organization_id, @p_search_term, @p_status)",
            r => new CAPA
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                FindingId = r.finding_id != null ? r.finding_id.ToString() : string.Empty,
                Title = (string)r.title,
                RootCause = (string)r.root_cause ?? string.Empty,
                CorrectiveAction = (string)r.corrective_action ?? string.Empty,
                Owner = (string)r.owner,
                DueDate = (DateTime)r.due_date,
                Status = (CapaStatus)(int)r.status,
                EffectivenessVerification = (string)r.effectiveness_verification ?? string.Empty
            },
            parameters,
            request.PageNumber,
            request.PageSize);
    }

    public Task<CAPA?> GetCapaByIdAsync(string id, int? organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);
        parameters.Add("p_organization_id", organizationId);

        return QueryMappedFirstOrDefaultAsync(
            "SELECT * FROM sp_capas_get_by_id(@id, @p_organization_id)",
            r => new CAPA
            {
                Id = r.id.ToString(),
                OrganizationId = (int)r.organization_id,
                Code = (string)r.code,
                FindingId = r.finding_id != null ? r.finding_id.ToString() : string.Empty,
                Title = (string)r.title,
                RootCause = (string)r.root_cause ?? string.Empty,
                CorrectiveAction = (string)r.corrective_action ?? string.Empty,
                Owner = (string)r.owner,
                DueDate = (DateTime)r.due_date,
                Status = (CapaStatus)(int)r.status,
                EffectivenessVerification = (string)r.effectiveness_verification ?? string.Empty
            },
            parameters);
    }

    public async Task<CAPA> CreateCapaAsync(CAPA capa)
    {
        int.TryParse(capa.FindingId, out var findingId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", capa.OrganizationId);
        parameters.Add("p_code", capa.Code);
        parameters.Add("p_finding_id", findingId > 0 ? (int?)findingId : null);
        parameters.Add("p_title", capa.Title);
        parameters.Add("p_root_cause", capa.RootCause);
        parameters.Add("p_corrective_action", capa.CorrectiveAction);
        parameters.Add("p_owner", capa.Owner);
        parameters.Add("p_due_date", capa.DueDate);
        parameters.Add("p_status", (int)capa.Status);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_capas_create(@p_organization_id, @p_code, @p_finding_id, @p_title, @p_root_cause, @p_corrective_action, @p_owner, @p_due_date, @p_status)", parameters);
        capa.Id = insertedId.ToString();
        return capa;
    }

    public async Task<CAPA?> UpdateCapaAsync(CAPA capa, int organizationId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", capa.Id);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_title", capa.Title);
        parameters.Add("p_root_cause", capa.RootCause);
        parameters.Add("p_corrective_action", capa.CorrectiveAction);
        parameters.Add("p_owner", capa.Owner);
        parameters.Add("p_due_date", capa.DueDate);
        parameters.Add("p_status", (int)capa.Status);
        parameters.Add("p_effectiveness_verification", capa.EffectivenessVerification);

        var updated = await QuerySingleOrDefaultAsync<bool>("SELECT sp_capas_update(@p_id, @p_organization_id, @p_title, @p_root_cause, @p_corrective_action, @p_owner, @p_due_date, @p_status, @p_effectiveness_verification)", parameters);
        return updated ? capa : null;
    }

    public async Task<bool> DeleteCapaAsync(string id, int organizationId)
    {
        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_capas_delete(@id, @organizationId)", new { id, organizationId });
    }

    public async Task<bool> AddActionItemAsync(string capaId, CapaActionItem item, int organizationId)
    {
        int.TryParse(capaId, out var cId);
        var parameters = new DynamicParameters();
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_capa_id", cId);
        parameters.Add("p_title", item.Title);
        parameters.Add("p_assigned_to", item.AssignedTo);
        parameters.Add("p_due_date", item.DueDate);

        var insertedId = await QuerySingleAsync<int>("SELECT sp_capa_action_items_create(@p_organization_id, @p_capa_id, @p_title, @p_assigned_to, @p_due_date)", parameters);
        item.Id = insertedId.ToString();
        item.OrganizationId = organizationId;
        return true;
    }

    public async Task<bool> ToggleActionItemAsync(string capaId, string actionItemId, int organizationId)
    {
        int.TryParse(actionItemId, out var itemId);

        var currentValue = await QueryFirstOrDefaultAsync<bool?>(
            "SELECT is_completed FROM capa_action_items WHERE id = @itemId AND organization_id = @organizationId",
            new { itemId, organizationId });
        if (currentValue == null)
        {
            return false;
        }

        var parameters = new DynamicParameters();
        parameters.Add("p_id", itemId);
        parameters.Add("p_organization_id", organizationId);
        parameters.Add("p_is_completed", !currentValue.Value);

        return await QuerySingleOrDefaultAsync<bool>("SELECT sp_capa_action_items_toggle(@p_id, @p_organization_id, @p_is_completed)", parameters);
    }
}
