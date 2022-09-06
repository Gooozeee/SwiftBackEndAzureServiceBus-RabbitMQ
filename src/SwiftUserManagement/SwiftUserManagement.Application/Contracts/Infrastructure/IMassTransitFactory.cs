namespace SwiftUserManagement.Application.Contracts.Infrastructure
{
    using Microsoft.AspNetCore.Http;

    public interface IMassTransitFactory
    {
        Task<bool> EmitGameAnalysis(int result1, int result2);

        Task<string> ReceiveGameAnalysis();

        Task<bool> EmitVideonalysis(IFormFile video);

        Task<string> ReceiveVideonalysis();
    }
}
