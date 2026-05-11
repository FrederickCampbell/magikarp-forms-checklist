const { app, BrowserWindow, ipcMain, dialog } = require("electron");
const path = require("path");

let mainWindow = null;
let isQuitting = false;

const gotLock = app.requestSingleInstanceLock();

if (!gotLock) {
  app.exit(0);
}

function getIconPath() {
  return path.join(__dirname, "assets", "app-icon.png");
}

function quitHard(exitCode = 0) {
  isQuitting = true;

  try {
    for (const win of BrowserWindow.getAllWindows()) {
      if (!win.isDestroyed()) {
        win.destroy();
      }
    }
  } catch {
    // ignore destroy errors
  }

  app.exit(exitCode);
}

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 760,
    height: 720,
    minWidth: 560,
    minHeight: 520,
    frame: false,
    backgroundColor: "#101116",
    show: false,
    resizable: true,
    icon: getIconPath(),
    webPreferences: {
      preload: path.join(__dirname, "preload.js"),
      contextIsolation: true,
      nodeIntegration: false
    }
  });

  mainWindow.removeMenu();

  mainWindow.once("ready-to-show", () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.show();
    }
  });

  mainWindow.on("closed", () => {
    mainWindow = null;

    if (isQuitting) {
      return;
    }

    if (process.platform !== "darwin") {
      app.quit();
    }
  });

  mainWindow.webContents.on("render-process-gone", (_event, details) => {
    console.error("Renderer process gone:", details);
    quitHard(1);
  });

  mainWindow.loadFile(path.join(__dirname, "index.html"));
}

app.whenReady().then(() => {
  createWindow();
});

app.on("second-instance", () => {
  if (!mainWindow || mainWindow.isDestroyed()) {
    createWindow();
    return;
  }

  if (mainWindow.isMinimized()) {
    mainWindow.restore();
  }

  mainWindow.focus();
});

app.on("activate", () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

app.on("before-quit", () => {
  isQuitting = true;
});

app.on("window-all-closed", () => {
  app.quit();
});

process.on("uncaughtException", (error) => {
  console.error("Uncaught exception:", error);

  try {
    dialog.showErrorBox(
      "Magikarp Forms Checklist Error",
      String(error && error.stack ? error.stack : error)
    );
  } catch {
    // ignore dialog errors
  }

  quitHard(1);
});

process.on("unhandledRejection", (reason) => {
  console.error("Unhandled rejection:", reason);
  quitHard(1);
});

ipcMain.on("window:minimize", (event) => {
  const win = BrowserWindow.fromWebContents(event.sender);

  if (win && !win.isDestroyed()) {
    win.minimize();
  }
});

ipcMain.on("window:toggleMaximize", (event) => {
  const win = BrowserWindow.fromWebContents(event.sender);

  if (!win || win.isDestroyed()) {
    return;
  }

  if (win.isMaximized()) {
    win.unmaximize();
  } else {
    win.maximize();
  }
});

ipcMain.on("window:close", () => {
  quitHard(0);
});