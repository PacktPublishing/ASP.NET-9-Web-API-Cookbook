$baseUrl = "http://localhost:5148"
$testEndpoint = "/Products?pageSize=10"
$fullUrl = $baseUrl + $testEndpoint;

$response = Invoke-WebRequest -Uri $fullUrl -Headers @{"Accept" = "application/json"}

$xPaginationHeader = $response.Headers["X-Pagination"]
$xPagination = $xPaginationHeader | ConvertFrom-Json 

$nextPageUrl = $baseUrl + $xPagination.NextPageUrl

$response = Invoke-WebRequest -Uri $nextPageUrl
$jsonContent = $response.Content | ConvertFrom-Json

$jsonContent | Format-Table -AutoSize | Out-String | Write-Host


