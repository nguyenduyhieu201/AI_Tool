
using Users.Domain.Abstractions;

namespace Users.Domain.Abstractions
{
    public abstract class Entity<T> : IEntity<T>
    {
        public T Id { set; get; }
        public DateTime? CreatedAt { set; get; }
        public string? CreatedBy { set; get; }
        public DateTime? LastModified { set; get; }
        public string? LastModifiedBy { set; get; }
    }
}
