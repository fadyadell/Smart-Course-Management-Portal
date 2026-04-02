# 🚦 Rate Limiting Screenshot #33 Guide

## What is Rate Limiting?

The API enforces **100 requests per minute** limit. After exceeding this, you get a `429 Too Many Requests` response.

---

## Quick PowerShell Script

Copy and paste this into **PowerShell** to test rate limiting:

```powershell
# Rate Limiting Test - Screenshot 33
$baseUrl = "http://localhost:5202/api"
$api = "$baseUrl/courses"  # Any endpoint works

Write-Host "Testing Rate Limit (100 req/min)..." -ForegroundColor Cyan
Write-Host "Making 101 rapid requests..." -ForegroundColor Yellow

# Make 101 requests
for ($i = 1; $i -le 101; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $api -Method GET -ErrorAction Stop -TimeoutSec 2 -UseBasicParsing
        if ($i -eq 101) {
            Write-Host "Request $i : Status $($response.StatusCode)" -ForegroundColor Red
        }
    }
    catch {
        $status = $_.Exception.Response.StatusCode.Value__
        if ($status -eq 429) {
            Write-Host "✅ Rate Limited! Request $i returned 429 Too Many Requests" -ForegroundColor Green
            Write-Host "Response: $($_.Exception.Response.StatusDescription)" -ForegroundColor Green
            break
        }
    }
    
    if ($i % 10 -eq 0) { Write-Host "Progress: $i requests..." }
}
```

---

## Manual Test (If Script Fails)

1. Open Swagger UI: http://localhost:5202/swagger
2. Click any GET endpoint (e.g., `GET /api/courses`)
3. Click "Try It Out"
4. Click "Execute" repeatedly **very quickly** 
5. After ~101 clicks, you should see:

```
Status: 429 Too Many Requests
{
  "error": "Rate limit exceeded. Maximum 100 requests per minute allowed."
}
```

---

## Capturing Screenshot 33

When you see the 429 response:

1. **Take a screenshot** showing:
   - Endpoint: `GET /api/courses` (or whichever you used)
   - Status Code: **429**
   - Response Body: The error message
   
2. Save as: `33_Rate_Limiting_429.png`

---

## 💡 Tips

- Rate limit resets every minute
- Limit applies **per IP address**
- All endpoints count toward the limit
- The header `X-RateLimit-Remaining` shows requests left

---

**That's it! Ready for Screenshot 33? 🚀**
