-- =============================================
-- Actividad 3: Evidencia de pruebas
-- Procedimiento sp_CreateOrder
--
USE e_commerce
GO


-- ---------------------------------------------
-- Estado inicial de los productos que se usarán
-- ---------------------------------------------
SELECT ProductId, SKU, Name, Price, StockQuantity, IsActive
FROM Products
WHERE ProductId IN (1, 13, 49);


-- ---------------------------------------------
-- CASO 1 | Orden válida: 2 piezas de Laptop Pro 15 (1200.00 c/u)
-- Esperado: code = 1, TotalAmount = 2400.00, stock baja de 50 a 48
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 1,
    @Quantity  = 2;


-- ---------------------------------------------
-- CASO 2 | Stock insuficiente: Servidor Torre tiene 5, se piden 10
-- Esperado: code = 0, 'Stock insuficiente. Disponible: 5, solicitado: 10'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 13,
    @Quantity  = 10;


-- ---------------------------------------------
-- CASO 3 | Cantidad negativa
-- Esperado: code = 0, 'La cantidad debe ser mayor a cero'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 1,
    @Quantity  = -5;


-- ---------------------------------------------
-- CASO 4 | Cantidad en cero
-- Esperado: code = 0, 'La cantidad debe ser mayor a cero'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 1,
    @Quantity  = 0;


-- ---------------------------------------------
-- CASO 5 | Usuario que no existe
-- Esperado: code = 0, 'El usuario especificado no existe'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 999,
    @ProductId = 1,
    @Quantity  = 1;


-- ---------------------------------------------
-- CASO 6 | Producto que no existe
-- Esperado: code = 0, 'El producto especificado no existe'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 999,
    @Quantity  = 1;


-- ---------------------------------------------
-- CASO 7 | Falta un parámetro
-- Esperado: code = 0, 'No se ha especificado el parámetro Quantity'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 1;


-- ---------------------------------------------
-- CASO 8 | Producto dado de baja
-- Se desactiva CLE-001, se intenta comprar y se vuelve a activar
-- Esperado: code = 0, 'El producto ya no se encuentra activo'
-- ---------------------------------------------
UPDATE Products SET IsActive = 0 WHERE ProductId = 49;

EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 49,
    @Quantity  = 1;

UPDATE Products SET IsActive = 1 WHERE ProductId = 49;


-- ---------------------------------------------
-- CASO 9 | Compra exactamente todo el stock disponible
-- Servidor Torre tiene 5, se piden 5. Debe permitirlo y dejar el stock en 0
-- Esperado: code = 1, StockRestante = 0
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 2,
    @ProductId = 13,
    @Quantity  = 5;


-- ---------------------------------------------
-- CASO 10 | Ahora ya no queda stock del Servidor Torre
-- Esperado: code = 0, 'Stock insuficiente. Disponible: 0, solicitado: 1'
-- ---------------------------------------------
EXEC sp_CreateOrder
    @UserId    = 1,
    @ProductId = 13,
    @Quantity  = 1;


-- =============================================
-- VERIFICACIÓN: las tres tablas quedaron consistentes
-- =============================================

-- Órdenes creadas con su detalle
SELECT
    o.OrderId,
    o.UserId,
    u.Username,
    o.OrderDate,
    o.TotalAmount,
    o.Status,
    d.ProductId,
    p.Name        AS Producto,
    d.Quantity,
    d.UnitPrice,
    (d.Quantity * d.UnitPrice) AS SubtotalCalculado
FROM Orders o
    JOIN Users        u ON u.UserId    = o.UserId
    JOIN OrderDetails d ON d.OrderId   = o.OrderId
    JOIN Products     p ON p.ProductId = d.ProductId
ORDER BY o.OrderId;


-- Stock después de las compras
SELECT ProductId, SKU, Name, StockQuantity
FROM Products
WHERE ProductId IN (1, 13, 49);


-- =============================================
-- LIMPIEZA (opcional)
-- Devuelve el inventario consumido y borra las órdenes de prueba.
-- Seleccionar este bloque completo y ejecutarlo.
-- =============================================
/*
UPDATE p
SET p.StockQuantity = p.StockQuantity + x.Consumido
FROM Products p
    JOIN (
        SELECT ProductId, SUM(Quantity) AS Consumido
        FROM OrderDetails
        GROUP BY ProductId
    ) x ON x.ProductId = p.ProductId;

DELETE FROM OrderDetails;
DELETE FROM Orders;

DBCC CHECKIDENT ('OrderDetails', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);

SELECT ProductId, SKU, StockQuantity FROM Products WHERE ProductId IN (1, 13, 49);
*/
