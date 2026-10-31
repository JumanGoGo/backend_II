-- =====================================================================
-- ACTIVIDAD: Búsqueda de productos por NOMBRE
-- Procedimiento: sp_GetAllActiveProducts  (se le agrega un método nuevo)
--
-- IMPORTANTE: ALTER PROCEDURE reemplaza la definición COMPLETA, no agrega.
-- Por eso este archivo contiene los tres métodos. Si después se ejecuta
-- 'ejercicio1.sql' (que solo tiene dos), el método nuevo se perdería.
-- Este archivo es ahora la versión vigente del procedimiento.
-- =====================================================================

USE e_commerce
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllActiveProducts]
    @method   varchar(100),
    @isActive bit          = null,
    @SKU      varchar(50)  = null,
    -- ===== INICIO ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
    -- Se declara del mismo tipo y tamaño que la columna Products.Name
    -- (NVARCHAR(100)) para que SQL Server no tenga que hacer una
    -- conversión implícita al comparar.
    @Name     nvarchar(100) = null,
    -- ===== FIN ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
    -- Parámetros agregados por el profesor (API keys, órdenes y login)
    @KeyValue  uniqueidentifier = null,
    @UserId    int              = null,
    @ProductId int              = null,
    @Quantity  int              = null,
    @Username  nvarchar(50)     = null
AS
BEGIN
    SET NOCOUNT ON;

    -- Variable propia para no reutilizar el parámetro @isActive
    DECLARE @ProductoActivo bit = NULL;

    -- ===== INICIO ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
    -- @Patron se dimensiona holgado: al neutralizar comodines un texto
    -- puede crecer hasta el triple ('%' se convierte en '[%]').
    DECLARE @Patron        nvarchar(400) = NULL;
    DECLARE @Coincidencias int           = 0;
    -- ===== FIN ACTIVIDAD: BÚSQUEDA POR NOMBRE =====


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


    -- ===== INICIO ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
    -- MÉTODO: GetProductsByName
    -- A diferencia de GetProductBySKU, el nombre NO es único: este método
    -- devuelve una LISTA de coincidencias parciales, no un solo registro.
    IF @method = 'GetProductsByName'
    BEGIN
        -- Validación 1: el parámetro no fue enviado
        IF @Name IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'No se ha especificado el parámetro Name' as 'message'
            RETURN
        END

        -- Se limpian los espacios de los extremos antes de validar y buscar
        SET @Name = LTRIM(RTRIM(@Name));

        -- Validación 2: el parámetro llegó vacío o con puros espacios
        IF @Name = ''
        BEGIN
            SELECT
                '0' as 'code',
                'El parámetro Name no puede estar vacío' as 'message'
            RETURN
        END

        -- Se neutralizan los comodines de LIKE encerrándolos en corchetes,
        -- para que se busquen como texto literal. Sin esto, un usuario que
        -- enviara '%' obtendría el catálogo completo.
        -- El corchete se reemplaza primero, si no se dañarían los siguientes.
        SET @Patron = '%' +
                      REPLACE(
                          REPLACE(
                              REPLACE(@Name, '[', '[[]'),
                          '%', '[%]'),
                      '_', '[_]') + '%';

        -- Se cuenta antes de consultar. COUNT(*) siempre devuelve una fila,
        -- así que @Coincidencias queda asignada con certeza.
        SELECT @Coincidencias = COUNT(*)
        FROM Products
        WHERE Name LIKE @Patron
          AND IsActive = 1;

        -- Validación 3: la búsqueda no arrojó resultados.
        -- Sin este bloque el procedimiento devolvería cero filas y el
        -- cliente no recibiría ningún 'code' ni 'message'.
        IF @Coincidencias = 0
        BEGIN
            SELECT
                '0' as 'code',
                'No se encontraron productos activos que coincidan con: ' + @Name as 'message'
            RETURN
        END

        -- Caso exitoso: se devuelven todas las coincidencias ordenadas.
        -- Se listan las columnas una por una, nunca SELECT *, para no
        -- exponer IsActive ni CreatedAt.
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
            Name LIKE @Patron
            AND IsActive = 1
        ORDER BY
            Name ASC;

        RETURN
    END
    -- ===== FIN ACTIVIDAD: BÚSQUEDA POR NOMBRE =====


    -- MÉTODO: ValidateApiKey (del profesor)
    IF @method = 'ValidateApiKey'
    BEGIN
        IF @KeyValue IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'No se ha especificado el parámetro KeyValue' as 'message'
            RETURN
        END

        IF EXISTS (SELECT 1 FROM ApiKeys WHERE KeyValue = @KeyValue AND IsActive = 1)
        BEGIN
            SELECT
                KeyName,
                '1' as 'code',
                'Success' as 'message'
            FROM ApiKeys
            WHERE KeyValue = @KeyValue AND IsActive = 1
        END
        ELSE
        BEGIN
            SELECT
                '0' as 'code',
                'La API Key no es válida o no está activa' as 'message'
        END

        RETURN
    END


    -- MÉTODO: CreateOrder (del profesor)
    IF @method = 'CreateOrder'
    BEGIN
        IF @UserId IS NULL OR @ProductId IS NULL OR @Quantity IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'Faltan parámetros para crear la orden' as 'message'
            RETURN
        END

        DECLARE @CurrentStock int;
        DECLARE @UnitPrice    decimal(18,2);
        DECLARE @TotalAmount  decimal(18,2);
        DECLARE @NewOrderId   int;

        SELECT
            @CurrentStock = StockQuantity,
            @UnitPrice    = Price
        FROM Products
        WHERE ProductId = @ProductId AND IsActive = 1;

        IF @CurrentStock IS NULL OR @CurrentStock < @Quantity
        BEGIN
            SELECT
                '0' as 'code',
                'Stock insuficiente o producto inactivo' as 'message'
            RETURN
        END

        SET @TotalAmount = @UnitPrice * @Quantity;

        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Orders (UserId, TotalAmount, Status, OrderDate)
            VALUES (@UserId, @TotalAmount, 'Completed', GETDATE());

            SET @NewOrderId = SCOPE_IDENTITY();

            INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
            VALUES (@NewOrderId, @ProductId, @Quantity, @UnitPrice);

            UPDATE Products
            SET StockQuantity = StockQuantity - @Quantity
            WHERE ProductId = @ProductId;

            COMMIT TRANSACTION;

            SELECT
                @NewOrderId as 'OrderId',
                '1' as 'code',
                'Orden creada con éxito' as 'message'
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            SELECT
                '0' as 'code',
                ERROR_MESSAGE() as 'message'
        END CATCH

        RETURN
    END


    -- MÉTODO: GetUserForLogin (del profesor)
    IF @method = 'GetUserForLogin'
    BEGIN
        IF @Username IS NULL
        BEGIN
            SELECT
                '0' as 'code',
                'No se ha especificado el parámetro Username' as 'message'
            RETURN
        END

        IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
        BEGIN
            SELECT
                UserId,
                PasswordHash,
                Role,
                '1' as 'code',
                'Success' as 'message'
            FROM Users
            WHERE Username = @Username
        END
        ELSE
        BEGIN
            SELECT
                '0' as 'code',
                'Usuario no encontrado' as 'message'
        END

        RETURN
    END


    -- Ningún método coincidió
    SELECT
        '0' as 'code',
        'El método especificado no es válido' as 'message'
END
GO
