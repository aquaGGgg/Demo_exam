1) папка Models предназначена для хранение моделей для API для данного задание есть две основные сущности, это order(заявка) и user(пользователь) так же реализованные DTO(DAO пока что нету).
2) папка front представляется собой некоторые html развёртки и fetch методы для работы с API.
3) папка Service предствялет собой некоторые сервисы для работы (на текущий момент только работа с JWT-токеном для авторизации и аутефикации пользователя).
4) файл Program.cs является точкой входа программы и создаёт весь фукционал, все http методы для работы API(жалко что нету контроллеров).
5) папка реалзирует DBContext для подлючения базы данных и работы с ней.