// Descarga de archivos generados por el API (por ejemplo, el reporte municipal en CSV).
// Blazor entrega el contenido como DotNetStreamReference; aquí se convierte en un enlace temporal.
window.archivos = {
    descargar: async function (nombre, tipo, flujo) {
        const buffer = await flujo.arrayBuffer();
        const url = URL.createObjectURL(new Blob([buffer], { type: tipo }));
        const enlace = document.createElement("a");
        enlace.href = url;
        enlace.download = nombre || "archivo";
        document.body.appendChild(enlace);
        enlace.click();
        enlace.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
};
