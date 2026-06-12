using Microsoft.EntityFrameworkCore;
using Biblioteca.Dominio.Entidades;

namespace TiendaBatarazo.AccesoDatos.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Inventario> Inventario { get; set; }
    public virtual DbSet<Descarga> Descargas { get; set; }
    public virtual DbSet<Prestamo> Prestamos { get; set; }
    public virtual DbSet<Sancion> Sanciones { get; set; }
    public virtual DbSet<HorarioSeccion> HorariosSecciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Biblioteca_LDRF;Username=admin_biblioteca;Password=1235");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios", table =>
            {
                table.HasCheckConstraint("chk_usuarios_rol", "rol IN ('administracion', 'maestro', 'estudiante')");
                table.HasCheckConstraint("chk_usuarios_estado", "estado IN ('activo', 'inactivo')");
            });

            entity.HasKey(e => e.IdUsuario).HasName("usuarios_pkey");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario").ValueGeneratedOnAdd();
            entity.Property(e => e.Credencial).HasColumnName("credencial").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellidos).HasColumnName("apellidos").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Cedula).HasColumnName("cedula").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Seccion).HasColumnName("seccion").HasMaxLength(50);
            entity.Property(e => e.Clave).HasColumnName("clave").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Rol).HasColumnName("rol").HasMaxLength(20).HasDefaultValue("estudiante");
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("activo");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("NOW()");
            entity.Property(e => e.TokenActivo).HasColumnName("token_activo").HasMaxLength(255);

            entity.HasIndex(e => e.Credencial).IsUnique();
            entity.HasIndex(e => e.Cedula).IsUnique();
            entity.HasIndex(e => e.Correo).IsUnique();
        });

        modelBuilder.Entity<Inventario>(entity =>
        {
            entity.ToTable("inventario", table =>
            {
                table.HasCheckConstraint("chk_inventario_tipo_objeto", "tipo_objeto IN ('libro_fisico', 'libro_digital', 'equipo_institucion')");
                table.HasCheckConstraint("chk_inventario_tipo_archivo", "tipo_archivo IN ('pdf', 'epub') OR tipo_archivo IS NULL");
                table.HasCheckConstraint("chk_inventario_estado", "estado IN ('disponible', 'prestado', 'danado', 'baja')");
                table.HasCheckConstraint("chk_inventario_stock", "stock_fisico >= 0");
            });

            entity.HasKey(e => e.IdProducto).HasName("inventario_pkey");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto").ValueGeneratedOnAdd();
            entity.Property(e => e.CodigoInterno).HasColumnName("codigo_interno").HasMaxLength(100);
            entity.Property(e => e.TipoObjeto).HasColumnName("tipo_objeto").HasMaxLength(30).IsRequired();
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.Autor).HasColumnName("autor").HasMaxLength(150);
            entity.Property(e => e.Editorial).HasColumnName("editorial").HasMaxLength(150);
            entity.Property(e => e.NumeroSaga).HasColumnName("numero_saga").HasMaxLength(50);
            entity.Property(e => e.Edicion).HasColumnName("edicion").HasMaxLength(255);
            entity.Property(e => e.Categoria).HasColumnName("categoria").HasMaxLength(100);
            entity.Property(e => e.Ubicacion).HasColumnName("ubicacion").HasMaxLength(100);
            entity.Property(e => e.Portada).HasColumnName("portada").HasMaxLength(255);
            entity.Property(e => e.ArchivoUrl).HasColumnName("archivo_url").HasMaxLength(500);
            entity.Property(e => e.TipoArchivo).HasColumnName("tipo_archivo").HasMaxLength(10);
            entity.Property(e => e.TamanoMb).HasColumnName("tamano_mb").HasPrecision(5, 2);
            entity.Property(e => e.StockFisico).HasColumnName("stock_fisico").HasDefaultValue(0);
            entity.Property(e => e.PermitePrestamo).HasColumnName("permite_prestamo").HasDefaultValue(true);
            entity.Property(e => e.PermiteDescarga).HasColumnName("permite_descarga").HasDefaultValue(false);
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("disponible");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.CodigoInterno).IsUnique();
        });

        modelBuilder.Entity<Descarga>(entity =>
        {
            entity.ToTable("descargas");
            entity.HasKey(e => e.IdDescarga).HasName("descargas_pkey");
            entity.Property(e => e.IdDescarga).HasColumnName("id_descarga").ValueGeneratedOnAdd();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.FechaDescarga).HasColumnName("fecha_descarga").HasDefaultValueSql("NOW()");
            entity.Property(e => e.IpDescarga).HasColumnName("ip_descarga").HasMaxLength(45);
            entity.Property(e => e.Dispositivo).HasColumnName("dispositivo").HasMaxLength(150);
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");

            entity.HasOne(e => e.Usuario)
                .WithMany(e => e.Descargas)
                .HasForeignKey(e => e.IdUsuario)
                .HasConstraintName("fk_descargas_usuario")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Producto)
                .WithMany(e => e.Descargas)
                .HasForeignKey(e => e.IdProducto)
                .HasConstraintName("fk_descargas_producto")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.ToTable("prestamos", table =>
            {
                table.HasCheckConstraint("chk_prestamos_estado", "estado IN ('solicitado', 'activo', 'devuelto', 'atrasado', 'perdido')");
                table.HasCheckConstraint("chk_prestamos_renovaciones", "renovaciones >= 0");
            });

            entity.HasKey(e => e.IdPrestamo).HasName("prestamos_pkey");
            entity.Property(e => e.IdPrestamo).HasColumnName("id_prestamo").ValueGeneratedOnAdd();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdProducto).HasColumnName("id_producto");
            entity.Property(e => e.FechaPrestamo).HasColumnName("fecha_prestamo").HasDefaultValueSql("NOW()");
            entity.Property(e => e.FechaLimite).HasColumnName("fecha_limite");
            entity.Property(e => e.FechaDevolucion).HasColumnName("fecha_devolucion");
            entity.Property(e => e.Renovaciones).HasColumnName("renovaciones").HasDefaultValue(0);
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("activo");
            entity.Property(e => e.EntregadoPor).HasColumnName("entregado_por");
            entity.Property(e => e.RecibidoPor).HasColumnName("recibido_por");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Usuario)
                .WithMany(e => e.Prestamos)
                .HasForeignKey(e => e.IdUsuario)
                .HasConstraintName("fk_prestamos_usuario")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Producto)
                .WithMany(e => e.Prestamos)
                .HasForeignKey(e => e.IdProducto)
                .HasConstraintName("fk_prestamos_producto")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UsuarioEntrega)
                .WithMany(e => e.PrestamosEntregados)
                .HasForeignKey(e => e.EntregadoPor)
                .HasConstraintName("fk_prestamos_entregado_por")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UsuarioRecibe)
                .WithMany(e => e.PrestamosRecibidos)
                .HasForeignKey(e => e.RecibidoPor)
                .HasConstraintName("fk_prestamos_recibido_por")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sancion>(entity =>
        {
            entity.ToTable("sanciones", table =>
            {
                table.HasCheckConstraint("chk_sanciones_estado", "estado IN ('activa', 'cumplida', 'anulada')");
                table.HasCheckConstraint("chk_sanciones_dias", "dias_sancion > 0");
            });

            entity.HasKey(e => e.IdSancion).HasName("sanciones_pkey");
            entity.Property(e => e.IdSancion).HasColumnName("id_sancion").ValueGeneratedOnAdd();
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdPrestamo).HasColumnName("id_prestamo");
            entity.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(255).IsRequired();
            entity.Property(e => e.DiasSancion).HasColumnName("dias_sancion").HasDefaultValue(1);
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio").HasDefaultValueSql("CURRENT_DATE");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("activa");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Usuario)
                .WithMany(e => e.Sanciones)
                .HasForeignKey(e => e.IdUsuario)
                .HasConstraintName("fk_sanciones_usuario")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Prestamo)
                .WithMany(e => e.Sanciones)
                .HasForeignKey(e => e.IdPrestamo)
                .HasConstraintName("fk_sanciones_prestamo")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HorarioSeccion>(entity =>
        {
            entity.ToTable("horarios_secciones", table =>
            {
                table.HasCheckConstraint("chk_horarios_dia", "dia_semana IN ('lunes', 'martes', 'miercoles', 'jueves', 'viernes')");
                table.HasCheckConstraint("chk_horarios_estado", "estado IN ('clase', 'libre', 'receso')");
                table.HasCheckConstraint("chk_horarios_horas", "hora_inicio < hora_fin");
            });

            entity.HasKey(e => e.IdHorario).HasName("horarios_secciones_pkey");
            entity.Property(e => e.IdHorario).HasColumnName("id_horario").ValueGeneratedOnAdd();
            entity.Property(e => e.Seccion).HasColumnName("seccion").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DiaSemana).HasColumnName("dia_semana").HasMaxLength(20).IsRequired();
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.Materia).HasColumnName("materia").HasMaxLength(100);
            entity.Property(e => e.Docente).HasColumnName("docente").HasMaxLength(150);
            entity.Property(e => e.Aula).HasColumnName("aula").HasMaxLength(50);
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("clase");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("NOW()");
            entity.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("NOW()");
        });
    }
}
