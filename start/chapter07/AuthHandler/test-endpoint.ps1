# First, let's get the token
$loginUrl = "http://localhost:5000/api/auth/login"  # Adjust the URL as needed
$credentials = @{
    Username = "testuser@example.com"
    Password = "YourTestPassword123!"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $credentials -ContentType "application/json"
$token = $loginResponse.token

# Now, let's use the token to call getBookById
$bookId = 1  # Replace with the ID of the book you want to retrieve
$booksUrl = "http://localhost:5000/api/Books/$bookId"  # Adjust the URL as needed

$headers = @{
    Authorization = "Bearer $token"
}

try {
    $bookResponse = Invoke-RestMethod -Uri $booksUrl -Method Get -Headers $headers
    Write-Output "Book details:"
    Write-Output $bookResponse
}
catch {
    Write-Error "An error occurred: $_"
    Write-Error "Status Code: $($_.Exception.Response.StatusCode.value__)"
    Write-Error "Status Description: $($_.Exception.Response.StatusDescription)"
}
