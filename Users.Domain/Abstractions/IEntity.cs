namespace Users.Domain.Abstractions
{
    public interface IEntity
    {
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }

    public interface IEntity<T> : IEntity
    {
        public T Id
        {
            get; set;
        }
    }
}
