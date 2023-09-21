using System.ComponentModel.DataAnnotations;

namespace Domain.BaseEntity
{
    public abstract class BaseClass
    {

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0,7);


        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
