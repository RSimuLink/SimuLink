# App brand assets

`roche-logo.png` — the Roche logo shown in the top-right of the main toolbar.
`SimuLinkLogo.png` — the SimuLink product logo shown in the top-left of the
main toolbar (in line with the Roche logo).

The app loads these files at runtime from the `Assets` folder next to the
executable. If a file is missing, that logo area is left blank (startup is not
affected).

Recommended: transparent-background PNGs sized for high-DPI displays.
- `roche-logo.png`: roughly 2:1 aspect; displayed at ~80×40 px with `Zoom`
  scaling (e.g. an 800×400 source).
- `SimuLinkLogo.png`: roughly 3:1 aspect; displayed at ~130×40 px with `Zoom`
  scaling.
