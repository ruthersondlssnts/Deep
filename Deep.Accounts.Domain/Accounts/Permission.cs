namespace Deep.Accounts.Domain.Accounts;

public sealed class Permission
{
    public static readonly Permission RegisterItAdmin = new("account.register.itadmin");
    public static readonly Permission RegisterProgramOwner = new("account.register.programowner");
    public static readonly Permission RegisterManager = new("account.register.manager");
    public static readonly Permission RegisterCoordinator = new("account.register.coordinator");
    public static readonly Permission RegisterBrandAmbassador = new("account.register.brandambassador");

    public static readonly Permission ReadCoordinators = new("account.read.coordinator");
    public static readonly Permission ReadProgramOwners = new("account.read.programowner");
    public static readonly Permission ReadBrandAmbassadors = new("account.read.brandambassador");
    public static readonly Permission ReadManagers = new("account.read.manager");
    public static readonly Permission ReadItAdmins = new("account.read.itadmin");

    public static readonly Permission ReadOwnPrograms = new("programs.read.own");
    public static readonly Permission ReadAllPrograms = new("programs.read.all");
    public static readonly Permission CreatePrograms = new("programs.create");
    public static readonly Permission ModifyPrograms = new("programs.update");

    public static readonly Permission AssignCoOwner = new("programs.assign.coowner");
    public static readonly Permission AssignCoordinator = new("programs.assign.coordinator");
    public static readonly Permission AssignBrandAmbassador = new("programs.assign.brandambassador");

    public string Code { get; }

    public Permission(string code)
    {
        Code = code;
    }
}