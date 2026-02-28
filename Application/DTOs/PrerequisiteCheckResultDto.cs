namespace Application.DTOs
{
    public sealed class PrerequisiteCheckResultDto
    {
        public bool Passed { get; init; }
        public bool IsFirstRun { get; init; }
        public IReadOnlyList<string> Messages { get; init; } = [];
    }
}
