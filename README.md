# Magikarp Forms Checklist

A small offline Electron checklist for the 20 PolishedDex Magikarp forms.

## Features

- Local images
- Works offline after build
- Four forms per row
- Dark mode interface
- Clean frameless window
- Custom minimize, maximize, and close buttons
- Saves checked forms locally

## Development

Install dependencies:

    npm install

Run locally:

    npm start

Build portable Windows EXE:

    npm run build

The finished EXE appears in:

    release\Magikarp-Forms-Checklist-Portable.exe

## Notes

The source is intentionally simple so people can inspect it before running the app.

Do not commit node_modules or release builds to GitHub. They are ignored by default.
