FROM microsoft/dotnet:1.1-sdk-msbuild

# Copy the source and restore dependencies
COPY . /app

WORKDIR /app

RUN ["dotnet", "restore"]

RUN ["dotnet", "build"]

# Expose the port and start the app
EXPOSE 5000

CMD ["dotnet", "run", "--server.urls", "http://*:5000"]