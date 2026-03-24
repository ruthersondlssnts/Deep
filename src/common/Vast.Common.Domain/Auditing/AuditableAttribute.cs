namespace Vast.Common.Domain.Auditing;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class AuditableAttribute : Attribute;
