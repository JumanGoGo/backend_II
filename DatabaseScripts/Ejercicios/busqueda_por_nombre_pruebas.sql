-- =====================================================================
-- ACTIVIDAD: Búsqueda de productos por NOMBRE
-- Evidencia de pruebas del método GetProductsByName
-- =====================================================================

USE e_commerce
GO


-- ---------------------------------------------
-- Referencia: productos cuyo nombre contiene 'Laptop'
-- ---------------------------------------------
SELECT ProductId, SKU, Name, IsActive
FROM Products
WHERE Name LIKE '%Laptop%';


-- ---------------------------------------------
-- CASO 1 | Coincidencia parcial
-- Esperado: code = 1, varias filas. Incluye 'Cargador Universal Laptop',
--           lo que demuestra que busca en cualquier parte del nombre.
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = 'Laptop';


-- ---------------------------------------------
-- CASO 2 | Una sola coincidencia
-- Esperado: code = 1, una fila (Laptop Pro 15)
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = 'Pro 15';


-- ---------------------------------------------
-- CASO 3 | Sin coincidencias
-- Esperado: code = 0, 'No se encontraron productos activos...'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = 'Bicicleta';


-- ---------------------------------------------
-- CASO 4 | No se envía el parámetro
-- Esperado: code = 0, 'No se ha especificado el parámetro Name'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName';


-- ---------------------------------------------
-- CASO 5 | Cadena vacía
-- Esperado: code = 0, 'El parámetro Name no puede estar vacío'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = '';


-- ---------------------------------------------
-- CASO 6 | Solo espacios
-- Esperado: code = 0, 'El parámetro Name no puede estar vacío'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = '      ';


-- ---------------------------------------------
-- CASO 7 | Espacios alrededor del texto: se recortan antes de buscar
-- Esperado: code = 1, mismas filas que el CASO 1
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = '   Laptop   ';


-- ---------------------------------------------
-- CASO 8 | Comodín '%': debe tratarse como texto literal.
-- Si NO estuviera neutralizado, devolvería el catálogo completo.
-- Esperado: code = 0, no hay ningún producto cuyo nombre contenga '%'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = '%';


-- ---------------------------------------------
-- CASO 9 | Comodín '_': igual que el anterior.
-- '_' significa 'un carácter cualquiera' en LIKE.
-- Esperado: code = 0, ningún nombre contiene un guion bajo literal
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = '_';


-- ---------------------------------------------
-- CASO 10 | Mayúsculas: la collation Modern_Spanish_CI_AS
--           no distingue mayúsculas de minúsculas
-- Esperado: code = 1, mismas filas que el CASO 1
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = 'LAPTOP';


-- ---------------------------------------------
-- CASO 11 | Producto inactivo: no debe aparecer en los resultados.
-- Se desactiva CLE-001, se busca y se vuelve a activar.
-- Esperado: code = 0, 'No se encontraron productos activos...'
-- ---------------------------------------------
SELECT ProductId, SKU, Name FROM Products WHERE ProductId = 49;

UPDATE Products SET IsActive = 0 WHERE ProductId = 49;

EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = 'Limpiador';

UPDATE Products SET IsActive = 1 WHERE ProductId = 49;

-- Ahora que está activo de nuevo, sí debe aparecer
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = 'Limpiador';


-- ---------------------------------------------
-- CASO 12 | Intento de inyección SQL.
-- Los parámetros de un procedimiento almacenado se tratan como
-- valores, nunca como código: el texto se busca literalmente.
-- Esperado: code = 0, sin coincidencias, tabla intacta
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductsByName',
    @Name   = ''' OR 1=1 --';


-- ---------------------------------------------
-- CASO 13 | Los métodos anteriores siguen funcionando.
-- ALTER PROCEDURE reemplaza toda la definición, así que hay que
-- comprobar que no se perdió nada.
-- Esperado: code = 1 en los dos
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method   = 'Products',
    @isActive = 1;

EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = 'LAP-001';


-- ---------------------------------------------
-- Verificación final: la tabla no sufrió cambios
-- ---------------------------------------------
SELECT COUNT(*) AS TotalProductos,
       SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS Activos
FROM Products;
