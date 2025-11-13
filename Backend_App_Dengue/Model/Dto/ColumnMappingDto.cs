namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para mapeo de columnas en importación de casos
    /// Permite mapear múltiples columnas del archivo a un solo campo del sistema
    /// </summary>
    public class ColumnMappingDto
    {
        /// <summary>
        /// Mapeo de campos del sistema a columnas del archivo
        /// Key: nombre del campo del sistema ("ciudad", "barrio", "direccion", "latitud", etc.)
        /// Value: array de nombres de columnas del archivo que se concatenarán
        ///
        /// Ejemplo:
        /// {
        ///   "ciudad": ["Columna_Ciudad", "Columna_Municipio"],
        ///   "barrio": ["nom_barrio"],
        ///   "edad": ["edad_"]
        /// }
        /// </summary>
        public Dictionary<string, List<string>> Mapping { get; set; } = new();

        /// <summary>
        /// Separador a usar cuando se concatenan múltiples columnas
        /// Por defecto: espacio " "
        /// </summary>
        public string Separator { get; set; } = " ";
    }
}
