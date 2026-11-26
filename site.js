const compare = document.querySelector("#comparar");
const comparison = document.querySelector("#comparacion");
const viewer = document.querySelector("#visor");
const viewerTitle = document.querySelector("#titulo-visor");
const viewerImage = document.querySelector("#captura-visor");

compare.addEventListener("input", () => {
  comparison.style.setProperty("--reveal", compare.value + "%");
  compare.setAttribute("aria-valuetext", compare.value + " % de día");
});

for (const shot of document.querySelectorAll(".shot")) {
  shot.addEventListener("click", (event) => {
    if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
    event.preventDefault();
    viewerTitle.textContent = shot.dataset.title;
    viewerImage.src = shot.href;
    viewerImage.alt = shot.dataset.alt;
    viewer.showModal();
  });
}
