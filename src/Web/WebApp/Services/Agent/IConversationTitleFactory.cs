using Dilcore.WebApp.Http.AiAgent.Dtos;

namespace Dilcore.WebApp.Services.Agent;

public interface IConversationTitleFactory
{
    string BuildTitle(ThreadResponseDto thread);

    string BuildTitle(ThreadStateDto thread);
}
