-- =============================================
-- Actividad 4: Obtener usuario para login
-- Procedimiento sp_GetUserForLogin
-- =============================================

USE e_commerce
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserForLogin]
    @Username NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Centinela: UserId es la llave primaria, nunca es NULL en la tabla.
    -- Si queda en NULL es porque no se encontró ninguna fila.
    DECLARE @UserIdEncontrado INT = NULL;

    -- Validación 1: el parámetro no fue enviado
    IF @Username IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'No se ha especificado el parámetro Username' as 'message'
        RETURN
    END

    -- Validación 2: el parámetro llegó vacío o con puros espacios
    IF LTRIM(RTRIM(@Username)) = ''
    BEGIN
        SELECT
            '0' as 'code',
            'El parámetro Username no puede estar vacío' as 'message'
        RETURN
    END

    -- Se busca al usuario
    SELECT @UserIdEncontrado = UserId
    FROM Users
    WHERE Username = @Username;

    -- Validación 3: el usuario no existe.
    -- El mensaje es genérico a propósito: si dijera 'el usuario no existe',
    -- un atacante podría probar nombres y descubrir cuáles son válidos
    -- antes de intentar adivinar contraseñas.
    IF @UserIdEncontrado IS NULL
    BEGIN
        SELECT
            '0' as 'code',
            'Usuario o contraseña incorrectos' as 'message'
        RETURN
    END

    -- Caso exitoso: se entregan los datos que la aplicación necesita
    -- para comparar la contraseña y decidir los permisos del usuario.
    SELECT
        UserId,
        Username,
        PasswordHash,
        Role,
        '1'       as 'code',
        'Success' as 'message'
    FROM Users
    WHERE Username = @Username
END
GO
