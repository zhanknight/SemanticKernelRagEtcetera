using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


string apikey = Environment.GetEnvironmentVariable("chatappApiKey")!;

// Initialize the kernel

IKernelBuilder kb = Kernel.CreateBuilder();
kb.AddAzureOpenAIChatCompletion(Environment.GetEnvironmentVariable("chatappDeploymentName")!, Environment.GetEnvironmentVariable("chatappEndpoint")!, apiKey: apikey);
kb.Services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = kb.Build();

//Kernel kernel = Kernel.CreateBuilder()
//    .AddAzureOpenAIChatCompletion("OAIZK35t", "https://oaizk.openai.azure.com/", apiKey: apikey)
//    .Build();

kernel.ImportPluginFromFunctions("DateTimeHelpers",
[
    kernel.CreateFunctionFromMethod(() => DateTime.UtcNow.ToString("R"), "GetCurrentUtcTime", "Retrieves the current time in UTC."), 
    //kernel.CreateFunctionFromMethod(
    //    () => $"{DateTime.UtcNow:r}", "Now", "Gets the current date and time"), 
kernel.CreateFunctionFromMethod((string cityName) =>
                cityName switch
                {
                    "Boston" => "61 and rainy",
                    "London" => "55 and cloudy",
                    "Miami" => "80 and sunny",
                    "Paris" => "60 and rainy",
                    "Tokyo" => "50 and sunny",
                    "Sydney" => "75 and sunny",
                    "Tel Aviv" => "80 and sunny",
                    _ => "31 and snowing",
                }, "Get_Weather_For_City", "Gets the current weather for the specified city"),
]);

// Create a new chat
//IChatCompletionService ai = kernel.GetRequiredService<IChatCompletionService>();
//ChatHistory chat = new("You are an AI assistant that helps people find information.");
//StringBuilder builder = new();

//OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };




//KernelFunction qa = kernel.CreateFunctionFromPrompt("""
//    The current date and time is {{ datetimehelpers.now }}.
//    {{ $input }}
//    """);

//// var arguments = new KernelArguments();

//// Q&A loop
//while (true)
//{
//    Console.Write("Question: ");
//    chat.AddUserMessage(Console.ReadLine()!);

//    builder.Clear();
//    await foreach (StreamingChatMessageContent message in ai.GetStreamingChatMessageContentsAsync(chat))
//    {
//        Console.Write(message);
//        builder.Append(message.Content);
//    }
//    Console.WriteLine();
//    chat.AddAssistantMessage(builder.ToString());

//    Console.WriteLine();
//}


// Create a new chat
var ai = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chat = new("You are an AI assistant that helps people find information.");
StringBuilder builder = new();
OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


// Q&A loop
while (true)
{
    Console.Write("Question: ");
    chat.AddUserMessage(Console.ReadLine()!);

    builder.Clear();
    await foreach (var message in ai.GetStreamingChatMessageContentsAsync(chat, settings, kernel))
    {
        Console.Write(message);
        builder.Append(message.Content);
    }
    Console.WriteLine();
    chat.AddAssistantMessage(builder.ToString());
}