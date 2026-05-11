const STORAGE_KEY = "magikarp-forms-checklist-v1";

function loadState() {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY)) || {};
  } catch {
    return {};
  }
}

function saveState(state) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
}

function updateCounter() {
  const boxes = Array.from(document.querySelectorAll("input[type='checkbox']"));
  const checked = boxes.filter(box => box.checked).length;
  document.querySelector("#counter").textContent = `${checked}/${boxes.length} checked`;
}

window.addEventListener("DOMContentLoaded", () => {
  let state = loadState();

  document.querySelectorAll("[data-form]").forEach(card => {
    const key = card.dataset.form;
    const checkbox = card.querySelector("input[type='checkbox']");

    checkbox.checked = Boolean(state[key]);

    checkbox.addEventListener("change", () => {
      state[key] = checkbox.checked;
      saveState(state);
      updateCounter();
    });
  });

  document.querySelector("#reset").addEventListener("click", () => {
    state = {};

    document.querySelectorAll("[data-form]").forEach(card => {
      const checkbox = card.querySelector("input[type='checkbox']");
      checkbox.checked = false;
    });

    saveState(state);
    updateCounter();
  });

  document.querySelector("#minimize").addEventListener("click", () => {
    window.appWindow.minimize();
  });

  document.querySelector("#maximize").addEventListener("click", () => {
    window.appWindow.toggleMaximize();
  });

  document.querySelector("#close").addEventListener("click", () => {
    window.appWindow.close();
  });

  updateCounter();
});
