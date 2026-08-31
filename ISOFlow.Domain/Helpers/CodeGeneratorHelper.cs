namespace ISOFlow.Domain.Helpers;

public static class CodeGeneratorHelper
{
    public static string GenerateControlCode(int sequence) => $"CTRL-{sequence:D3}";
    public static string GenerateRiskCode(int sequence) => $"RISK-{sequence:D3}";
    public static string GenerateTreatmentCode(int sequence) => $"TRT-{sequence:D3}";
    public static string GeneratePolicyCode(int sequence) => $"POL-{sequence:D3}";
    public static string GenerateProcessCode(int sequence) => $"PROC-{sequence:D3}";
    public static string GenerateTaskCode(int sequence, int year = 2026) => $"TASK-{year}-{sequence:D3}";
    public static string GenerateEvidenceCode(int sequence, int year = 2026) => $"EVI-{year}-{sequence:D3}";
    public static string GenerateAuditCode(int sequence, int year = 2026) => $"AUD-{year}-{sequence:D3}";
    public static string GenerateFindingCode(int sequence) => $"FIND-{sequence:D3}";
    public static string GenerateCapaCode(int sequence) => $"CAPA-{sequence:D3}";
    public static string GenerateImprovementCode(int sequence) => $"IMP-{sequence:D3}";
}
