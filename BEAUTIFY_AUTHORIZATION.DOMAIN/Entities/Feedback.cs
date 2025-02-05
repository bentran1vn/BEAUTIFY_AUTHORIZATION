using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Feedback : AggregateRoot<Guid>, IAuditableEntity
{
    /*
     * Thêm thằng OrderDetailId vào kh phân biệt được chiều của relationship
     */
    //   public Guid OrderDetailId { get; set; }
    public virtual OrderDetail? OrderDetail { get; set; }
    [MaxLength(500)] public string? Content { get; set; }
    public int Rating { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}