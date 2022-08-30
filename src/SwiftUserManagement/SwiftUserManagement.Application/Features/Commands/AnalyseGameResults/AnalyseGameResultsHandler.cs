using AutoMapper;
using MediatR;
using SwiftUserManagement.Application.Contracts.Infrastructure;
using SwiftUserManagement.Application.Contracts.Persistence;

namespace SwiftUserManagement.Application.Features.Commands.AnalyseGameResults
{
    public class AnalyseGameResultsHandler : IRequestHandler<AnalyseGameResultsCommand, string>
    {
        private readonly IMassTransitRepository _massTransitRepository;
        private readonly IUserRepository _userRepository;

        public AnalyseGameResultsHandler(IMassTransitRepository massTransitRepository, IUserRepository userRepository)
        {
            _massTransitRepository = massTransitRepository ?? throw new ArgumentNullException(nameof(massTransitRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<string> Handle(AnalyseGameResultsCommand request, CancellationToken cancellationToken)
        {
            var result = await _massTransitRepository.EmitGameAnalysis(request.result1, request.result2);
            if (!result)
                return "User not found";
            Thread.Sleep(1000);
                
            var receivedData = await _massTransitRepository.ReceiveGameAnalysis();

            

            await _userRepository.AddGameAnalysisData(request.result1, request.result2, request.User_Id, request.level, receivedData);

            return receivedData;
        }
    }
}
