-- =============================================
-- Actividad 2: Evidencia de pruebas
-- Procedimiento sp_ValidateApiKey
--
-- Ejecutar un solo caso: seleccionar el bloque y Ctrl + Shift + E
-- =============================================

USE e_commerce
GO


-- ---------------------------------------------
-- Referencia: API Keys existentes en la base
-- ---------------------------------------------
SELECT ApiKeyId, KeyName, KeyValue, IsActive
FROM ApiKeys;


-- ---------------------------------------------
-- CASO 1 | API Key válida y activa
-- Esperado: code = 1, KeyName = ERP_Logistica
-- ---------------------------------------------
EXEC sp_ValidateApiKey
    @KeyValue = 'F5D9BAD4-36DC-4953-9ADC-9342DC70EA5E';


-- ---------------------------------------------
-- CASO 2 | Segunda API Key válida y activa
-- Esperado: code = 1, KeyName = Proveedor_Webhooks
-- ---------------------------------------------
EXEC sp_ValidateApiKey
    @KeyValue = '7359AEE5-0C20-40B0-BC75-27EED529E646';


-- ---------------------------------------------
-- CASO 3 | No se envía el parámetro
-- Esperado: code = 0, 'No se ha especificado el parámetro KeyValue'
-- ---------------------------------------------
EXEC sp_ValidateApiKey;


-- ---------------------------------------------
-- CASO 4 | Se envía NULL de forma explícita
-- Esperado: code = 0, 'No se ha especificado el parámetro KeyValue'
-- ---------------------------------------------
EXEC sp_ValidateApiKey
    @KeyValue = NULL;


-- ---------------------------------------------
-- CASO 5 | GUID con formato correcto pero que no existe
-- Esperado: code = 0, 'La API Key proporcionada no es válida'
-- ---------------------------------------------
EXEC sp_ValidateApiKey
    @KeyValue = '00000000-0000-0000-0000-000000000000';


-- ---------------------------------------------
-- CASO 6 | API Key que existe pero fue desactivada
-- Se desactiva ERP_Logistica, se valida y se vuelve a activar
-- Esperado: code = 0, 'La API Key se encuentra desactivada'
-- ---------------------------------------------
UPDATE ApiKeys SET IsActive = 0 WHERE KeyName = 'ERP_Logistica';

EXEC sp_ValidateApiKey
    @KeyValue = 'F5D9BAD4-36DC-4953-9ADC-9342DC70EA5E';

UPDATE ApiKeys SET IsActive = 1 WHERE KeyName = 'ERP_Logistica';


-- ---------------------------------------------
-- Verificación final: ambas Keys deben quedar activas
-- ---------------------------------------------
SELECT KeyName, IsActive
FROM ApiKeys;
