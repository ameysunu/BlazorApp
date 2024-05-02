# Wellbeing

Wellbeing is an application dedicated to promoting mental health and well-being through various resources, articles, and tools.

## Features

- **Articles:** Access a curated collection of articles on topics related to mental health, mindfulness, stress reduction, and more.
- **Blogs:** Explore insightful blog posts covering personal stories, tips for self-care, and reflections on mental health journeys.
- **Resource Library:** Discover a range of mental health resources including recommended books, podcasts, apps, and online courses.
- **Generative AI Integration:** Utilize generative AI models to dynamically generate articles, blog posts, and informational content on mental health topics.
- **Personal Journal:** Maintain a personal journal to track mood fluctuations, reflect on daily experiences, and set mental health goals.
- **Chatbot:** Talk to an AI chatbot to help you feel better or get any instant recommendations.

## Tech Stack

- **Frontend:** Blazor WebAssembly, HTML, CSS, JavaScript
- **Backend:** ASP.NET Core, Azure Functions
- **Languages:** C#
- **Database:** Azure Cosmos DB
- **Authentication:** Okta Auth0
- **Generative AI:** Google Gemini AI
- **Hosting:** Azure App Service

## Getting Started

1. Clone the repository:

    ```
    git clone https://github.com/ameysunu/BlazorApp
    ```

2. Navigate to the project directory:

    ```
    cd BlazorApp
    ```

3. Install dependencies:

    ```
    dotnet restore
    ```

4. Configure appsettings.json:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Auth0:Domain": "AUTH0-DOMAIN",
  "Auth0:ClientId": "AUTH0-CLIENTID",
  "isSandbox": "true",
  "CosmosConnectionString": "COSMOS-DB-CONNECTION-STRING",
  "CosmosDB": "COSMOSDB-NAME",
  "isUSAppService": "true",
  "CosmosUserContainer": "COSMOS-DB-USERS-CONTAINER-NAME",
  "GeminiAPIKey": "GEMINI-API-KEY",
  "GeminiEndpoint": "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent",
  "RecommendationEngineUrl": "AZURE-FUNCTION-URL"
}
```

5. Run the application:

    ```
    dotnet run
    ```

## Fun Fact

This entire README.md file was auto written by Google Gemini :)