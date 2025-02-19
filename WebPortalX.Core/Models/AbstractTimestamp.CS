namespace WebPortalX.Core.Models
{
    public interface IAbstractTimestamp
    {
        DateTime CreatedAt { get; }
        DateTime UpdatedAt { get; }
    }

}