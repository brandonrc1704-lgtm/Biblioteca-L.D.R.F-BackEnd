using AutoMapper;
using Biblioteca.Dominio.Entidades;
using Biblioteca.Dominio.EntidadesTipadas;
namespace Biblioteca.Dominio.DTO
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Usuario, TUsuario>().ReverseMap();
            CreateMap<Inventario, TInventario>().ReverseMap();
            CreateMap<Descarga, TDescarga>().ReverseMap();
            CreateMap<Prestamo, TPrestamo>().ReverseMap();
            CreateMap<Sancion, TSancion>().ReverseMap();
            CreateMap<HorarioSeccion, THorarioSeccion>().ReverseMap();
        }
    }
}
