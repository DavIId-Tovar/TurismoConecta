// Mapa de viaje del itinerario (componente MapaViaje.razor):
//  - dibujar      → pines numerados por parada + línea de la ruta (estimada, en línea recta)
//  - negocios     → capa "Dónde dormir / comer" (negocios aprobados con ubicación)
//  - miUbicacion  → "Estoy aquí": pide la ubicación al navegador y la marca en el mapa
//  - destruir     → al salir de la página
// Cada mapa se guarda por el id de su contenedor: puede haber más de uno a la vez.
window.mapaViaje = (function () {
    const CENTRO_BOYACA = [5.45, -73.36];
    const TESELAS = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
    const ATRIBUCION = '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>';
    const COLOR_RUTA = '#213B31';
    const COLOR_DORADO = '#E0A94D';
    const COLOR_AVISO = '#A33A2B';   // parada con alerta municipal vigente

    const mapas = new Map();   // idElemento → { mapa, capaRuta, capaNegocios, pinYo }

    function escaparHtml(texto) {
        const div = document.createElement('div');
        div.textContent = texto ?? '';
        return div.innerHTML;
    }

    function esClasePhosphor(icono) {
        return typeof icono === 'string' && icono.trim().toLowerCase().startsWith('ph');
    }

    // conAviso: la parada tiene una alerta municipal vigente → borde rojo y un "!" en la esquina
    function iconoParada(numero, conAviso) {
        const borde = conAviso ? COLOR_AVISO : COLOR_DORADO;
        const insignia = conAviso
            ? '<span style="position:absolute;top:-6px;right:-6px;width:18px;height:18px;border-radius:50%;' +
              'background:' + COLOR_AVISO + ';border:2px solid #FFFDF8;color:#fff;font:800 11px Montserrat,sans-serif;' +
              'display:grid;place-items:center;box-shadow:0 2px 6px rgba(0,0,0,.35);">!</span>'
            : '';
        return L.divIcon({
            className: '',
            iconSize: [36, 36],
            iconAnchor: [18, 36],
            popupAnchor: [0, -34],
            html:
                '<div style="position:relative;width:36px;height:36px;">' +
                '<div style="width:36px;height:36px;border-radius:50% 50% 50% 0;transform:rotate(-45deg);' +
                'background:' + COLOR_RUTA + ';border:3px solid ' + borde + ';box-shadow:0 6px 14px rgba(0,0,0,.35);' +
                'display:grid;place-items:center;">' +
                '<span style="transform:rotate(45deg);color:#F7F1E3;font:700 14px Montserrat,sans-serif;">' + numero + '</span>' +
                '</div>' + insignia + '</div>'
        });
    }

    function iconoNegocio(icono) {
        const clase = esClasePhosphor(icono) ? icono : 'ph-fill ph-storefront';
        return L.divIcon({
            className: '',
            iconSize: [28, 28],
            iconAnchor: [14, 14],
            popupAnchor: [0, -12],
            html:
                '<div style="width:28px;height:28px;border-radius:50%;background:#FFFDF8;border:2px solid #9E5B3D;' +
                'box-shadow:0 3px 8px rgba(0,0,0,.25);display:grid;place-items:center;">' +
                '<i class="' + escaparHtml(clase) + '" style="color:#9E5B3D;font-size:14px;"></i>' +
                '</div>'
        });
    }

    function iconoYo() {
        return L.divIcon({
            className: '',
            iconSize: [22, 22],
            iconAnchor: [11, 11],
            html:
                '<div style="width:22px;height:22px;border-radius:50%;background:#2F80ED;border:4px solid #fff;' +
                'box-shadow:0 0 0 6px rgba(47,128,237,.25),0 3px 8px rgba(0,0,0,.3);"></div>'
        });
    }

    function obtenerOCrear(idElemento) {
        const existente = mapas.get(idElemento);
        const contenedor = document.getElementById(idElemento);
        if (!contenedor || typeof L === 'undefined') return null;

        // Blazor pudo re-crear el <div>: si el mapa quedó en un contenedor viejo, se rehace
        if (existente && existente.mapa.getContainer() === contenedor) return existente;
        if (existente) destruir(idElemento);

        const mapa = L.map(contenedor, { scrollWheelZoom: false }).setView(CENTRO_BOYACA, 8);
        L.tileLayer(TESELAS, { maxZoom: 19, attribution: ATRIBUCION }).addTo(mapa);

        const estado = {
            mapa,
            capaRuta: L.layerGroup().addTo(mapa),
            capaNegocios: L.layerGroup().addTo(mapa),
            pinYo: null,
            observador: null
        };
        mapas.set(idElemento, estado);

        // Si el contenedor aparece tarde o cambia de tamaño (animaciones, pestaña oculta, celular girado),
        // Leaflet debe recalcular su tamaño y volver a encuadrar la ruta; si no, queda mal centrado
        if ('ResizeObserver' in window) {
            estado.observador = new ResizeObserver(() => {
                if (contenedor.clientWidth === 0 || contenedor.clientHeight === 0) return;
                mapa.invalidateSize();
                encuadrar(estado);
            });
            estado.observador.observe(contenedor);
        } else {
            setTimeout(() => { mapa.invalidateSize(); encuadrar(estado); }, 400);
        }
        return estado;
    }

    function popupParada(p) {
        const tramo = p.kmDesdeAnterior != null
            ? '<div style="color:#9E5B3D;font-size:12px;margin-top:2px;">~' + p.kmDesdeAnterior + ' km · ' + escaparHtml(p.duracion) +
              ' desde ' + escaparHtml(p.desdeNombre) + '</div>'
            : '<div style="color:#5A6961;font-size:12px;margin-top:2px;">Punto de partida</div>';

        const foto = p.imagenUrl
            ? '<img src="' + escaparHtml(p.imagenUrl) + '" alt="" style="width:100%;height:90px;object-fit:cover;border-radius:8px;margin-bottom:6px;" ' +
              'onerror="this.remove()" />'
            : '';

        const waze = 'https://waze.com/ul?ll=' + p.lat + ',' + p.lng + '&navigate=yes';
        const google = 'https://www.google.com/maps/dir/?api=1&destination=' + p.lat + ',' + p.lng + '&travelmode=driving';

        return '<div style="min-width:180px;font-family:Montserrat,sans-serif;">' + foto +
            '<div style="font-size:11px;font-weight:700;color:#9E5B3D;text-transform:uppercase;letter-spacing:.5px;">Día ' + p.dia + ' · Parada ' + p.numero + '</div>' +
            '<div style="font-size:15px;font-weight:700;color:#1E2B25;">' + escaparHtml(p.nombre) + '</div>' + tramo +
            (p.aviso
                ? '<div style="margin-top:6px;padding:4px 8px;border-radius:6px;background:#F6E1DC;color:' + COLOR_AVISO +
                  ';font-size:12px;font-weight:600;">&#9888; ' + escaparHtml(p.aviso) + '</div>'
                : '') +
            '<div style="display:flex;gap:10px;margin-top:8px;font-size:12px;font-weight:700;">' +
            '<a href="/municipios/' + p.idMunicipio + '" target="_blank" rel="noopener">Ver guía</a>' +
            '<a href="' + waze + '" target="_blank" rel="noopener">Waze</a>' +
            '<a href="' + google + '" target="_blank" rel="noopener">Google Maps</a>' +
            '</div></div>';
    }

    // paradas: [{ numero, dia, idMunicipio, nombre, lat, lng, imagenUrl, kmDesdeAnterior, duracion, desdeNombre }]
    // Solo llegan las que tienen ubicación, ya en orden de la ruta.
    // geometria: [[lat, lng], …] de la carretera real (OpenRouteService) o null → línea recta punteada
    function dibujar(idElemento, paradas, geometria) {
        const estado = obtenerOCrear(idElemento);
        if (!estado) return;

        estado.capaRuta.clearLayers();
        if (!paradas || paradas.length === 0) {
            estado.mapa.setView(CENTRO_BOYACA, 8);
            return;
        }

        const puntos = paradas.map(p => [p.lat, p.lng]);

        if (geometria && geometria.length > 1) {
            // Carretera real: línea continua que sigue la vía, con borde claro para leerse sobre el mapa
            L.polyline(geometria, { color: '#FFFDF8', weight: 8, opacity: 0.9 }).addTo(estado.capaRuta);
            L.polyline(geometria, { color: COLOR_RUTA, weight: 5 }).addTo(estado.capaRuta);
        } else if (puntos.length > 1) {
            // Estimación: borde claro debajo + línea recta punteada encima
            L.polyline(puntos, { color: '#FFFDF8', weight: 7, opacity: 0.9 }).addTo(estado.capaRuta);
            L.polyline(puntos, { color: COLOR_RUTA, weight: 4, dashArray: '8 8' }).addTo(estado.capaRuta);
        }

        paradas.forEach(p => {
            L.marker([p.lat, p.lng], { icon: iconoParada(p.numero, !!p.aviso), zIndexOffset: 1000 })
                .bindPopup(popupParada(p))
                .bindTooltip(p.numero + '. ' + escaparHtml(p.nombre), { direction: 'top', offset: [0, -34] })
                .addTo(estado.capaRuta);
        });

        encuadrar(estado);
    }

    function encuadrar(estado) {
        // Sin tamaño todavía: el ResizeObserver lo encuadra cuando el contenedor aparezca
        const contenedor = estado.mapa.getContainer();
        if (contenedor.clientWidth === 0 || contenedor.clientHeight === 0) return;

        const capas = [];
        estado.capaRuta.eachLayer(l => { if (l.getLatLng) capas.push(l.getLatLng()); });
        if (estado.pinYo) capas.push(estado.pinYo.getLatLng());

        if (capas.length === 1) {
            estado.mapa.setView(capas[0], 12);
        } else if (capas.length > 1) {
            estado.mapa.fitBounds(L.latLngBounds(capas), { padding: [40, 40], maxZoom: 13 });
        }
    }

    // negocios: [{ idMunicipio, nombre, categoria, iconoCategoria, nombreMunicipio, lat, lng }]
    function negocios(idElemento, lista) {
        const estado = mapas.get(idElemento);
        if (!estado) return;

        estado.capaNegocios.clearLayers();
        (lista || []).forEach(n => {
            const popup =
                '<div style="font-family:Montserrat,sans-serif;">' +
                '<div style="font-size:11px;font-weight:700;color:#9E5B3D;text-transform:uppercase;">' + escaparHtml(n.categoria) + '</div>' +
                '<div style="font-size:14px;font-weight:700;color:#1E2B25;">' + escaparHtml(n.nombre) + '</div>' +
                '<div style="font-size:12px;color:#5A6961;">' + escaparHtml(n.nombreMunicipio) + '</div>' +
                '<div style="display:flex;gap:10px;margin-top:6px;font-size:12px;font-weight:700;">' +
                '<a href="/municipios/' + n.idMunicipio + '" target="_blank" rel="noopener">Ver en el municipio</a>' +
                '<a href="https://waze.com/ul?ll=' + n.lat + ',' + n.lng + '&navigate=yes" target="_blank" rel="noopener">Waze</a>' +
                '</div></div>';

            L.marker([n.lat, n.lng], { icon: iconoNegocio(n.iconoCategoria) })
                .bindPopup(popup)
                .addTo(estado.capaNegocios);
        });
    }

    // "Estoy aquí": devuelve { lat, lng } o { error } (permiso negado, sin GPS, etc.)
    function miUbicacion(idElemento) {
        return new Promise(resolve => {
            if (!('geolocation' in navigator)) {
                resolve({ error: 'Tu navegador no permite obtener la ubicación.' });
                return;
            }

            navigator.geolocation.getCurrentPosition(
                pos => {
                    const lat = pos.coords.latitude;
                    const lng = pos.coords.longitude;
                    const estado = mapas.get(idElemento);

                    if (estado) {
                        if (estado.pinYo) estado.pinYo.setLatLng([lat, lng]);
                        else estado.pinYo = L.marker([lat, lng], { icon: iconoYo(), zIndexOffset: 2000 })
                            .bindTooltip('Estás aquí', { direction: 'top', offset: [0, -10] })
                            .addTo(estado.mapa);
                        encuadrar(estado);
                    }

                    resolve({ lat, lng });
                },
                err => resolve({
                    error: err.code === 1
                        ? 'No diste permiso para ver tu ubicación.'
                        : 'No pudimos obtener tu ubicación. Inténtalo de nuevo.'
                }),
                { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 });
        });
    }

    function destruir(idElemento) {
        const estado = mapas.get(idElemento);
        if (!estado) return;
        if (estado.observador) estado.observador.disconnect();
        estado.mapa.remove();
        mapas.delete(idElemento);
    }

    return { dibujar, negocios, miUbicacion, destruir };
})();
