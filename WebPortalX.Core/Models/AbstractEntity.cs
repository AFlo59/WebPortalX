using System.ComponentModel.DataAnnotations;

namespace WebPortalX.Core.Models
{
    public abstract class AbstractEntity
    {
        [Key]
        public long Id { get; private set; }
    }
}