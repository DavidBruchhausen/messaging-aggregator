FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/MessagingAggregator.Api/MessagingAggregator.Api.csproj", "MessagingAggregator.Api/"]
COPY ["src/MessagingAggregator.Application/MessagingAggregator.Application.csproj", "MessagingAggregator.Application/"]
COPY ["src/MessagingAggregator.HostedService/MessagingAggregator.HostedService.csproj", "MessagingAggregator.HostedService/"]
COPY ["src/MessagingAggregator.Gateway/MessagingAggregator.Gateway.csproj", "MessagingAggregator.Gateway/"]

RUN dotnet restore "MessagingAggregator.Api/MessagingAggregator.Api.csproj"
COPY /src .
WORKDIR "/src/MessagingAggregator.Api"

FROM build AS publish
RUN dotnet publish "MessagingAggregator.Api.csproj" -c Release -o /app/publish
WORKDIR "/"
COPY ["/entrypoint.sh","/app/publish/"]

FROM base AS final
WORKDIR /app
RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app

COPY --from=publish /app/publish/entrypoint.sh /app/entrypoint.sh
RUN chmod u+x ./entrypoint.sh

USER app  
COPY --from=publish /app/publish .

ENTRYPOINT ["sh", "entrypoint.sh"]