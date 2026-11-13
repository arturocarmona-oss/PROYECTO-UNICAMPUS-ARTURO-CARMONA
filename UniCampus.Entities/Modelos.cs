using System;
using System.Collections.Generic;

namespace UniCampus.Entities
{
    public class Actividad
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public string Modalidad { get; set; }
        public string Lugar { get; set; }
        public int Cupos { get; set; }
        public string Estado { get; set; }
        public int Inscritos { get; set; }

        public int Disponibles
        {
            get
            {
                return Cupos - Inscritos;
            }
        }
    }

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Programa { get; set; }
        public string Rol { get; set; }
    }

    public class Inscripcion
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Programa { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class Resena
    {
        public string Nombre { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class Filtro
    {
        public string Texto { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public DateTime? Fecha { get; set; }
    }

    public class ReglaException : Exception
    {
        public ReglaException(string mensaje) : base(mensaje)
        {
        }
    }

    public interface ICampusRepository
    {
        IList<Actividad> Listar(Filtro filtro);
        Actividad Obtener(int id);
        int Guardar(Actividad actividad);
        void Desactivar(int id);
        IList<Usuario> Usuarios();
        Usuario Usuario(int id);
        IList<Inscripcion> Inscritos(int id);
        void Inscribir(int actividad, int usuario);
        void Cancelar(int actividad, int usuario);
        IList<Resena> Resenas(int actividad);
        void Valorar(int actividad, int usuario, int nota, string comentario);
    }
}
