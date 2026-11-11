// Saltos a secciones dentro de la misma página (por ejemplo, el índice de la ficha del municipio).
// Blazor cambia la dirección con "#seccion" pero no desplaza la página; esto sí lo hace.
window.navegacion = {
    irASeccion: function (id) {
        const destino = document.getElementById(id);
        if (!destino) return;
        const reducir = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        destino.scrollIntoView({ behavior: reducir ? "auto" : "smooth", block: "start" });
        // Ruta completa: con <base href="/"> un "#id" suelto quedaría como "/#id"
        history.replaceState(history.state, "", location.pathname + location.search + "#" + id);
    }
};
