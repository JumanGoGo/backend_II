-- =============================================
-- Actividad 1: Evidencia de pruebas
-- Método 'GetProductBySKU' de sp_GetAllActiveProducts

USE e_commerce
GO


-- ---------------------------------------------
-- CASO 1 | SKU válido
-- Esperado: code = 1, datos del producto
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = 'LAP-001';


-- ---------------------------------------------
-- CASO 2 | No se envía el parámetro SKU
-- Esperado: code = 0, 'No se ha especificado el parámetro SKU'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU';


-- ---------------------------------------------
-- CASO 3 | SKU vacío
-- Esperado: code = 0, 'El parámetro SKU no puede estar vacío'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = '';


-- ---------------------------------------------
-- CASO 4 | SKU con puros espacios
-- Esperado: code = 0, 'El parámetro SKU no puede estar vacío'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = '   ';


-- ---------------------------------------------
-- CASO 5 | SKU que no existe en el catálogo
-- Esperado: code = 0, 'No existe un producto con el SKU proporcionado'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = 'NO-EXISTE';


-- ---------------------------------------------
-- CASO 6 | SKU inexistente enviando además @isActive = 0
-- Comprueba que el parámetro @isActive ya no contamina el resultado
-- Esperado: code = 0, 'No existe un producto con el SKU proporcionado'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method  = 'GetProductBySKU',
    @SKU     = 'NO-EXISTE',
    @isActive = 0;


-- ---------------------------------------------
-- CASO 7 | Nombre de método inválido
-- Esperado: code = 0, 'El método especificado no es válido'
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method = 'MetodoInventado';


-- ---------------------------------------------
-- CASO 8 | El método Products sigue funcionando
-- Esperado: code = 1, listado de productos activos
-- ---------------------------------------------
EXEC sp_GetAllActiveProducts
    @method   = 'Products',
    @isActive = 1;


-- ---------------------------------------------
-- CASO 9 | Producto existente pero dado de baja
-- Se desactiva CLE-001, se consulta y se vuelve a activar
-- Esperado: code = 0, 'El producto ya no se encuentra activo'
-- ---------------------------------------------
UPDATE Products SET IsActive = 0 WHERE SKU = 'CLE-001';

EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = 'CLE-001';

UPDATE Products SET IsActive = 1 WHERE SKU = 'CLE-001';


-- ---------------------------------------------
-- Verificación final: no deben quedar productos inactivos
-- ---------------------------------------------
SELECT COUNT(*) AS ProductosInactivos
FROM Products
WHERE IsActive = 0;
