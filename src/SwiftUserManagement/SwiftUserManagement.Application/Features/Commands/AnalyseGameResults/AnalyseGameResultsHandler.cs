using AutoMapper;
using MediatR;
using SwiftUserManagement.Application.Contracts.Infrastructure;
using SwiftUserManagement.Application.Contracts.Persistence;

namespace SwiftUserManagement.Application.Features.Commands.AnalyseGameResults
{
    public class AnalyseGameResultsHandler : IRequestHandler<AnalyseGameResultsCommand, string>
    {
        private readonly IMassTransitFactory _massTransitRepository;
        private readonly IUserRepository _userRepository;

        public AnalyseGameResultsHandler(IMassTransitFactory massTransitRepository, IUserRepository userRepository)
        {
            _massTransitRepository = massTransitRepository ?? throw new ArgumentNullException(nameof(massTransitRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<string> Handle(AnalyseGameResultsCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUser(request.UserName);
            if (user == null)
                return "User not found";

            var result = await _massTransitRepository.EmitGameAnalysis(request.result1, request.result2);

            if (result == "Connection error")
            {
                return "Can't connect to RabbitMQ/ Received an error";
            }
            Thread.Sleep(1000);
                
            var receivedData = await _massTransitRepository.ReceiveGameAnalysis();

           
            await _userRepository.AddGameAnalysisData(request.result1, request.result2, request.User_Id, request.level, receivedData);

            return receivedData;
        }
    }
}
