-- ============================================================
-- PARTE A: DDL - Diseño de la Base de Datos
-- Sistema de Navegación de Dron Automatizado
-- ============================================================

-- Tabla principal: guarda la cabecera de cada ejecución exitosa
CREATE TABLE IF NOT EXISTS tb_master_control (
    id          SERIAL          PRIMARY KEY,          -- PK autonumérica
    fecha_hora  TIMESTAMP       NOT NULL DEFAULT NOW(),-- marca de tiempo
    dimension_n INTEGER         NOT NULL,             -- tamaño del terreno NxN
    coord_x     INTEGER         NOT NULL,             -- fila de despegue
    coord_y     INTEGER         NOT NULL              -- columna de despegue
);

-- Tabla detalle: guarda cada paso del recorrido del dron (relación 1 a N)
CREATE TABLE IF NOT EXISTS tb_det_log (
    id              SERIAL      PRIMARY KEY,           -- PK autonumérica propia
    id_master       INTEGER     NOT NULL,              -- FK hacia tb_master_control
    nro_paso        INTEGER     NOT NULL,              -- etiqueta del paso (ofuscada)
    coord_x         INTEGER     NOT NULL,              -- fila pisada en este paso
    coord_y         INTEGER     NOT NULL,              -- columna pisada en este paso

    -- Restricción de Clave Foránea: vinculación 1 a Muchos
    CONSTRAINT fk_det_master FOREIGN KEY (id_master)
        REFERENCES tb_master_control(id)
        ON DELETE CASCADE                              -- si se borra el master, se borran los detalles
);
