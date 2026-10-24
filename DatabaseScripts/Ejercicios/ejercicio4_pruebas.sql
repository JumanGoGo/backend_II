-- =============================================
-- Actividad 4: Evidencia de pruebas
-- Procedimiento sp_GetUserForLogin
-- =============================================

USE e_commerce
GO


-- ---------------------------------------------
-- Referencia: usuarios existentes
-- ---------------------------------------------
SELECT UserId, Username, PasswordHash, Role
FROM Users;


-- ---------------------------------------------
-- CASO 1 | Usuario administrador
-- Esperado: code = 1, UserId = 1, Role = Admin
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = 'admin_user';


-- ---------------------------------------------
-- CASO 2 | Usuario estándar
-- Esperado: code = 1, UserId = 2, Role = User
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = 'employee_01';


-- ---------------------------------------------
-- CASO 3 | No se envía el parámetro
-- Esperado: code = 0, 'No se ha especificado el parámetro Username'
-- ---------------------------------------------
EXEC sp_GetUserForLogin;


-- ---------------------------------------------
-- CASO 4 | Cadena vacía
-- Esperado: code = 0, 'El parámetro Username no puede estar vacío'
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = '';


-- ---------------------------------------------
-- CASO 5 | Solo espacios
-- Esperado: code = 0, 'El parámetro Username no puede estar vacío'
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = '    ';


-- ---------------------------------------------
-- CASO 6 | Usuario que no existe
-- Esperado: code = 0, 'Usuario o contraseña incorrectos'
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = 'usuario_inventado';


-- ---------------------------------------------
-- CASO 7 | Mayúsculas: la collation Modern_Spanish_CI_AS
--          no distingue mayúsculas de minúsculas
-- Esperado: code = 1, encuentra a admin_user
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = 'ADMIN_USER';


-- ---------------------------------------------
-- CASO 8 | Acentos: la collation SÍ distingue acentos
-- Esperado: code = 0, 'Usuario o contraseña incorrectos'
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = 'admín_user';


-- ---------------------------------------------
-- CASO 9 | Espacios al final: SQL Server los ignora al comparar
-- Esperado: code = 1, encuentra a admin_user
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = 'admin_user     ';


-- ---------------------------------------------
-- CASO 10 | Intento de inyección SQL
-- Los parámetros de un procedimiento almacenado se tratan como
-- valores, nunca como código. El texto se busca literalmente.
-- Esperado: code = 0, 'Usuario o contraseña incorrectos'
-- ---------------------------------------------
EXEC sp_GetUserForLogin
    @Username = ''' OR 1=1 --';


-- ---------------------------------------------
-- Verificación: la tabla Users sigue intacta
-- ---------------------------------------------
SELECT COUNT(*) AS TotalUsuarios FROM Users;
