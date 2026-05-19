using lumires.Domain.Enums;

namespace lumires.Core.Mappers;

public static class PersonDepartmentMapper
{
    public static PersonDepartment FromString(string? department) => department switch
    {
        "Acting" => PersonDepartment.Acting,
        "Directing" => PersonDepartment.Directing,
        _ => PersonDepartment.Unknown
    };
    
    public static string ToString(PersonDepartment? department) => department switch
    {
        PersonDepartment.Acting => "Acting",
        PersonDepartment.Directing => "Directing",
        PersonDepartment.Unknown => "Unknown"
    };

}