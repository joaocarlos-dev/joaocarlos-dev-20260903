namespace EmployeeManagmentSystem.Domain.Common;

public abstract class AuditableEntity
{
    protected AuditableEntity()
    {
    }

    protected AuditableEntity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("The entity identifier cannot be empty.");
        }

        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    protected void MarkAsUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    protected void MarkAsDeleted()
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = DateTimeOffset.UtcNow;
        MarkAsUpdated();
    }

    protected void EnsureNotDeleted(string entityName)
    {
        if (IsDeleted)
        {
            throw new DomainException($"The {entityName} has been deleted.");
        }
    }
}
