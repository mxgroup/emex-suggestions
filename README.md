# О сервисе

Сервис предоставляет API для получения подсказок при вводе номера детали в строку поиска. Подсказки запрашиваются у [ABCP](https://www.abcp.ru/).

# Структура сервиса и проектов

Основные компоненты сервиса:

- Suggestions.RestApi - API для работы с сервисом по http
  - Suggestions.Logic - содержит прикладную логику приложения (например в форме UseCase)
  - Suggestions.Infrastructure - содержит классы для работы с внешними сервисами 
  - Suggestions.Common - общий код
  - Suggestions.IntegrationTests - интеграционные тесты

# Внешние зависимости

Сервис зависит от следующих внешних компонентов

- SearchHistory.RestApi - сервис истории поиска ([TFS](http://tfs:8080/tfs/emex/v1.0/_git/emex.ru-search.history)). Необходим для получения истории поиска.
- IntegrationApi - сервис предоставляющий API для внешних сервисов  ([TFS](http://tfs:8080/tfs/emex/v1.0/_git/IntegrationApi)). необходим для получения маппинга производителей с ABCP.
- Web - старый сайт ([TFS](http://tfs:8080/tfs/emex/v1.0/_git/web)). Необходим для авторизации пользователя.

# Софт для разработки

Разработка ведется в Visual Studio 2019 и выше

В качестве IDE для работы с Postgres можно использовать [DBeaver](https://dbeaver.io/download/)

Для работы с Redis можно использовать Redis Desktop Manager ([ссылка для скачивания](https://www.softpedia.com/dyn-postdownload.php/8a9af214595877a9103d2b55a30d0c55/5fec7296/3dec8/0/3))

Для работы с docker контейнерами рекомендуется использовать [portainer](https://www.portainer.io/) либо [docker station](https://dockstation.io/)

# Локальный запуск проекта

Перед запуском сервиса необходимо запустить все сервисы от которых он [зависит](#Внешние зависимости).

# Запуск интеграционных тестов

Интеграционные тесты устроены по аналогии с подходом описанным в [данной статье](https://docs.microsoft.com/ru-ru/aspnet/core/test/integration-tests?view=aspnetcore-5.0). Проект Suggestions.IntegrationTests в каждом тесте поднимает в памяти инстанс Suggestions.RestApi, дополнительно конфигурирует его и далее работает с ним. Все внешние зависимости SearchHistory, IntegrationApi, all.sln подменяются, поэтому для запуска тестов никакой подготовки дополнительной инфраструктуры не требуется.