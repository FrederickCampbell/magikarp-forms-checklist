const { contextBridge, ipcRenderer } = require("electron");

contextBridge.exposeInMainWorld("appWindow", {
  minimize: () => ipcRenderer.send("window:minimize"),
  toggleMaximize: () => ipcRenderer.send("window:toggleMaximize"),
  close: () => ipcRenderer.send("window:close")
});
