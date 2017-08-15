FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "AzureContainerInstances.Logging.dll"]
