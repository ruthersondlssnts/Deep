using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public static class ProgramErrors
{
    public static Error NotFound(Guid programId) =>
        Error.NotFound("Programs.NotFound", $"The program with the identifier {programId} was not found");

    public static readonly Error StartDateInPast = Error.Problem(
        "Programs.StartDateInPast",
        "The program start date is in the past");

    public static readonly Error EndDatePrecedesStartDate = Error.Problem(
        "Programs.EndDatePrecedesStartDate",
        "The program end date precedes the start date");

    public static readonly Error NotDraft = Error.Problem(
        "Programs.NotDraft",
        "The program is not in draft status");
    public static readonly Error AlreadyCancelled = Error.Problem(
        "Programs.AlreadyCanceled",
        "The program was already canceled");

    public static readonly Error AlreadyStarted = Error.Problem(
        "Programs.AlreadyStarted",
        "The program has already started");

    public static Error ProgramUserNotFound = Error.Problem(
        "Programs.UserNotFound",
        "The program user/s were not found");

    public static Error CoordinatorRequired = Error.Problem(
       "Programs.CoordinatorRequired",
       "At least one program user with the coordinator role is required");

    public static Error BrandAmbassadorRequired = Error.Problem(
      "Programs.BrandAmbassadorRequired",
      "At least one program user with the brand ambassador role is required");

    public static Error TooManyCoOwners = Error.Problem(
      "Programs.TooManyCoOwners",
      "The number of program users with the program owner role exceeds the allowed limit");

    public static Error AtLeastOneProductRequired = Error.Problem(
        "Programs.AtLeastOneProductRequired",
        "At least one product is required");

    public static Error InvalidProduct = Error.Problem(
        "Programs.InvalidProduct",
        "The product is invalid");
}
