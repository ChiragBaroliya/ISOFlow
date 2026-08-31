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

    public Task<List<CAPA>> GetAllCapasAsync()
    {
        return QueryMappedListAsync("SELECT * FROM sp_capas_get_all()", r => new CAPA
        {
            Id = r.id.ToString(),
            Code = (string)r.code,
            FindingId = r.finding_id != null ? r.finding_id.ToString() : string.Empty,
            Title = (string)r.title,
            RootCause = (string)r.root_cause ?? string.Empty,
            CorrectiveAction = (string)r.corrective_action ?? string.Empty,
            Owner = (string)r.owner,
            DueDate = (DateTime)r.due_date,
            Status = (CapaStatus)(int)r.status,
            EffectivenessVerification = (string)r.effectiveness_verification ?? string.Empty
        });
    }

    public Task<PagedResponse<CAPA>> GetPagedCapasAsync(PagedRequestDto request)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_page_number", request.PageNumber);
        parameters.Add("p_page_size", request.PageSize);
        parameters.Add("p_search_term", string.IsNullOrWhiteSpace(request.SearchTerm) ? null : request.SearchTerm.Trim());
        parameters.Add("p_status", int.TryParse(request.StatusFilter, out var st) ? st : (int?)null);

        return QueryPagedAsync(
            "SELECT * FROM sp_capas_get_paged(@p_page_number, @p_page_size, @p_search_term, @p_status)",
            r => new CAPA
            {
                Id = r.id.ToString(),
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

    public Task<CAPA?> GetCapaByIdAsync(string id)
    {
        return QueryMappedFirstOrDefaultAsync(
            "SELECT id, code, finding_id, title, root_cause, corrective_action, owner, due_date, status, effectiveness_verification FROM capas WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)",
            r => new CAPA
            {
                Id = r.id.ToString(),
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
            new { id });
    }

    public async Task<CAPA> CreateCapaAsync(CAPA capa)
    {
        int.TryParse(capa.FindingId, out var findingId);
        var parameters = new DynamicParameters();
        parameters.Add("p_code", capa.Code);
        parameters.Add("p_finding_id", findingId > 0 ? (int?)findingId : null);
        parameters.Add("p_title", capa.Title);
        parameters.Add("p_root_cause", capa.RootCause);
        parameters.Add("p_corrective_action", capa.CorrectiveAction);
        parameters.Add("p_owner", capa.Owner);
        parameters.Add("p_due_date", capa.DueDate);
        parameters.Add("p_status", (int)capa.Status);
        parameters.Add("p_effectiveness_verification", capa.EffectivenessVerification);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO capas (code, finding_id, title, root_cause, corrective_action, owner, due_date, status, effectiveness_verification) VALUES (@p_code, @p_finding_id, @p_title, @p_root_cause, @p_corrective_action, @p_owner, @p_due_date, @p_status, @p_effectiveness_verification) RETURNING id", parameters);
        capa.Id = insertedId.ToString();
        return capa;
    }

    public async Task<CAPA?> UpdateCapaAsync(CAPA capa)
    {
        int.TryParse(capa.FindingId, out var findingId);
        var parameters = new DynamicParameters();
        parameters.Add("p_id", capa.Id);
        parameters.Add("p_finding_id", findingId > 0 ? (int?)findingId : null);
        parameters.Add("p_title", capa.Title);
        parameters.Add("p_root_cause", capa.RootCause);
        parameters.Add("p_corrective_action", capa.CorrectiveAction);
        parameters.Add("p_owner", capa.Owner);
        parameters.Add("p_due_date", capa.DueDate);
        parameters.Add("p_status", (int)capa.Status);
        parameters.Add("p_effectiveness_verification", capa.EffectivenessVerification);

        var rows = await ExecuteAsync("UPDATE capas SET finding_id = @p_finding_id, title = @p_title, root_cause = @p_root_cause, corrective_action = @p_corrective_action, owner = @p_owner, due_date = @p_due_date, status = @p_status, effectiveness_verification = @p_effectiveness_verification, updated_at = (CURRENT_TIMESTAMP AT TIME ZONE 'Asia/Kolkata') WHERE id::VARCHAR = @p_id OR LOWER(code) = LOWER(@p_id)", parameters);
        return rows > 0 ? capa : null;
    }

    public async Task<bool> DeleteCapaAsync(string id)
    {
        var rows = await ExecuteAsync("DELETE FROM capas WHERE id::VARCHAR = @id OR LOWER(code) = LOWER(@id)", new { id });
        return rows > 0;
    }

    public async Task<bool> AddActionItemAsync(string capaId, CapaActionItem item)
    {
        int.TryParse(capaId, out var cId);
        var parameters = new DynamicParameters();
        parameters.Add("p_capa_id", cId);
        parameters.Add("p_title", item.Title);
        parameters.Add("p_assigned_to", item.AssignedTo);
        parameters.Add("p_due_date", item.DueDate);
        parameters.Add("p_is_completed", item.IsCompleted);

        var insertedId = await QuerySingleAsync<int>("INSERT INTO capa_action_items (capa_id, title, assigned_to, due_date, is_completed) VALUES (@p_capa_id, @p_title, @p_assigned_to, @p_due_date, @p_is_completed) RETURNING id", parameters);
        item.Id = insertedId.ToString();
        return true;
    }

    public async Task<bool> ToggleActionItemAsync(string capaId, string actionItemId)
    {
        var rows = await ExecuteAsync("UPDATE capa_action_items SET is_completed = NOT is_completed WHERE id::VARCHAR = @actionItemId", new { actionItemId });
        return rows > 0;
    }
}
