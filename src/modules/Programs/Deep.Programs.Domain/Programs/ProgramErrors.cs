using Deep.Common.Domain;

namespace Deep.Programs.Domain.Programs;

public static class ProgramErrors
{
    public static Error NotFound(Guid programId) =>
        Error.NotFound(
            "Programs.NotFound",
            $"The program with the identifier {programId} was not found"
        );

    public static readonly Error StartDateInPast = Error.Problem(
        "Programs.StartDateInPast",
        "The program start date is in the past"
    );

    public static readonly Error EndDatePrecedesStartDate = Error.Problem(
        "Programs.EndDatePrecedesStartDate",
        "The program end date precedes the start date"
    );

    public static readonly Error NotDraft = Error.Problem(
        "Programs.NotDraft",
        "The program is not in draft status"
    );
    public static readonly Error AlreadyCancelled = Error.Problem(
        "Programs.AlreadyCanceled",
        "The program was already canceled"
    );
    public static readonly Error AlreadyStarted = Error.Problem(
        "Programs.AlreadyStarted",
        "The program has already started"
    );

    public static readonly Error ProgramUserNotFound = Error.Problem(
        "Programs.UserNotFound",
        "The program user/s were not found"
    );

    public static readonly Error CoordinatorRequired = Error.Problem(
        "Programs.CoordinatorRequired",
        "At least one program user with the coordinator role is required"
    );

    public static readonly Error BrandAmbassadorRequired = Error.Problem(
        "Programs.BrandAmbassadorRequired",
        "At least one program user with the brand ambassador role is required"
    );

    public static readonly Error TooManyCoOwners = Error.Problem(
        "Programs.TooManyCoOwners",
        "The number of program users with the program owner role exceeds the allowed limit"
    );

    public static readonly Error AtLeastOneProductRequired = Error.Problem(
        "Programs.AtLeastOneProductRequired",
        "At least one product is required"
    );

    public static readonly Error InvalidProduct = Error.Problem(
        "Programs.InvalidProduct",
        "The product is invalid"
    );

    public static readonly Error InvalidSku = Error.Problem(
        "Programs.InvalidSku",
        "The product SKU is invalid"
    );

    public static readonly Error InvalidUnitPrice = Error.Problem(
        "Programs.InvalidUnitPrice",
        "The unit price must be zero or greater"
    );

    public static readonly Error InvalidStock = Error.Problem(
        "Programs.InvalidStock",
        "The stock must be zero or greater"
    );

    public static readonly Error InvalidQuantity = Error.Problem(
        "Programs.InvalidQuantity",
        "The quantity must be greater than zero"
    );

    public static Error InsufficientStock(string sku, int available, int requested) =>
        Error.Problem(
            "Programs.InsufficientStock",
            $"Insufficient stock for product {sku}. Available: {available}, Requested: {requested}"
        );

    public static readonly Error InvalidReservedQuantity = Error.Problem(
        "Programs.InvalidReservedQuantity",
        "The reserved quantity is invalid"
    );

    public static Error ProductNotFound(string sku) =>
        Error.NotFound("Programs.ProductNotFound", $"The product with SKU {sku} was not found");

    public static readonly Error ProgramNotActive = Error.Problem(
        "Programs.ProgramNotActive",
        "The program is not active and cannot process transactions"
    );
}
