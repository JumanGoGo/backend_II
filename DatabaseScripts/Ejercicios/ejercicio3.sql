-- =============================================
-- Actividad 3: Creación de Órdenes
-- Procedimiento sp_CreateOrder
-- =============================================

USE e_commerce
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_CreateOrder]
    @UserId    INT = NULL,
    @ProductId INT = NULL,
    @Quantity  INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Si ocurre un error de motor durante la transacción, SQL Server la
    -- deshace completa de forma automática en lugar de dejarla a medias.
    SET XACT_ABORT ON;

    -- Variables de trabajo. Inician en NULL para poder detectar
    -- si las consultas encontraron o no una fila.
    DECLARE @UsuarioEncontrado  INT           = NULL;
    DECLARE @ProductoEncontrado INT           = NULL;
    DECLARE @ProductoActivo     BIT           = NULL;
    DECLARE @Precio             DECIMAL(18,2) = NULL;
    DECLARE @Stock              INT           = NULL;
    DECLARE @Total              DECIMAL(18,2);
    DECLARE @OrderId            INT;

    -- =========================================
    -- VALIDACIONES DE PARÁMETROS
    -- =========================================

    IF @UserId IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'No se ha especificado el parámetro UserId' as 'message'
        RETURN
    END

    IF @ProductId IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'No se ha especificado el parámetro ProductId' as 'message'
        RETURN
    END

    IF @Quantity IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'No se ha especificado el parámetro Quantity' as 'message'
        RETURN
    END

    -- Una cantidad de cero o negativa generaría un total inválido
    -- y, peor aún, el UPDATE del stock terminaría sumando inventario.
    IF @Quantity <= 0
    BEGIN
        SELECT
            '0' as 'code',
            'La cantidad debe ser mayor a cero' as 'message'
        RETURN
    END

    -- =========================================
    -- VALIDACIONES DE NEGOCIO
    -- =========================================

    -- El usuario debe existir (Orders tiene llave foránea hacia Users)
    SELECT @UsuarioEncontrado = UserId
    FROM Users
    WHERE UserId = @UserId;

    IF @UsuarioEncontrado IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'El usuario especificado no existe' as 'message'
        RETURN
    END

    -- Se leen de una sola vez los datos del producto
    SELECT
        @ProductoEncontrado = ProductId,
        @ProductoActivo     = IsActive,
        @Precio             = Price,
        @Stock              = StockQuantity
    FROM Products
    WHERE ProductId = @ProductId;

    IF @ProductoEncontrado IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'El producto especificado no existe' as 'message'
        RETURN
    END

    IF @ProductoActivo = 0
    BEGIN
        SELECT
            '0' as 'code',
            'El producto ya no se encuentra activo' as 'message'
        RETURN
    END

    -- Requisito principal de la actividad: verificar el stock
    IF @Stock < @Quantity
    BEGIN
        SELECT
            '0' as 'code',
            'Stock insuficiente. Disponible: ' + CAST(@Stock AS VARCHAR(10))
                + ', solicitado: ' + CAST(@Quantity AS VARCHAR(10)) as 'message'
        RETURN
    END

    -- =========================================
    -- REGISTRO DE LA ORDEN
    -- Las tres operaciones van juntas: o se aplican todas, o ninguna.
    -- =========================================

    SET @Total = @Precio * @Quantity;

    BEGIN TRANSACTION;

        -- 1. Cabecera de la orden. Status toma su valor por defecto ('Pending')
        INSERT INTO Orders (UserId, TotalAmount)
        VALUES (@UserId, @Total);

        -- 2. Id que acaba de generar la columna IDENTITY
        SET @OrderId = SCOPE_IDENTITY();

        -- 3. Detalle de la orden. Se guarda el precio del momento,
        --    para que la orden no cambie si mañana sube el precio.
        INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
        VALUES (@OrderId, @ProductId, @Quantity, @Precio);

        -- 4. Descuento del inventario
        UPDATE Products
        SET StockQuantity = StockQuantity - @Quantity
        WHERE ProductId = @ProductId;

    COMMIT TRANSACTION;

    -- =========================================
    -- Respuesta exitosa
    -- =========================================
    SELECT
        @OrderId              as 'OrderId',
        @Total                as 'TotalAmount',
        @Quantity             as 'Quantity',
        (@Stock - @Quantity)  as 'StockRestante',
        '1'                   as 'code',
        'Success'             as 'message'
END
GO
