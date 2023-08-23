using System.ComponentModel.DataAnnotations;

namespace Domain.BaseEntity
{
    public abstract class BaseClass
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();


        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
