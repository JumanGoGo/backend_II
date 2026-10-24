namespace backend_II.Models
{
    public class ProductSearchRequest
    {
        public string Sku { get; set; } = string.Empty;

        // ===== INICIO ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
        // Permite que un mismo modelo sirva para buscar por SKU o por nombre,
        // según el endpoint que reciba la petición.
        public string Name { get; set; } = string.Empty;
        // ===== FIN ACTIVIDAD: BÚSQUEDA POR NOMBRE =====
    }
}
