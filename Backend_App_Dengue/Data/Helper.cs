using System.Data;
using System.Reflection;

namespace Backend_App_Dengue.Data
{
    public static class Helper
    {
        public static List<T>? DataTableToList<T>(this DataTable table) where T : class, new()
        {
            if (table == null || table.Rows.Count == 0)
            {
                return new List<T>();
            }

            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            if (table.Columns.Contains(prop.Name))
                            {
                                var value = row[prop.Name];

                                if (value != null && value != DBNull.Value)
                                {
                                    // Manejo especial para tipos nullable
                                    Type propertyType = prop.PropertyType;
                                    Type underlyingType = Nullable.GetUnderlyingType(propertyType);

                                    if (underlyingType != null)
                                    {
                                        prop.SetValue(obj, Convert.ChangeType(value, underlyingType), null);
                                    }
                                    else
                                    {
                                        prop.SetValue(obj, Convert.ChangeType(value, propertyType), null);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log: Error al asignar propiedad {prop.Name}: {ex.Message}
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch (Exception ex)
            {
                // Log: Error crítico en DataTableToList: {ex.Message}
                throw new Exception("Error al convertir DataTable a List", ex);
            }
        }
    }
}
