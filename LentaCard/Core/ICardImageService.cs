namespace LentaCard.Core
{
    public interface ICardImageService
    {
        byte[] GenerateCardImage(string code);
    }

}
