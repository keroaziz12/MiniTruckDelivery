namespace MiniTruckDelivery.Models;

public sealed class AdminDashboardViewModel
{
    public int TotalUsers { get; init; }
    public int DriverAccounts { get; init; }
    public int DriverProfiles { get; init; }
    public int UnlinkedDriverProfiles { get; init; }
}

public sealed class AdminDriverRowViewModel
{
    public int Id { get; init; }
    public int? DriverId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public bool HasAccount { get; init; }
    public bool HasProfile { get; init; }
    public bool HasAccess { get; init; }
    public int TripsCount { get; init; }
    public decimal TotalExpenses { get; init; }
    public bool IsActive { get; init; }
}

public sealed class AdminTripRowViewModel
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string DriverName { get; init; } = string.Empty;
    public string StartLocation { get; init; } = string.Empty;
    public string EndLocation { get; init; } = string.Empty;
    public DateTime TripDate { get; init; }
    public decimal AgreedAmount { get; init; }
    public decimal TotalExpenses { get; init; }
    public bool IsCancelled { get; init; }
}

public sealed class AdminDriverDetailsViewModel
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public bool HasAccount { get; init; }
    public bool IsActive { get; init; }
    public int TripsCount { get; init; }
    public decimal TotalExpenses { get; init; }
    public List<AdminTripRowViewModel> Trips { get; init; } = [];
}

public sealed class AdminTripDetailsViewModel
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string DriverName { get; init; } = string.Empty;
    public string StartLocation { get; init; } = string.Empty;
    public string EndLocation { get; init; } = string.Empty;
    public DateTime TripDate { get; init; }
    public decimal AgreedAmount { get; init; }
    public bool IsCancelled { get; init; }
    public DateTime? CancelledAt { get; init; }
    public List<TripExpense> Expenses { get; init; } = [];
    public decimal TotalExpenses => Expenses.Sum(expense => expense.Amount);
}
