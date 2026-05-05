public record RegisterUserRequest(
    string FullName,
    string Email,
    string Password,
    string? WhatsAppNumber
);