using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using System.Text.Json;
using System.Text.Json.Serialization;
using LexHack.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Register HTTP Client for Gemini REST calls
builder.Services.AddHttpClient();

// 1. Register Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure SQLite database is created on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated(); 
}

// Enable CORS using the configured policy
app.UseCors("AllowAll");

// 3. The API Endpoint with Native Gemini REST Extraction
app.MapPost("/api/audit/upload", async (IFormFile file, [FromServices] IConfiguration config, [FromServices] AppDbContext db, [FromServices] IHttpClientFactory httpClientFactory) =>
{
    if (file is null || file.Length == 0) return Results.BadRequest("No file uploaded.");

    // --- STEP A: PDF Extraction ---
    string extractedText = "";
    using (var stream = file.OpenReadStream())
    using (var pdf = PdfDocument.Open(stream))
    {
        foreach (var page in pdf.GetPages()) extractedText += page.Text + " ";
    }
    if (extractedText.Length > 25000) extractedText = extractedText[..25000];

    // --- STEP B: Native Gemini API Extraction ---
    var apiKey = config["Gemini:ApiKey"] ?? throw new Exception("API Key is missing.");
    var client = httpClientFactory.CreateClient();
    var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";    
    var payload = new
    {
        contents = new[] {
            new { parts = new[] { new { text = "You are a legal contract analyzer. Extract the requested fields from the following contract text. Use safe defaults if missing (0 for amounts, false for booleans). Contract text: " + extractedText } } }
        },
        generationConfig = new
        {
            responseMimeType = "application/json",
            responseSchema = new
            {
                type = "OBJECT",
                properties = new Dictionary<string, object>
                {
                    { "governing_law", new { type = "STRING" } },
                    { "liability_cap_amount", new { type = "NUMBER" } },
                    { "is_indemnification_mutual", new { type = "BOOLEAN" } },
                    { "non_solicitation_months", new { type = "INTEGER" } },
                    { "unilateral_termination_days", new { type = "INTEGER" } }
                },
                required = new[] { "governing_law", "liability_cap_amount", "is_indemnification_mutual", "non_solicitation_months", "unilateral_termination_days" }
            }
        }
    };

    ContractExtraction? extractedData;
    try 
    {
        var response = await client.PostAsJsonAsync(requestUrl, payload);
        if (!response.IsSuccessStatusCode) {
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n=== GEMINI API ERROR ===\n{errorBody}\n========================\n");
            return Results.Problem($"Gemini API Error: {response.StatusCode}");
        }

        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        var textResult = jsonDoc?.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString();

        if (string.IsNullOrEmpty(textResult)) return Results.Problem("Empty response from Gemini.");

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        extractedData = JsonSerializer.Deserialize<ContractExtraction>(textResult, jsonOptions);
        if (extractedData == null) return Results.Problem("Failed to deserialize Gemini response.");
    }
    catch (Exception ex) {
        Console.WriteLine($"\n=== EXCEPTION ===\n{ex.Message}\n=================\n");
        return Results.Problem($"Extraction Failed: {ex.Message}");
    }

    // --- STEP C: Playbook Evaluation Engine ---
    var playbook = await db.Playbooks.Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == 1);
    if (playbook == null) return Results.Problem("System Error: Standard Playbook not found.");

    var auditRun = new AuditRun { FileName = file.FileName, RiskScore = 0 };
    var resultsList = new List<AuditResult>();

    foreach(var rule in playbook.Rules)
    {
        bool passed = false;
        string extVal = "Not Found";

        if (rule.ClauseType == "governing_law") {
            extVal = extractedData.GoverningLaw ?? "Not Found";
            passed = (rule.Operator == "Equals" && string.Equals(extVal, rule.TargetValue, StringComparison.OrdinalIgnoreCase));
        }
        else if (rule.ClauseType == "liability_cap_amount") {
            extVal = extractedData.LiabilityCapAmount.ToString();
            if (rule.Operator == "LessThanOrEqual" && decimal.TryParse(rule.TargetValue, out var targetDec)) {
                passed = extractedData.LiabilityCapAmount <= targetDec;
            }
        }
        else if (rule.ClauseType == "is_indemnification_mutual") {
            extVal = extractedData.IsIndemnificationMutual.ToString();
            if (rule.Operator == "Equals" && bool.TryParse(rule.TargetValue, out var targetBool)) {
                passed = extractedData.IsIndemnificationMutual == targetBool;
            }
        }
        else if (rule.ClauseType == "non_solicitation_months") {
            extVal = extractedData.NonSolicitationMonths.ToString();
            if (rule.Operator == "LessThanOrEqual" && int.TryParse(rule.TargetValue, out var targetInt)) {
                passed = extractedData.NonSolicitationMonths <= targetInt;
            }
        }
        else if (rule.ClauseType == "unilateral_termination_days") {
            extVal = extractedData.UnilateralTerminationDays.ToString();
            if (rule.Operator == "GreaterThanOrEqual" && int.TryParse(rule.TargetValue, out var targetInt)) {
                passed = extractedData.UnilateralTerminationDays >= targetInt;
            }
        }

        if (!passed) auditRun.RiskScore += rule.Severity == "Critical" ? 50 : 20;

        resultsList.Add(new AuditResult { ClauseType = rule.ClauseType, ExtractedValue = extVal, Passed = passed });
    }

    auditRun.Results = resultsList;
    
    // --- STEP D: Save to Database and Return ---
    db.AuditRuns.Add(auditRun);
    await db.SaveChangesAsync();

    return Results.Ok(new { 
        success = true, 
        extraction = extractedData, 
        audit = auditRun 
    });
}).DisableAntiforgery();

app.Run();

// 4. Type Declarations
public record ContractExtraction(
    [property: JsonPropertyName("governing_law")] string? GoverningLaw,
    [property: JsonPropertyName("liability_cap_amount")] decimal LiabilityCapAmount,
    [property: JsonPropertyName("is_indemnification_mutual")] bool IsIndemnificationMutual,
    [property: JsonPropertyName("non_solicitation_months")] int NonSolicitationMonths,
    [property: JsonPropertyName("unilateral_termination_days")] int UnilateralTerminationDays
);