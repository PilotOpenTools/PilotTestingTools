# Выпуск версии

1. Поднимите `Version` в [Directory.Build.props](Directory.Build.props). Версия общая для всех проектов.
2. Перенесите записи из раздела «Не выпущено» в [CHANGELOG.md](CHANGELOG.md) под заголовок новой версии с датой и поправьте ссылки внизу файла. Workflow сверяет только тег с `Version` и наличие раздела версии в `CHANGELOG.md`.
3. Обновите упоминания версии в документации, они не подставляются автоматически: `Версия ...` и `PackageReference` в [README.md](README.md), `PackageReference` в [docs/pilot-test-host.md](docs/pilot-test-host.md). Найти их: `git grep -n "<предыдущая версия>" -- README.md docs`.
4. Проверьте локально:

   ```powershell
   dotnet build PilotTestingTools.sln -c Release
   dotnet test PilotTestingTools.sln -c Release --no-build
   powershell -ExecutionPolicy Bypass -File tools/pack-release.ps1
   ```

   Файлы релиза появятся в `artifacts/`: архив плагина-хоста, CLI, инспектор, два пакета `.nupkg` с символами `.snupkg` и `SHA256SUMS.txt`.
5. Закоммитьте изменения, поставьте тег и отправьте его:

   ```powershell
   git tag -a v<версия> -m "v<версия>"
   git push origin main v<версия>
   ```

6. Workflow [release.yml](.github/workflows/release.yml) по тегу `v*` проверит, что версия тега совпадает с `Version`, соберёт и проверит решение, подготовит те же файлы и создаст на GitHub черновик релиза с пометкой pre-release. Описание берётся из раздела версии в `CHANGELOG.md`. Проверьте черновик и опубликуйте его.
7. Там же workflow отправляет пакеты в nuget.org через Trusted Publishing: на nuget.org настроена политика для репозитория `PilotOpenTools/PilotTestingTools` и файла `release.yml`, ключ API не хранится. Если публикация из workflow не прошла, отправьте пакеты вручную (нужен ключ API):

   ```powershell
   dotnet nuget push artifacts/PilotTestHost.Sdk.<версия>.nupkg --source https://api.nuget.org/v3/index.json --api-key <ключ>
   dotnet nuget push artifacts/PilotUITesting.<версия>.nupkg --source https://api.nuget.org/v3/index.json --api-key <ключ>
   ```

   Файлы `.snupkg` с символами лежат рядом, `dotnet nuget push` отправляет их вместе с пакетом.

Публикация пакета в nuget.org необратима: версию можно только скрыть, но не переиспользовать. Перед выпуском на новую версию клиента Pilot проверьте, что плагин-хост в ней загружается, и повторите контрактные тесты каталога (см. [docs/pilot-ui-testing.md](docs/pilot-ui-testing.md)).
