-- 1) Посмотреть последние 10 типов оценок
SELECT "Id", "Name", "Description"
FROM grade_types
ORDER BY "Id"

-- 2) Добавить новый тип оценки
INSERT INTO grade_types ("Name", "Description")
VALUES ('RAW_Тестовый_тип', 'Добавлено через SQL консоль');

-- 3) Обновить описание у добавленного типа
UPDATE grade_types
SET "Description" = 'Обновлено через SQL консоль'
WHERE "Name" = 'RAW_Тестовый_тип';

-- 4) Проверить, что изменения применились
SELECT "Id", "Name", "Description"
FROM grade_types
WHERE "Name" = 'RAW_Тестовый_тип'
ORDER BY "Id" DESC;

-- 5) Удалить тестовый тип оценки
DELETE FROM grade_types
WHERE "Name" = 'RAW_Тестовый_тип';
