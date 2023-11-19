using ProyectoTFG.Shared;
using System.Diagnostics.CodeAnalysis;

namespace ProyectoTFG.Client.Models
{
    public class ComparadorProductos : IEqualityComparer<ProductoAPI>
    {
        public bool Equals(ProductoAPI? x, ProductoAPI? y)
        {
            return x.id == y.id;
        }

        public int GetHashCode([DisallowNull] ProductoAPI obj)
        {
            return obj.id.GetHashCode();
        }
    }
}
