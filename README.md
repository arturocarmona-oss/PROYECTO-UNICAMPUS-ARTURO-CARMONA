# UniCampus - actividades académicas

Autor: Arturo Carmona.

Repositorio: https://github.com/arturocarmona-oss/PROYECTO-UNICAMPUS-ARTURO-CARMONA

Aplicación ASP.NET Web Forms con arquitectura de tres capas y MVP, adaptada del proyecto `Formularioweb` y de la tabla `EventosDB.dbo.Registros`. Se mantiene .NET Framework 4.7.2, la versión del proyecto original. La guía usa 4.8.1 como referencia; el documento permite respetar la versión de la solución existente.

## Abrir y ejecutar

1. Instalar Visual Studio con la carga **Desarrollo de ASP.NET y web** y el paquete de destino .NET Framework 4.7.2. Usar SQL Server Express y SQL Server Management Studio.
2. En SSMS conectarse a `.\SQLEXPRESS` con autenticación de Windows. Abrir y ejecutar `03_BaseDatos.sql`. El script amplía EventosDB; conserva Registros y sus filas. En una instalación limpia crea la base y la tabla original. No ejecutar el script original que crea la misma base nuevamente.
3. Abrir `UniCampus.sln`. Establecer **UniCampus.Web** como proyecto de inicio si fuera necesario.
4. Revisar `UniCampus.Web/Web.config`: la conexión `UniCampusDb` utiliza `.\SQLEXPRESS`, `EventosDB` y autenticación integrada. Cambiar solo servidor y base si el entorno lo requiere. No se necesitan paquetes NuGet.
5. Compilar la solución y ejecutar con IIS Express, Ctrl+F5. El puerto lo asigna Visual Studio. En este equipo la prueba se ejecutó en `http://localhost:62574/`.
6. Entrar con **Administrador Demo** para gestionar actividades y participantes. Usar **Ana Demo** o **Carlos Demo** para probar inscripciones y valoraciones. Los datos de demostración son ficticios y usan correos example.com.

El selector de usuarios es exclusivamente una demostración local de roles. Solo funciona en solicitudes locales con `ModoDemostracion=true`. Antes de una publicación real debe sustituirse por autenticación de usuarios e identidad del servidor. El modo no es un sistema de contraseñas y no se presenta como tal. No se publicó el sitio en Internet.

## Qué contiene

- Formulario de actividades: título, descripción, tipo, fechas, horas, modalidad, lugar, cupos y estado.
- Crear, consultar, editar y desactivar conservando historial. La reactivación se realiza editando el estado.
- Búsqueda por título, tipo, estado y fecha; la fecha coincide con cualquier día comprendido por la actividad. Paginación de cinco filas.
- Detalle de actividad, vista del estudiante y resumen de la consulta actual. El resumen de inscripciones cuenta reservas, no estudiantes únicos.
- Inscripciones: participantes existentes de Registros, control de cupos y duplicados. El administrador inscribe a estudiantes; el estudiante solo se inscribe a sí mismo.
- Cancelación antes del inicio; libera el cupo. No se permite después del inicio.
- Una valoración por participante inscrito, solo después de finalizar la actividad, con nota de 1 a 5 y comentario obligatorio.
- El formulario original se conserva en **Registro.aspx**. Se separó su SQL del code-behind, se validan datos en negocio y se reemplazó la eliminación física por desactivación. Crear y listar participantes requiere administrador.

## Proyectos y dependencias

```text
UniCampus.Web      -> UniCampus.BLL + UniCampus.Entities
UniCampus.BLL      -> UniCampus.DAL + UniCampus.Entities
UniCampus.DAL      -> UniCampus.Entities
UniCampus.Entities -> sin dependencias de interfaz o base de datos
UniCampus.Tests    -> pruebas aisladas de negocio
```

`Default.aspx` implementa `ICampusView`. Los eventos delegan en `CampusPresenter`, que coordina `CampusService`. `CampusRepository` ejecuta ADO.NET con parámetros tipados. Para participantes se aplica la misma secuencia con `RegistroPresenter`, `RegistroService` y `RegistroRepository`. Ninguna página contiene consultas SQL.

Ejemplo de Guardar: controles -> DTO Actividad -> Presenter -> CampusService.Guardar -> validación de rol/campos/fechas/cupos -> CampusRepository.Guardar -> INSERT/UPDATE parametrizado -> SQL Server -> identificador -> actualización del listado y mensaje.

## Modelo reutilizado

No se creó una segunda tabla de estudiantes. `Registros` conserva sus columnas originales y recibe `Rol` y `Activo`. El DTO `Usuario` es una proyección de esos registros, no una tabla adicional. El rol de las filas anteriores queda como Estudiante.

```text
Registros 1 ----- N Inscripcion N ----- 1 Actividad
                        |
                        | 1
                        | 0..1
                      Resena
```

