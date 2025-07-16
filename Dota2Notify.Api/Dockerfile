# Use the official .NET 8 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory inside the container
WORKDIR /app

# Copy the published application from the publish folder
COPY publish/ .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "Dota2Notify.Api.dll"]