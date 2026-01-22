using System.Text.Json.Serialization;

namespace lumires.Api.Shared.Models;

public record MovieSource(
    string ProviderName, 
    string Type,        
    string Url,        
    string Quality,     
    double? Price        
);