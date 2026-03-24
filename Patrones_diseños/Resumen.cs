/*
 ══════════════════════════════════════════════════════════════════
  LOS 23 PATRONES GOF — AGUA MINAMI — IMPLEMENTACIÓN COMPLETA
 ══════════════════════════════════════════════════════════════════

 CREACIONALES (5)
 ─────────────────────────────────────────────────────────────────
  1. Singleton         Config global: BD, red LAN, impresoras
  2. Factory Method    Documentos: facturas, volantes de nómina
  3. Builder           Construcción de pedidos (local y ruta)
 18. Abstract Factory  Familias de reportes (JSON, CSV, impresora)
 22. Prototype         Plantillas de pedidos recurrentes

 ESTRUCTURALES (7)
 ─────────────────────────────────────────────────────────────────
  9. Decorator         Logging y caché sobre repositorios
 10. Facade            Interfaz unificada del proceso de venta
 11. Proxy             Control de acceso a reportes por rol
 13. Composite         Árbol de categorías de productos
 19. Bridge            Tipos de reporte × formatos de salida
 20. Flyweight         Instancia única de cada producto en memoria
 21. Adapter           Integración con SDK e-CF de la DGII

 COMPORTAMIENTO (11)
 ─────────────────────────────────────────────────────────────────
  4. Observer          Alertas automáticas de stock bajo mínimo
  5. State             Ciclo de vida de la OrdenCompra
  6. Strategy          Nómina: sueldo, vacaciones, regalía pascual
  7. Chain of Resp.    Validación de ventas en cadena
  8. Command           Acciones de inventario con deshacer
 12. Template Method   Proceso de despacho en ruta
 14. Iterator          Historial de movimientos con filtros
 15. Mediator          Coordinación entre módulos sin acoplamiento
 16. Memento           Snapshots de OrdenCompra (deshacer estados)
 17. Visitor           Estadísticas sobre el árbol de productos
 23. Interpreter       Reglas de ofertas escritas como texto en BD

 ══════════════════════════════════════════════════════════════════
  FLUJO DE UNA VENTA — TODOS LOS PATRONES EN ACCIÓN
 ══════════════════════════════════════════════════════════════════

  1. Vendedor elige plantilla semanal del cliente
     → Prototype clona la plantilla

  2. Builder construye el Pedido línea a línea
     → Flyweight garantiza una sola instancia de cada producto

  3. Chain of Responsibility valida:
     → Auth (Proxy verifica rol)
     → Stock (Observer preparado para alertar)
     → Precios (Singleton provee parámetros)
     → Ofertas (Interpreter evalúa reglas de BD)

  4. Command registra los movimientos de inventario (con deshacer)
     → Observer dispara alertas si stock < mínimo
     → Mediator notifica a módulos suscritos en paralelo

  5. Factory Method elige el tipo de documento
     → Builder del documento arma el contenido
     → Bridge elige el formato de salida (JSON/CSV/impresora)

  6. Adapter envía el e-CF a la DGII
     → Memento guarda snapshot del estado final

  7. Strategy calcula la nómina (si es fin de quincena)
     → Template Method orquesta el despacho en ruta
     → Iterator recorre el historial para el reporte
     → Visitor calcula estadísticas del catálogo (Composite)
     → Abstract Factory produce todos los reportes en el formato elegido
*/