`Inscripcion(ActividadId,UsuarioId)` tiene clave única y claves foráneas hacia Actividad y Registros. `Resena(ActividadId,UsuarioId)` referencia esa pareja y también es única. La base comprueba cupos positivos, fechas ordenadas, valores de tipo/modalidad/estado y calificación entre 1 y 5.

Las inscripciones, las modificaciones de cupos y las desactivaciones bloquean primero la misma fila de Actividad dentro de una transacción serializable. La inscripción verifica estado, fecha, duplicados y ocupación dentro de esa transacción. Esto evita que dos solicitudes de la aplicación ocupen simultáneamente el último cupo. No se afirma haber ejecutado una prueba de carga concurrente; está pendiente antes de un uso real. El control presupone que las escrituras pasan por estos repositorios.

La fecha de inicio y fin almacenan también la hora. Se usa hora local del servidor; mantener el equipo/servidor en la zona horaria de Colombia para las demostraciones. Las fechas de prueba SQL se calculan al ejecutar el script para que haya actividades futuras y una finalizada.

## Validación realizada

Se compiló la solución completa y se precompilaron las páginas ASPX. Se ejecutaron 32 pruebas automatizadas de negocio con repositorio simulado. Para repetirlas, compilar y ejecutar `UniCampus.Tests/bin/UniCampus.Tests.exe`. No requieren SQL Server. El resultado se guarda en `Pruebas/Resultado_negocio.txt`.

También se verificaron en navegador con SQL Server real:

| Caso | Resultado observado |
|---|---|
| Guardar sin título | Rechaza con mensaje de validación |
| Registrar actividad completa | INSERT correcto y actividad visible en listado/detalle |
| Editar cupos | Actualización persistida |
| Búsqueda por Arquitectura | Recupera la actividad creada |
| Inscribir Ana y Carlos Demo | Inscripciones guardadas y cupos descontados |
| Reducir capacidad a 1 con 2 inscritos | Rechaza sin reducir capacidad |
| Capacidad 2 con 2 inscritos | Rechaza una nueva inscripción por falta de cupos |
| Inscribir Ana Demo nuevamente en Taller de Programación Web | Rechaza duplicado desde SQL Server |
| Valorar Encuentro de Investigación como Ana Demo | Reseña guardada con nota 5 |
| Repetir la misma valoración | Rechaza duplicado |
| Consultar participantes como estudiante | No muestra datos; requiere administrador |
| Registrar Laura Demo desde el formulario original | INSERT correcto en Registros con código EVT único generado |
| Pantallas de escritorio y móvil | Revisión visual; sin desbordamiento horizontal de la página |

La cancelación, la desactivación y los permisos se cubren además en las pruebas de negocio según los casos del código. La cancelación real y una carrera concurrente de cupos deben comprobarse en una base de pruebas antes de presentarlas como evidencia SQL. La prueba automatizada de valoración verifica el paso al repositorio; la pertenencia de la inscripción y duplicados se comprueban adicionalmente en SQL y se probó el caso real indicado arriba.

`Pruebas/Verificacion.sql` permite revisar el resultado en SSMS sin exportar datos personales. El filtro por Id 3 a 11 corresponde exclusivamente a los nueve registros que existían en este equipo al comenzar; no es una regla del sistema.

## Respaldo y conservación

Se respaldó el código original antes de migrarlo. La copia está fuera del paquete de código final, en la carpeta de trabajo de esta tarea. También se creó una copia SQL `COPY_ONLY` con checksum en la carpeta Backup de la instancia, con nombre `EventosDB_antes_UniCampus_20261112.bak`. No subir ese respaldo a GitHub, pues contiene los registros existentes.

Los nueve registros iniciales no se exportan al ZIP ni al script. La solución final está en la carpeta outputs/UniCampus de esta tarea y está abierta en Visual Studio. El proyecto fuente original permanece como referencia; el trabajo continúa en la copia organizada.

## Pendientes para entregar en la plataforma

- Completar integrantes y responsabilidades en el informe. No se inventaron autores.
- Agregar el enlace del repositorio al informe y a la plataforma, y verificar el acceso de la docente.
- Confirmar con la docente el alcance y la rúbrica definitiva; presentar la demostración y explicar el código.
- Completar la evidencia de cancelación real, filtros combinados, paginación con más de cinco actividades y concurrencia antes de afirmar cobertura total.
- Para producción: autenticación real, HTTPS, configuración de secretos fuera del repositorio y registro persistente de errores. El alcance entregado es una aplicación académica local.

Los tres archivos principales están preparados como `01_Informe_Tecnico.pdf`, `02_Solucion_VisualStudio.zip` y `03_BaseDatos.sql`. El informe distingue comprobaciones realizadas de pendientes.
