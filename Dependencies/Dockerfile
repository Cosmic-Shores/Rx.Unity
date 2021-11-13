FROM mcr.microsoft.com/dotnet/sdk:5.0 AS deps-build
WORKDIR /app/Dependencies
COPY ./Dependencies ./
RUN dotnet build
RUN rm ./bin/Debug/netstandard2.1/System.Runtime.InteropServices.WindowsRuntime.dll
RUN rm ./bin/Debug/netstandard2.1/Rx.Unity.Dependencies.pdb
RUN rm ./bin/Debug/netstandard2.1/Rx.Unity.Dependencies.*


FROM node:16-alpine AS patcher-build
WORKDIR /app/patcher
COPY ./patcher ./
RUN npm ci


FROM alpine/git AS rx-retriver
WORKDIR /app
# git clone --branch rxnet-v5.0.0 --depth 1 https://github.com/dotnet/reactive.git
RUN git clone --branch PrivateTypesTestSimplification --depth 1 https://github.com/Cosmic-Shores/reactive.git
RUN cp ./reactive/Rx.NET/Source/Directory.build.props ./reactive/Rx.NET/Source/src/System.Reactive/Directory.build.props

FROM patcher-build AS rx-patcher
WORKDIR /app/patcher
COPY --from=rx-retriver /app ../
RUN npm run gen-rx-proj

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS rx-build
WORKDIR /app/reactive
COPY --from=rx-patcher /app/reactive ./
COPY ./Patches/SchedulerDefaults.cs ./Rx.NET/Source/src/System.Reactive/Concurrency/SchedulerDefaults.cs
COPY ./Patches/InternalsVisibleTo.cs ./Rx.NET/Source/src/System.Reactive/Properties/InternalsVisibleTo.cs
COPY ./Patches/global.json ./Rx.NET/Source/src/global.json

WORKDIR /app/reactive/Rx.NET/Source/src/System.Reactive
RUN dotnet build


FROM alpine/git AS rxdata-retriver
WORKDIR /app
RUN git clone --branch v1.0.2 --depth 1 https://github.com/Cosmic-Shores/Rx.Data.git

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS rxdata-build
WORKDIR /app/Rx.Data
COPY --from=rx-build /app/reactive/Rx.NET/Source/src/System.Reactive/bin/Debug/netstandard2.0 ./src/
COPY --from=rxdata-retriver /app ../
COPY ./Patches/Directory.build.props ./src/Directory.build.props
COPY ./Patches/References.build.props ./src/References.build.props
RUN dotnet build


FROM patcher-build AS test-patcher
WORKDIR /app/patcher
COPY --from=rx-retriver /app ../
COPY --from=rxdata-retriver /app ../
RUN npm run gen-tests


FROM scratch AS export-stage
COPY --from=deps-build /app/Dependencies/bin/Debug/netstandard2.1 ./Dependencies/out/
COPY --from=rx-build /app/reactive/Rx.NET/Source/src/System.Reactive/bin/Debug/netstandard2.0 ./Dependencies/out/
COPY --from=rxdata-build /app/Rx.Data/src/Rx.Extendibility/bin/Debug/netstandard2.0 ./Dependencies/out/
COPY --from=rxdata-build /app/Rx.Data/src/Rx.Data/bin/Debug/netstandard2.0 ./Dependencies/out/
COPY --from=test-patcher /app/Tests ./Assets/Tests/
