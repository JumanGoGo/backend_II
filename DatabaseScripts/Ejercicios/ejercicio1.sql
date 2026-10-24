-- =============================================
-- Actividad 1: Búsqueda de Productos por SKU
-- Se agrega el método 'GetProductBySKU' al procedimiento sp_GetAllActiveProducts
-- =============================================

USE e_commerce
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_GetAllActiveProducts]
    @method varchar(100),
    @isActive bit = null,
    @SKU varchar(50) = null
AS
BEGIN
    SET NOCOUNT ON;

    -- Variable propia para no reutilizar el parámetro @isActive
    DECLARE @ProductoActivo bit = NULL;


    -- MÉTODO: Products
    IF @method = 'Products'
    BEGIN
        IF @isActive IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'No se ha especificado el parámetro isActive' as 'message'
            RETURN
        END

        SELECT
            ProductId,
            SKU,
            Name,
            Description,
            Price,
            StockQuantity,
            '1' as 'code',
            'Success' as 'message'
        FROM
            Products
        WHERE
            IsActive = @isActive
        ORDER BY
            Name ASC;

        RETURN
    END


    -- MÉTODO: GetProductBySKU
    IF @method = 'GetProductBySKU'
    BEGIN
        -- Validación 1: el parámetro no fue enviado
        IF @SKU IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'No se ha especificado el parámetro SKU' as 'message'
            RETURN
        END

        -- Validación 2: el parámetro llegó vacío o con puros espacios
        IF LTRIM(RTRIM(@SKU)) = ''
        BEGIN
            SELECT
                '0' as 'code',
                'El parámetro SKU no puede estar vacío' as 'message'
            RETURN
        END

        -- Se consulta una sola vez el estado del producto
        SELECT
            @ProductoActivo = IsActive
        FROM Products
        WHERE SKU = @SKU;

        -- Validación 3: el SKU no existe en el catálogo
        IF @ProductoActivo IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'No existe un producto con el SKU proporcionado' as 'message'
            RETURN
        END

        -- Validación 4: el producto existe pero fue dado de baja
        IF @ProductoActivo = 0
        BEGIN
            SELECT
                '0' as 'code',
                'El producto ya no se encuentra activo' as 'message'
            RETURN
        END

        -- Caso exitoso
        SELECT
            ProductId,
            SKU,
            Name,
            Description,
            Price,
            StockQuantity,
            '1' as 'code',
            'Success' as 'message'
        FROM
            Products
        WHERE
            SKU = @SKU
            AND IsActive = 1;

        RETURN
    END

    -- Ningún método coincidió
    SELECT
        '0' as 'code',
        'El método especificado no es válido' as 'message'
END
GO


EXEC sp_GetAllActiveProducts
    @method = 'GetProductBySKU',
    @SKU    = 'LAP-001';