namespace Deep.Common.Domain.Auditing;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class NotAuditableAttribute : Attribute;
