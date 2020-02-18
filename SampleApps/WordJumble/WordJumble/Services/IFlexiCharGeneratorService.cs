using WordJumble.Models;

namespace WordJumble.Services
{
    public interface IFlexiCharGeneratorService
    {
        FlexiChar GetRandomFlexiChar(char character);
    }
}