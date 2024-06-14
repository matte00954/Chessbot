
namespace ChessChallenge.API
{
    public interface IChessBot
    {
        Move Think(Board board, Timer timer);
    }
    public static class UCIHandler
    {
        public static string UCIString = "";
    }
}
