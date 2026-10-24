-- =============================================
-- Actividad 2: Validación de Integraciones
-- Procedimiento sp_ValidateApiKey
-- =============================================

USE e_commerce
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_ValidateApiKey]
    @KeyValue UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Variables de trabajo. Se inicializan en NULL para poder detectar
    -- si la consulta encontró o no una fila.
    DECLARE @ApiKeyId  INT          = NULL;
    DECLARE @KeyName   NVARCHAR(50) = NULL;
    DECLARE @KeyActiva BIT          = NULL;

    -- Validación 1: el parámetro no fue enviado
    IF @KeyValue IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'No se ha especificado el parámetro KeyValue' as 'message'
        RETURN
    END

    -- Se consulta la tabla una sola vez y se guardan los tres datos
    SELECT
        @ApiKeyId  = ApiKeyId,
        @KeyName   = KeyName,
        @KeyActiva = IsActive
    FROM ApiKeys
    WHERE KeyValue = @KeyValue;

    -- Validación 2: la API Key no existe
    -- ApiKeyId es la llave primaria, nunca es NULL en la tabla.
    -- Si sigue en NULL es porque la consulta no devolvió ninguna fila.
    IF @ApiKeyId IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'La API Key proporcionada no es válida' as 'message'
        RETURN
    END

    -- Validación 3: la API Key existe pero fue desactivada
    IF @KeyActiva = 0
    BEGIN
        SELECT
            '0' as 'code',
            'La API Key se encuentra desactivada' as 'message'
        RETURN
    END

    -- Caso exitoso: se devuelve el nombre de la Key y el flag de éxito.
    -- Los valores ya están en variables, no hace falta volver a leer la tabla.
    SELECT
        @ApiKeyId as 'ApiKeyId',
        @KeyName  as 'KeyName',
        '1'       as 'code',
        'Success' as 'message'
END
GO
