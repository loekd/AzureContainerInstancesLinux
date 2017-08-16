FROM microsoft/aspnetcore:1.1
ARG source
WORKDIR /app
EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "AzureContainerInstances.JobProcessing.dll"]
